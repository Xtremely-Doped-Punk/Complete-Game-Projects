using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace KC
{
    public class PlateKitchenObject : KitchenObject
        // here we want plate behavoiur same as kitchen obj with timy bit of extra logic
    {
        public class IngredientsChangedEventArgs : EventArgs 
        { public Ingredient ingredient; public bool isAdded; }
        public event EventHandler<IngredientsChangedEventArgs> OnIngredientChanged;
        public event EventHandler OnIngredientDropViewSwitched;

        //[SerializeField] private KitchenItemSO[] validIngredientKitchenItemSOArray; 
        // to remove raw ingredients filtered out ( added as a enum directly to KitchenItemSO )
        [SerializeField] private Transform plateContentsViewParentUITransform;

        // debug serialized
        [SerializeField] private List<Ingredient> plateIngredientsHeldList = new();

        private bool canDrop;
        private Action<bool> actionAfterDropTemp;
        private bool autoLeaveInventoryAfterDropTemp;

        protected override void Awake()
        {
            base.Awake();
        }

        public void AddIngredient(KitchenObject kitchenObject)
        {
            if (TryAddIngredient(kitchenObject.KitchenItemSO))
                kitchenObject.DestrorSelf();
        }

        private bool TryAddIngredient(KitchenItemSO kitchenItemSO)
        {
            if (kitchenItemSO.ObjectType != MainTypes.Edible || kitchenItemSO.EdibleType == EdibleTypes.raw)
                return false;

            Ingredient ingredientFound = Ingredient.FindIngredient(plateIngredientsHeldList, kitchenItemSO);
            if (ingredientFound != null)
            {
                ingredientFound.ingredientCount++;
            }
            else
            {
                if (plateIngredientsHeldList.Count == 0)
                    plateContentsViewParentUITransform.gameObject.SetActive(true);
                // for the first ingredient ever added make contents display by default

                ingredientFound = new Ingredient { kitchenItemSO = kitchenItemSO, ingredientCount = 1 };
                plateIngredientsHeldList.Add(ingredientFound);
            }

            OnIngredientChanged?.Invoke(this,
                new IngredientsChangedEventArgs
                {
                    ingredient = ingredientFound, isAdded = true,
                });

            return true;
        }
        public void RemoveIngredient(KitchenItemSO kitchenItemSO, IKitchenObjectHolder switchKitchenObjectHolder)
        {
            if (kitchenItemSO == null) return;
            if (switchKitchenObjectHolder == null || switchKitchenObjectHolder.HasKitchenObject()) return;
            if (!switchKitchenObjectHolder.CanHoldKitchenObject(kitchenItemSO)) return;

            Ingredient ingredientFound = Ingredient.FindIngredient(plateIngredientsHeldList, kitchenItemSO);
            if (ingredientFound == null)
            {
                Debug.LogError("Given kitchen object:" + kitchenItemSO + 
                    " is not found as a ingredient at the plate:" + this);
                return;
            }

            // decrease count of ingredients if present
            ingredientFound.ingredientCount--;
            if (autoLeaveInventoryAfterDropTemp)
                SetCanDrop(false);

            if (ingredientFound.ingredientCount == 0)
            {
                plateIngredientsHeldList.Remove(ingredientFound);
                if (canDrop)
                    SetCanDrop(false); 
            }

            OnIngredientChanged?.Invoke(this,
                new IngredientsChangedEventArgs
                {
                    ingredient = ingredientFound, isAdded = false,
                });

            KitchenObject.SpawnKitchenObject(kitchenItemSO, switchKitchenObjectHolder);
        }

        public void TogglePlateContentsDropView(Action<bool> actionAfterDrop = null, bool autoLeaveInventoryAfterDrop = false)
        {
            if (plateIngredientsHeldList.Count == 0) return;
            actionAfterDropTemp = actionAfterDrop;
            autoLeaveInventoryAfterDropTemp = autoLeaveInventoryAfterDrop;
            SetCanDrop(!canDrop);
            
            //plateContentsViewParentUI.SetActive(!plateContentsViewParentUI.activeSelf);
        }

        private void SetCanDrop(bool val)
        {
            if (canDrop == val) return;

            canDrop = val;
            OnIngredientDropViewSwitched?.Invoke(this, EventArgs.Empty);
            actionAfterDropTemp?.Invoke(canDrop);

            if (!val) // if set to false, unsubscribe action after drop
            {
                actionAfterDropTemp = null;
                autoLeaveInventoryAfterDropTemp = false; // reset
            }
        }

        public Transform GetPlateContentsViewParent() { return plateContentsViewParentUITransform; }

        public bool CanDrop() => canDrop;

        public IReadOnlyList<Ingredient> GetPlateIngredientsHeldList() => plateIngredientsHeldList;

        public Ingredient.NetworkData[] GetPlateIngredientsNetworkDatasList()
        {
            Ingredient.NetworkData[] plateIngredientsNetworkDatas = new Ingredient.NetworkData[plateIngredientsHeldList.Count];

            for (int i = 0; i < plateIngredientsHeldList.Count; i++)
                plateIngredientsNetworkDatas[i] = plateIngredientsHeldList[i].GetNetworkData();

            return plateIngredientsNetworkDatas;
        }
    }
}