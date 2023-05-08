using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KC
{
    [CreateAssetMenu(menuName = "KC/KitchenItem")]
    public class KitchenItemSO : ScriptableObject
    {
        [field: SerializeField] public string Name { get; private set; } = null;
        [field: SerializeField] public GameObject Prefab { get; private set; } = null;
        [field: SerializeField] public Sprite Icon { get; private set; } = null;
        [field: SerializeField] public KitchenObject.MainTypes ObjectType { get; private set; } = 0;

        [field:Header("Edible SubCategory")]
        [field: SerializeField] public KitchenObject.EdibleTypes EdibleType { get; private set; } = 0;
        [field: Header("NonEdible SubCategory")]
        [field: SerializeField] public KitchenObject.NonEdibleTypes NonEdibleType { get; private set; } = 0;
    }


}