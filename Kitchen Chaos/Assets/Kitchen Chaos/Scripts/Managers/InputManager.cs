using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.Windows;
using static KC.InputManager;

namespace KC
{
    public class InputManager : MonoBehaviour
    {
        private const string PLAYER_PREFS_BINDINGS = "InputBindings";

        public static InputManager Instance { get; private set; } = null;
        public event EventHandler OnPrimaryInteractAction;
        public event EventHandler OnSecondaryInteractAction;
        public event EventHandler OnInventoryInteractAction;
        public event EventHandler OnPauseAction;
        public event EventHandler OnBindingRebinded;

        public enum Binding
        {
            MoveUp,
            MoveDown,
            MoveLeft,
            MoveRight,
            PrimaryInteract,
            SecondaryInteract,
            InventoryView,
            Pause,
        }

        // make sure to keep the platform based binding indexes in this order (or update the order here)
        public enum Platform { PC = 0, Gamepad = 1, }

        private IA_KitchenChaos InpAct; // short for input-action-map

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else if (Instance != this)
                Destroy(this);

            InpAct = new();
            if (PlayerPrefs.HasKey(PLAYER_PREFS_BINDINGS)) // load previously made override rebindings if exists
                InpAct.LoadBindingOverridesFromJson(PlayerPrefs.GetString(PLAYER_PREFS_BINDINGS));

            InpAct.Enable();
            InpAct.Player.Enable();

            InpAct.Player.PrimaryInteract.performed += PlayerPrimaryInteraction;
            InpAct.Player.SecondaryInteract.performed += PlayerSecondaryInteraction;
            InpAct.Player.InventoryView.performed += PlayerInventoryView;
            InpAct.Player.Pause.performed += PlayerPause;
        }

        private void PlayerPrimaryInteraction(InputAction.CallbackContext obj) 
        {
            //this.Log($"PlayerPrimaryInteraction input triggered, IsNull:{OnPrimaryInteractAction==null} InvocationList.Length:{OnPrimaryInteractAction.GetInvocationList().Length}");
            OnPrimaryInteractAction?.Invoke(this, EventArgs.Empty); 
        }
        private void PlayerSecondaryInteraction(InputAction.CallbackContext obj) => OnSecondaryInteractAction?.Invoke(this, EventArgs.Empty);
        private void PlayerInventoryView(InputAction.CallbackContext obj) => OnInventoryInteractAction?.Invoke(this, EventArgs.Empty);
        private void PlayerPause(InputAction.CallbackContext obj) => OnPauseAction?.Invoke(this, EventArgs.Empty);

        private void OnDestroy()
        {
            // unsubcribe to input actions when scene gets changed
            // to avoid null-ref error, as the object IA_KitchenChaos() remains still active

            //InpAct.Disable();
            //Note: InputActionTrace allocates unmanaged memory and needs to be disposed of so that it doesn't create memory leaks.
            // thus better to dispose, rather than disable, also better than unsubscribing every callback
            // just to super safe, unsubscribing is better to ensure it gets collected by garbage collector
            InpAct.Player.PrimaryInteract.performed -= PlayerPrimaryInteraction;
            InpAct.Player.SecondaryInteract.performed -= PlayerSecondaryInteraction;
            InpAct.Player.InventoryView.performed -= PlayerInventoryView;
            InpAct.Player.Pause.performed -= PlayerPause;

            // also unlike static events, apparently actions cant be set to null in order to reset

            InpAct.Dispose();
        }

        public Vector2 TickMovementVectorNormalized()
        {
            #region Old Input System
            /*
            Vector2 dirVec = Vector2.zero;

            if (Input.GetKey(KeyCode.W))
            {
                dirVec.y += 1;
            }
            if (Input.GetKey(KeyCode.A))
            {
                dirVec.x += -1;
            }
            if (Input.GetKey(KeyCode.S))
            {
                dirVec.y += -1;
            }
            if (Input.GetKey(KeyCode.D))
            {
                dirVec.x += 1;
            }

            dirVec = dirVec.normalized;
            */
            #endregion

            #region New Input System

            Vector2 dirVec = InpAct.Player.Movement.ReadValue<Vector2>();
            // normalize processor added to input action itself

            return dirVec;

            #endregion
        }

