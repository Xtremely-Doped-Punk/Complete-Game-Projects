using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Netcode;
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

        [SerializeField, Tooltip("Debug")] 
        private NetworkVariable<int> activeNoOfPlates = new(value: 0);
        // write permission is available only for server/owner, thus cant directly update from all non-owner clients
        // only work-arround is to update when the ServerRpc (RequireOwnership=False) call is made 

        private float plateSpawnTimer;

        public override void OnNetworkSpawn()
        {
            activeNoOfPlates.OnValueChanged += OnNoOfPlatesChanged;
            // as this is just a delegate, no need to unsubscribe like it an event, iguess

            if (activeNoOfPlates.Value != 0)
                OnNoOfPlatesChanged(0, activeNoOfPlates.Value);
        }
        private void OnNoOfPlatesChanged(int previousCount, int currentCount)
        {
            if (previousCount == currentCount) return;
            // visually update the no of plates counter
            int changeCount = math.abs(currentCount - previousCount);

            if (currentCount > previousCount)
                for (int i = 0; i < changeCount; i++)
                    OnPlateSpawned?.Invoke(this, EventArgs.Empty);
            else
                for (int i = 0; i < changeCount; i++)
                    OnPlatePicked?.Invoke(this, EventArgs.Empty);
        }

        private void Update()
        {
            if (!IsServer) return; // need to sync by server end only

            if (!GameManager.Instance.IsGamePlaying || activeNoOfPlates.Value == maxPlateCapacity) return;

            plateSpawnTimer += Time.deltaTime;
            if (plateSpawnTimer > plateSpawnTimerMax)
                SpawnPlateServerRpc();
        }
        
        // here we simply want [Server](Only) attribute, as NetCode doesn't have this seperately we use ServerRpc alternatively
        [ServerRpc] 
        private void SpawnPlateServerRpc()
        {
            plateSpawnTimer = 0;
            activeNoOfPlates.Value++;
            //SpawnPlateClientRpc();
        }
        [ClientRpc]
        private void SpawnPlateClientRpc()
        {
            OnPlateSpawned?.Invoke(this, EventArgs.Empty);
            // visually update the no of plates counter
        }


        public override void InteractPrimary(PlayerController player)
        {
            if (activeNoOfPlates.Value == 0) return;

            if (player.HasKitchenObject())
            {
                Debug.LogWarning("Player:" + player + " already holds a object!");
            }
            else
            {
                // spawn actual game prefab onto player on interaction
                KitchenObject.SpawnKitchenObject(plateKitchenItemSO, player);
                InteractPrimaryServerRpc();
            }
        }


        [ServerRpc(RequireOwnership = false)]
        private void InteractPrimaryServerRpc()
        {
            activeNoOfPlates.Value--;
            //InteractPrimaryCallbackClientRpc();
        }

        [ClientRpc]
        private void InteractPrimaryCallbackClientRpc()
        {
            OnPlatePicked?.Invoke(this, EventArgs.Empty); // visual update
        }
    }
}