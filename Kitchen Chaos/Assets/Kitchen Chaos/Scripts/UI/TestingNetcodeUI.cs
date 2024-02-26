using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace KC
{
    public class TestingNetcodeUI : MonoBehaviour
    {
        [SerializeField] private Button startHostBtn;
        [SerializeField] private Button startClientBtn;
        [SerializeField] private CanvasGroup netcodeCanvasGroup;
        [SerializeField] private float maxRetryConnTimer = 3;
        private float retryConnTimer;

        private void Awake()
        {
            if (netcodeCanvasGroup == null)
                netcodeCanvasGroup = GetComponent<CanvasGroup>();

            startHostBtn.onClick.AddListener(() =>
            {
                this.Log("HOST!");
                MultiplayerManager.Singleton.StartHost();
                //StartCoroutine(CheckConnection());
            });

            startClientBtn.onClick.AddListener(() =>
            {
                this.Log("CLIENT!");
                MultiplayerManager.Singleton.StartClient();
                //StartCoroutine(CheckConnection());
            });

            NetworkManager.Singleton.OnClientStarted += Hide;
        }
        private void Start()
        {
            if (GameManager.Instance.Testing)
            {
                NetworkManager.Singleton.OnClientStopped += (_) => Show();
                NetworkManager.Singleton.OnTransportFailure += Show;
            }
        }

        private IEnumerator CheckConnection()
        {
            Hide();
            retryConnTimer = 0f;
            while (retryConnTimer < maxRetryConnTimer)
            {
                if (NetworkManager.Singleton.IsListening)
                    yield break;
                yield return new WaitForEndOfFrame();
                retryConnTimer += Time.deltaTime;
            }
            Show();
        }

        private void Hide()
        {
            netcodeCanvasGroup.alpha = 0f;
            netcodeCanvasGroup.blocksRaycasts = false;
        }
        private void Show()
        {
            netcodeCanvasGroup.alpha = 1f;
            netcodeCanvasGroup.blocksRaycasts = true;
        }
    }
}