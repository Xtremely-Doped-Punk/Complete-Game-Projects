using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KC
{
    public class DeliveryManagerUI : MonoBehaviour
    {
        public class OrderID_DeliveryRecipeSO_Map
        {
            //public int orderID;
            public DeliveryRecipeSO deliveryRecipeSO;
            public DeliverySingleOrderUI orderInstanceUI;

            public static OrderID_DeliveryRecipeSO_Map FindOrderIDRecipeSOMap(
                List<OrderID_DeliveryRecipeSO_Map> orderRecipeMaps, DeliveryRecipeSO findDeliveryRecipeSO)
            {
                foreach (OrderID_DeliveryRecipeSO_Map order_receipe_map in orderRecipeMaps)
                {
                    if (order_receipe_map.deliveryRecipeSO == findDeliveryRecipeSO)
                    {
                        return order_receipe_map;
                    }
                }
                return null;
            }
        }

        [SerializeField] private Transform orderContainerParent;
        [SerializeField] private DeliverySingleOrderUI orderTemplate;

        private List<OrderID_DeliveryRecipeSO_Map> waitingOrderIDDeliveryRecipeSOMaps = new();

        private void Start()
        {
            orderTemplate.gameObject.SetActive(false);

            DeliveryManager.Instance.OnDeliveryOrdersChanged += HandleUIOnDeliveryOrdersChanged;
        }

        private void HandleUIOnDeliveryOrdersChanged(object sender, DeliveryManager.OrdersChangedEventArgs e)
        {

            if (e.isAdded)
            {
                // new order added to waiting order list
                DeliverySingleOrderUI orderInstance = Instantiate(orderTemplate, orderContainerParent);
                orderInstance.SetupOrderRecipeUI(e.deliveryRecipeSOChanged);
                orderInstance.gameObject.SetActive(true);

                OrderID_DeliveryRecipeSO_Map orderRecipeMap = 
                    new OrderID_DeliveryRecipeSO_Map 
                    { 
                        deliveryRecipeSO = e.deliveryRecipeSOChanged, 
                        orderInstanceUI = orderInstance 
                    };
                waitingOrderIDDeliveryRecipeSOMaps.Add(orderRecipeMap);
            }
            else
            {
                // an order might have succesfully completed
                OrderID_DeliveryRecipeSO_Map orderRecipeMap =
                    OrderID_DeliveryRecipeSO_Map.FindOrderIDRecipeSOMap
                    (waitingOrderIDDeliveryRecipeSOMaps, e.deliveryRecipeSOChanged);

                if (orderRecipeMap != null)
                {
                    Destroy(orderRecipeMap.orderInstanceUI.gameObject);
                    waitingOrderIDDeliveryRecipeSOMaps.Remove(orderRecipeMap);
                }
                else
                {
                    this.Log("Order:" + e.deliveryRecipeSOChanged + " not found in delivery manager ui, something went wrong!!");
                }
            }
        }

    }
}