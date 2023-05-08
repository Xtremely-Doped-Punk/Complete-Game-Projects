using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace KC
{
    public class DisplayBindingUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI keyBtnText;
        [SerializeField] InputManager.Platform platform;
        [SerializeField] InputManager.Binding binding;

        private void Awake()
        {
            if (keyBtnText == null)
            {
                keyBtnText = GetComponent<TextMeshProUGUI>();
                if (keyBtnText == null)
                    keyBtnText = GetComponentInChildren<TextMeshProUGUI>();
            }
        }

        void Start()
        {
            HandleDisplayBindingOnBindingRebinded(null, System.EventArgs.Empty);
            InputManager.Instance.OnBindingRebinded += HandleDisplayBindingOnBindingRebinded;
        }

        private void HandleDisplayBindingOnBindingRebinded(object sender, System.EventArgs e)
        {
            keyBtnText.text = InputManager.Instance.GetBindingValueText(platform, binding);
        }
    }
}