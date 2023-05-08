using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KC
{
    [CreateAssetMenu(menuName ="KC/CuttingRecipe")]
    public class CuttingRecipeSO : ScriptableObject
    {
        [field: SerializeField] public KitchenItemSO InputKitchenItemSO { get; private set; } = null;
        [field: SerializeField] public KitchenItemSO OutputKitchenItemSO { get; private set; } = null;
        [field: SerializeField] public int CuttingProgressMax { get; private set; } = 0;
    }
}