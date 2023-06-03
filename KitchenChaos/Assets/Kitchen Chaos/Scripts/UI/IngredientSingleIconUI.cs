using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KC
{
    public class IngredientSingleIconUI : MonoBehaviour
    {
        [SerializeField] private Button selectionBtn; // set by default non interactable
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI iconText;
        // note: TextMeshPro and TextMeshProUGUI are seperate comps (UGUI indicates it is ui-element)
        private Ingredient ingredient;
        
        public void UpdateIngredientVisuals()
        {
            iconText.text = ingredient.ingredientCount.ToString();
        }

        public void SetupIngredientIconCount(Ingredient ingredient)
        {
            this.ingredient = ingredient;
            iconImage.sprite = ingredient.kitchenItemSO.Icon;
            iconText.text = ingredient.ingredientCount.ToString();
        }

        public void SetButtonAction(PlateKitchenObject plateKitchenObject, Action action = null)
        {
            selectionBtn.onClick.AddListener(() =>
            {
                //Debug.Log("--> ingredient single icon UI clicked! " + iconImage.sprite.name);
                plateKitchenObject.RemoveIngredient(ingredient.kitchenItemSO, PlayerController.Instance.GetSelectedCounter());
                action?.Invoke();
            });
        }

        public void SelectButton() => selectionBtn.Select();
        public void EnableButton() => selectionBtn.interactable = true;
        public void DisableButton() => selectionBtn.interactable = false;
    }
}