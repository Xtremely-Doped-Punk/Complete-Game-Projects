using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace KC
{
    public class CounterCutting : BaseCounter, IHasProgressBar
    {
        public static event EventHandler OnAnyCut;

        new public static void ResetStaticData()
        {
            //Debug.Log("CounterCutting static subcribers:"+ OnAnyCut?.GetInvocationList().Length);
            OnAnyCut = null;
        }

        public event EventHandler<IHasProgressBar.ProgessChangedEventArg> OnProgessChanged; // secondary interaction event

        public event EventHandler OnPlayerSwitchInteraction; // primary interaction event

        [SerializeField] private CuttingRecipeSO[] cuttingRecipeSOArray;
        private NetworkVariable<int> cuttingProgress = new(0);

        public override void InteractPrimary(PlayerController player)
        {
            if (!HasKitchenObject())
            {
                // there is no kitch object in this counter
                if (player.HasKitchenObject())
                {
                    KitchenObject playerKitchenObj = player.GetKitchenObject();
                    if (TryFindingCuttingRecipe(playerKitchenObj.KitchenItemSO, out _))
                    {
                        // player is carrying some kitchen object that can be cut
                        playerKitchenObj.SetKitchenObjectHolder(this); // place object on counter
                        ProgressUpdate();
                        InteractSwitchPrimaryServerRpc();
                    }
                    else
                        this.LogWarning($"Player kitchen-object: {playerKitchenObj} doesnt have cutting recipe defined.");
                }
                else
                {
                    // both player and counter has not object to do interaction with
                }
            }
            else
            {
                // there is some kitch object already in this counter
                if (!player.HasKitchenObject())
                {
                    // player is not carrying anything
                    this.GetKitchenObject().SetKitchenObjectHolder(player); // take object on counter
                    InteractSwitchPrimaryServerRpc();
                }
                else
                {
                    // both player and counter has objects held by them, multiple objects cant be interacted at a same time
                    CheckPossiblePlateInteractions(player, canCounterHoldPlate: false);
                }
            }
        }

        // override server callback for plate interactions call back
        protected override void CheckPossiblePlateInteractionsServerCallback(bool havePlaveInteractionTaken)
        {
            if (havePlaveInteractionTaken)
                InteractSwitchPrimaryServerRpc();
        }

        [ServerRpc(RequireOwnership = false)] public void InteractSwitchPrimaryServerRpc() => InteractSwitchPrimaryClientRpc();
        [ClientRpc] public  void InteractSwitchPrimaryClientRpc() => OnPlayerSwitchInteraction?.Invoke(this, EventArgs.Empty);

        public override void InteractSecondary(PlayerController player)
        {
            if (HasKitchenObject()) // local check
                InteractSecondaryServerRpc();
            //else
                // there is no kitch object in this counter
        }

        [ServerRpc(RequireOwnership = false)]
        private void InteractSecondaryServerRpc()
        {
            if (!HasKitchenObject()) // server check
            {
                this.LogWarning("Invalid InteractSecondaryServerRpc Callback made!");
                return;
            }
            // there is some kitch object already in this counter
            KitchenObject thisKitchenObj = GetKitchenObject();

            if (!TryFindingCuttingRecipe(thisKitchenObj.KitchenItemSO, out int cuttingRecipeSOIndex))
            {
                this.Log("This kitchen-object:" + thisKitchenObj + " has already been cut.");
                return;
            }

            var cuttingRecipeSO = cuttingRecipeSOArray[cuttingRecipeSOIndex];
            ProgressUpdate(cuttingRecipeSO.CuttingProgressMax); // server itself runs the other server-rpc

            if (cuttingProgress.Value >= cuttingRecipeSO.CuttingProgressMax)
            {
                // server itself runs the server-rpc calls only once rather than mutiple times from each client-rpc
                SwitchNewKitchenObject(cuttingRecipeSO.OutputKitchenItemSO);
            }
        }

        private void ProgressUpdate(float ProgressMax = 0)
        {
            ProgressUpdateServerRpc(ProgressMax);
        }

        [ServerRpc(RequireOwnership = false)]
        private void ProgressUpdateServerRpc(float ProgressMax = 0)
        {
            float progressNormalized = 0f;
            if (ProgressMax > 0) // update
            {
                cuttingProgress.Value++;
                progressNormalized = (float)cuttingProgress.Value / ProgressMax;
            }
            else
                cuttingProgress.Value = 0;

            ProgressUpdateClientRpc(progressNormalized);
        }
        [ClientRpc]
        private void ProgressUpdateClientRpc(float ProgressNormalized)
        {
            if (ProgressNormalized > 0)
                OnAnyCut?.Invoke(this, EventArgs.Empty);

            OnProgessChanged?.Invoke(this, new IHasProgressBar.ProgessChangedEventArg
            { progressNormalized = ProgressNormalized });
        }

        private bool TryFindingCuttingRecipe(KitchenItemSO inpKitchenItemSO, out int matchCuttingRecipeSOIndex)
        {
            matchCuttingRecipeSOIndex = -1;
            for (int i = 0; i < cuttingRecipeSOArray.Length; i++)
            {
                if (cuttingRecipeSOArray[i].InputKitchenItemSO == inpKitchenItemSO)
                {
                    matchCuttingRecipeSOIndex = i;
                    return true;
                }
            }
            return false;
        }

        public override bool CanHoldKitchenObject(KitchenItemSO kitchenItemSO)
        {
            return (kitchenItemSO.ObjectType == KitchenObject.MainTypes.Edible) && (kitchenItemSO.EdibleType == KitchenObject.EdibleTypes.raw);
        }
    }
}