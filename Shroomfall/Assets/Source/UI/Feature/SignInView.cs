using Assets.Source.Core;
using Assets.Source.Enum;
using Assets.Source.Service;
using Assets.Source.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Source.UI.Feature
{
    public class SignInView : MonoBehaviour
    {
        #region Attributes
        [Header("Root")]
        [SerializeField] private GameObject canvas;

        [Header("Icons")]
        [SerializeField] private Sprite tickIcon;
        [SerializeField] private Sprite crossIcon;
        [SerializeField] private Image emailWarnIcon;
        [SerializeField] private Image passwordWarnIcon;

        [Header("Buttons")]
        [SerializeField] private Button backButton;
        [SerializeField] private Button signInButton;
        [SerializeField] private Button googleAuthButton;

        [Header("Text Fields")]
        [SerializeField] private TMP_InputField emailTextField;
        [SerializeField] private TMP_InputField passwordTextField;

        private UIService uiService;
        private AuthService authService;

        private bool isEmailValid;
        private bool isPasswordValid;

        private string email;
        private string password;
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
            signInButton.onClick.AddListener(OnSignInClicked);
            googleAuthButton.onClick.AddListener(OnGoogleAuthClicked);

            // Text Fields
            emailTextField.onValueChanged.AddListener(OnEmailChanged);
            passwordTextField.contentType = TMP_InputField.ContentType.Password;
            passwordTextField.ForceLabelUpdate();
            passwordTextField.onValueChanged.AddListener(OnPasswordChanged);

            UpdateSignInButton();
        }

        private void OnDestroy()
        {
            // UI Service
            uiService.OnUIStateChanged -= OnUIStateChanged;
            uiService.OnGlobalInteractableChanged -= SetInteractable;

            // Buttons
            backButton.onClick.RemoveListener(OnBackClicked);
            signInButton.onClick.RemoveListener(OnSignInClicked);
            googleAuthButton.onClick.RemoveListener(OnGoogleAuthClicked);

            // Text Fields
            emailTextField.onValueChanged.RemoveListener(OnEmailChanged);
            passwordTextField.onValueChanged.RemoveListener(OnPasswordChanged);
        }

        private void OnUIStateChanged(
            UIService service)
        {
            canvas.SetActive(service.ShowSignIn);
        }

        private void SetInteractable(
            bool interactable)
        {
            backButton.interactable = interactable;
            signInButton.interactable = interactable;
            googleAuthButton.interactable = interactable;
            emailTextField.interactable = interactable;
            passwordTextField.interactable = interactable;
        }

        private void OnBackClicked()
        {
            uiService.SetState(UIState.MainMenu);
        }

        private void OnSignInClicked()
        {
            AsyncHelper.Run(uiService, async () =>
            {
                await authService.Login(email, password);
            });
        }

        private void OnGoogleAuthClicked()
        {
            AsyncHelper.Run(uiService, async () =>
            {
                await authService.GoogleAuth();
            });
        }

        private void OnEmailChanged(
            string value)
        {
            email = value;
            isEmailValid = !string.IsNullOrWhiteSpace(email) && email.Contains("@");
            SetEmailValid(isEmailValid);
            UpdateSignInButton();
        }

        private void OnPasswordChanged(
            string value)
        {
            password = value;
            isPasswordValid = !string.IsNullOrWhiteSpace(password);
            SetPasswordValid(isPasswordValid);
            UpdateSignInButton();
        }

        private void UpdateSignInButton()
        {
            bool canSignIn = isEmailValid && isPasswordValid;
            signInButton.interactable = canSignIn;
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
        #endregion
    }
}