using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KC
{
    public class SelectedVisuals : MonoBehaviour
    {
        [SerializeField] private BaseCounter counter;
        [SerializeReference] private GameObject selectedVisualObject;

        private void Start()
        {
            counter ??= GetComponent<BaseCounter>();
            //Debug.Log(counter);
            PlayerController.Instance.OnSelectedCounterChanged += HandleCounterSelectionVisually;
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