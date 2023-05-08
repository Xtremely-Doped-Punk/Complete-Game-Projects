using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KC
{
    public interface IKitchenObjectHolder
    {
        public Transform GetHolderTransform();
        public void SetKitchenObject(KitchenObject kitchenObject);
        public KitchenObject GetKitchenObject();
        public void ClearKitchenObject();
        public bool HasKitchenObject();
        public bool CanHoldKitchenObject(KitchenItemSO kitchenItemSO);
    }
}