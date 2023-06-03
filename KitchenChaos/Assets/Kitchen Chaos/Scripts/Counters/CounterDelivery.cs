using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KC
{
    public class CounterDelivery : BaseCounter
    {
        public event EventHandler CounterOnDeliverySuccess, CounterOnDeliveryFailure;
        public override void InteractPrimary(PlayerController player)
        {
            if (player.HasKitchenObject() && 
                player.GetKitchenObject().TryGetPlate(out PlateKitchenObject playerPlateKitchenObject))
            {
                bool checkIfDelivered = DeliveryManager.Instance.DeliverRecipe(this, playerPlateKitchenObject);
                // player is holding a plate
                if (checkIfDelivered)
                {
                    // accept delivery
                    Debug.Log("Delivery acccepted :)");
                    CounterOnDeliverySuccess?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    // reject delivery
                    Debug.Log("Delivery rejected :(");
                    CounterOnDeliveryFailure?.Invoke(this, EventArgs.Empty);
                }
                playerPlateKitchenObject.DestrorSelf();
            }
            else
            {
                // player doesnt have any object or it is not a plate object
                return;
            }
        }

        public override bool CanHoldKitchenObject(KitchenItemSO kitchenItemSO)
        {
            return (kitchenItemSO.ObjectType == KitchenObject.MainTypes.NonEdible); // take only plate tat is non edible
        }
    }
}