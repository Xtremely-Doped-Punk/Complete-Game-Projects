using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
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
                // single player logic
                /*
                bool checkIfDelivered = DeliveryManager.Instance.DeliverRecipe(this, playerPlateKitchenObject);
                // player is holding a plate
                if (checkIfDelivered)
                    ClientInvokeEventCounterOnDeliverySuccess();
                else
                    ClientInvokeEventCounterOnDeliveryFailure();
                */

                DeliveryManager.Instance.ClientDeliverRecipe(this, playerPlateKitchenObject);
                KitchenObject.DestroyKitchenObject(playerPlateKitchenObject);
            }
            else
            {
                // player doesnt have any object or it is not a plate object
                return;
            }
        }

        public void ClientInvokeEventCounterOnDeliverySuccess()
        {
            // accept delivery
            this.Log("Delivery acccepted :)");
            CounterOnDeliverySuccess?.Invoke(this, EventArgs.Empty);
        }
        public void ClientInvokeEventCounterOnDeliveryFailure()
        {
            // reject delivery
            this.Log("Delivery rejected :(");
            CounterOnDeliveryFailure?.Invoke(this, EventArgs.Empty);
        }

        public override bool CanHoldKitchenObject(KitchenItemSO kitchenItemSO)
        {
            return (kitchenItemSO.ObjectType == KitchenObject.MainTypes.NonEdible); // take only plate tat is non edible
        }
    }
}