using Assets.Utilities;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SignInView : MonoBehaviour
{
    #region Attributes
    [Header("Icons")]
    [SerializeField] private Sprite tickIcon;
    [SerializeField] private Sprite crossIcon;
    [SerializeField] private Image emailWarnIcon;
    [SerializeField] private Image passwordWarnIcon;

    [Header("Buttons")]
    [SerializeField] private Button backButton;
    [SerializeField] private TextButton signInButton;

    [Header("Labels")]
    [SerializeField] private TMP_Text viewLabel;
    [SerializeField] private TMP_Text emailLabel;
    [SerializeField] private TMP_Text passwordLabel;

    [Header("Placeholders")]
    [SerializeField] private TMP_Text emailPlaceholder;
    [SerializeField] private TMP_Text passwordPlaceholder;

    [Header("Text Fields")]
    [SerializeField] private TMP_InputField emailTextField;
    [SerializeField] private TMP_InputField passwordTextField;
    #endregion

    #region Properties
    public event Action OnBackClicked;
    public event Action OnSignInClicked;
    public event Action<string> OnEmailChanged;
    public event Action<string> OnPasswordChanged;
    #endregion

    #region Methods
    private void Awake()
    {
        // Button
        backButton.onClick.AddListener(() => { OnBackClicked?.Invoke(); });
        signInButton.onClick.AddListener(() => { OnSignInClicked?.Invoke(); });

        // Text Fields
        emailTextField.onValueChanged.AddListener(v => OnEmailChanged?.Invoke(v));
        passwordTextField.contentType = TMP_InputField.ContentType.Password;
        passwordTextField.ForceLabelUpdate();
        passwordTextField.onValueChanged.AddListener(v => OnPasswordChanged?.Invoke(v));
    }

    public void SetVisible(
        bool visible)
    {
        gameObject.SetActive(visible);
        if (visible) RefreshLocalizedText();
    }

    public void SetSignInInteractable(
        bool value)
    {
        signInButton.interactable = value;
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

    public void SetInteractable(
        bool interactable)
    {
        backButton.interactable = interactable;
        signInButton.interactable = interactable;
        emailTextField.interactable = interactable;
        passwordTextField.interactable = interactable;
    }

    private void RefreshLocalizedText()
    {
        // Button
        signInButton.SetText(UILocalizationTable.Get("sign-in.btn-signin"));

        // Placeholders
        emailPlaceholder.text = UILocalizationTable.Get("sign-in.placeholder-email");
        passwordPlaceholder.text = UILocalizationTable.Get("sign-in.placeholder-password");

        // Labels
        viewLabel.text = UILocalizationTable.Get("sign-in.label-view");
        emailLabel.text = UILocalizationTable.Get("sign-in.label-email");
        passwordLabel.text = UILocalizationTable.Get("sign-in.label-password");
    }
    #endregion
}