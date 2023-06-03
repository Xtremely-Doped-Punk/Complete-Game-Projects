using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KC
{
    public class SelectedVisuals : MonoBehaviour
    {
        [SerializeField] private BaseCounter counter;
        [SerializeReference] private GameObject selectedVisualObject;

        private bool isInitialized;

        private void Start()
        {
            if (counter == null) 
                counter = GetComponent<BaseCounter>();

            if (PlayerController.LocalInstance != null)
                ListenPlayerEvents();
            else
                PlayerController.OnAnyPlayerSpawned += SetupSelectedVisualsOnAnyPlayerSpawned;
        }

        private void SetupSelectedVisualsOnAnyPlayerSpawned(object sender, System.EventArgs e)
        {
            if (isInitialized)
            {
                PlayerController.OnAnyPlayerSpawned -= SetupSelectedVisualsOnAnyPlayerSpawned;
                return;
            }
            else if (PlayerController.LocalInstance != null)
            {
                PlayerController.OnAnyPlayerSpawned -= SetupSelectedVisualsOnAnyPlayerSpawned;
                ListenPlayerEvents();
            }
        }

        private void ListenPlayerEvents()
        {
            isInitialized = true;
            PlayerController.LocalInstance.OnSelectedCounterChanged += HandleCounterSelectionVisually;
        }

        private void HandleCounterSelectionVisually(object sender, PlayerController.SelectedCounterChangedEventArgs e)
        {
            if (e.SelectedCounter == counter)
                ShowVisual();
            else
                HideVisual();
        }

        private void HideVisual()
        {
            selectedVisualObject.SetActive(false);
        }

        private void ShowVisual()
        {
            selectedVisualObject.SetActive(true);
        }
    }
}