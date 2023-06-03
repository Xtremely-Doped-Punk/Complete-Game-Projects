using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace KC
{
    public class CounterContainer : BaseCounter
    {
        public event EventHandler OnPlayerInteraction;

        [SerializeField] private KitchenItemSO spawnKitchenItemSO;
        public KitchenItemSO KitchenItemSO => spawnKitchenItemSO;

        public override void InteractPrimary(PlayerController player)
        {
            if (player.HasKitchenObject())
            {
                Debug.LogWarning("Player:"+player+" already holds a object!");
            }
            else
            {
                // spawn onto player's holder as counter shouldn't have in it holder
                KitchenObject.SpawnKitchenObject(spawnKitchenItemSO, player);

                // in order to sync animations that listen to events in tha main logic in all clients
                InteractPrimaryServerRpc();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void InteractPrimaryServerRpc() => InteractPrimaryCallbackClientRpc();

        [ClientRpc]
        private void InteractPrimaryCallbackClientRpc()
        {
            OnPlayerInteraction?.Invoke(this, EventArgs.Empty);
        }
    }
}