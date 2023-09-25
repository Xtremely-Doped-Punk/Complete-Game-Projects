using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace KC
{
    public class TutorialUI : MonoBehaviour
    {
        [SerializeField] Transform parent;
        [SerializeField] Transform tutorialTransform;
        [SerializeField] Transform waitingTransform;
        [SerializeField] Image loadingBar;
        [SerializeField] bool reverseTimer = true;

        private void Awake()
        {
            if (parent == null)
            {
                parent = transform;
            }
        }

        private void Start()
        {
            ShowLoadingScreen(); ShowTutorialScreen();
            GameManager.Instance.OnAnyPlayerToggleReady += HandleTutorialUIOnTogglePlayerReady;
            GameManager.Instance.OnGameStateChanged += HandleTutorialUIOnGameStateChanged;
        }

        private void HandleTutorialUIOnTogglePlayerReady(object sender, KeyValuePair<ulong,bool> isClientReadyPair)
        {
            if (isClientReadyPair.Key != GameManager.LocalClientID) return; // for now just showing for local player

            if (GameManager.Instance.IsLocalPlayerReady)
            {
                HideTutorialScreen();
                ShowWaitingScreen();
            }
            else
            {
                HideWaitingScreen();
                ShowTutorialScreen();
            }
        }

        private void HandleTutorialUIOnGameStateChanged(object sender, System.EventArgs e)
        {
            if (GameManager.Instance.IsCountdownActive)
                HideLoadingScreen();
            if (!GameManager.Instance.IsReadyToPlay)
                loadingBar.fillAmount = reverseTimer ? 0 : 1;
        }

        private void Update()
        {
            if (GameManager.Instance.IsReadyToPlay)
            {
                float timerNormalized = GameManager.Instance.GetWaitingTimerNormalized();
                loadingBar.fillAmount = reverseTimer ? timerNormalized : 1 - timerNormalized;
                if (timerNormalized <= 0)
                    enabled = false;
            }
        }

        public void ShowTutorialScreen() => tutorialTransform.gameObject.SetActive(true);
        public void HideTutorialScreen() => tutorialTransform.gameObject.SetActive(false);
        public void ShowWaitingScreen() => waitingTransform.gameObject.SetActive(true);
        public void HideWaitingScreen() => waitingTransform.gameObject.SetActive(false);
        public void ShowLoadingScreen() => parent.gameObject.SetActive(true);
        public void HideLoadingScreen() => parent.gameObject.SetActive(false);
    }
}