using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static KC.FryingRecipeSO;

namespace KC
{
    [CreateAssetMenu(menuName = "KC/FryingRecipe")]
    public class FryingRecipeSO : ScriptableObject
    {
        [field: SerializeField] public KitchenItemSO InputKitchenItemSO { get; private set; } = null;
        [field: SerializeField] public CounterStove.State InFryingState { get; private set; } = CounterStove.State.Frying;
        [field: SerializeField] public KitchenItemSO OutputKitchenItemSO { get; private set; } = null;
        [field: SerializeField] public CounterStove.State OutFryingState { get; private set; } = CounterStove.State.Fried;
        [field: SerializeField] public float FryingTimerMax { get; private set; } = 0;
    }
}