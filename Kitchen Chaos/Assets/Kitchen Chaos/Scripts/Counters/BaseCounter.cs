using KC;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace KC
{
    public abstract class BaseCounter : NetworkBehaviour, IKitchenObjectHolder
    {
        public static event EventHandler OnPlayerDroppedSomethingOnCounters;
        public static void ResetStaticData()
        {
            //Debug.Log("BaseCounter static subcribers:"+ OnPlayerDroppedSomethingOnCounters.GetInvocationList().Length);
            OnPlayerDroppedSomethingOnCounters = null;
        }

        // lets leave these as private sot that it will be accessed only through base class funtions
        [SerializeField] private Transform counterTopPoint;
        private KitchenObject kitchenObjHeld;

        public abstract void InteractPrimary(PlayerController player);
        public virtual void InteractSecondary(PlayerController player)
        {
            // all counters need not neccessarily have multiple interactions
            this.LogWarning("No Secondary Interaction set for this counter!");
        }

        protected void SwitchNewKitchenObject(KitchenItemSO newKitchenItemSO)
        {
            KitchenObject.DestroyKitchenObject(GetKitchenObject());
            KitchenObject.SpawnKitchenObject(newKitchenItemSO, this);
        }


        // use this contition when both player and counter has its own objects
        // keep ( checkCounterAlso = false ), if the counter cant hold plate as its kitchenObjectHeld, to save time
        protected void CheckPossiblePlateInteractions(PlayerController player, bool canCounterHoldPlate = true) => 
            CheckPossiblePlateInteractionsServerRpc(player, canCounterHoldPlate);

        [ServerRpc(RequireOwnership = false)]
        protected void CheckPossiblePlateInteractionsServerRpc(NetworkBehaviourReference playerBehaviourRef, bool canCounterHoldPlate = true)
        {
            if (!playerBehaviourRef.TryGet(out PlayerController player))
            {
                this.LogWarning($"Invalid {nameof(playerBehaviourRef)} passed to {nameof(CheckPossiblePlateInteractionsServerRpc)}!");
                return;
            }

            // returns bool if the interaction wass succesful or not
            bool returnVal;
            // in multiplayer, will be replicated through rpc's

            PlateKitchenObject playerPlateKitchenObject = null, counterPlateKitchenObject = null;
            bool isPlayerKitchenObjectPlate = player.GetKitchenObject().TryGetPlate(out playerPlateKitchenObject);
            bool isCounterKitchenObjectPlate = canCounterHoldPlate && 
                this.GetKitchenObject().TryGetPlate(out counterPlateKitchenObject);

            bool isBothHoldingPlateObj = (isCounterKitchenObjectPlate && isPlayerKitchenObjectPlate);
            bool isBothHoldingKitchenObj = (!isCounterKitchenObjectPlate && !isPlayerKitchenObjectPlate);

            if (isBothHoldingPlateObj || isBothHoldingKitchenObj)
            {
                // both player and counter holds a plate or a kitchen-object
                this.LogWarning($"Both Player[{player}] & Counter[{this}] are already holding objects! " +
                    $"{nameof(isBothHoldingKitchenObj)}:{isBothHoldingKitchenObj}, {nameof(isBothHoldingPlateObj)}:{isBothHoldingPlateObj}");
                returnVal = false;
            }
            else
            {
                IKitchenObjectHolder ingredientKitchenObjectHolder;
                PlateKitchenObject plateKitchenObject;

                if (isPlayerKitchenObjectPlate)
                {
                    ingredientKitchenObjectHolder = this;
                    plateKitchenObject = playerPlateKitchenObject;
                }
                else //if (isCounterKitchenObjectPlate)
                {
                    ingredientKitchenObjectHolder = player;
                    plateKitchenObject = counterPlateKitchenObject;
                }

                AddIngredientToPlate(plateKitchenObject, ingredientKitchenObjectHolder.GetKitchenObject());
                returnVal = true;
            }

            CheckPossiblePlateInteractionsServerCallback(returnVal);

            // by default, lets notify all client through client rpc (for now not needed)
            //CheckPossiblePlateInteractionsClientRpc(player.OwnerClientId, returnVal);
        }

        #region Plate Interactions Overridable Callbacks
        // change this logic to apply based return bool val of CheckPossiblePlateInteractionsServerRpc if any interation have taken place or not
        protected virtual void CheckPossiblePlateInteractionsServerCallback(bool havePlaveInteractionTaken)
        {
            //if (!IsServer) return; // should only be executed by server
        }

        [Obsolete("not needed for now, just for easier identification marked as obsolete")]
        [ClientRpc] // callback client rpc instead of return val
        protected virtual void CheckPossiblePlateInteractionsClientRpc(ulong targetClientID, bool havePlaveInteractionTaken)
        {
            /* Example:
            if (NetworkManager.LocalClientId == targetClientID)
            {
                // targeted client rpc
            }
            else
            {
                // observers that are not the resp target
            }
            */
        }
        #endregion

        protected static void AddIngredientToPlate(PlateKitchenObject plateKitchenObject, KitchenObject kitchenObject)
        {
            plateKitchenObject.AddIngredient(kitchenObject);
        }

        #region Interface: KitchenObjectHolder
        public virtual bool CanHoldKitchenObject(KitchenItemSO kitchenItemSO) => false; // this needs to defined seperately for each counter
        public Transform GetHolderTransform() => counterTopPoint;
        public virtual void SetKitchenObject(KitchenObject kitchenObject)
        {
            // when ever any script trys to set counter's kictchen object,
            if (kitchenObject == null)
            {
                ClearKitchenObject();
                return;
            }
            this.kitchenObjHeld = kitchenObject;

            // if (kitchenObject != null) : means player has dropped something on the counters
            // else: means player has picked something from counters (implemented in general, in player script)

            if (kitchenObject.GetKitchenObjectHolder() is PlayerController) // check if prev owner was player
                OnPlayerDroppedSomethingOnCounters?.Invoke(GetHolderTransform(), EventArgs.Empty);
            // else: counter must swapped the kitched-object to a next stage recipe processed/cooked kitchen-object
            
        }
        public KitchenObject GetKitchenObject() => kitchenObjHeld;
        public void ClearKitchenObject() => this.kitchenObjHeld = null;
        public bool HasKitchenObject() => kitchenObjHeld != null;
        public NetworkObject GetNetworkObject() => NetworkObject;
        #endregion
    }
}