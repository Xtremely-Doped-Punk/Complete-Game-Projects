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
                    // player is carrying some kitchen object 
                    player.GetKitchenObject().SetKitchenObjectHolder(this); // place object on counter

                    // this code moved to "KitchenObject.SetCounter()"
                    /*
                    1. make parent as counterTopPoint and make localPosition as (0,0,0)
                    2. assign kitchenCounter to the resp component
                    */
                    Debug.Log("Clean Counter interacted holding: " + GetKitchenObject().KitchenItemSO.Name + " from player.");
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
                    this.GetKitchenObject().SetKitchenObjectHolder(player); // give object to player
                    Debug.Log("Clean Counter interacted giving: " + player.GetKitchenObject().KitchenItemSO.Name + " to player");
                }
                else
                {
                    // both player and counter is holding its own objects
                    CheckPossiblePlateInteractions(player , canCounterHoldPlate: true);
                }
            }
        }

        public override bool CanHoldKitchenObject(KitchenItemSO kitchenItemSO) => true;
    }
}