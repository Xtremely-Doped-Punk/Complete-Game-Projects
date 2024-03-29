using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace KC
{
    public class MultiplayerManager : NetworkBehaviour
    {
        public static MultiplayerManager Singleton { get; private set; } = null;
        [SerializeField] private NetworkKitchenItemsListSO networkKitchenItemsListSO;

        private void Awake()
        {
            if (Singleton == null)
                Singleton = this;
            else if (Singleton != this)
                Destroy(this);
        }

        public void StartHost()
        {
            NetworkManager.ConnectionApprovalCallback += MultiplayerManager_ConnectionApprovalCallback;
            NetworkManager.StartHost();
        }

        private void MultiplayerManager_ConnectionApprovalCallback(
            NetworkManager.ConnectionApprovalRequest connAprReqt, 
            NetworkManager.ConnectionApprovalResponse connAprResp)
        {
            // allow players to connect only at the begining of the game, i.e. late joins not allowed
            if (GameManager.Instance.IsWaitingToStart)
                connAprResp.Approved = true; // approves the connection
            else
            {
                connAprResp.Approved = false; // disapproves the connection
                connAprResp.Reason = "Game Session under process!";
            }
            this.Log($"client:{connAprReqt.ClientNetworkId} tring to connect, response:{connAprResp.Approved}");
        }

        public void StartClient()
        {
            NetworkManager.StartClient();
        }


        public static int GetNetworkKitchenItemIndex(KitchenItemSO kitchenItemSO)
        {
            int index = Singleton.networkKitchenItemsListSO.IndexOf(kitchenItemSO);
            if (index == -1)
                Singleton.LogWarning("KitchenItem-NetworkIndex not found, 'NetworkKitchenItemsListSO' has been changed!!!");
            return index;
        }
        public static KitchenItemSO GetNetworkKitchenItem(int index)
        {
            KitchenItemSO kitchenItemSO = Singleton.networkKitchenItemsListSO.AtIndex(index);
            if (kitchenItemSO == null)
                Singleton.LogWarning("KitchenItem not found, as given Network-Index is invalid!!!");
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
            // checking if the given NetworkObjectReference is valid from server end
            if (!kitchenObjHolderNetworkObjRef.TryGet(out NetworkObject kitchenObjHolderNetworkObj))
            {
                this.LogWarning($"Invalid {nameof(kitchenObjHolderNetworkObjRef)} passed to {nameof(SpawnKitchenObjectServerRpc)}!");
                return;
            }

            KitchenItemSO kitchenItemSO = GetNetworkKitchenItem(networkKitchenItemIndex);
            Transform kitchenItemTransform = Instantiate(kitchenItemSO.Prefab).transform;

            NetworkObject kitchenItemNetworkObj = kitchenItemTransform.GetComponent<NetworkObject>();

            // this NetworkObject.Spawn() can only be called by server, thus the need of ServerRpc
            kitchenItemNetworkObj.Spawn(destroyWithScene: true); // by default, Spawn(destroyWithScene:false)

            var kitchenObj = kitchenItemNetworkObj.GetComponent<KitchenObject>();
            IKitchenObjectHolder kitchenObjHolder = kitchenObjHolderNetworkObj.GetComponent<IKitchenObjectHolder>();
            //this.Log($"Spawning[ServerRpc] KitchenObject: {kitchenItemSO.UniqueName} on Holder: {kitchenObjHolder}");

            kitchenObj.SetKitchenObjectHolder(kitchenObjHolder);
        }


        public void DestroyKitchenObject(KitchenObject kitchenObject) => DestrorKitchenObjectServerRpc(kitchenObject);

        [ServerRpc(RequireOwnership = false)]
        private void DestrorKitchenObjectServerRpc(NetworkBehaviourReference kitchenObjBehaviourRef)
        {
            if (!kitchenObjBehaviourRef.TryGet(out KitchenObject kitchenObject))
            {
                this.LogWarning($"Invalid {nameof(kitchenObjBehaviourRef)} passed to {nameof(DestrorKitchenObjectServerRpc)}!");
                return;
            }

            if (kitchenObject.GetKitchenObjectHolder() != null)
                DestrorKitchenObjectCallbackClientRpc(kitchenObject.GetKitchenObjectHolder().GetNetworkObject());

            kitchenObject.DestrorSelf();
        }

        [ClientRpc]
        private void DestrorKitchenObjectCallbackClientRpc(NetworkObjectReference kitchenObjHolderNetworkObjRef)
        {
            if (!kitchenObjHolderNetworkObjRef.TryGet(out NetworkObject kitchenObjHolderNetworkObj))
            {
                this.LogWarning($"Invalid {nameof(kitchenObjHolderNetworkObjRef)} passed to {nameof(DestrorKitchenObjectCallbackClientRpc)}!");
                return;
            }
            IKitchenObjectHolder kitchenObjectHolder = kitchenObjHolderNetworkObj.GetComponent<IKitchenObjectHolder>();

            kitchenObjectHolder.ClearKitchenObject(); // remove reference from parent holder
            // trash counter deletes any object tats been set to it, in that case,
            // 'kitchenObjectHolder' might still be uninitialized
        }
    }
}