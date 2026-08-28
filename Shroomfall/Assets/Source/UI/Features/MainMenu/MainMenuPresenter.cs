using Assets.Source.Enum;
using Assets.Source.Service;
using Assets.Source.Utilities;
using System;
using UnityEngine;

namespace Assets.UI.Features.MainMenu
{
    public class MainMenuPresenter
    {
        #region Attributes
        private readonly GameService gameService;
        private readonly UIService uiService;
        private readonly MainMenuView mainMenuView;

        private bool disposed;
        #endregion

        #region Properties
        #endregion

        public MainMenuPresenter(
            GameService gameService,
            UIService uiService,
            MainMenuView mainMenuView)
        {
            this.gameService = gameService;
            this.uiService = uiService;
            this.mainMenuView = mainMenuView;

            Bind();
        }

        #region Methods
        public void Dispose()
        {
            if (disposed) return;
            disposed = true;

            // Outbound
            uiService.OnUIStateChanged -= OnUIStateChanged;
            uiService.OnGlobalInteractableChanged -= mainMenuView.SetInteractable;

            // Inbound
            mainMenuView.OnSteamAuthClicked -= OnSteamAuthClicked;
            mainMenuView.OnSignInClicked -= OnSignInClicked;
            mainMenuView.OnSignUpClicked -= OnSignUpClicked;
            mainMenuView.OnSettingClicked -= OnSettingClicked;
            mainMenuView.OnQuitClicked -= OnQuitClicked;
            mainMenuView.OnInstagramClicked -= OnInstagramClicked;
            mainMenuView.OnFacebookClicked -= OnFacebookClicked;
            mainMenuView.OnTiktokClicked -= OnTiktokClicked;
            mainMenuView.OnDiscordClicked -= OnDiscordClicked;
            mainMenuView.OnYoutubeClicked -= OnYoutubeClicked;
        }

        private void Bind()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(MainMenuPresenter));

            // Outbound
            uiService.OnUIStateChanged += OnUIStateChanged;
            uiService.OnGlobalInteractableChanged += mainMenuView.SetInteractable;

            // Inbound
            mainMenuView.OnSteamAuthClicked += OnSteamAuthClicked;
            mainMenuView.OnSignInClicked += OnSignInClicked;
            mainMenuView.OnSignUpClicked += OnSignUpClicked;
            mainMenuView.OnSettingClicked += OnSettingClicked;
            mainMenuView.OnQuitClicked += OnQuitClicked;
            mainMenuView.OnInstagramClicked += OnInstagramClicked;
            mainMenuView.OnFacebookClicked += OnFacebookClicked;
            mainMenuView.OnTiktokClicked += OnTiktokClicked;
            mainMenuView.OnDiscordClicked += OnDiscordClicked;
            mainMenuView.OnYoutubeClicked += OnYoutubeClicked;
        }

        private void OnUIStateChanged(
            UIService service)
        {
            mainMenuView.SetVisible(service.ShowMainMenu);
        }

        private void OnSteamAuthClicked()
        {
            AsyncHelper.Run(uiService, async () =>
            {
                await gameService.PlayerSteamAuthenticate();
            });
        }

        private void OnSignInClicked()
        {
            gameService.PushPhase(GamePhase.SignIn);
        }

        private void OnSignUpClicked()
        {
            gameService.PushPhase(GamePhase.SignUp);
        }

        private void OnSettingClicked()
        {
            gameService.PushPhase(GamePhase.SettingMenu);
        }

        private void OnQuitClicked()
        {
            AsyncHelper.Run(uiService, async () =>
            {
                await gameService.QuitGame();
            });
        }

        private void OnInstagramClicked()
        {
            OpenSocialLink(Configuration.INSTAGRAM_URL);
        }

        private void OnFacebookClicked()
        {
            OpenSocialLink(Configuration.FACEBOOK_URL);
        }

        private void OnTiktokClicked()
        {
            OpenSocialLink(Configuration.TIKTOK_URL);
        }

        private void OnDiscordClicked()
        {
            OpenSocialLink(Configuration.DISCORD_URL);
        }

        private void OnYoutubeClicked()
        {
            OpenSocialLink(Configuration.YOUTUBE_URL);
        }

        private static void OpenSocialLink(
            string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return;

            Application.OpenURL(url);
        }
        #endregion
    }
}
