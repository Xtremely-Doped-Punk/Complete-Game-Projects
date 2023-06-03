using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KC
{
    public class TutorialUI : MonoBehaviour
    {
        [SerializeField] Transform LoadingTransform;
        [SerializeField] Transform Parent;
        private void Awake()
        {
            if (Parent == null)
            {
                Parent = transform;
            }
        }

        private void Start()
        {
            ShowTutorialScreen(); ShowLoading();
            GameManager.Instance.OnGameStateChanged += HandleTutorialUIOnGameStateChanged;
        }

        private void HandleTutorialUIOnGameStateChanged(object sender, System.EventArgs e)
        {
            if (GameManager.Instance.IsReadyToPlay)
                HideLoading();
            else if (GameManager.Instance.IsCountdownActive)
                HideTutorialScreen();
        }

        public void ShowTutorialScreen() => Parent.gameObject.SetActive(true);
        public void HideTutorialScreen() => Parent.gameObject.SetActive(false);
        public void ShowLoading() => LoadingTransform.gameObject.SetActive(true);
        public void HideLoading() => LoadingTransform.gameObject.SetActive(false);
    }
}