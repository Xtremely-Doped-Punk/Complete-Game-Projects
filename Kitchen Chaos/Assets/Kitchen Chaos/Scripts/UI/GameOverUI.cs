using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace KC
{
    public class GameOverUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI gameOverReasonText;
        [SerializeField] TextMeshProUGUI noOfOrdersDeliveredText;
        [SerializeField] private Button MainMenuBtn;
        [SerializeField] Transform Parent;
        bool IsActive => Parent.gameObject.activeSelf;

        private void Awake()
        {
            MainMenuBtn.onClick.AddListener(() =>
            {
                NetworkManager.Singleton.Shutdown(); // shutdown network connection

                // reset time scale that had been changed to 0 before switchiing scene
                if (NetworkManager.Singleton.IsServer) Time.timeScale = 1;
                SceneLoader.Load(SceneLoader.Scene.MainMenuScene);
            });
        }

        private void Start()
        {
            GameManager.Instance.OnGameStateChanged += HandleGameOverUIOnGameStateChanged;
            GameManager.Instance.OnAnyPlayerDisconnected += HandleGameOverUIOnPlayerDisconnected;
            if (Parent == null )
            {
                Parent = transform;
                HideGameOverScreen();
            }
        }

        private void HandleGameOverUIOnPlayerDisconnected(object sender, ulong id)
        {
            //this.Log($"GameOverMenu:: Handling Client[{id}] Disconnect, host disconnected:{id == NetworkManager.ServerClientId}, Late Join Disapproval:{id == GameManager.LocalClientID}");
            if (id != NetworkManager.ServerClientId) return;
         
            string disconnectReason = NetworkManager.Singleton.DisconnectReason;
            if (disconnectReason != string.Empty) // connection approval failed due to late join
                ShowGameOverScreen(disconnectReason);
            else
                ShowGameOverScreen("Host Disconnected"); // if Host disconnects

            // once game over, scene is changed, thus redoing this small fix wont matter
            GameManager.Instance.OnGameStateChanged -= HandleGameOverUIOnGameStateChanged; // to avoid late callbacks
        }

        private void HandleGameOverUIOnGameStateChanged(object sender, EventArgs e)
        {
            if (GameManager.Instance.IsGameOver && !IsActive)
                ShowGameOverScreen("Time's Up");
            else if (IsActive)
                HideGameOverScreen();
        }

        private void ShowGameOverScreen(string reason)
        {
            //this.Log($"ShowGameOver::Reason:{reason}");
            gameOverReasonText.text = reason; // add reason for game over
            Parent.gameObject.SetActive(true);
            MainMenuBtn.Select();
            noOfOrdersDeliveredText.text = DeliveryManager.Instance.NoOfSucessfulDeliveries.ToString();
        }

        private void HideGameOverScreen()
        {
            //this.Log("HideGameOver");
            gameOverReasonText.text = null; // reset
            Parent.gameObject.SetActive(false);
        }
    }
}