using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KC
{
    [CreateAssetMenu(menuName = "KC/KitchenItem")]
    public class KitchenItemSO : ScriptableObject
    {
        [field: SerializeField, Tooltip("Make sure to give it a unique name with max 128-chars, as the multipler data-transfer depend on it.")] 
        public string UniqueName { get; private set; } = null;
        [field: SerializeField] public GameObject Prefab { get; private set; } = null;
        [field: SerializeField] public Sprite Icon { get; private set; } = null;
        [field: SerializeField] public KitchenObject.MainTypes ObjectType { get; private set; } = 0;

        [field:Header("Edible SubCategory")]
        [field: SerializeField] public KitchenObject.EdibleTypes EdibleType { get; private set; } = 0;
        [field: Header("NonEdible SubCategory")]
        [field: SerializeField] public KitchenObject.NonEdibleTypes NonEdibleType { get; private set; } = 0;
    }


}