using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace KC
{
    public class CounterTrash : BaseCounter
    {
        public static EventHandler OnAnyObjectTrashed;
        new public static void ResetStaticData()
        {
            //Debug.Log("CounterTrash static subcribers:" + OnAnyObjectTrashed.GetInvocationList().Length);
            OnAnyObjectTrashed = null;
        }
        public override void InteractPrimary(PlayerController player)
        {
            if (player.HasKitchenObject())
            {
                // player is carrying some kitchen object
                TrashKitchenObject(player.GetKitchenObject());
            }
        }

        private void TrashKitchenObject(KitchenObject kitchenObject)
        {
            KitchenObject.DestroyKitchenObject(kitchenObject);
            InteractPrimaryServerRpc();
        }


        [ServerRpc(RequireOwnership = false)]
        private void InteractPrimaryServerRpc() => InteractPrimaryCallbackClientRpc();

        [ClientRpc]
        private void InteractPrimaryCallbackClientRpc()
        {
            OnAnyObjectTrashed?.Invoke(GetHolderTransform(), EventArgs.Empty);
        }

        public override void SetKitchenObject(KitchenObject kitchenObject) => TrashKitchenObject(kitchenObject);

        public override bool CanHoldKitchenObject(KitchenItemSO kitchenItemSO) => true;
    }
}