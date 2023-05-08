using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace KC
{
    public class Visual_CounterStove : MonoBehaviour
    {
        private const string IS_FLASHING = "IsFlashing";

        [SerializeField] private GameObject[] fryingVisualGameObjects;
        [SerializeField] private CounterStove counterStove;
        [SerializeField] private ProgressBarUI progressBar;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private Transform stoveBurningWarningUI;
        [SerializeField, Range(0, 1)] private float burnWarningShowThresholdNormalized = 0.5f;
        [SerializeField, Range(0, .5f)] private float warningSoundTimerDelay = 0.2f;
        [SerializeField] private Animator stoveBurnFlashingBarAnimator;

        private bool shouldWarn;
        private float warningSoundTimer;

        private void Awake()
        {
            if (counterStove == null)
                Debug.Log("Counter Stove reference not set to Visual Script:" + this);

            if (audioSource == null)
                audioSource = counterStove.GetHolderTransform().gameObject.AddComponent<AudioSource>();

            progressBar.SetHasProgressBarReference(counterStove);
        }

        private void Start()
        {
            counterStove.OnStoveStateChanged += HandleStoveVisualsOnStoveStateChanged;
            counterStove.OnProgessChanged += HandleStoveVisualsOnProgessChanged;
            audioSource.clip = SoundManager.Instance.AudioClipRefsSO.StoveSizzle;
        }

        private void Update()
        {
            if (!GameManager.Instance.IsGamePlaying || !shouldWarn) return;

            warningSoundTimer -= Time.deltaTime;
            if (warningSoundTimer < 0f)
            {
                warningSoundTimer = warningSoundTimerDelay;
                SoundManager.Instance.PlayWarningSound(counterStove.GetHolderTransform().position);
            }
        }

        private void HandleStoveVisualsOnProgessChanged(object sender, IHasProgressBar.ProgessChangedEventArg e)
        {
            bool shouldWarn = counterStove.IsGoingToBurn && 
                (e.progressNormalized >= burnWarningShowThresholdNormalized && e.progressNormalized < 1f);

            if (this.shouldWarn == shouldWarn) return;

            this.shouldWarn = shouldWarn;
            stoveBurningWarningUI.gameObject.SetActive(shouldWarn);
            stoveBurnFlashingBarAnimator.SetBool(IS_FLASHING, shouldWarn);
        }

        private void HandleStoveVisualsOnStoveStateChanged(object sender, EventArgs e)
        {
            bool isFrying = counterStove.IsFrying;

            VisualsSetActive(isFrying);
            progressBar.gameObject.SetActive(isFrying);

            if (isFrying)
            { 
                if (!audioSource.isPlaying) 
                    audioSource.Play();
            }
            else
                audioSource.Pause();
        }

        private void VisualsSetActive(bool active)
        {
            foreach(GameObject visualGameObj in fryingVisualGameObjects)
            {
                visualGameObj.SetActive(active);
            }
        }
    }
}