        public void RebindBinding(Platform platform, Binding binding, Action onActionRebounded = null)
        {
            InpAct.Player.Disable(); // disable before remapping

            InputAction inputAction = GetInputAction(binding);
            int binding_index = GetBindingIndex(inputAction, platform, binding);

            this.Log("Rebinding started for platform:" + platform + " ,binding:" + binding);

            inputAction.PerformInteractiveRebinding(binding_index).OnComplete(callback =>
            {
                // callback when action is rebinded (here we are passing callback as lambda fn)
                this.Log("Previous Binding: " + callback.action.bindings[binding_index].path);
                this.Log("Current Binding: " + callback.action.bindings[binding_index].overridePath);

                /* in previous version of input system, 
                after rebinding, callback need to be manually disposed orelse it might throw memory leak error
                in newer version, it automatically disposes itself, (just to be safe, dispose it manually) */
                callback.Dispose();

                InpAct.Player.Enable(); // finally enable after succesful rebinding

                onActionRebounded?.Invoke();
                // action can hold a fun callback (useful when we dont want to fire up event and
                // make a list of subscribers but only to invoke a simple fn call)

                string inputBindingOverides = InpAct.SaveBindingOverridesAsJson(); // returns all the overides made as in json formatted string
                // that can saved as json file, so that whevev game reloads, the settings remain the same as previously edited
                PlayerPrefs.SetString(PLAYER_PREFS_BINDINGS, inputBindingOverides);
                PlayerPrefs.Save();

                OnBindingRebinded?.Invoke(this, EventArgs.Empty);
            }
            ).Start();
            // probably like coroutine, that invokes the callbacks fn passsed when its operation is completed
        }

        private int GetBindingIndex(InputAction inputAction, Platform platform, Binding binding)
        {
            int binding_index = (int)platform;

            /* note for composite bindings, 
            the composite 2d vector is present nth, then, n+1, n+2, n+3, n+4, represent each indiviual key-binding of the composite,
            for example: composite_binding[0] = WASD, then binding[1]='W', binding[2]='A', binding[3]='S', binding[4]='D',
            */
            if (inputAction.bindings[binding_index].isComposite)
            {
                switch (binding)
                {
                    case Binding.MoveUp:
                        binding_index += 1; break;

                    case Binding.MoveDown:
                        binding_index += 2; break;

                    case Binding.MoveLeft:
                        binding_index += 3; break;

                    case Binding.MoveRight:
                        binding_index += 4; break;
                }
            }

            return binding_index;
        }

        private InputAction GetInputAction(Binding binding)
        {
            if (binding == Binding.MoveUp || binding == Binding.MoveDown || binding == Binding.MoveLeft || binding == Binding.MoveRight)
                return InpAct.Player.Movement;

            switch (binding)
            {
                case Binding.PrimaryInteract:
                    return InpAct.Player.PrimaryInteract;

                case Binding.SecondaryInteract:
                    return InpAct.Player.SecondaryInteract;

                case Binding.InventoryView:
                    return InpAct.Player.InventoryView;

                case Binding.Pause:
                    return InpAct.Player.Pause;

                default:
                    return null;
            }
        }

        public string GetBindingValueText(Platform platform, Binding binding)
        {
            InputAction inputAction = GetInputAction(binding);
            if (inputAction == null)
            {
                this.LogError("Input-Action not defined for given binding:" + binding);
                return null;
            }
            else
            {
                int binding_index = GetBindingIndex(inputAction, platform, binding);
                return inputAction.bindings[binding_index].ToDisplayString();
            }
        }

        public string[] GetAllBindingValueTexts(Platform platform)
        {
            Binding[] bindings = GetAllBindings();
            string[] bindingTexts = new string[bindings.Length];

            for (int i=0; i<bindings.Length; i++)
            {
                bindingTexts[i] = GetBindingValueText(platform, bindings[i]);
            }

            return bindingTexts;
        }

        public string[] GetAllBindingTypeTexts()
        {
            string[] bindingValues =  Enum.GetNames(typeof(Binding));

            for (int i = 0; i < bindingValues.Length; i++)
            {
                bindingValues[i] = AddSpacesToSentence(bindingValues[i], false);
            }
            return bindingValues;
        }

        private static string AddSpacesToSentence(string text, bool preserveAcronyms)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;
            StringBuilder newText = new StringBuilder(text.Length * 2);
            newText.Append(text[0]);
            for (int i = 1; i < text.Length; i++)
            {
                if (char.IsUpper(text[i]))
                    if ((text[i - 1] != ' ' && !char.IsUpper(text[i - 1])) ||
                        (preserveAcronyms && char.IsUpper(text[i - 1]) &&
                         i < text.Length - 1 && !char.IsUpper(text[i + 1])))
                        newText.Append(' ');
                newText.Append(text[i]);
            }
            return newText.ToString();
        }

        public static Binding[] GetAllBindings()
        {
            var bindingVals = Enum.GetValues(typeof(Binding));
            Binding[] bindings = new Binding[bindingVals.Length];
            int i = 0;
            foreach (var bindingVal in bindingVals)
            {
                bindings[i] = (Binding)bindingVal;
                i++;
            }
            return bindings;
        }
        public static Platform[] GetAllPlatforms()
        {
            var platformVals = Enum.GetValues(typeof(Platform));
            Platform[] platforms = new Platform[platformVals.Length];
            int i = 0;
            foreach (var bindingVal in platformVals)
            {
                platforms[i] = (Platform)bindingVal;
                i++;
            }
            return platforms;
        }
    }
}