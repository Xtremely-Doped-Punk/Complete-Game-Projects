using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace KC
{
    public class DeliveryManager : NetworkBehaviour
    {
        public class OrdersChangedEventArgs : EventArgs { public DeliveryRecipeSO deliveryRecipeSOChanged; public bool isAdded; }
        public event EventHandler<OrdersChangedEventArgs> OnDeliveryOrdersChanged;
        public event EventHandler OnDeliverySuccess, OnDeliveryFailure;

        public static DeliveryManager Instance { get; private set; } = null;

        [SerializeField] private float spawnDeliveryTimerMax = 4f;
        [SerializeField] private int waitingDeliveryMax = 4;
        [SerializeField] private FoodMenuSO menuSO;
        [SerializeField] private List<CounterDelivery> networkDeliveryCountersList;

        private readonly List<DeliveryRecipeSO> waitingDeliveryRecipeSOList = new();
        private float spawnDeliveryTimer;
        public int NoOfSucessfulDeliveries { get; private set; } = 0;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(this);
        }

        private void Start()
        {
            if (GameManager.Instance.Testing)
                spawnDeliveryTimer = spawnDeliveryTimerMax;
        }

        private void Update()
        {
            if (!IsServer) return; // run logic from server side only

            if (!GameManager.Instance.IsGamePlaying || waitingDeliveryRecipeSOList.Count >= waitingDeliveryMax) return;

            spawnDeliveryTimer -= Time.deltaTime;
            if (spawnDeliveryTimer <= 0)
            {
                spawnDeliveryTimer = spawnDeliveryTimerMax;
                int randomDeliveryRecipeIndex = Random.Range(0, menuSO.DeliveryRecipeSOArray.Length);

                SpawnNewDeliveryRecipeOverNetworkClientRpc(randomDeliveryRecipeIndex);
                // also note that in rpc call, make sure tto use only serializable data types, like int here
            }
        }

        [ClientRpc] // make sure of naming convention 'ClientRpc' at the end of the string
        private void SpawnNewDeliveryRecipeOverNetworkClientRpc(int spawnedDeliveryRecipeIndex)
        {
            DeliveryRecipeSO spawnedDeliveryRecipeSO = menuSO.DeliveryRecipeSOArray[spawnedDeliveryRecipeIndex];

            waitingDeliveryRecipeSOList.Add( // add random recipe to be delivered
                spawnedDeliveryRecipeSO);
            OnDeliveryOrdersChanged?.Invoke(this,
                new OrdersChangedEventArgs { deliveryRecipeSOChanged = spawnedDeliveryRecipeSO, isAdded = true });
        }

        private int FindDeliveryCounterIndex(CounterDelivery deliveryCounter)
        {
            for (int i = 0; i < networkDeliveryCountersList.Count; i++)
            {
                if (deliveryCounter == networkDeliveryCountersList[i])
                    return i;
            }
            return -1;
        }


        [Obsolete] public bool DeliverRecipe(CounterDelivery whichDeliveryCounter, PlateKitchenObject plateKitchenObject)
        {
            // single player logic direct implementation with rpc's
            for (int index = 0; index < waitingDeliveryRecipeSOList.Count; index++)
            {
                if (CheckPlateDeliveryRecipeMatch(plateKitchenObject.GetPlateIngredientsHeldList(), index))
                {
                    DeliveryRecipeSO deliveryRecipeSO = waitingDeliveryRecipeSOList[index];

                    NoOfSucessfulDeliveries++;
                    waitingDeliveryRecipeSOList.RemoveAt(index);

                    OnDeliveryOrdersChanged?.Invoke(this,
                        new OrdersChangedEventArgs { deliveryRecipeSOChanged = deliveryRecipeSO, isAdded = false });

                    OnDeliverySuccess?.Invoke(whichDeliveryCounter, EventArgs.Empty);
                    return true;
                }
            }

            OnDeliveryFailure?.Invoke(whichDeliveryCounter, EventArgs.Empty);
            return false;
        }

        [Obsolete] private bool CheckPlateDeliveryRecipeMatch(IReadOnlyList<Ingredient> plateIngredientsHeldList, int deliveryRecipeIndex)
        {
            DeliveryRecipeSO deliveryRecipeSO = waitingDeliveryRecipeSOList[deliveryRecipeIndex];

            if (plateIngredientsHeldList.Count != deliveryRecipeSO.IngredientsArray.Length) return false;
            // if length of recipe doesnt match, no need to check each kitchen object in them

            foreach (Ingredient recipeIngredient in deliveryRecipeSO.IngredientsArray)
            {
                Ingredient ingredientFound = Ingredient.FindIngredient(plateIngredientsHeldList, recipeIngredient.kitchenItemSO);
                if (ingredientFound == null || ingredientFound.ingredientCount != recipeIngredient.ingredientCount)
                {
                    // if any ingredient not found or if the ingredient count doesn't match
                    return false;
                }
            }
            return true;
        }

        public void ClientDeliverRecipe(CounterDelivery whichDeliveryCounter, PlateKitchenObject plateKitchenObject)
        {
            int deliveryCounterIndex = FindDeliveryCounterIndex(whichDeliveryCounter);
            Ingredient.NetworkData[] plateIngredientsNetworkDatas = plateKitchenObject.GetPlateIngredientsNetworkDatasList();

            CheckDeliveryRecipeServerRpc(deliveryCounterIndex, plateIngredientsNetworkDatas);
        }

        private bool CheckPlateDeliveryRecipeMatch(IReadOnlyList<Ingredient.NetworkData> plateIngredientsNetworkDataList, int deliveryRecipeIndex)
        {
            DeliveryRecipeSO deliveryRecipeSO = waitingDeliveryRecipeSOList[deliveryRecipeIndex];
            Debug.Log("Checking Delivery Recipe Match for:" + deliveryRecipeSO+ ", plate Ingredients Count:"+plateIngredientsNetworkDataList.Count);

            if (plateIngredientsNetworkDataList.Count != deliveryRecipeSO.IngredientsArray.Length) return false;
            // if length of recipe doesnt match, no need to check each kitchen object in them

            foreach (Ingredient recipeIngredient in deliveryRecipeSO.IngredientsArray)
            {
                int ingredientFoundCount = Ingredient.NetworkData.FindIngredientCount(plateIngredientsNetworkDataList, recipeIngredient.kitchenItemSO);
                if (ingredientFoundCount != recipeIngredient.ingredientCount)
                {
                    // if any ingredient not found or if the ingredient count doesn't match
                    return false;
                }
            }
            return true;
        }


        // by default only owners can call serverRPC, as delivery manager is server owned obj,
        // clients also need a way to invoke them, thus by setting the attribute of requiring ownership to false
        
        [ServerRpc(RequireOwnership = false)] 
        private void CheckDeliveryRecipeServerRpc(int whichDeliveryCounterIndex, Ingredient.NetworkData[] plateIngredientsNetworkDataList)
        {
            if (plateIngredientsNetworkDataList == null || whichDeliveryCounterIndex == -1)
            {
                Debug.LogError("CheckCorrectDeliveryRecipeServerRpc() called with wrong parameter types!");
                return;
            }

            for (int index = 0; index < waitingDeliveryRecipeSOList.Count; index++)
            {
                if (CheckPlateDeliveryRecipeMatch(plateIngredientsNetworkDataList, index))
                {
                    DeliveryRecipeSO deliveryRecipeSO = waitingDeliveryRecipeSOList[index];

                    DeliverySuccessClientRpc(whichDeliveryCounterIndex, index);
                    return;
                }
            }
            DeliveryFailureClientRpc(whichDeliveryCounterIndex);
        }

        [ClientRpc]
        private void DeliverySuccessClientRpc(int whichDeliveryCounterIndex, int deliveryRecipeIndex)
        {
            NoOfSucessfulDeliveries++;
            DeliveryRecipeSO deliveryRecipeSO = waitingDeliveryRecipeSOList[deliveryRecipeIndex];
            waitingDeliveryRecipeSOList.RemoveAt(deliveryRecipeIndex);

            OnDeliveryOrdersChanged?.Invoke(this,
                new OrdersChangedEventArgs { deliveryRecipeSOChanged = deliveryRecipeSO, isAdded = false });

            CounterDelivery whichDeliveryCounter = networkDeliveryCountersList[whichDeliveryCounterIndex];

            OnDeliverySuccess?.Invoke(whichDeliveryCounter, EventArgs.Empty);
            whichDeliveryCounter.ClientInvokeEventCounterOnDeliverySuccess();
        }

        [ClientRpc]
        private void DeliveryFailureClientRpc(int whichDeliveryCounterIndex)
        {
            CounterDelivery whichDeliveryCounter = networkDeliveryCountersList[whichDeliveryCounterIndex];

            OnDeliveryFailure?.Invoke(whichDeliveryCounter, EventArgs.Empty);
            whichDeliveryCounter.ClientInvokeEventCounterOnDeliveryFailure();
        }

        public IReadOnlyList<DeliveryRecipeSO> GetWaitingDeliveryRecipes() => waitingDeliveryRecipeSOList;
        public IReadOnlyCollection<DeliveryRecipeSO> GetMenuDeliveryRecipeSOs => menuSO.DeliveryRecipeSOArray;
    }
}