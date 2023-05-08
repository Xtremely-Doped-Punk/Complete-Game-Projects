using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KC
{
    public class CounterPlates : BaseCounter
    {
        public event EventHandler OnPlateSpawned;
        public event EventHandler OnPlatePicked;

        [SerializeField] private int maxPlateCapacity = 5;
        [SerializeField] private float plateSpawnTimerMax = 4f;
        [SerializeField] private KitchenItemSO plateKitchenItemSO;

        private int activeNoOfPlates = 0;
        private float plateSpawnTimer;

        private void Update()
        {
            if (!GameManager.Instance.IsGamePlaying || activeNoOfPlates == maxPlateCapacity) return;

            plateSpawnTimer += Time.deltaTime;
            if (plateSpawnTimer > plateSpawnTimerMax)
            {
                activeNoOfPlates++; plateSpawnTimer = 0;

                OnPlateSpawned?.Invoke(this, EventArgs.Empty); 
                // visually update the no of plates counter
            }
        }

        public override void InteractPrimary(PlayerController player)
        {
            if (activeNoOfPlates == 0) return;

            if (player.HasKitchenObject())
            {
                Debug.LogWarning("Player:" + player + " already holds a object!");
            }
            else
            {
                // spawn actual game prefab onto player on interaction
                KitchenObject.SpawnKitchenObject(plateKitchenItemSO, player);
                activeNoOfPlates--;
                OnPlatePicked?.Invoke(this, EventArgs.Empty); // visual update
            }
        }
    }
}