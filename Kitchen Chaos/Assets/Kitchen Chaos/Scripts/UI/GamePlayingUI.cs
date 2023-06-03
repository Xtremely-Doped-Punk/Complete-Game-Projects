using System;
using UnityEngine;
using UnityEngine.UI;

namespace KC
{
    public class GamePlayingUI : MonoBehaviour
    {
        [SerializeField] Image gamePlayingTimerCLock;
        [SerializeField] bool reverseTimer = true;
        [SerializeField] Transform Parent;

        private void Start()
        {
            GameManager.Instance.OnGameStateChanged += HandleGamePlayingUIOnGameStateChanged;
            if (Parent == null )
            {
                Parent = transform;
                HideGamePlayingTimerClock();
            }
            gamePlayingTimerCLock.fillAmount = reverseTimer ? 1 : 0;
        }

        private void Update()
        {
            if (GameManager.Instance.IsGamePlaying)
            {
                float timerNormalized = GameManager.Instance.GetGamePlayingTimerNormalized();
                gamePlayingTimerCLock.fillAmount = reverseTimer ? timerNormalized : 1-timerNormalized;
                if (timerNormalized <= 0)
                    enabled = false;
            }
        }

        private void HandleGamePlayingUIOnGameStateChanged(object sender, EventArgs e)
        {
            if (GameManager.Instance.IsGamePlaying)
                ShowGamePlayerTimerCLock();
            else
                HideGamePlayingTimerClock();
        }

        private void ShowGamePlayerTimerCLock()
        {
            enabled = true;
            Parent.gameObject.SetActive(true);
        }

        private void HideGamePlayingTimerClock()
        {
            Parent.gameObject.SetActive(false);
        }
    }
}