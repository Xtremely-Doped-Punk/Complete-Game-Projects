using UnityEngine;
using UnityEngine.EventSystems;

namespace KC
{
    public class Visual_PlateComplete : MonoBehaviour
    {
        [SerializeField] private PlateKitchenObject plateKitchenObject;
        [SerializeField] private IngredientSingleIconUI ingredientIconTemplate;
        [SerializeField] private KitchenItemSO_GameObject_Map[] kitchenItemsToVisualObjectsMap;

        private IngredientSingleIconUI[] ingredientIconInstancesUIMap;
        private int selectedDropIndex;

        private void Start()
        {
            plateKitchenObject.OnIngredientChanged += HandlePlateVisualOnIngredientChanged;
            plateKitchenObject.OnIngredientDropViewSwitched += HandlePlateVisualOnIngredientDropViewSwitched;

            foreach (KitchenItemSO_GameObject_Map item_visual_map in kitchenItemsToVisualObjectsMap)
            {
                item_visual_map.visualGameObject.SetActive(false);
            }
            ingredientIconTemplate.gameObject.SetActive(false);

            ingredientIconInstancesUIMap = new IngredientSingleIconUI[kitchenItemsToVisualObjectsMap.Length];
            // instance are kept seperate for easiler accessing
        }

        private int FindFirstNotNullIngredientIconIndex()
        {
            for (int i = 0; i < ingredientIconInstancesUIMap.Length; i++)
            {
                if (ingredientIconInstancesUIMap[i] != null)
                    return i;
            }
            return -1;
        }

        private void HandlePlateVisualOnIngredientDropViewSwitched(object sender, System.EventArgs e)
        {
            IngredientSingleIconUI ingredientIconInstanceUIMap = (selectedDropIndex == -1) ?
                null : ingredientIconInstancesUIMap[selectedDropIndex];

            if (plateKitchenObject.CanDrop())
            {
                EnableAllIngredientIconButtons();
                if (ingredientIconInstanceUIMap == null)
                {
                    selectedDropIndex = FindFirstNotNullIngredientIconIndex();
                    ingredientIconInstanceUIMap = ingredientIconInstancesUIMap[selectedDropIndex];
                }
                ingredientIconInstanceUIMap.SelectButton();
            }
            else
            {
                DisableIngredientIconButtons();
                //DeselectAnyButton();
            }
        }

        private void HandlePlateVisualOnIngredientChanged(object sender, PlateKitchenObject.IngredientsChangedEventArgs e)
        {
            //this.Log($"{nameof(HandlePlateVisualOnIngredientChanged)} args => {e.ingredient.KitchenItemSO}, {e.ingredient.ingredientCount}, {(e.isAdded ? "Added" : "Removed")}");
            int mapIndex = kitchenItemsToVisualObjectsMap.FindItemVisualMapIndex(e.ingredient.KitchenItemSO);

            if (mapIndex == -1)
                return;

            var ingredientIconInstanceUI = ingredientIconInstancesUIMap[mapIndex];
            var visualGameObject = kitchenItemsToVisualObjectsMap[mapIndex].visualGameObject;
            var kitchenItemSO = kitchenItemsToVisualObjectsMap[mapIndex].kitchenItemSO;

            if (e.isAdded)
            {
                // ingredient was added to the plate

                if (ingredientIconInstanceUI == null)
                {
                    // create new instance, as it is first time being added
                    visualGameObject.SetActive(true);

                    // if icon doesnt exist, create one when newly added
                    IngredientSingleIconUI iconInstance =
                        Instantiate(ingredientIconTemplate, plateKitchenObject.GetPlateContentsViewParent());

                    iconInstance.SetupIngredientIconCount(e.ingredient);
                    iconInstance.SetButtonAction(plateKitchenObject);
                    iconInstance.gameObject.SetActive(true);

                    // save to original struct
                    ingredientIconInstancesUIMap[mapIndex] = iconInstance;
                }
                else
                {
                    // update text alone, as it is already existing
                    ingredientIconInstanceUI.UpdateIngredientCountVisual(e.ingredient);
                }
            }
            else
            {
                // ingredient was removed to the plate

                if (e.ingredient.ingredientCount == 0)
                {
                    // delete instance as count is 0
                    visualGameObject.SetActive(false);
                    Destroy(ingredientIconInstanceUI.gameObject);
                }
                else
                {
                    // update text alone, as it is already existing
                    ingredientIconInstanceUI.UpdateIngredientCountVisual(e.ingredient);
                }
            }

            if (e.ingredient.ingredientCount > 1)
            {
                // update text alone, as it is already existing
                ingredientIconInstanceUI.UpdateIngredientCountVisual(e.ingredient);
            }
            else
            {
                
                // note that final changes are to be done in original struct, as unlike classes,
                // struct are passed by value, not by refernce. so making change to local variable does'nt affect the source
            }
        }
        private void DeselectAnyButton()
        {
            if (EventSystem.current.currentSelectedGameObject != null)
                EventSystem.current.SetSelectedGameObject(null);
        }

        private void EnableAllIngredientIconButtons()
        {
            foreach (IngredientSingleIconUI ingredientSingleIconUI in ingredientIconInstancesUIMap)
                if (ingredientSingleIconUI != null)
                    ingredientSingleIconUI.EnableButton();

        }

        private void DisableIngredientIconButtons()
        {
            foreach (IngredientSingleIconUI ingredientSingleIconUI in ingredientIconInstancesUIMap)
                if (ingredientSingleIconUI != null)
                    ingredientSingleIconUI.DisableButton();

        }
    }

    [System.Serializable]
    public class KitchenItemSO_GameObject_Map
    {
        public KitchenItemSO kitchenItemSO;
        public GameObject visualGameObject;
    }
    public static class KitchenItemSO_GameObject_Map_Extentions
    {
        public static int FindItemVisualMapIndex(this KitchenItemSO_GameObject_Map[] itemVisualMaps, KitchenItemSO findKitchenItemSO)
        {
            for (int i = 0; i < itemVisualMaps.Length; i++)
            {
                if (itemVisualMaps[i].kitchenItemSO == findKitchenItemSO)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}