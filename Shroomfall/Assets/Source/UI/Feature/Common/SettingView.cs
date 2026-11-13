using Assets.Source.Core;
using Assets.Source.Enum;
using Assets.Source.Service;
using Assets.Source.UI.Component.Shared;
using Contract;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Source.UI.Feature.Common
{
    public class SettingView : MonoBehaviour
    {
        #region Attributes
        [Header("Root")]
        [SerializeField] private GameObject canvas;

        [Header("Buttons")]
        [SerializeField] private Button englishButton;
        [SerializeField] private Button vietnameseButton;
        [SerializeField] private Button closeButton;

        [Header("Discrete Sliders")]
        [SerializeField] private DiscreteSlider musicSoundSlider;
        [SerializeField] private DiscreteSlider ambientSoundSlider;
        [SerializeField] private DiscreteSlider uiSoundSlider;

        [Header("Slide Toggles")]
        [SerializeField] private SlideToggle musicSlideToggle;

        [Header("Dropdowns")]
        [SerializeField] private TMP_Dropdown screenSizeDropdown;
        private List<string> screenSizeLocalizedKeyOpts = new List<string>
        {
            "setting-view.screen-size-opt-small",
            "setting-view.screen-size-opt-medium",
            "setting-view.screen-size-opt-full"
        };

        [SerializeField] private TMP_Dropdown screenEffectDropdown;
        private List<string> screenEffectLocalizedKeyOpts = new List<string>
        {
            "setting-view.screen-effect-opt-leaf",
            "setting-view.screen-effect-opt-flower",
            "setting-view.screen-effect-opt-mushroom",
            "setting-view.screen-effect-opt-none"
        };

        private UIService uiService;
        private LocalizeService localizeService;
        private AudioService audioService;
        private ScreenService screenService;

        public static SettingView Instance { get; private set; }
        #endregion

        #region Properties
        #endregion

        #region Methods
        public void Awake()
        {
            // Singleton pattern & DontDestroyOnLoad logic
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(transform.root.gameObject); // Keeps the entire root Canvas/GameObject alive
            }
            else
            {
                Destroy(transform.root.gameObject);
                return;
            }

            uiService = ServiceProvider.Get<UIService>();
            localizeService = ServiceProvider.Get<LocalizeService>();
            audioService = ServiceProvider.Get<AudioService>();
            screenService = ServiceProvider.Get<ScreenService>();

            // UI Service
            uiService.OnSettingRequested += Open;
            uiService.OnUIStateChanged += Close;
            uiService.OnGlobalInteractableChanged += SetInteractable;

            // Localize Service
            localizeService.OnLocaleChanged += RefreshLocalizedDropdowns;

            // Buttons
            englishButton.onClick.AddListener(OnEnglishClicked);
            vietnameseButton.onClick.AddListener(OnVietnameseClicked);
            closeButton.onClick.AddListener(OnClose);

            // Discrete Sliders
            musicSoundSlider.OnValueChanged += OnMusicVolumeChanged;
            ambientSoundSlider.OnValueChanged += OnAmbientVolumeChanged;
            uiSoundSlider.OnValueChanged += OnUIVolumeChanged;

            // Slide Toggles
            musicSlideToggle.OnValueChanged += OnMusicToggleChanged;

            // Dropdowns
            screenSizeDropdown.onValueChanged.AddListener(OnScreenSizeChanged);
            screenEffectDropdown.onValueChanged.AddListener(OnScreenEffectChanged);

            canvas.SetActive(false);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                // UI Service
                uiService.OnSettingRequested -= Open;
                uiService.OnUIStateChanged -= Close;
                uiService.OnGlobalInteractableChanged -= SetInteractable;

                // Localize Service
                localizeService.OnLocaleChanged -= RefreshLocalizedDropdowns;

                // Buttons
                englishButton.onClick.RemoveListener(OnEnglishClicked);
                vietnameseButton.onClick.RemoveListener(OnVietnameseClicked);
                closeButton.onClick.RemoveListener(OnClose);

                // Discrete Sliders
                musicSoundSlider.OnValueChanged -= OnMusicVolumeChanged;
                ambientSoundSlider.OnValueChanged -= OnAmbientVolumeChanged;
                uiSoundSlider.OnValueChanged -= OnUIVolumeChanged;

                // Slide Toggles
                musicSlideToggle.OnValueChanged -= OnMusicToggleChanged;

                // Dropdowns
                screenSizeDropdown.onValueChanged.RemoveListener(OnScreenSizeChanged);
                screenEffectDropdown.onValueChanged.RemoveListener(OnScreenEffectChanged);
            }
        }

        public void Open()
        {
            canvas.SetActive(true);
            ApplyServiceValuesToUI();
        }

        public void Close(
            UIService service)
        {
            canvas.SetActive(false);
        }

        private void SetInteractable(
            bool interactable)
        {
            englishButton.interactable = interactable;
            vietnameseButton.interactable = interactable;
            musicSoundSlider.enabled = interactable;
            ambientSoundSlider.enabled = interactable;
            uiSoundSlider.enabled = interactable;
            screenSizeDropdown.interactable = interactable;
            screenEffectDropdown.interactable = interactable;
        }

        private void ApplyServiceValuesToUI()
        {
            // Audio
            if (audioService != null)
            {
                musicSoundSlider.SetValueWithoutNotify(audioService.MusicVolume);
                ambientSoundSlider.SetValueWithoutNotify(audioService.AmbientVolume);
                uiSoundSlider.SetValueWithoutNotify(audioService.UIVolume);
                musicSlideToggle.SetValueWithoutNotify(audioService.IsMusicPlaying);
            }

            // Dropdowns
            RefreshLocalizedDropdowns();
        }

        private void RefreshLocalizedDropdowns()
        {
            RefreshDropdownOptions(screenSizeDropdown, screenSizeLocalizedKeyOpts);
            RefreshDropdownOptions(screenEffectDropdown, screenEffectLocalizedKeyOpts);
        }

        private void OnEnglishClicked()
        {
            localizeService.SetLocale(Constraint.DEFAULT_LOCALE);
        }

        private void OnVietnameseClicked()
        {
            localizeService.SetLocale(Constraint.VIETNAMESE_LOCALE);
        }

        public void OnClose()
        {
            canvas.SetActive(false);
        }

        private void OnMusicVolumeChanged(
            float value)
        {
            audioService.SetMusicVolume(value);
        }

        private void OnAmbientVolumeChanged(
            float value)
        {
            audioService.SetAmbientVolume(value);
        }

        private void OnUIVolumeChanged(
            float value)
        {
            audioService.SetUIVolume(value);
        }

        private void OnMusicToggleChanged(
            bool isOn)
        {
            if (isOn)
                audioService.PlayMusic();
            else
                audioService.StopMusic();
        }

        private void OnScreenSizeChanged(
            int index)
        {
            screenService.SetScreenSize(GetScreenPreset(index));
        }

        private void OnScreenEffectChanged(
            int index)
        {
            screenService.SetScreenEffect(index);
        }

        private void RefreshDropdownOptions(
            TMP_Dropdown dropdown,
            List<string> localizedKeys)
        {
            if (dropdown == null || localizedKeys == null)
                return;

            int previousIndex = dropdown.value;
            var options = new List<TMP_Dropdown.OptionData>();
            foreach (string key in localizedKeys)
            {
                string localizedText = string.IsNullOrEmpty(key)
                    ? string.Empty : localizeService != null
                    ? localizeService.Get(key) : key;
                options.Add(new TMP_Dropdown.OptionData(localizedText));
            }

            dropdown.ClearOptions();
            dropdown.AddOptions(options);

            if (options.Count == 0)
            {
                dropdown.SetValueWithoutNotify(0);
                dropdown.RefreshShownValue();
                return;
            }

            int selectedIndex = Mathf.Clamp(previousIndex, 0, options.Count - 1);
            dropdown.SetValueWithoutNotify(selectedIndex);
            dropdown.RefreshShownValue();
        }

        private ScreenPreset GetScreenPreset(
            int index)
        {
            switch (index)
            {
                case 0:
                    return ScreenPreset.Small;

                case 1:
                    return ScreenPreset.Medium;

                case 2:
                default:
                    return ScreenPreset.Full;
            }
        }
        #endregion
    }
}