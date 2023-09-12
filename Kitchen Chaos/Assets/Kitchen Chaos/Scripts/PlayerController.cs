using System;
using Unity.Multiplayer.Samples.Utilities.ClientAuthority;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace KC
{
    public class PlayerController : NetworkBehaviour, IKitchenObjectHolder
    {
        #region Declarations: Events
        public class SelectedCounterChangedEventArgs : EventArgs { public BaseCounter SelectedCounter; }
        public event EventHandler<SelectedCounterChangedEventArgs> OnSelectedCounterChanged;
        public static event EventHandler OnPlayerPickedSomething;
        public static event EventHandler OnAnyPlayerSpawned; // multiplayer
        
        public static void ResetStaticData()
        {
            //Debug.Log("PlayerController static subcribers:" + OnPlayerPickedSomething.GetInvocationList().Length);
            OnPlayerPickedSomething = null;
            OnAnyPlayerSpawned = null;
        }
        #endregion

        #region Singleton
        public static PlayerController LocalInstance { get; private set; } = null;

        public override void OnNetworkSpawn()
        {
            // wait till IsSpawned is turned to true, as before that all the network variables are not initialized
            // thus, this cant be initialized at Awake() anymore, as at awake, IsOwner and other network properties are not yet assigned

            if (IsOwner) // simply add the local player to static reference
            {
                LocalInstance = this;
                //Debug.Log("Local Player Instance Set!, instance-id:" + GetInstanceID() + "hash-code:" + GetHashCode());
            }
            OnAnyPlayerSpawned?.Invoke(this, EventArgs.Empty);

            name = name.Replace("Clone", $"ID:{OwnerClientId}"); // chaning the GameObject-name for easiler identification in local hierarchy
        }
        #endregion

        #region Declarations: Exposed Properties
        [Header("Player Controls")]
        [SerializeField] private float movementSpeed = 7f;
        [SerializeField] private float rotationSpeed = 10f;
        [SerializeField] private CapsuleCollider capsuleCollider = null; // just for a more clean visuallization
        [SerializeField] private float interactiveRadius = 1.5f;
        [SerializeField] private LayerMask countersLayerMask;
        [SerializeField] private Transform objHoldingPoint;

        [field:Header("Debug Display")] // for debugging
        [SerializeField] private BaseCounter selectedCounter;
        [SerializeField] private KitchenObject KitchenObjHeld;
        [SerializeField] private bool isColliding;
        #endregion

        #region Declarations: Private Properties
        private bool triggerWalkAnim; public bool IsWalkAnimTriggered => triggerWalkAnim;
        private bool isLookingInventory = false;
        private Vector3 dirVec;
        private bool isClientAuth;
        #endregion

        private void Start()
        {
            if (capsuleCollider == null)
                capsuleCollider = GetComponent<CapsuleCollider>();

            InputManager.Instance.OnPrimaryInteractAction += HandlePlayerPrimaryInteraction;
            InputManager.Instance.OnSecondaryInteractAction += HandlePlayerSecondaryInteraction;
            InputManager.Instance.OnInventoryInteractAction += HandlePlayerInventoryInteraction;

            isClientAuth = GetComponent<NetworkTransform>() is ClientNetworkTransform;
            // client authentication, means that the game update logic done by the player is not verified from server side
        }

        #region Handle Interactions

        private void HandlePlayerPrimaryInteraction(object sender, EventArgs e)
        {
            //this.Log($"Handling Player Primary Interaction: IsGamePlaying:{GameManager.Instance.IsGamePlaying}, isLookingInventory:{isLookingInventory}, selectedCounter:{selectedCounter}");
            if (!GameManager.Instance.IsGamePlaying) return;

            //UpdateInteractions();
            if (isLookingInventory) 
                HandlePlayerInventoryInteraction(this, null); // before interaction, turn off inventory mode

            selectedCounter?.InteractPrimary(this); // give reference to which player is interacting with it
        }
        private void HandlePlayerSecondaryInteraction(object sender, EventArgs e)
        {
            if (!GameManager.Instance.IsGamePlaying) return;

            selectedCounter?.InteractSecondary(this);
        }

        private void HandlePlayerInventoryInteraction(object sender, EventArgs e)
        {
            if (!GameManager.Instance.IsGamePlaying) return;

            if (!HasKitchenObject() || !GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject))
                return;

            // toggle on/off plat contents
            plateKitchenObject.TogglePlateContentsDropView(SetIsLookingInventory, autoLeaveInventoryAfterDrop:false);
        }

        private void SetIsLookingInventory(bool isLookingInventory)
        {
            this.isLookingInventory = isLookingInventory;
            triggerWalkAnim = !isLookingInventory; // lock movement when holding plate inventory    
        }

        #endregion

        private void Update()
        {
            // multiplayer check condition
            if (!IsOwner) return;

            // rest game mechanics
            if (isLookingInventory) return;

            NetworkHandleMovementAuth();

            UpdateSelections();
        }

        private void UpdateSelections()
        {
            // make sure dirDir is initialized before (espicially client side)
            if (!Physics.Raycast(transform.position, dirVec, out RaycastHit hit, interactiveRadius, countersLayerMask))
            {
                ChangeSelectedCounter(null);
                return;
            }

            if (hit.transform.TryGetComponent<BaseCounter>(out var counter))
            {
                //counter.Interact();
                ChangeSelectedCounter(counter);
            }
            else
            {
                ChangeSelectedCounter(null);
            }            
        }

        private void ChangeSelectedCounter(BaseCounter counter)
        {
            if (selectedCounter == counter) return;

            selectedCounter = counter;
            OnSelectedCounterChanged?.Invoke(this,
                new SelectedCounterChangedEventArgs { SelectedCounter = this.selectedCounter });

            //Debug.Log("Active Selection Changed: " + (SelectedCounter == null ? "none" : SelectedCounter));
        }

        private void NetworkHandleMovementAuth() // client calls server to authenticate
        {
            Vector2 inpVec = InputManager.Instance.TickMovementVectorNormalized();
            // get input from client end and feed it to server to do the reqired action
            triggerWalkAnim = (inpVec != Vector2.zero);

            // initialize dirVec in client end, so that selection-visuals and interactions can take place
            if (inpVec == Vector2.zero)
            {
                dirVec = transform.forward;
                // incase input is not given, set default moveDir as forward dir of transform, so that interactions can take place
                return;
            }
            dirVec = new Vector3(inpVec.x, 0, inpVec.y);

            if (isClientAuth)
                UpdateMovement(dirVec); // without netcode
            else
                NetworkHandleMovementServerRpc(dirVec); // with netcode, server rpc call
        }

        [ServerRpc(RequireOwnership = false)] // server rpc to reponds to client's request
        private void NetworkHandleMovementServerRpc(Vector3 dirVec)
        {
            UpdateMovement(dirVec);
        }

        private void UpdateMovement(Vector3 dirVec)
        {
            this.dirVec = dirVec; // just in case of sync in server side also

            // transform's local z axis to made to look at move-dir
            transform.forward = Vector3.Slerp(transform.forward, dirVec, rotationSpeed * Time.deltaTime);

            Vector3 moveDir = dirVec;
            float moveAmtDT = movementSpeed * Time.deltaTime;
            bool canMove = !CheckCollisionDetection(moveDir, moveAmtDT);

            if (!canMove)
            {
                /* attempt single axis movements
                    only the resp axis's inp values are not zero,
                    or else it might cause unwanted situations, say 
                    even when x-inp is 0 here if it doesnt have collision, it will make can move in x-axis with value 0
                    similary when z-inp is 0, and no collision is found in z axis, 
                    depending upon in which order these axes checkes
                */

                // X-Axis (if inpDir.x is not 0 only)
                Vector3 moveDirX = new Vector3(moveDir.x, 0, 0).normalized;

                canMove = (moveDir.x > 0.5f || moveDir.x < -0.5f) // add a stick zone theshold, to avoid movement for slight offsets
                    && !CheckCollisionDetection(moveDirX, moveAmtDT);

                if (canMove)
                    moveDir = moveDirX;
                else
                {
                    // Z-Axis (if inpDir.z is not 0 only)
                    Vector3 moveDirZ = new Vector3(0, 0, moveDir.z).normalized;

                    canMove = (moveDir.z > 0.5f || moveDir.z < -0.5f) // add a stick zone theshold, to avoid movement for slight offsets
                        && !CheckCollisionDetection(moveDirZ, moveAmtDT);

                    if (canMove)
                        moveDir = moveDirZ;
                }
            }

            if (canMove)
                transform.position += moveAmtDT * moveDir;
        }

        private bool CheckCollisionDetection(Vector3 direction, float distance)
        {
            bool collisionDetected;
            //canMove = Physics.Raycast(transform.position, moveDir, collisionDetectionRadius);
            // raycasting is not best case senario as it doesn't consider the entire bodu of the object to cast for collision detection

            var capsuleHeightMedian = Vector3.up * (capsuleCollider.height / 2);
            var capsuleRadius = capsuleCollider.radius;
            var capsuleCenter = capsuleCollider.center;

            collisionDetected = Physics.CapsuleCast(transform.position + capsuleCenter - capsuleHeightMedian,
                transform.position + capsuleCenter + capsuleHeightMedian,
                capsuleRadius, direction, distance);
            // point1 -> bottom end point of the capsule, point2 -> top end point of the capsule

            // Note: collider.Raycast is execute when this collider receive a ray, it means the origin of the ray is from other object
            return collisionDetected;
        }


        public BaseCounter GetSelectedCounter() => selectedCounter;

        #region Interface: KitchenObjectHolder
        public Transform GetHolderTransform() => objHoldingPoint;
        public void SetKitchenObject(KitchenObject kitchenObject)
        {
            // when ever any script trys to set player's kictchen object,
            if (kitchenObject == null)
            {
                ClearKitchenObject(); 
                return;
            }
            this.KitchenObjHeld = kitchenObject;

            //if (kitchenObject != null) // means player has pickup something
            OnPlayerPickedSomething?.Invoke(GetHolderTransform(), EventArgs.Empty);
            // else means player has dropped something (need to be called from dropped location)
        }
        public KitchenObject GetKitchenObject() => KitchenObjHeld;
        public void ClearKitchenObject()
        {
            //Debug.Log($"Player[{name}] :: Cleared Holding Kitchen Object: {this.KitchenObjHeld}");
            this.KitchenObjHeld = null;
        }
        public bool HasKitchenObject() => KitchenObjHeld != null; 
        public bool CanHoldKitchenObject(KitchenItemSO kitchenItemSO) => true;
        public NetworkObject GetNetworkObject() => NetworkObject;
        #endregion
    }
}