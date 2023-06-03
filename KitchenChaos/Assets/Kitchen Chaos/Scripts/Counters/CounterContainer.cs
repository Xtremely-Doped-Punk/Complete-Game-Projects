using System;
using System.Collections;
using System.Collections.Generic;
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

                OnPlayerInteraction?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}