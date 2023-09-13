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
        private KitchenItemSO kitchenItemSO;
        
        public void UpdateIngredientCountVisual(Ingredient ingredient) // for updating value
        {
            if (kitchenItemSO != ingredient.KitchenItemSO)
                kitchenItemSO = ingredient.KitchenItemSO; // just for safe checks

            iconText.text = ingredient.ingredientCount.ToString();
        }

        public void SetupIngredientIconCount(Ingredient ingredient)
        {
            this.kitchenItemSO = ingredient.KitchenItemSO;
            iconImage.sprite = ingredient.KitchenItemSO.Icon;
            iconText.text = ingredient.ingredientCount.ToString();
        }

        public void SetButtonAction(PlateKitchenObject plateKitchenObject, Action action = null)
        {
            selectionBtn.onClick.AddListener(() =>
            {
                //Debug.Log("--> ingredient single icon UI clicked! " + iconImage.sprite.name);
                plateKitchenObject.RemoveIngredient(kitchenItemSO, PlayerController.LocalInstance.GetSelectedCounter());
                action?.Invoke();
            });
        }

        public void SelectButton() => selectionBtn.Select();
        public void EnableButton() => selectionBtn.interactable = true;
        public void DisableButton() => selectionBtn.interactable = false;
    }
}