using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace KC
{
    [System.Serializable]
    public class Ingredient
    {
        public KitchenItemSO kitchenItemSO;
        [Range(1, 5)] public int ingredientCount;

        public struct NetworkData : INetworkSerializable
        {
            public int networkKitchenItemIndex; // use hash-code or fixed-string, both has its pros and cons
            public int ingredientCount;
            /* pros and cons:
            in case of HashCode, using a stringVariable.GetHashCode() and "yourString".GetHashCode(), 
            might not be sometimes neccessary same, as accoding to documentations of .NET:
            " The hash code itself is not guaranteed to be stable. Hash codes for identical strings can differ across .NET implementations, 
            across .NET versions, and across .NET platforms (such as 32-bit and 64-bit) for a single version of .NET. 
            In some cases, they can even differ by application domain. 
            This implies that two subsequent runs of the same program may return different hash codes. "

            incase of Fixed-N-String, only problem is that the length of the string passed to it should have length less than 'N'
            orelse it might throw a argument exception error...

            here, i have used a common asset (scriptable object) to main the list of kitchenItemSO, 
            that should be in same order of build versions in order to work correctly accross all clients, 
            as we using the index of that list to sync data between clients...
            */
            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
            {
                serializer.SerializeValue(ref networkKitchenItemIndex);
                serializer.SerializeValue(ref ingredientCount);
            }

            public static int FindIngredientCount(IReadOnlyList<NetworkData> networkDatas, KitchenItemSO kitchenItemSO)
            {
                int networkKitchenItemIndex = MultiplayerManager.Instance.GetNetworkKitchenItemIndex(kitchenItemSO);
                return FindIngredientCount(networkDatas, networkKitchenItemIndex);
            }

            public static int FindIngredientCount(IReadOnlyList<NetworkData> networkDatas, int networkKitchenItemIndex)
            {
                if (networkKitchenItemIndex != -1)
                {
                    foreach (NetworkData networkData in networkDatas)
                    {
                        if (networkData.networkKitchenItemIndex == networkKitchenItemIndex)
                            return networkData.ingredientCount;
                    }
                }
                return 0;
            }
        }

        public static Ingredient FindIngredient(IReadOnlyList<Ingredient> ingredients, KitchenItemSO kitchenItemSO)
        {
            foreach (Ingredient ingredient in ingredients)
            {
                if (ingredient.kitchenItemSO == kitchenItemSO)
                {
                    return ingredient;
                }
            }
            return null;
        }

        public NetworkData GetNetworkData()
        {
            return new NetworkData 
            { 
                networkKitchenItemIndex = MultiplayerManager.Instance.GetNetworkKitchenItemIndex(kitchenItemSO), 
                ingredientCount = ingredientCount 
            };
        }
    }
}