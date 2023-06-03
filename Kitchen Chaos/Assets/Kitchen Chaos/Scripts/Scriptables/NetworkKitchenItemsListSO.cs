using KC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "KC/NetworkKitchenItemsList")]
public class NetworkKitchenItemsListSO : ScriptableObject
{
    [field: SerializeField] public List<KitchenItemSO> KitchenItemsSO { get; private set; } = null;
    public int Count => KitchenItemsSO.Count;
    public int IndexOf(KitchenItemSO item) => KitchenItemsSO.IndexOf(item);
    public KitchenItemSO AtIndex(int i) => KitchenItemsSO[i];
}
