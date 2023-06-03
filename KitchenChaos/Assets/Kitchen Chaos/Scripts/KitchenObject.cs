using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KC
{
    public class KitchenObject : MonoBehaviour
    {
        public enum MainTypes { Edible, NonEdible, }
        public enum EdibleTypes { raw, processed, }
        public enum NonEdibleTypes { disposable, reusable, }

        [field: SerializeField] public KitchenItemSO KitchenItemSO { get; private set; } = null;

        private IKitchenObjectHolder kitchenObjectHolder;
        // note: interfaces cant be serialized as they don't have any actual data unlike classes

        public IKitchenObjectHolder GetKitchenObjectHolder() => kitchenObjectHolder;
        public void SetKitchenObjectHolder(IKitchenObjectHolder switchKitchenObjectHolder) 
        {
            if (switchKitchenObjectHolder == null)
            {
                Debug.LogError("Cant place at null KitchenObjectHolder!");
                return;
            }
            if (switchKitchenObjectHolder.HasKitchenObject())
            {
                Debug.LogError("Cant place at " + switchKitchenObjectHolder + " as it already holds a object!");
                return;
            }

            // note: first set KO in new holder, so that new holder can see previous holder was
            switchKitchenObjectHolder.SetKitchenObject(this);
            this.kitchenObjectHolder?.ClearKitchenObject(); // then, remove kitchen object ref from prev holder

            //Debug.Log(">>> kitchen object: " + KitchenItemSO.Name + " has been switched from "
            //            + this.kitchenObjectHolder + " to" + switchKitchenObjectHolder);

            this.kitchenObjectHolder = switchKitchenObjectHolder;

            transform.parent = switchKitchenObjectHolder.GetHolderTransform();
            transform.localPosition = Vector3.zero;
        }

        public void TrySetKitchenObjectHolder(IKitchenObjectHolder switchKitchenObjectHolder)
        {

        }

        public void DestrorSelf()
        {
            if (kitchenObjectHolder != null)
                kitchenObjectHolder.ClearKitchenObject(); // remove reference from parent holder
            // trash counter deletes any object tats been set to it, in that case,
            // 'kitchenObjectHolder' might still be uninitialized
            Destroy(gameObject); // then destroy the object
        }

        public static KitchenObject SpawnKitchenObject(KitchenItemSO kitchenItemSO, IKitchenObjectHolder kitchenObjHolder)
        {
            Transform kitchenItemTransform = Instantiate(kitchenItemSO.Prefab).transform;
            var kitchenObj = kitchenItemTransform.GetComponent<KitchenObject>();
            kitchenObj.SetKitchenObjectHolder(kitchenObjHolder);
            return kitchenObj;
        }

        public bool TryGetPlate(out PlateKitchenObject plateKitchenObject)
        {
            plateKitchenObject =  this as PlateKitchenObject;
            if (this is PlateKitchenObject)
                return true;
            else
                return false;
        }
    }
}