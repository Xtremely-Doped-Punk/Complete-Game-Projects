using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace KC
{
    public class KitchenObject : NetworkBehaviour
    {
        public enum MainTypes { Edible, NonEdible, }
        public enum EdibleTypes { raw, processed, }
        public enum NonEdibleTypes { disposable, reusable, }

        [field: SerializeField] public KitchenItemSO KitchenItemSO { get; private set; } = null;

        private IKitchenObjectHolder kitchenObjectHolder;
        private FollowTransform followTransform;
        protected virtual void Awake()
        {
            followTransform = GetComponent<FollowTransform>();
        }

        // note: interfaces cant be serialized as they don't have any actual data unlike classes

        public IKitchenObjectHolder GetKitchenObjectHolder() => kitchenObjectHolder;
        public void SetKitchenObjectHolder(IKitchenObjectHolder switchKitchenObjectHolder)
        {
            if (switchKitchenObjectHolder == null)
            {
                this.LogError("Cant place at null KitchenObjectHolder!");
                return;
            }
            if (switchKitchenObjectHolder.HasKitchenObject())
            {
                this.LogError("Cant place at " + switchKitchenObjectHolder + " as it already holds a object!");
                return;
            }

            SetKitchenObjectHolderServerRpc(switchKitchenObjectHolder.GetNetworkObject());
        }

        [ServerRpc (RequireOwnership = false)] // so that client can also call, as to sync counters and players - holding different objs
        public void SetKitchenObjectHolderServerRpc(NetworkObjectReference switchKitchenObjHolderNetworkObjRef)
        {
            if (!switchKitchenObjHolderNetworkObjRef.TryGet(out NetworkObject switchKitchenObjHolderNetworkObj))
            {
                this.LogWarning($"Invalid {nameof(switchKitchenObjHolderNetworkObjRef)} passed to {nameof(SetKitchenObjectHolderServerRpc)}!");
                return;
            }
            
            IKitchenObjectHolder switchKitchenObjectHolder = switchKitchenObjHolderNetworkObj.GetComponent<IKitchenObjectHolder>();

            if (switchKitchenObjectHolder == null || switchKitchenObjectHolder.HasKitchenObject())
                return; // simple recheck conditions from server side as well

            SetKitchenObjectHolderCallbackClientRpc(switchKitchenObjHolderNetworkObj);
        }

        [ClientRpc]
        public void SetKitchenObjectHolderCallbackClientRpc(NetworkObjectReference switchKitchenObjHolderNetworkObjRef)
        {
            if (!switchKitchenObjHolderNetworkObjRef.TryGet(out NetworkObject switchKitchenObjHolderNetworkObj))
            {
                this.LogWarning($"Invalid {nameof(switchKitchenObjHolderNetworkObjRef)} passed to {nameof(SetKitchenObjectHolderCallbackClientRpc)}!");
                return;
            }
            IKitchenObjectHolder switchKitchenObjectHolder = switchKitchenObjHolderNetworkObj.GetComponent<IKitchenObjectHolder>();

            // note: first set KO in new holder, so that new holder can see previous holder was
            switchKitchenObjectHolder.SetKitchenObject(this);
            this.kitchenObjectHolder?.ClearKitchenObject(); // then, remove kitchen object ref from prev holder

            //Debug.Log(">>> kitchen object: " + KitchenItemSO.UniqueName + " has been switched from "
            //            + this.kitchenObjectHolder + " to" + switchKitchenObjectHolder + ", by playerID: "+switchKitchenObjHolderNetworkObj.OwnerClientId);

            this.kitchenObjectHolder = switchKitchenObjectHolder;

            //transform.parent = switchKitchenObjectHolder.GetHolderTransform();
            //transform.localPosition = Vector3.zero;
            /*            
            Debug.Log(switchKitchenObjectHolder.GetHolderTransform().GetComponentInParent<NetworkObject>() == null ? 
                "Holder Parent does'nt have NetworkObject" : "Holder Parent has NetworkObject");

            As the the Kitchen Object is a Network Behaviour, it is supposed to have a Network Object Component attached to it
            as a result, parenting of NetworkObject must always moved under another Parent having NetworkObject.
            But also note that, applying these parrent changes during runtime between dynamically spawned NetworkObjects
            is not possible and it would throw errors, so is our case here, as KitchenObj and Player are dynamically spawned NetworkObj
            Thus, one work arround for these cases in multiplayers especially is, to have FollowTransform custom script attached
            to the child object with the reference of parent assigned dynamically to it, so that it simply behaves the same way as 
            parenting the tranform...
             */

            // making only the player, to make follow rotation updates, makes it looks
            // more dynamical as in same case as previously done way, using parenting..
            bool shouldUpdateRotation = switchKitchenObjectHolder is PlayerController;
            followTransform.SetTargetTransform(switchKitchenObjectHolder.GetHolderTransform(), shouldUpdateRotation);
        }

        //[Server only call]
        public void DestrorSelf()
        {
            if (!IsServer)
            {
                this.LogWarning("KitchenObj DestroySelf() should only be called from server end!");
                return;
            }

            Destroy(gameObject); // for testing same, just add a delay, so that client's holder's can sync
        }


        public static void SpawnKitchenObject(KitchenItemSO kitchenItemSO, IKitchenObjectHolder kitchenObjHolder)
        {
            /* network spawning can't be called through a static functions, thus we use a static instance of a GameObject
             as in the MutiplayerManager's instance, which will used to spawn through network for all clients, also
            note that, network spawn also takes place through a ServerRpc request and ClientRpc call on all clients to sync spawning
            thus as ususal RPC call must be of 'void' return type;
            */

            MultiplayerManager.Singleton.SpawnKitchenObject(kitchenItemSO, kitchenObjHolder);
        }
        public static void DestroyKitchenObject(KitchenObject kitchenObject)
        {
            MultiplayerManager.Singleton.DestroyKitchenObject(kitchenObject);
        }

        public bool TryGetPlate(out PlateKitchenObject plateKitchenObject)
        {
            plateKitchenObject =  this as PlateKitchenObject;
            if (this is PlateKitchenObject)
                return true;
            else
                return false;
        }
    }
}