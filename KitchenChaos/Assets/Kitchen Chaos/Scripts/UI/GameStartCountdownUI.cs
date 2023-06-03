using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace KC
{
    public class GameStartCountdownUI : MonoBehaviour
    {
        private const string NUMBER_POPUP = "NumberPopup";

        [SerializeField] TextMeshProUGUI countdownText;
        [SerializeField] Animator countdownAnimator;
        [SerializeField] Transform Parent;

        private int previousCountDownNum;

        private void Start()
        {
            GameManager.Instance.OnGameStateChanged += HandleCountdownUIOnGameStateChanged;
            if (Parent == null)
            {
                Parent = transform;
                HideCountdown();
            }
        }

        private void Update()
        {
            if (GameManager.Instance.IsCountdownActive)
            {
                int countdownTimer = GameManager.Instance.GetCountdownTimerValue();
                countdownText.text = countdownTimer.ToString();

                if (previousCountDownNum != countdownTimer)
                {
                    previousCountDownNum = countdownTimer;
                    countdownAnimator.SetTrigger(NUMBER_POPUP);
                    SoundManager.Instance.PlayCountdownSound();
                }

                if (countdownTimer <= 0)
                    enabled = false;
            }
        }

        private void HandleCountdownUIOnGameStateChanged(object sender, EventArgs e)
        {
            if (GameManager.Instance.IsCountdownActive)
                ShowCountdown();
            else
                HideCountdown();
        }

        private void ShowCountdown()
        {
            enabled = true;
            Parent.gameObject.SetActive(true);
        }

        private void HideCountdown()
        {
            Parent.gameObject.SetActive(false);
        }

    }
}