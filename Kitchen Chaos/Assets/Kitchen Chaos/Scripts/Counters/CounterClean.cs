using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KC
{
    public class CounterClean : BaseCounter
    {
        public override void InteractPrimary(PlayerController player)
        {
            if (!HasKitchenObject())
            {
                // there is no kitch object in this counter
                if (player.HasKitchenObject())
                {
                    this.Log("Clean Counter (RPC)-interacted holding: " + player.GetKitchenObject().KitchenItemSO.UniqueName + " from player.");

                    // player is carrying some kitchen object 
                    player.GetKitchenObject().SetKitchenObjectHolder(this); // place object on counter

                    // this code moved to "KitchenObject.SetCounter()"
                    /*
                    1. make parent as counterTopPoint and make localPosition as (0,0,0)
                    2. assign kitchenCounter to the resp component
                    */
                }
                else
                {
                    // nothing to interact as both player and counter has no obj on them\
                }
            }
            else
            {
                if (!player.HasKitchenObject())
                {
                    this.Log("Clean Counter (RPC)-interacted giving: " + this.GetKitchenObject().KitchenItemSO.UniqueName + " to player");

                    this.GetKitchenObject().SetKitchenObjectHolder(player); // give object to player
                }
                else
                {
                    // both player and counter is holding its own objects
                    CheckPossiblePlateInteractions(player , canCounterHoldPlate: true);
                }
            }
        }

        // as SetKitchenObject() are made through Rpc calls, its best not access the object immediately,
        // as req made server-rpc and callback make to client-rpc only sets the kitchen-obj in multiplier

        public override bool CanHoldKitchenObject(KitchenItemSO kitchenItemSO) => true;
    }
}