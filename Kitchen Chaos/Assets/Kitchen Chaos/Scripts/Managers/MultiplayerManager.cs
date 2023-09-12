using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace KC
{
    public class MultiplayerManager : NetworkBehaviour
    {
        public static MultiplayerManager Instance { get; private set; } = null;
        [SerializeField] private NetworkKitchenItemsListSO networkKitchenItemsListSO;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else if (Instance != this)
                Destroy(this);
        }

        public static int GetNetworkKitchenItemIndex(KitchenItemSO kitchenItemSO)
        {
            int index = Instance.networkKitchenItemsListSO.IndexOf(kitchenItemSO);
            if (index == -1)
                Instance.LogError("KitchenItem-NetworkIndex not found, 'NetworkKitchenItemsListSO' has been changed!!!");
            return index;
        }
        public static KitchenItemSO GetNetworkKitchenItem(int index)
        {
            KitchenItemSO kitchenItemSO = Instance.networkKitchenItemsListSO.AtIndex(index);
            if (kitchenItemSO == null)
                Instance.LogError("KitchenItem not found, as given Network-Index is invalid!!!");
            return kitchenItemSO;
        }


        public void SpawnKitchenObject(KitchenItemSO kitchenItemSO, IKitchenObjectHolder kitchenObjHolder)
        {
            int networkKitchenItemIndex = GetNetworkKitchenItemIndex(kitchenItemSO);
            //this.Log($"Spawning KitchenObject: {kitchenItemSO.UniqueName} on Holder: {kitchenObjHolder}");
            SpawnKitchenObjectServerRpc(networkKitchenItemIndex, kitchenObjHolder.GetNetworkObject());
        }

        [ServerRpc(RequireOwnership = false)] // to allow clients to call server rpc's to spawn objects as only server can spawn objects
        private void SpawnKitchenObjectServerRpc(int networkKitchenItemIndex, NetworkObjectReference kitchenObjHolderNetworkObjRef)
        {
            KitchenItemSO kitchenItemSO = GetNetworkKitchenItem(networkKitchenItemIndex);
            Transform kitchenItemTransform = Instantiate(kitchenItemSO.Prefab).transform;

            NetworkObject kitchenItemNetworkObj = kitchenItemTransform.GetComponent<NetworkObject>();

            // this NetworkObject.Spawn() can only be called by server, thus the need of ServerRpc
            kitchenItemNetworkObj.Spawn(destroyWithScene: true); // by default, Spawn(destroyWithScene:false)

            // checking if the given NetworkObjectReference is valid from server end
            if (!kitchenObjHolderNetworkObjRef.TryGet(out NetworkObject kitchenObjHolderNetworkObj))
            {
                this.LogWarning("Invalid KitchenObjHolderNetworkObjectReference passed to SpawnKitchenObject-ServerRpc!");
                return;
            }

            var kitchenObj = kitchenItemNetworkObj.GetComponent<KitchenObject>();
            IKitchenObjectHolder kitchenObjHolder = kitchenObjHolderNetworkObj.GetComponent<IKitchenObjectHolder>();
            //this.Log($"Spawning[ServerRpc] KitchenObject: {kitchenItemSO.UniqueName} on Holder: {kitchenObjHolder}");

            kitchenObj.SetKitchenObjectHolder(kitchenObjHolder);
        }


        public void DestroyKitchenObject(KitchenObject kitchenObject) => DestrorKitchenObjectServerRpc(kitchenObject.NetworkObject);

        [ServerRpc(RequireOwnership = false)]
        private void DestrorKitchenObjectServerRpc(NetworkObjectReference kitchenObjNetworkObjRef)
        {
            if (!kitchenObjNetworkObjRef.TryGet(out NetworkObject kitchenObjNetworkObj))
            {
                this.LogWarning("Invalid switchKitchenObjectNetworkObjectRef passed to DestrorKitchenObject-ServerRpc!");
                return;
            }
            KitchenObject kitchenObject = kitchenObjNetworkObj.GetComponent<KitchenObject>();

            if (kitchenObject.GetKitchenObjectHolder() != null)
                DestrorKitchenObjectCallbackClientRpc(kitchenObject.GetKitchenObjectHolder().GetNetworkObject());

            kitchenObject.DestrorSelf();
        }

        [ClientRpc]
        private void DestrorKitchenObjectCallbackClientRpc(NetworkObjectReference kitchenObjHolderNetworkObjRef)
        {
            if (!kitchenObjHolderNetworkObjRef.TryGet(out NetworkObject kitchenObjHolderNetworkObj))
            {
                this.LogWarning("Invalid switchKitchenObjectHolderNetworkObjectRef passed to DestrorKitchenObjectCallback-ClientRpc!");
                return;
            }
            IKitchenObjectHolder kitchenObjectHolder = kitchenObjHolderNetworkObj.GetComponent<IKitchenObjectHolder>();

            kitchenObjectHolder.ClearKitchenObject(); // remove reference from parent holder
            // trash counter deletes any object tats been set to it, in that case,
            // 'kitchenObjectHolder' might still be uninitialized
        }
    }
}