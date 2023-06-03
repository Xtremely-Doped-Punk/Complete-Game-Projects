using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KC
{
    [CreateAssetMenu(menuName = "KC/FoodMenu")]
    public class  FoodMenuSO : ScriptableObject
    {
        [field: SerializeField] public DeliveryRecipeSO[] DeliveryRecipeSOArray { get; private set; } = new DeliveryRecipeSO[0];
    }
}