using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace KC
{
    public class DeliverySingleOrderUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI recipeNameText;
        [SerializeField] private Transform kitchenObjectIconsParent;
        [SerializeField] private IngredientSingleIconUI ingredientIconTemplate;

        public void SetupOrderRecipeUI(DeliveryRecipeSO deliveryRecipeSO)
        {
            recipeNameText.text = "Order: " + deliveryRecipeSO.RecipeName;
            foreach (Ingredient ingredient in deliveryRecipeSO.IngredientsArray)
            {
                IngredientSingleIconUI ingredientIconUI =
                    Instantiate(ingredientIconTemplate, kitchenObjectIconsParent);
                ingredientIconUI.SetupIngredientIconCount(ingredient);
            }
            Destroy(ingredientIconTemplate.gameObject);
        }
    }
}