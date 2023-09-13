using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        [Header("Debug")]
        [SerializeField] private List<CounterDelivery> deliveryCountersList;
        private readonly List<DeliveryRecipeSO> waitingDeliveryRecipeSOList = new();
        private float spawnDeliveryTimer;
        public int NoOfSucessfulDeliveries { get; private set; } = 0;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(this);
            
            if (deliveryCountersList != null)
                deliveryCountersList.RemoveAll(x => x == null);
            if (deliveryCountersList.Count == 0)
            deliveryCountersList = FindObjectsOfType<CounterDelivery>().ToList();
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
            for (int i = 0; i < deliveryCountersList.Count; i++)
            {
                if (deliveryCounter == deliveryCountersList[i])
                    return i;
            }
            return -1;
        }

        #region single player logic, i.e. done use in multiplayer implementation
        [Obsolete] public bool DeliverRecipe_SinglePlayer(CounterDelivery whichDeliveryCounter, PlateKitchenObject plateKitchenObject)
        {
            // single player logic direct implementation with rpc's
            for (int index = 0; index < waitingDeliveryRecipeSOList.Count; index++)
            {
                if (CheckPlateDeliveryRecipeMatch_SinglePlayer(plateKitchenObject.GetPlateIngredientsList(), index))
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

        [Obsolete] private bool CheckPlateDeliveryRecipeMatch_SinglePlayer(IReadOnlyList<Ingredient> plateIngredientsHeldList, int deliveryRecipeIndex)
        {
            DeliveryRecipeSO deliveryRecipeSO = waitingDeliveryRecipeSOList[deliveryRecipeIndex];

            if (plateIngredientsHeldList.Count != deliveryRecipeSO.IngredientsArray.Length) return false;
            // if length of recipe doesnt match, no need to check each kitchen object in them

            foreach (Ingredient recipeIngredient in deliveryRecipeSO.IngredientsArray)
            {
                if (plateIngredientsHeldList.FindIngredient(recipeIngredient.KitchenItemSO, out Ingredient ingredientFound) == -1
                    || ingredientFound.ingredientCount != recipeIngredient.ingredientCount)
                {
                    // if any ingredient not found or if the ingredient count doesn't match
                    return false;
                }
            }
            return true;
        }
        #endregion

        public void ClientDeliverRecipe(CounterDelivery whichDeliveryCounter, PlateKitchenObject plateKitchenObject)
        {
            int deliveryCounterIndex = FindDeliveryCounterIndex(whichDeliveryCounter);
            Ingredient[] plateIngredients = plateKitchenObject.GetPlateIngredientsList().ToArray();

            CheckDeliveryRecipeServerRpc(deliveryCounterIndex, plateIngredients);
        }

        private bool CheckPlateDeliveryRecipeMatch(IReadOnlyList<Ingredient> plateIngredientsList, int deliveryRecipeIndex)
        {
            DeliveryRecipeSO deliveryRecipeSO = waitingDeliveryRecipeSOList[deliveryRecipeIndex];
            //this.Log("Checking Delivery Recipe Match for:" + deliveryRecipeSO+ ", plate Ingredients Count:"+plateIngredientsList.Count);

            if (plateIngredientsList.Count != deliveryRecipeSO.IngredientsArray.Length) return false;
            // if length of recipe doesnt match, no need to check each kitchen object in them

            foreach (Ingredient recipeIngredient in deliveryRecipeSO.IngredientsArray)
            {
                int ingredientFoundCount = plateIngredientsList.FindIngredientCount(recipeIngredient.KitchenItemSO);
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
        private void CheckDeliveryRecipeServerRpc(int whichDeliveryCounterIndex, Ingredient[] plateIngredientsArray)
        {
            if (whichDeliveryCounterIndex == -1)
            {
                this.LogError("CheckCorrectDeliveryRecipeServerRpc() called with wrong parameter types!");
                return;
            }
            if (plateIngredientsArray == null)
            {
                DeliveryFailureClientRpc(whichDeliveryCounterIndex);
                return;
            }

            for (int index = 0; index < waitingDeliveryRecipeSOList.Count; index++)
            {
                if (CheckPlateDeliveryRecipeMatch(plateIngredientsArray, index))
                {
                    //DeliveryRecipeSO deliveryRecipeSO = waitingDeliveryRecipeSOList[index];
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

            CounterDelivery whichDeliveryCounter = deliveryCountersList[whichDeliveryCounterIndex];

            OnDeliverySuccess?.Invoke(whichDeliveryCounter, EventArgs.Empty);
            whichDeliveryCounter.ClientInvokeEventCounterOnDeliverySuccess();
        }

        [ClientRpc]
        private void DeliveryFailureClientRpc(int whichDeliveryCounterIndex)
        {
            CounterDelivery whichDeliveryCounter = deliveryCountersList[whichDeliveryCounterIndex];

            OnDeliveryFailure?.Invoke(whichDeliveryCounter, EventArgs.Empty);
            whichDeliveryCounter.ClientInvokeEventCounterOnDeliveryFailure();
        }

        public IReadOnlyList<DeliveryRecipeSO> GetWaitingDeliveryRecipes() => waitingDeliveryRecipeSOList;
        public IReadOnlyCollection<DeliveryRecipeSO> GetMenuDeliveryRecipeSOs => menuSO.DeliveryRecipeSOArray;
    }
}