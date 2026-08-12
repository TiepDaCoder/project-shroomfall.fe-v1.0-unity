using Assets.Utilities;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SignUpView : MonoBehaviour
{
    #region Attributes
    [Header("Icons")]
    [SerializeField] private Sprite tickIcon;
    [SerializeField] private Sprite crossIcon;
    [SerializeField] private Image emailWarnIcon;
    [SerializeField] private Image passwordWarnIcon;
    [SerializeField] private Image nameWarnIcon;

    [Header("Buttons")]
    [SerializeField] private Button backButton;
    [SerializeField] private TextButton signUpButton;

    [Header("Labels")]
    [SerializeField] private TMP_Text viewLabel;
    [SerializeField] private TMP_Text emailLabel;
    [SerializeField] private TMP_Text passwordLabel;
    [SerializeField] private TMP_Text nameLabel;

    [Header("Placeholders")]
    [SerializeField] private TMP_Text emailPlaceholder;
    [SerializeField] private TMP_Text passwordPlaceholder;
    [SerializeField] private TMP_Text namePlaceholder;

    [Header("Text Fields")]
    [SerializeField] private TMP_InputField emailTextField;
    [SerializeField] private TMP_InputField passwordTextField;
    [SerializeField] private TMP_InputField nameTextField;
    #endregion

    #region Properties
    public event Action OnBackClicked;
    public event Action OnSignUpClicked;
    public event Action<string> OnEmailChanged;
    public event Action<string> OnPasswordChanged;
    public event Action<string> OnNameChanged;
    #endregion

    #region Methods
    private void Awake()
    {
        // Buttons
        backButton.onClick.AddListener(() => { OnBackClicked?.Invoke(); });
        signUpButton.onClick.AddListener(() => { OnSignUpClicked?.Invoke(); });

        // Inputs
        emailTextField.onValueChanged.AddListener(v => OnEmailChanged?.Invoke(v));
        passwordTextField.contentType = TMP_InputField.ContentType.Password;
        passwordTextField.ForceLabelUpdate();
        passwordTextField.onValueChanged.AddListener(v => OnPasswordChanged?.Invoke(v));
        nameTextField.onValueChanged.AddListener(v => OnNameChanged?.Invoke(v));
    }

    public void SetVisible(
        bool visible)
    {
        gameObject.SetActive(visible);
        if (visible) RefreshLocalizedText();
    }

    public void SetSignUpInteractable(
        bool value)
    {
        signUpButton.interactable = value;
    }

    public void SetEmailValid(
        bool isValid)
    {
        emailWarnIcon.enabled = true;
        emailWarnIcon.sprite = isValid ? tickIcon : crossIcon;
    }

    public void SetPasswordValid(
        bool isValid)
    {
        passwordWarnIcon.enabled = true;
        passwordWarnIcon.sprite = isValid ? tickIcon : crossIcon;
    }

    public void SetNameValid(
        bool isValid)
    {
        nameWarnIcon.enabled = true;
        nameWarnIcon.sprite = isValid ? tickIcon : crossIcon;
    }

    public void SetInteractable(
        bool interactable)
    {
        backButton.interactable = interactable;
        signUpButton.interactable = interactable;
        emailTextField.interactable = interactable;
        passwordTextField.interactable = interactable;
        nameTextField.interactable = interactable;
    }

    private void RefreshLocalizedText()
    {
        // Buttons
        signUpButton.SetText(UILocalizationTable.Get("sign-up.btn-signup"));

        // Placeholders
        emailPlaceholder.text = UILocalizationTable.Get("sign-up.placeholder-email");
        passwordPlaceholder.text = UILocalizationTable.Get("sign-up.placeholder-password");
        namePlaceholder.text = UILocalizationTable.Get("sign-up.placeholder-name");

        // Labels
        viewLabel.text = UILocalizationTable.Get("sign-up.label-view");
        emailLabel.text = UILocalizationTable.Get("sign-up.label-email");
        passwordLabel.text = UILocalizationTable.Get("sign-up.label-password");
        nameLabel.text = UILocalizationTable.Get("sign-up.label-name");
    }
    #endregion
}