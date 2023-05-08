using System;
using System.Collections;
using System.Collections.Generic;
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
        private int cuttingProgress;

        public override void InteractPrimary(PlayerController player)
        {
            if (!HasKitchenObject())
            {
                // there is no kitch object in this counter
                if (player.HasKitchenObject())
                {
                    KitchenObject playerKitchenObj = player.GetKitchenObject();
                    if (TryFindingCuttingRecipe(playerKitchenObj.KitchenItemSO, out CuttingRecipeSO cuttingRecipeSO))
                    {
                        // player is carrying some kitchen object that can be cut
                        playerKitchenObj.SetKitchenObjectHolder(this); // place object on counter
                        ProgressUpdate();
                        OnPlayerSwitchInteraction?.Invoke(this, EventArgs.Empty);
                    }
                    else
                        Debug.LogWarning("Player kitchen-object:" + playerKitchenObj + " doesnt have cutting recipe defined.");
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
                    OnPlayerSwitchInteraction?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    // both player and counter has objects held by them, multiple objects cant be interacted at a same time
                    if (CheckPossiblePlateInteractions(player, canCounterHoldPlate: false))
                        OnPlayerSwitchInteraction?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        public override void InteractSecondary(PlayerController player)
        {
            if (HasKitchenObject())
            {
                // there is some kitch object already in this counter
                KitchenObject thisKitchenObj = GetKitchenObject();

                if (!TryFindingCuttingRecipe(thisKitchenObj.KitchenItemSO, out CuttingRecipeSO cuttingRecipeSO))
                {
                    Debug.LogWarning("This kitchen-object:" + thisKitchenObj + " has already been cut.");
                    return;
                }

                ProgressUpdate(cuttingRecipeSO.CuttingProgressMax);

                if (cuttingProgress >= cuttingRecipeSO.CuttingProgressMax)
                {
                    SwitchNewKitchenObject(cuttingRecipeSO.OutputKitchenItemSO);
                }
            }
            else
            {
                // there is no kitch object in this counter
            }
        }

        private void ProgressUpdate(float ProgressMax = 0)
        {
            if (ProgressMax > 0)
            {
                // update
                cuttingProgress++; OnAnyCut?.Invoke(this, EventArgs.Empty);

                OnProgessChanged?.Invoke(this, new IHasProgressBar.ProgessChangedEventArg
                { progressNormalized = (float)cuttingProgress / ProgressMax });
            }
            else
            {
                // reset
                cuttingProgress = 0;
                OnProgessChanged?.Invoke(this, new IHasProgressBar.ProgessChangedEventArg
                { progressNormalized = 0f });
            }
        }

        private bool TryFindingCuttingRecipe(KitchenItemSO inpKitchenItemSO, out CuttingRecipeSO matchCuttingRecipeSO)
        {
            matchCuttingRecipeSO = null;
            foreach (CuttingRecipeSO cuttingRecipeSO in cuttingRecipeSOArray)
            {
                if (cuttingRecipeSO.InputKitchenItemSO == inpKitchenItemSO)
                {
                    matchCuttingRecipeSO = cuttingRecipeSO;
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