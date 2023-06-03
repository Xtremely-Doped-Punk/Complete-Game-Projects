using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KC
{
    public class OptionsUI : MonoBehaviour
    {
        [SerializeField] private Button soundFXBtn;
        [SerializeField] private Button musicBtn;
        [SerializeField] private Button closeBtn;
        [SerializeField] private TextMeshProUGUI soundFXBBtnText;
        [SerializeField] private TextMeshProUGUI musicBtnText;
        [SerializeField] private float volumeMax = 10f;
        [SerializeField] private OptionsBindingSingleUI bindingTemplate;
        [SerializeField] private Transform bindingParentTransform;
        [SerializeField] private Transform pressToRebindKeyTransform;
        [SerializeField] Transform Parent;

        private OptionsBindingSingleUI[] bindings;
        private Action OnCloseOptionsMenu;

        private void Awake()
        {
            if (Parent == null)
            {
                Parent = transform;
                HideOptionsMenu();
            }

            soundFXBtn.onClick.AddListener(() =>
            {
                SoundManager.Instance.ChangeVolume(1f / volumeMax);
                UpdateSoundFXBtnVisual();
            });

            musicBtn.onClick.AddListener(() =>
            {
                MusicManager.Instance.ChangeVolume(1f / volumeMax);
                UpdateMusicBtnVisual();
            });

            closeBtn.onClick.AddListener(() =>
            {
                HideOptionsMenu();
                OnCloseOptionsMenu?.Invoke();
            });
        }

        private void Start()
        {
            UpdateSoundFXBtnVisual(); UpdateMusicBtnVisual();

            // initialize binding remaping options
            SetupBindingReMapListeners();
        }

        private void SetupBindingReMapListeners()
        {
            InputManager.Binding[] bindings = InputManager.GetAllBindings();
            InputManager.Platform[] platforms = InputManager.GetAllPlatforms();

            string[] bindingTypes = InputManager.Instance.GetAllBindingTypeTexts();

            this.bindings = new OptionsBindingSingleUI[bindingTypes.Length];
            for (int i = 0; i < bindingTypes.Length; i++)
            {
                InputManager.Platform[] keyPlatforms = platforms;

                if (bindings[i] == InputManager.Binding.MoveUp ||
                    bindings[i] == InputManager.Binding.MoveDown ||
                    bindings[i] == InputManager.Binding.MoveLeft ||
                    bindings[i] == InputManager.Binding.MoveRight)
                    keyPlatforms = new InputManager.Platform[] { InputManager.Platform.PC, }; 
                // keep only pc option for movement key binds

                this.bindings[i] = Instantiate(bindingTemplate, bindingParentTransform);
                this.bindings[i].InitializeBindingUI(this,
                    bindingTypes[i],
                    keyPlatforms,
                    bindings[i]);
            }
            bindingTemplate.gameObject.SetActive(false);
        }

        private void UpdateMusicBtnVisual() =>
            musicBtnText.text = "Music: " + Mathf.RoundToInt(MusicManager.Instance.GetVolume() * volumeMax).ToString();

        private void UpdateSoundFXBtnVisual() =>
            soundFXBBtnText.text = "Sound Effects: " + Mathf.RoundToInt(SoundManager.Instance.GetVolume() * volumeMax).ToString();

        public void ShowOptionsMenu(Action OnCloseButtonAction = null)
        {
            Parent.gameObject.SetActive(true);
            closeBtn.Select(); // always keep one button selected in a menu to support multi-platform
            OnCloseOptionsMenu = OnCloseButtonAction;
        }
        public void HideOptionsMenu() => Parent.gameObject.SetActive(false);

        public void ShowPressToRebindKeyScreen() => pressToRebindKeyTransform.gameObject.SetActive(true);
        public void HidePressToRebindKeyScreen() => pressToRebindKeyTransform.gameObject.SetActive(false);
    }
}