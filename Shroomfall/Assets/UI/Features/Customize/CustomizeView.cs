using Assets.UI.Abstraction;
using Assets.Utilities;
using Contract.Enum.EntityDomain;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CustomizeView : MonoBehaviour, IHUDView
{
    #region Attributes
    [Header("Root")]
    [SerializeField] private GameObject root;

    [Header("Buttons")]
    [SerializeField] private TextButton submitButton;
    [SerializeField] private Button skinToLeftButton;
    [SerializeField] private Button skinToRightButton;

    [Header("Scrolls")]
    [SerializeField] private ScrollCollector skinScroll;

    [Header("Colors")]
    [SerializeField] private HSVCollector skinColorCollector;

    [Header("Labels")]
    [SerializeField] private TMP_Text nameLabel;
    [SerializeField] private TMP_Text dobLabel;

    [Header("Placeholders")]
    [SerializeField] private TMP_Text namePlaceholder;
    [SerializeField] private TMP_Text dobDayPlaceHolder;
    [SerializeField] private TMP_Text dobMonthPlaceHolder;
    [SerializeField] private TMP_Text dobYearPlaceHolder;

    [Header("Text Fields")]
    [SerializeField] private TMP_InputField nameTextField;
    [SerializeField] private TMP_InputField dobDayTextField;
    [SerializeField] private TMP_InputField dobMonthTextField;
    [SerializeField] private TMP_InputField dobYearTextField;

    [Header("Preview")]
    [SerializeField] private EntityPreviewView preview;
    #endregion

    #region Properties
    public event Action OnSubmitClicked;
    public event Action OnSkinToLeftClicked;
    public event Action OnSkinToRightClicked;

    public event Action<string> OnSkinChanged;

    public event Action<Color> OnSkinColorChanged;

    public event Action<string> OnNameChanged;
    public event Action<string> OnDobDayChanged;
    public event Action<string> OnDobMonthChanged;
    public event Action<string> OnDobYearChanged;

    public event Action OnViewShown;
    #endregion

    #region Methods
    private void Awake()
    {
        // Button
        submitButton.onClick.AddListener(() => OnSubmitClicked?.Invoke());
        skinToLeftButton.onClick.AddListener(() => OnSkinToLeftClicked?.Invoke());
        skinToRightButton.onClick.AddListener(() => OnSkinToRightClicked?.Invoke());

        // Scrolls
        skinScroll.OnValueChanged += v => OnSkinChanged?.Invoke(v);

        // Colors
        skinColorCollector.OnColorChanged += c => OnSkinColorChanged?.Invoke(c);

        // Text Fields
        nameTextField.onValueChanged.AddListener(v => OnNameChanged?.Invoke(v));
        dobDayTextField.onValueChanged.AddListener(v => OnDobDayChanged?.Invoke(v));
        dobMonthTextField.onValueChanged.AddListener(v => OnDobMonthChanged?.Invoke(v));
        dobYearTextField.onValueChanged.AddListener(v => OnDobYearChanged?.Invoke(v));
    }

    public void Show()
    {
        root.SetActive(true);
        OnViewShown?.Invoke();
        RefreshLocalizedText();
    }

    public void Hide()
    {
        root.SetActive(false);
    }

    public void ApplyCurrentSelection(
        string skinId,
        Color skinColor)
    {
        skinScroll.SetCurrentByID(skinId);
        skinColorCollector.SetCurrentByColor(skinColor);
    }

    public void SetSkinValues(
        List<ScrollValue> values, int startIndex = 0)
    {
        skinScroll.SetValues(values, startIndex);
    }

    public void SetSkinPreview(
        EntityAsset entity,
        Color color,
        EntityDirection direction)
    {
        preview.Apply(entity, color, direction);
    }

    public void SetInteractable(
        bool interactable)
    {
        submitButton.interactable = interactable;
    }

    private void RefreshLocalizedText()
    {
        // Button
        submitButton.SetText(UILocalizationTable.Get("customize.btn-submit"));

        // Placeholders
        namePlaceholder.text = UILocalizationTable.Get("customize.placeholder-name");
        dobDayPlaceHolder.text = UILocalizationTable.Get("customize.placeholder-dob-day");
        dobMonthPlaceHolder.text = UILocalizationTable.Get("customize.placeholder-dob-month");
        dobYearPlaceHolder.text = UILocalizationTable.Get("customize.placeholder-dob-year");

        // Labels
        nameLabel.text = UILocalizationTable.Get("customize.label-name");
        dobLabel.text = UILocalizationTable.Get("customize.label-dob");
    }
    #endregion
}