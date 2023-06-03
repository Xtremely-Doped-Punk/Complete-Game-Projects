using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KC
{
    public class GameOverUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI noOfOrdersDeliveredText;
        [SerializeField] private Button MainMenuBtn;
        [SerializeField] Transform Parent;

        private void Awake()
        {
            MainMenuBtn.onClick.AddListener(() =>
            {
                // reset timw scale that has been changed to 0 become switchiing scene
                Time.timeScale = 1;
                SceneLoader.Load(SceneLoader.Scene.MainMenuScene);
            });
        }

        private void Start()
        {
            GameManager.Instance.OnGameStateChanged += HandleGameOverUIOnGameStateChanged;
            if (Parent == null )
            {
                Parent = transform;
                HideGameOverScreen();
            }
        }

        private void HandleGameOverUIOnGameStateChanged(object sender, EventArgs e)
        {
            if (GameManager.Instance.IsGameOver)
                ShowGameOverScreen();
            else
                HideGameOverScreen();
        }

        private void ShowGameOverScreen()
        {
            Parent.gameObject.SetActive(true);
            MainMenuBtn.Select();
            noOfOrdersDeliveredText.text = DeliveryManager.Instance.NoOfSucessfulDeliveries.ToString();
        }

        private void HideGameOverScreen()
        {
            Parent.gameObject.SetActive(false);
        }
    }
}