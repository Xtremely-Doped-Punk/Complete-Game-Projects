using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KC
{
    [CreateAssetMenu(menuName = "KC/DeliveryRecipe")]
    public class  DeliveryRecipeSO : ScriptableObject
    {
        [field: SerializeField] public string RecipeName { get; private set; } = null;
        [field: SerializeField] public Ingredient[] IngredientsArray { get; private set; } = new Ingredient[0];
    }
}