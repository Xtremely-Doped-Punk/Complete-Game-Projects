using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KC
{
    public class CounterStove : BaseCounter, IHasProgressBar
    {
        public enum State { Idle, Frying, Fried, Burnt } // burnt or overcooked

        public event EventHandler<IHasProgressBar.ProgessChangedEventArg> OnProgessChanged;

        public event EventHandler OnStoveStateChanged; // primary interaction event

        [SerializeField] private FryingRecipeSO[] cuttingRecipeSOArray;

        private float fryingTimer;
        private State stoveState = State.Idle;
        private FryingRecipeSO currentFryingRecipeSO;

        public override void InteractPrimary(PlayerController player)
        {
            if (!HasKitchenObject())
            {
                // there is no kitch object in this counter
                if (player.HasKitchenObject())
                {
                    KitchenObject playerKitchenObj = player.GetKitchenObject();
                    if (TryFindingFryingRecipe(playerKitchenObj.KitchenItemSO, out currentFryingRecipeSO))
                    {
                        // player is carrying some kitchen object that can be fried
                        playerKitchenObj.SetKitchenObjectHolder(this); // place object on counter
                        InitiateFrying();
                    }
                    else
                        Debug.LogWarning("Player kitchen-object:" + playerKitchenObj + " doesnt have frying recipe defined.");
                }
                else
                {
                    // both player and counter has no objects to do interaction with
                }
            }
            else
            {
                // there is some kitch object already in this counter
                if (!player.HasKitchenObject())
                {
                    // player is not carrying anything
                    this.GetKitchenObject().SetKitchenObjectHolder(player); // take object on counter

                    ChangeStoveState(State.Idle);
                    if (currentFryingRecipeSO != null) currentFryingRecipeSO = null;
                }
                else
                {
                    // both player and counter has objects held by them, multiple objects cant be interacted at a same time
                    if (CheckPossiblePlateInteractions(player, canCounterHoldPlate: false))
                        ChangeStoveState(State.Idle);
                }
            }
        }

        private void Update()
        {
            if (!HasKitchenObject())
                return; // there is no kitch object in this counter

            // there is some kitch object already in this counter
            if (stoveState == State.Idle || stoveState == State.Burnt)
                return; // fried completely until reciepe chain is over

            float currentFryingTimerMax = currentFryingRecipeSO.FryingTimerMax;
            ProgressUpdate(currentFryingTimerMax);

            if (fryingTimer >= currentFryingTimerMax)
            {
                ChangeFryingState();
            }
        }

        private bool TryFindingFryingRecipe(KitchenItemSO inpKitchenItemSO, out FryingRecipeSO matchFryingRecipeSO)
        {
            matchFryingRecipeSO = null;
            foreach (FryingRecipeSO fryingRecipeSO in cuttingRecipeSOArray)
            {
                if (fryingRecipeSO.InputKitchenItemSO == inpKitchenItemSO)
                {
                    matchFryingRecipeSO = fryingRecipeSO;
                    return true;
                }
            }
            return false;
        }

        private void InitiateFrying()
        {
            ChangeStoveState(currentFryingRecipeSO.InFryingState);
        }

        private void ChangeFryingState()
        {
            KitchenItemSO endKitchenItemSO = currentFryingRecipeSO.OutputKitchenItemSO;
            SwitchNewKitchenObject(endKitchenItemSO);
            ChangeStoveState(currentFryingRecipeSO.OutFryingState);

            if (TryFindingFryingRecipe(endKitchenItemSO, out currentFryingRecipeSO))
                InitiateFrying();
        }

        private void ChangeStoveState(State state)
        {
            stoveState = state;
            OnStoveStateChanged?.Invoke(this, EventArgs.Empty);
            ProgressUpdate();
        }

        private void ProgressUpdate(float ProgressMax = 0)
        {
            if (ProgressMax > 0)
            {
                // update
                fryingTimer += Time.deltaTime;

                OnProgessChanged?.Invoke(this, new IHasProgressBar.ProgessChangedEventArg
                { progressNormalized = fryingTimer / ProgressMax });
            }
            else
            {
                // reset
                fryingTimer = 0f;
                OnProgessChanged?.Invoke(this, new IHasProgressBar.ProgessChangedEventArg
                { progressNormalized = 0f });
            }
        }

        public bool IsGoingToBurn => (currentFryingRecipeSO != null) && currentFryingRecipeSO.OutFryingState == State.Burnt;
        public bool IsFrying => !(stoveState == State.Idle || stoveState == State.Burnt);

        public override bool CanHoldKitchenObject(KitchenItemSO kitchenItemSO)
        {
            return (kitchenItemSO.ObjectType == KitchenObject.MainTypes.Edible) && (kitchenItemSO.EdibleType == KitchenObject.EdibleTypes.raw);
        }
    }
}