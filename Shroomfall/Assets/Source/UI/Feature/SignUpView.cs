using Assets.Source.Core;
using Assets.Source.Enum;
using Assets.Source.Service;
using Assets.Source.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Source.UI.Feature
{
    public class SignUpView : MonoBehaviour
    {
        #region Attributes
        [Header("Root")]
        [SerializeField] private GameObject canvas;

        [Header("Icons")]
        [SerializeField] private Sprite tickIcon;
        [SerializeField] private Sprite crossIcon;
        [SerializeField] private Image emailWarnIcon;
        [SerializeField] private Image passwordWarnIcon;
        [SerializeField] private Image nameWarnIcon;

        [Header("Buttons")]
        [SerializeField] private Button backButton;
        [SerializeField] private Button signUpButton;

        [Header("Text Fields")]
        [SerializeField] private TMP_InputField emailTextField;
        [SerializeField] private TMP_InputField passwordTextField;
        [SerializeField] private TMP_InputField nameTextField;

        private UIService uiService;
        private AuthService authService;

        private bool isEmailValid;
        private bool isPasswordValid;
        private bool isNameValid;

        private string emailValue;
        private string passwordValue;
        private string nameValue;
        #endregion

        #region Properties
        #endregion

        #region Methods
        private void Awake()
        {
            uiService = ServiceProvider.Get<UIService>();
            authService = ServiceProvider.Get<AuthService>();

            // UI Service
            uiService.OnUIStateChanged += OnUIStateChanged;
            uiService.OnGlobalInteractableChanged += SetInteractable;

            // Buttons
            backButton.onClick.AddListener(OnBackClicked);
            signUpButton.onClick.AddListener(OnSignUpClicked);

            // Text Fields
            emailTextField.onValueChanged.AddListener(OnEmailChanged);
            passwordTextField.contentType = TMP_InputField.ContentType.Password;
            passwordTextField.ForceLabelUpdate();
            passwordTextField.onValueChanged.AddListener(OnPasswordChanged);
            nameTextField.onValueChanged.AddListener(OnNameChanged);

            UpdateRegisterButton();
        }

        private void OnDestroy()
        {
            // UI Service
            uiService.OnUIStateChanged -= OnUIStateChanged;
            uiService.OnGlobalInteractableChanged -= SetInteractable;

            // Buttons
            backButton.onClick.RemoveListener(OnBackClicked);
            signUpButton.onClick.RemoveListener(OnSignUpClicked);

            // Text Fields
            emailTextField.onValueChanged.RemoveListener(OnEmailChanged);
            passwordTextField.onValueChanged.RemoveListener(OnPasswordChanged);
            nameTextField.onValueChanged.RemoveListener(OnNameChanged);
        }

        private void OnUIStateChanged(
            UIService service)
        {
            canvas.SetActive(service.ShowSignUp);
        }

        private void SetInteractable(
            bool interactable)
        {
            backButton.interactable = interactable;
            signUpButton.interactable = interactable;
            emailTextField.interactable = interactable;
            passwordTextField.interactable = interactable;
            nameTextField.interactable = interactable;
        }

        private void OnBackClicked()
        {
            uiService.SetState(UIState.MainMenu);
        }

        private void OnSignUpClicked()
        {
            AsyncHelper.Run(uiService, async () =>
            {
                await authService.Register(emailValue, passwordValue, nameValue);
            });
        }

        private void OnEmailChanged(
            string value)
        {
            emailValue = value;
            isEmailValid = !string.IsNullOrWhiteSpace(emailValue) && emailValue.Contains("@");
            SetEmailValid(isEmailValid);
            UpdateRegisterButton();
        }

        private void OnPasswordChanged(
            string value)
        {
            passwordValue = value;
            isPasswordValid = !string.IsNullOrWhiteSpace(passwordValue);
            SetPasswordValid(isPasswordValid);
            UpdateRegisterButton();
        }

        private void OnNameChanged(
            string value)
        {
            nameValue = value;
            isNameValid = !string.IsNullOrWhiteSpace(nameValue);
            SetNameValid(isNameValid);
            UpdateRegisterButton();
        }

        private void UpdateRegisterButton()
        {
            bool canRegister = isEmailValid && isPasswordValid && isNameValid;
            signUpButton.interactable = canRegister;
        }

        private void SetEmailValid(
            bool isValid)
        {
            emailWarnIcon.enabled = true;
            emailWarnIcon.sprite = isValid ? tickIcon : crossIcon;
        }

        private void SetPasswordValid(
            bool isValid)
        {
            passwordWarnIcon.enabled = true;
            passwordWarnIcon.sprite = isValid ? tickIcon : crossIcon;
        }

        private void SetNameValid(
            bool isValid)
        {
            nameWarnIcon.enabled = true;
            nameWarnIcon.sprite = isValid ? tickIcon : crossIcon;
        }
        #endregion
    }
}