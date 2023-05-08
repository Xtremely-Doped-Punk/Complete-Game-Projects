using System;
using UnityEngine;
using UnityEngine.UI;

namespace KC
{
    public class GamePauseUI : MonoBehaviour
    {
        [SerializeField] private Button ResumeBtn;
        [SerializeField] private Button MainMenuBtn;
        [SerializeField] private Button OptionsBtn;
        [SerializeField] private OptionsUI OptionsUI;
        [SerializeField] private Transform Parent;

        private void Awake()
        {
            ResumeBtn.onClick.AddListener(() =>
            {
                GameManager.Instance.TogglePauseGame();
            });

            OptionsBtn.onClick.AddListener(() =>
            {
                HideGamePauseScreen(); 
                // hide pause menu, as the buttons automatic navigation might mess up 
                // when pause menu screen are also in the background

                OptionsUI.ShowOptionsMenu(ShowGamePauseScreen); 
                // pass a delagate fn to show back pause-menu on screen when option-menu is closed
            });

            MainMenuBtn.onClick.AddListener(() =>
            {
                // reset timw scale that has been changed to 0 become switchiing scene
                Time.timeScale = 1;
                SceneLoader.Load(SceneLoader.Scene.MainMenuScene);
            });
        }

        private void Start()
        {
            GameManager.Instance.OnGameTogglePaused  += HandleGameOverUIOnGameStateChanged;
            if (Parent == null)
            {
                Parent = transform;
                HideGamePauseScreen();
            }
        }

        private void HandleGameOverUIOnGameStateChanged(object sender, EventArgs e)
        {
            if (GameManager.Instance.IsGamePaused)
                ShowGamePauseScreen();
            else
                HideGamePauseScreen();
        }

        private void ShowGamePauseScreen()
        {
            Parent.gameObject.SetActive(true);
            ResumeBtn.Select();
            /* keep a button always selected so that it can switched
             (scrolled to other buttons) by input system suporting multi-platform
             using event-system's button navigation (default: automatic) which
             can be visualized in button component in inspector */
        }

        private void HideGamePauseScreen()
        {
            Parent.gameObject.SetActive(false);
            OptionsUI.HideOptionsMenu();
        }
    }
}