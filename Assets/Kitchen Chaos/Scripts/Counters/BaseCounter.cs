using KC;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KC
{
    public abstract class BaseCounter : MonoBehaviour, IKitchenObjectHolder
    {
        public static event EventHandler OnPlayerDroppedSomethingOnCounters;
        public static void ResetStaticData()
        {
            //Debug.Log("BaseCounter static subcribers:"+ OnPlayerDroppedSomethingOnCounters.GetInvocationList().Length);
            OnPlayerDroppedSomethingOnCounters = null;
        }

        // lets leave these as private sot that it will be accessed only through base class funtions
        [SerializeField] private Transform counterTopPoint;
        private KitchenObject kitchenObjHeld;

        public abstract void InteractPrimary(PlayerController player);
        public virtual void InteractSecondary(PlayerController player)
        {
            // all counters need not neccessarily have multiple interactions
            Debug.LogWarning("No Secondary Interaction set for this counter: " + this);
        }

        protected void SwitchNewKitchenObject(KitchenItemSO newKitchenItemSO)
        {
            GetKitchenObject().DestrorSelf();
            KitchenObject.SpawnKitchenObject(newKitchenItemSO, this);
        }

        // use this contition when both player and counter has its own objects
        // keep ( checkCounterAlso = false ), if the counter cant hold plate as its kitchenObjectHeld, to save time
        protected bool CheckPossiblePlateInteractions(PlayerController player, bool canCounterHoldPlate = true) 
        {// returns if the interaction wass succesful or not

            PlateKitchenObject playerPlateKitchenObject = null, counterPlateKitchenObject = null;
            bool isPlayerKitchenObjectPlate = player.GetKitchenObject().TryGetPlate(out playerPlateKitchenObject);
            bool isCounterKitchenObjectPlate = canCounterHoldPlate && 
                this.GetKitchenObject().TryGetPlate(out counterPlateKitchenObject);

            if ((isCounterKitchenObjectPlate && isPlayerKitchenObjectPlate)
                || (!isCounterKitchenObjectPlate && !isPlayerKitchenObjectPlate))
            {
                // both player and counter holds a plate or a kitchen-object
                Debug.LogWarning("Counter:" + this + "  already holds a object!");
                return false;
            }
            else
            {
                IKitchenObjectHolder ingredientKitchenObjectHolder;
                PlateKitchenObject plateKitchenObject;

                if (isPlayerKitchenObjectPlate)
                {
                    ingredientKitchenObjectHolder = this;
                    plateKitchenObject = playerPlateKitchenObject;
                }
                else //if (isCounterKitchenObjectPlate)
                {
                    ingredientKitchenObjectHolder = player;
                    plateKitchenObject = counterPlateKitchenObject;
                }

                AddIngredientToPlate(plateKitchenObject, ingredientKitchenObjectHolder.GetKitchenObject());
                return true;
            }
        }

        protected static void AddIngredientToPlate(PlateKitchenObject plateKitchenObject, KitchenObject kitchenObject)
        {
            plateKitchenObject.AddIngredient(kitchenObject);
        }

        #region Interface: KitchenObjectHolder
        public virtual bool CanHoldKitchenObject(KitchenItemSO kitchenItemSO) => false; // this needs to defined seperately for each counter
        public Transform GetHolderTransform() => counterTopPoint;
        public virtual void SetKitchenObject(KitchenObject kitchenObject)
        {
            // when ever any script trys to set counter's kictchen object,
            if (kitchenObject == null)
            {
                ClearKitchenObject();
                return;
            }
            this.kitchenObjHeld = kitchenObject;

            // if (kitchenObject != null) : means player has dropped something on the counters
            // else: means player has picked something from counters (implemented in general, in player script)

            if (kitchenObject.GetKitchenObjectHolder() is PlayerController) // check if prev owner was player
                OnPlayerDroppedSomethingOnCounters?.Invoke(GetHolderTransform(), EventArgs.Empty);
            // else: counter must swapped the kitched-object to a next stage recipe processed/cooked kitchen-object
            
        }
        public KitchenObject GetKitchenObject() => kitchenObjHeld;
        public void ClearKitchenObject() => this.kitchenObjHeld = null;
        public bool HasKitchenObject() => kitchenObjHeld != null;
        #endregion
    }
}