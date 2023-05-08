using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KC
{
    public class OptionsBindingSingleUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI actionText;
        [SerializeField] private BindingKeySingleUI bindingKeyUITemplate;
        [SerializeField] private Transform keyBtnsParentTransform;

        //private BindingKeySingleUI[] keyBtns;
        public void InitializeBindingUI(OptionsUI optionsUI, string action, InputManager.Platform[] platforms, InputManager.Binding binding)
        {
            if (platforms == null || platforms.Length == 0)
            {
                Destroy(gameObject); return;
            }

            actionText.text = action;
            //keyBtns = new BindingKeySingleUI[platforms.Length];

            for (int i=0; i<platforms.Length; i++)
            {
                BindingKeySingleUI bindinfKeyInstance = Instantiate(bindingKeyUITemplate, keyBtnsParentTransform);
                
                bindinfKeyInstance.InitializeBindingKeyUI(optionsUI,
                    InputManager.Instance.GetBindingValueText(platforms[i], binding),
                    platforms[i], binding);

                //keyBtns[i] = bindinfKeyInstance;
            }

            bindingKeyUITemplate.gameObject.SetActive(false);
        }
    }
}