using Assets.Source.Service;
using Assets.Source.Utilities;
using System;

namespace Assets.UI.Features.SignIn
{
    public class SignInPresenter : IDisposable
    {
        #region Attributes
        private readonly UIService uiService;
        private readonly GameService gameService;
        private readonly SignInView signInView;

        private bool isEmailValid;
        private bool isPasswordValid;
        private string email;
        private string password;

        private bool disposed;
        #endregion

        #region Properties
        #endregion

        public SignInPresenter(
            UIService uiService,
            GameService gameService,
            SignInView signInView)
        {
            this.uiService = uiService;
            this.gameService = gameService;
            this.signInView = signInView;

            Bind();

            UpdateSignInButton();
        }

        #region Methods
        public void Dispose()
        {
            if (disposed) return;
            disposed = true;

            // Outbound
            uiService.OnUIStateChanged -= OnUIStateChanged;
            uiService.OnGlobalInteractableChanged -= signInView.SetInteractable;

            // Inbound
            signInView.OnBackClicked -= OnBackClicked;
            signInView.OnSignInClicked -= OnSignInClicked;
            signInView.OnEmailChanged -= OnEmailChanged;
            signInView.OnPasswordChanged -= OnPasswordChanged;
        }

        private void Bind()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(SignInPresenter));

            // Outbound
            uiService.OnUIStateChanged += OnUIStateChanged;
            uiService.OnGlobalInteractableChanged += signInView.SetInteractable;

            // Inbound
            signInView.OnBackClicked += OnBackClicked;
            signInView.OnSignInClicked += OnSignInClicked;
            signInView.OnEmailChanged += OnEmailChanged;
            signInView.OnPasswordChanged += OnPasswordChanged;
        }

        private void OnUIStateChanged(
            UIService service)
        {
            signInView.SetVisible(service.ShowSignIn);
        }

        private void OnBackClicked()
        {
            gameService.PopPhase();
        }

        private void OnSignInClicked()
        {
            AsyncHelper.Run(uiService, async () =>
            {
                await gameService.PlayerSignIn(email, password);
            });
        }

        private void OnEmailChanged(
            string v)
        {
            email = v;
            isEmailValid = !string.IsNullOrWhiteSpace(email) && email.Contains("@");
            signInView.SetEmailValid(isEmailValid);
            UpdateSignInButton();
        }

        private void OnPasswordChanged(
            string v)
        {
            password = v;
            isPasswordValid = !string.IsNullOrWhiteSpace(password);
            signInView.SetPasswordValid(isPasswordValid);
            UpdateSignInButton();
        }

        private void UpdateSignInButton()
        {
            bool canSignIn = isEmailValid && isPasswordValid;
            signInView.SetSignInInteractable(canSignIn);
        }
        #endregion
    }
}