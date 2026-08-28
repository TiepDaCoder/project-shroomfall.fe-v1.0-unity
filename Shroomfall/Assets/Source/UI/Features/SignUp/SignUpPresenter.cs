using Assets.Source.Service;
using Assets.Source.Utilities;
using System;

namespace Assets.UI.Features.SignUp
{
    public class SignUpPresenter : IDisposable
    {
        #region Attributes
        private readonly UIService uiService;
        private readonly GameService gameService;
        private readonly SignUpView signUpView;

        private bool isEmailValid;
        private bool isPasswordValid;
        private bool isNameValid;
        private string email;
        private string password;
        private string name;

        private bool disposed;
        #endregion

        #region Properties
        #endregion

        public SignUpPresenter(
            UIService uiService,
            GameService gameService,
            SignUpView signUpView)
        {
            this.uiService = uiService;
            this.gameService = gameService;
            this.signUpView = signUpView;

            Bind();

            UpdateRegisterButton();
        }

        #region Methods
        public void Dispose()
        {
            if (disposed) return;
            disposed = true;

            // Outbound
            uiService.OnUIStateChanged -= OnUIStateChanged;
            uiService.OnGlobalInteractableChanged -= signUpView.SetInteractable;

            // Inbound
            signUpView.OnBackClicked -= OnBackClicked;
            signUpView.OnSignUpClicked -= OnSignUpClicked;
            signUpView.OnEmailChanged -= OnEmailChanged;
            signUpView.OnPasswordChanged -= OnPasswordChanged;
            signUpView.OnNameChanged -= OnNameChanged;
        }

        private void Bind()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(SignUpPresenter));

            // Outbound
            uiService.OnUIStateChanged += OnUIStateChanged;
            uiService.OnGlobalInteractableChanged += signUpView.SetInteractable;

            // Inbound
            signUpView.OnBackClicked += OnBackClicked;
            signUpView.OnSignUpClicked += OnSignUpClicked;
            signUpView.OnEmailChanged += OnEmailChanged;
            signUpView.OnPasswordChanged += OnPasswordChanged;
            signUpView.OnNameChanged += OnNameChanged;
        }

        private void OnUIStateChanged(
            UIService service)
        {
            signUpView.SetVisible(service.ShowSignUp);
        }

        private void OnBackClicked()
        {
            gameService.PopPhase();
        }

        private void OnSignUpClicked()
        {
            AsyncHelper.Run(uiService, async () =>
            {
                await gameService.PlayerSignUp(email, password, name);
            });
        }

        private void OnEmailChanged(
            string v)
        {
            email = v;
            isEmailValid = !string.IsNullOrWhiteSpace(email) && email.Contains("@");
            signUpView.SetEmailValid(isEmailValid);
            UpdateRegisterButton();
        }

        private void OnPasswordChanged(
            string v)
        {
            password = v;
            isPasswordValid = !string.IsNullOrWhiteSpace(password);
            signUpView.SetPasswordValid(isPasswordValid);
            UpdateRegisterButton();
        }

        private void OnNameChanged(
            string v)
        {
            name = v;
            isNameValid = !string.IsNullOrWhiteSpace(name);
            signUpView.SetNameValid(isNameValid);
            UpdateRegisterButton();
        }

        private void UpdateRegisterButton()
        {
            bool canRegister = isEmailValid && isPasswordValid && isNameValid;
            signUpView.SetSignUpInteractable(canRegister);
        }
        #endregion
    }
}
