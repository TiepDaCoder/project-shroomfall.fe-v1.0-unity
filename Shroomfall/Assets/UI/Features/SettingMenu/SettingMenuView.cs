using Assets.Enums;
using Assets.UI.Models;
using Assets.Utilities;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingMenuView : MonoBehaviour
{
    #region Attributes
    [Header("Selector")]
    [SerializeField] private Selector localeSelector;
    [SerializeField] private Selector screenSelector;

    [Header("Screen Icons")]
    [SerializeField] private Sprite smallScreenIcon;
    [SerializeField] private Sprite mediumScreenIcon;
    [SerializeField] private Sprite fullScreenIcon;

    [Header("Buttons")]
    [SerializeField] private Button backButton;

    [Header("Labels")]
    [SerializeField] private TMP_Text screenLabel;
    [SerializeField] private TMP_Text localeLabel;
    #endregion

    #region Properties
    public event Action OnBackClicked;
    public event Action<string, bool> OnScreenSelectionChanged;
    public event Action<string, bool> OnLocaleSelectionChanged;
    public event Action OnSettingMenuShown;
    #endregion

    #region Methods
    private void Awake()
    {
        // Buttons
        backButton.onClick.AddListener(() => { OnBackClicked?.Invoke(); });
    }

    public void SetVisible(
        bool visible)
    {
        gameObject.SetActive(visible);

        if (visible)
        {
            OnSettingMenuShown?.Invoke();
            RefreshLocalizedText();
        }
    }

    public void BindScreenSelector(
        string currentScreen)
    {
        var screens = new List<IconModel>
        {
            new()
            {
                Id = ScreenPreset.Small.ToString(),
                Icon = smallScreenIcon,
                Name = "Small Screen",
                Description = "1280 x 720"
            },

            new()
            {
                Id = ScreenPreset.Medium.ToString(),
                Icon = mediumScreenIcon,
                Name = "Medium Screen",
                Description = "1600 x 900"
            },

            new()
            {
                Id = ScreenPreset.Full.ToString(),
                Icon = fullScreenIcon,
                Name = "Fullscreen",
                Description = "1920 x 1080"
            }
        };


        screenSelector.Bind(
            screens,
            currentScreen
        );


        screenSelector.OnItemToggled += (model, isSelected) => OnScreenSelectionChanged?.Invoke(model.Id, isSelected);
    }

    public void BindLocaleIcons(
        List<IconModel> locale,
        string currentLocale)
    {
        localeSelector.Bind(locale, currentLocale);
        localeSelector.OnItemToggled += (model, isSelected) => { OnLocaleSelectionChanged?.Invoke(model.Id, isSelected); RefreshLocalizedText(); };
    }

    public void SetInteractable(
        bool interactable)
    {
        backButton.interactable = interactable;
    }

    private void RefreshLocalizedText()
    {
        // Labels
        screenLabel.text = UILocalizationTable.Get("setting-menu.label-screen");
        localeLabel.text = UILocalizationTable.Get("setting-menu.label-locale");
    }
    #endregion
}