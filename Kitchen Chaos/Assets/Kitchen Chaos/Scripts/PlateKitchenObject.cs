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

        [Header("Debug")]
        [SerializeField] private List<Ingredient> plateIngredientsHeldList = new();

        private bool canDrop;
        private Action<bool> actionAfterDropTemp;
        private bool autoLeaveInventoryAfterDropTemp;

        protected override void Awake()
        {
            base.Awake();
        }

        public void AddIngredient(KitchenObject kitchenObject) => AddIngredientServerRpc(kitchenObject);
        [ServerRpc] private void AddIngredientServerRpc(NetworkBehaviourReference kitchenObjBehaviourRef)
        {
            if (!kitchenObjBehaviourRef.TryGet(out KitchenObject kitchenObject))
            {
                this.LogWarning($"Invalid {nameof(kitchenObjBehaviourRef)} passed to {nameof(AddIngredientServerRpc)}!");
                return;
            }

            if (TryAddIngredient(kitchenObject.KitchenItemSO))
                KitchenObject.DestroyKitchenObject(kitchenObject);
        }
        [ClientRpc] private void AddIngredientClientRpc(int ingredientIndex, Ingredient ingredientFound)
        {
            if (ingredientIndex == plateIngredientsHeldList.Count) // i.e. when index == length, new element was added to end of list
            {
                plateIngredientsHeldList.Add(ingredientFound);

                if (plateIngredientsHeldList.Count == 0)
                    plateContentsViewParentUITransform.gameObject.SetActive(true);
                // for the first ingredient ever added make contents display by default
            }
            else
                plateIngredientsHeldList[ingredientIndex] = ingredientFound;
            
            OnIngredientChanged?.Invoke(this,
            new IngredientsChangedEventArgs
            {
                ingredient = ingredientFound, isAdded = true,
            });
        }

        private bool TryAddIngredient(KitchenItemSO kitchenItemSO)
        {
            if (!IsServer) return false; // should be run from server only

            if (kitchenItemSO.ObjectType != MainTypes.Edible || kitchenItemSO.EdibleType == EdibleTypes.raw)
                return false;

            int ingrFoundIndex = plateIngredientsHeldList.FindIngredient(kitchenItemSO, out Ingredient ingredientFound);
            if (ingrFoundIndex != -1)
            {
                ingredientFound.ingredientCount++;
            }
            else
            {
                ingrFoundIndex = plateIngredientsHeldList.Count; // taking length before list.add, which gives index of last element after added
                ingredientFound = new Ingredient(kitchenItemSO, 1);
                plateIngredientsHeldList.Add(ingredientFound);
            }

            AddIngredientClientRpc(ingrFoundIndex, ingredientFound);
            return true;
        }

        public void RemoveIngredient(KitchenItemSO kitchenItemSO, IKitchenObjectHolder switchKitchenObjectHolder)
        {
            if (kitchenItemSO == null || switchKitchenObjectHolder == null) return;

            RemoveIngredientServerRpc(NetworkManager.LocalClientId,
                MultiplayerManager.GetNetworkKitchenItemIndex(kitchenItemSO),
                switchKitchenObjectHolder.GetNetworkObject());

            if (autoLeaveInventoryAfterDropTemp)
                SetCanDrop(false);
        }

        [ServerRpc] public void RemoveIngredientServerRpc(ulong clientId, int kitchenItemSOIndex, NetworkObjectReference switchKitchenObjHolderNetworkObjRef)
        {
            if (!switchKitchenObjHolderNetworkObjRef.TryGet(out NetworkObject switchKitchenObjHolderNetworkObj))
            {
                this.LogWarning($"Invalid {nameof(switchKitchenObjHolderNetworkObjRef)} passed to {nameof(RemoveIngredientServerRpc)}!");
                return;
            }

            IKitchenObjectHolder switchKitchenObjectHolder = switchKitchenObjHolderNetworkObj.GetComponent<IKitchenObjectHolder>();
            if (kitchenItemSOIndex == -1 || switchKitchenObjectHolder == null ||
                switchKitchenObjectHolder.HasKitchenObject()) return;

            KitchenItemSO kitchenItemSO = MultiplayerManager.GetNetworkKitchenItem(kitchenItemSOIndex);
            if (!switchKitchenObjectHolder.CanHoldKitchenObject(kitchenItemSO)) return;

            int ingrFoundIndex = plateIngredientsHeldList.FindIngredient(kitchenItemSO, out Ingredient ingredientFound);
            if (ingrFoundIndex == -1)
            {
                this.LogError($"Given kitchen object:{kitchenItemSO} is not found as a ingredient at this plate.");
                return;
            }

            // decrease count of ingredients if present
            ingredientFound.ingredientCount--;
            RemoveIngredientClientRpc(clientId, ingrFoundIndex, ingredientFound);

            KitchenObject.SpawnKitchenObject(kitchenItemSO, switchKitchenObjectHolder);
        }
        [ClientRpc] public void RemoveIngredientClientRpc(ulong targetClientID, int ingredientIndex, Ingredient ingredientFound)
        {
            if (ingredientFound.ingredientCount <= 0)
            {
                plateIngredientsHeldList.Remove(ingredientFound);
                if (NetworkManager.LocalClientId == targetClientID && canDrop) // UI updation must be done to that specific playe only
                    SetCanDrop(false);
            }
            else
                plateIngredientsHeldList[ingredientIndex] = ingredientFound;

            OnIngredientChanged?.Invoke(this,
                new IngredientsChangedEventArgs
                {
                    ingredient = ingredientFound, isAdded = false,
                });
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

        public IReadOnlyList<Ingredient> GetPlateIngredientsList() => plateIngredientsHeldList;
    }
}