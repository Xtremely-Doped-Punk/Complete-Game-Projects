using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace KC
{
    public class CounterStove : BaseCounter, IHasProgressBar
    {
        public enum State { Idle, Frying, Fried, Burnt } // burnt or overcooked

        public event EventHandler<IHasProgressBar.ProgessChangedEventArg> OnProgessChanged;

        public event EventHandler OnStoveStateChanged; // primary interaction event

        [SerializeField] private FryingRecipeSO[] cuttingRecipeSOArray;

        private NetworkVariable<float> fryingTimer = new(0f);
        private NetworkVariable<State> stoveState = new(State.Idle);
        private NetworkVariable<int> currentFryingRecipeSOIndex = new(-1);
        [ServerRpc(RequireOwnership = false)] private void SetFryingRecipeSOIndexServerRpc(int val) => currentFryingRecipeSOIndex.Value = val;
        private FryingRecipeSO currentFryingRecipeSO
        {
            get
            {
                if (currentFryingRecipeSOIndex.Value <= -1 || currentFryingRecipeSOIndex.Value >= cuttingRecipeSOArray.Length)
                    return null;
                return cuttingRecipeSOArray[currentFryingRecipeSOIndex.Value];
            }
        }

        public override void InteractPrimary(PlayerController player)
        {
            if (!HasKitchenObject())
            {
                // there is no kitch object in this counter
                if (player.HasKitchenObject())
                {
                    KitchenObject playerKitchenObj = player.GetKitchenObject();
                    if (TryFindingFryingRecipe(playerKitchenObj.KitchenItemSO, out int FryingRecipeSOIndex))
                    {
                        SetFryingRecipeSOIndexServerRpc(FryingRecipeSOIndex);

                        // player is carrying some kitchen object that can be fried
                        playerKitchenObj.SetKitchenObjectHolder(this); // place object on counter
                        InitiateFryingServerRpc();
                    }
                    else
                        this.LogWarning("Player kitchen-object:" + playerKitchenObj + " doesnt have frying recipe defined.");
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
                    if (currentFryingRecipeSOIndex.Value != -1) SetFryingRecipeSOIndexServerRpc(-1);
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
            if (!IsServer) return; // Server Only Update CallBack

            if (!HasKitchenObject())
                return; // there is no kitch object in this counter

            // there is some kitch object already in this counter
            if (stoveState.Value == State.Idle || stoveState.Value == State.Burnt)
                return; // fried completely until reciepe chain is over

            float currentFryingTimerMax = currentFryingRecipeSO.FryingTimerMax;
            ProgressUpdate(currentFryingTimerMax);

            if (fryingTimer.Value >= currentFryingTimerMax)
            {
                ChangeFryingState();
            }
        }

        private bool TryFindingFryingRecipe(KitchenItemSO inpKitchenItemSO, out int matchFryingRecipeSOIndex)
        {
            matchFryingRecipeSOIndex = -1;
            for (int i=0; i<cuttingRecipeSOArray.Length; i++)
            {
                if (cuttingRecipeSOArray[i].InputKitchenItemSO == inpKitchenItemSO)
                {
                    matchFryingRecipeSOIndex = i;
                    return true;
                }
            }
            return false;
        }

        [ServerRpc(RequireOwnership = false)]
        private void InitiateFryingServerRpc()
        {
            ChangeStoveState(currentFryingRecipeSO.InFryingState);
        }

        private void ChangeFryingState()
        {
            KitchenItemSO endKitchenItemSO = currentFryingRecipeSO.OutputKitchenItemSO;
            SwitchNewKitchenObject(endKitchenItemSO);
            ChangeStoveState(currentFryingRecipeSO.OutFryingState);
            if (TryFindingFryingRecipe(endKitchenItemSO, out int FryingRecipeSOIndex))
            {
                SetFryingRecipeSOIndexServerRpc(FryingRecipeSOIndex);
                InitiateFryingServerRpc();
            }
        }

        #region ChangeStoveState()
        private void Start()
        {
            stoveState.OnValueChanged += ClientOnChangeStoveStateChanged;
        }
        private void ChangeStoveState(State state)
        {
            ChangeStoveStateServerRpc(state);
        }
        [ServerRpc(RequireOwnership = false)]
        private void ChangeStoveStateServerRpc(State state)
        {
            stoveState.Value = state;
            ProgressUpdate();
        }
        private void ClientOnChangeStoveStateChanged(State oldState, State newState)
        {
            OnStoveStateChanged?.Invoke(this, EventArgs.Empty);
        }
        #endregion
        #region ProgressUpdate()
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
                fryingTimer.Value += Time.deltaTime;
                progressNormalized = (float)fryingTimer.Value / ProgressMax;
            }
            else
                fryingTimer.Value = 0;

            ProgressUpdateClientRpc(progressNormalized);
        }
        [ClientRpc]
        private void ProgressUpdateClientRpc(float ProgressNormalized)
        {
            OnProgessChanged?.Invoke(this, new IHasProgressBar.ProgessChangedEventArg
            { progressNormalized = ProgressNormalized });
        }
        #endregion

        public bool IsGoingToBurn => (currentFryingRecipeSO != null) && currentFryingRecipeSO.OutFryingState == State.Burnt;
        public bool IsFrying => !(stoveState.Value == State.Idle || stoveState.Value == State.Burnt);

        public override bool CanHoldKitchenObject(KitchenItemSO kitchenItemSO)
        {
            return (kitchenItemSO.ObjectType == KitchenObject.MainTypes.Edible) && (kitchenItemSO.EdibleType == KitchenObject.EdibleTypes.raw);
        }
    }
}