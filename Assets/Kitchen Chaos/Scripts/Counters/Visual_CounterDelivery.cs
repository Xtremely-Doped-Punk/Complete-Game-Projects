using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KC
{
    public class Visual_CounterDelivery : MonoBehaviour
    {
        private const string POPUP = "Popup";

        // display delivery result ui
        [SerializeField] private CounterDelivery counterDelivery;
        [SerializeField] private Transform deliveryResultUIParent;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private Color bgSucessColor = Color.green, bgFailureColor = Color.red;
        [SerializeField] private Sprite successIconSprite, failureIconSprite;
        //[SerializeField] private float hideDelay = 2f;
        [SerializeField] private Animator deliveryResultUIAnimator; 
        // make sure animator is a child of canvas have look-at script component
        // as both will try update the rotation at the same time.
        // this way animator will change the local rotation under a child obj

        private void Start()
        {
            counterDelivery.CounterOnDeliverySuccess += HandleUIVisualsOnDeliverySuccess;
            counterDelivery.CounterOnDeliveryFailure += HandleVisualsUIOnDeliveryFailure;
        }

        private void HandleUIVisualsOnDeliverySuccess(object sender, System.EventArgs e)
        {
            //ShowResults(); CancelInvoke();
            deliveryResultUIAnimator.SetTrigger(POPUP);
            backgroundImage.color = bgSucessColor;
            iconImage.sprite = successIconSprite;
            messageText.text = "Delivery\nSuccess";
            //Invoke(nameof(HideResults), hideDelay);
        }
        
        private void HandleVisualsUIOnDeliveryFailure(object sender, System.EventArgs e)
        {
            //ShowResults(); CancelInvoke();
            deliveryResultUIAnimator.SetTrigger(POPUP);
            backgroundImage.color = bgFailureColor;
            iconImage.sprite = failureIconSprite;
            messageText.text = "Delivery\nFailure";
            //Invoke(nameof(HideResults), hideDelay);
        }
        private void ShowResults()
        {
            deliveryResultUIParent.gameObject.SetActive(true);
        }

        private void HideResults()
        {
            deliveryResultUIParent.gameObject.SetActive(false);
        }
    }
}