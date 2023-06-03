using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KC
{
    public class BindingKeySingleUI : MonoBehaviour
    {
        [SerializeField] private Button keyBtn;
        [SerializeField] private TextMeshProUGUI keyBtnText;

        public void InitializeBindingKeyUI(OptionsUI optionsUI, string keyText, 
            InputManager.Platform platform, InputManager.Binding binding)
        {
            keyBtnText.text = keyText;

            if (keyBtn == null) return;

            keyBtn.onClick.AddListener(() =>
            {
                optionsUI.ShowPressToRebindKeyScreen();
                InputManager.Instance.RebindBinding(platform, binding, () =>
                {
                    optionsUI.HidePressToRebindKeyScreen();
                    keyBtnText.text = InputManager.Instance.GetBindingValueText(platform, binding);
                });
            });
        }

        public void SetBtnNavigation(Navigation navigation) => keyBtn.navigation = navigation;

        public void SetExplicitBtnNavigation(Selectable onUp=null, Selectable onDown=null, Selectable onLeft=null, Selectable onRight=null)
        {
            Navigation navigation = keyBtn.navigation;
            
            // switch mode to Explicit to allow for custom assigned behavior
            navigation.mode = Navigation.Mode.Explicit;
            
            navigation.selectOnUp = onUp;
            navigation.selectOnDown = onDown;
            navigation.selectOnLeft = onLeft;
            navigation.selectOnRight = onRight;

            // reassign the struct data to the button
            keyBtn.navigation = navigation;
        }

        public void RemoveBtnNavigationPaths(bool up=false, bool down=false, bool left=false, bool right=false)
        {
            Navigation btnNav = keyBtn.navigation;
            Navigation navigation = Navigation.defaultNavigation;

            // switch mode to Explicit to allow for custom assigned behavior
            navigation.mode = Navigation.Mode.Explicit;

            if (up) navigation.selectOnUp = null;
            else navigation.selectOnUp = btnNav.selectOnUp;

            if (down) navigation.selectOnDown = null;
            else navigation.selectOnDown = btnNav.selectOnDown;

            if (left) navigation.selectOnLeft = null;
            else navigation.selectOnLeft = btnNav.selectOnLeft;

            if (right) navigation.selectOnRight = null;
            else navigation.selectOnRight = btnNav.selectOnRight;

            keyBtn.navigation = navigation;
        }
    }
}