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
            else
                Destroy(this);
        }

        public int GetNetworkKitchenItemIndex(KitchenItemSO kitchenItemSO)
        {
            int index = networkKitchenItemsListSO.IndexOf(kitchenItemSO);
            if (index == -1)
                Debug.LogError("KitchenItem-NetworkIndex not found, 'NetworkKitchenItemsListSO' has been changed!!!");
            return index;
        }
        public KitchenItemSO GetNetworkKitchenItem(int index)
        {
            KitchenItemSO kitchenItemSO = networkKitchenItemsListSO.AtIndex(index);
            if (kitchenItemSO == null)
                Debug.LogError("KitchenItem not found, as given Network-Index is invalid!!!");
            return kitchenItemSO;
        }


        public void SpawnKitchenObject(KitchenItemSO kitchenItemSO, IKitchenObjectHolder kitchenObjHolder)
        {
            int networkKitchenItemIndex = GetNetworkKitchenItemIndex(kitchenItemSO);
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
                Debug.Log("Invalid KitchenObjHolderNetworkObjectReference passed to SpawnKitchenObject-ServerRpc!");
                return;
            }

            var kitchenObj = kitchenItemNetworkObj.GetComponent<KitchenObject>();
            IKitchenObjectHolder kitchenObjHolder = kitchenObjHolderNetworkObj.GetComponent<IKitchenObjectHolder>();

            kitchenObj.SetKitchenObjectHolder(kitchenObjHolder);
        }
    }
}