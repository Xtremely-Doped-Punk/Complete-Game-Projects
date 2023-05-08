using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace KC
{
    public class DeliveryManager : MonoBehaviour
    {
        public class OrdersChangedEventArgs:EventArgs { public DeliveryRecipeSO deliveryRecipeSOChanged;  public bool isAdded; }
        public event EventHandler<OrdersChangedEventArgs> OnDeliveryOrdersChanged;
        public event EventHandler OnDeliverySuccess, OnDeliveryFailure;

        public static DeliveryManager Instance { get; private set; } = null;

        [SerializeField] private float spawnDeliveryTimerMax = 4f;
        [SerializeField] private int waitingDeliveryMax = 4;
        [SerializeField] private FoodMenuSO menuSO;
        
        private List<DeliveryRecipeSO> waitingDeliveryRecipeSOList = new();
        private float spawnDeliveryTimer;
        public int NoOfSucessfulDeliveries { get; private set; } = 0;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(this);
        }

        private void Update()
        {
            if (!GameManager.Instance.IsGamePlaying || waitingDeliveryRecipeSOList.Count >= waitingDeliveryMax) return;

            spawnDeliveryTimer -= Time.deltaTime;
            if (spawnDeliveryTimer <= 0)
            {
                spawnDeliveryTimer = spawnDeliveryTimerMax;
                DeliveryRecipeSO deliveryRecipeSOSpawned = menuSO.DeliveryRecipeSOArray[Random.Range(0, menuSO.DeliveryRecipeSOArray.Length)];
                waitingDeliveryRecipeSOList.Add( // add random recipe to be delivered
                    deliveryRecipeSOSpawned);
                OnDeliveryOrdersChanged?.Invoke(this,
                    new OrdersChangedEventArgs { deliveryRecipeSOChanged = deliveryRecipeSOSpawned, isAdded = true });
            }
        }

        public bool DeliverRecipe(CounterDelivery whichDeliveryCounter, PlateKitchenObject plateKitchenObject)
        {
            foreach (DeliveryRecipeSO deliveryRecipeSO in waitingDeliveryRecipeSOList)
            {
                if (plateKitchenObject.CheckDeliveryRecipeMatch(deliveryRecipeSO))
                {
                    NoOfSucessfulDeliveries++;
                    waitingDeliveryRecipeSOList.Remove(deliveryRecipeSO);

                    OnDeliveryOrdersChanged?.Invoke(this, 
                        new OrdersChangedEventArgs { deliveryRecipeSOChanged = deliveryRecipeSO, isAdded = false });

                    OnDeliverySuccess?.Invoke(whichDeliveryCounter, EventArgs.Empty);
                    return true;
                }
            }

            OnDeliveryFailure?.Invoke(whichDeliveryCounter, EventArgs.Empty);
            return false;
        }

        public IReadOnlyList<DeliveryRecipeSO> GetWaitingDeliveryRecipes() => waitingDeliveryRecipeSOList;
        public IReadOnlyCollection<DeliveryRecipeSO> GetMenuDeliveryRecipeSOs => menuSO.DeliveryRecipeSOArray;
    }
}