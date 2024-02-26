using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace KC
{
    public class GamePauseUI : MonoBehaviour
    {
        [SerializeField] private Button resumeBtn;
        [SerializeField] private Button mainMenuBtn;
        [SerializeField] private Button optionsBtn;
        [SerializeField] private OptionsUI optionsUI;
        [SerializeField] private Transform parent;
        [SerializeField] private Transform votePanel;
        [SerializeField] private PlayerIconSingleUI playerVoteUITemplate;

        private List<PlayerIconSingleUI> playerVoteUIList = new();
        private List<ulong> KeyIDs => playerVoteUIList.ConvertAll(x => x.ClientID);
        private List<bool> ValueReadys => playerVoteUIList.ConvertAll(x => x.Vote);

        private void Awake()
        {
            resumeBtn.onClick.AddListener(() =>
            {
                GameManager.Instance.ToggleReady();
            });

            optionsBtn.onClick.AddListener(() =>
            {
                HideGamePauseScreen(); 
                // hide pause menu, as the buttons automatic navigation might mess up 
                // when pause menu screen are also in the background

                optionsUI.ShowOptionsMenu(ShowGamePauseScreen); 
                // pass a delagate fn to show back pause-menu on screen when option-menu is closed
            });

            mainMenuBtn.onClick.AddListener(() =>
            {
                NetworkManager.Singleton.Shutdown(); // shutdown network connection when game is quit in between

                // reset time scale that had been changed to 0 before switchiing scene
                if (NetworkManager.Singleton.IsServer) Time.timeScale = 1;
                SceneLoader.Load(SceneLoader.Scene.MainMenuScene);
            });
        }

        private void Start()
        {
            GameManager.Instance.OnGameTogglePaused  += HandleGamePausedUIOnGameStateChanged;
            GameManager.Instance.OnAnyPlayerToggleReady += HandleGamePausedUIOnTogglePlayerReady;
            GameManager.Instance.OnAnyPlayerDisconnected += HandleGamePausedUIOnPlayerDisconnected;
            if (parent == null)
            {
                parent = transform;
                HideGamePauseScreen();
            }
        }

        private void HandleGamePausedUIOnPlayerDisconnected(object sender, ulong id)
        {
            if (id == NetworkManager.ServerClientId)
            {
                HideGamePauseScreen(); // game over when host disconnects
                return;
            }
            int keyIndex = KeyIDs.IndexOf(id);
            if (keyIndex != -1)
                Destroy(playerVoteUIList[keyIndex].gameObject);
        }

        private void HandleGamePausedUIOnTogglePlayerReady(object sender, KeyValuePair<ulong, bool> e)
        {
            int keyIndex = KeyIDs.IndexOf(e.Key);

            if (!GameManager.Instance.IsGamePlaying)
            {
                if (keyIndex == -1)
                    InstantiatePlayerVoteIcon(e); // adding exiting players icons before start of the match
                return;
            }

            if (keyIndex != -1)
                playerVoteUIList[keyIndex].UpdatePlayerIconVote(e.Value); // update colors of vote only at State.GamePlaying
            else
                InstantiatePlayerVoteIcon(e, true); // adding late joined players icons after start of the match

            DisplayVotePanel();
        }

        private void DisplayVotePanel()
        {
            if (GameManager.Instance.IsAllPlayersSameReady)
                votePanel.gameObject.SetActive(false);
            else
                votePanel.gameObject.SetActive(true);
        }

        private void InstantiatePlayerVoteIcon(KeyValuePair<ulong, bool> e, bool isLateJoin = false)
        {
            var voteUI = Instantiate(playerVoteUITemplate, votePanel);
            voteUI.SetupPlayerIconVote(e);
            playerVoteUIList.Add(voteUI);
            voteUI.gameObject.SetActive(true);

            if (isLateJoin)
                voteUI.UpdatePlayerIconVote();
        }

        private void HandleGamePausedUIOnGameStateChanged(object sender, EventArgs e)
        {
            if (!GameManager.Instance.IsGamePlaying) return;

            if (GameManager.Instance.IsGamePaused)
                ShowGamePauseScreen();
            else
                HideGamePauseScreen();

            if (GameManager.Instance.ResetPlayerStatesOnPaused)
                playerVoteUIList.ForEach(x => x.ResetPlayerIconVote(!GameManager.Instance.IsGamePaused));

            DisplayVotePanel();
        }

        private void ShowGamePauseScreen()
        {
            parent.gameObject.SetActive(true);
            resumeBtn.Select();
            /* keep a button always selected so that it can switched
             (scrolled to other buttons) by input system suporting multi-platform
             using event-system's button navigation (default: automatic) which
             can be visualized in button component in inspector */
        }

        private void HideGamePauseScreen()
        {
            parent.gameObject.SetActive(false);
            optionsUI.HideOptionsMenu();
        }
    }
}