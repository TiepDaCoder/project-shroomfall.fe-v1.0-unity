using Assets.Source.Core;
using Assets.Source.Enum;
using Assets.Source.Service;
using Assets.Source.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Source.UI.Feature
{
    public class MainMenuView : MonoBehaviour
    {
        #region Attributes
        [Header("Root")]
        [SerializeField] private GameObject canvas;

        [Header("Buttons")]
        [SerializeField] private Button tutorialButton;
        [SerializeField] private Button signInButton;
        [SerializeField] private Button signUpButton;
        [SerializeField] private Button settingButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private Button instagramButton;
        [SerializeField] private Button facebookButton;
        [SerializeField] private Button tiktokButton;
        [SerializeField] private Button discordButton;
        [SerializeField] private Button youtubeButton;

        private UIService uiService;
        private GameService gameService;
        #endregion

        #region Properties
        #endregion

        #region Methods
        private void Awake()
        {
            uiService = ServiceProvider.Get<UIService>();
            gameService = ServiceProvider.Get<GameService>();

            // UI Service
            uiService.OnUIStateChanged += OnUIStateChanged;
            uiService.OnGlobalInteractableChanged += SetInteractable;

            // Buttons
            tutorialButton.onClick.AddListener(OnTutorialClicked);
            signInButton.onClick.AddListener(OnSignInClicked);
            signUpButton.onClick.AddListener(OnSignUpClicked);
            settingButton.onClick.AddListener(OnSettingClicked);
            quitButton.onClick.AddListener(OnQuitClicked);
            instagramButton.onClick.AddListener(OnInstagramClicked);
            facebookButton.onClick.AddListener(OnFacebookClicked);
            tiktokButton.onClick.AddListener(OnTiktokClicked);
            discordButton.onClick.AddListener(OnDiscordClicked);
            youtubeButton.onClick.AddListener(OnYoutubeClicked);
        }

        private void OnDestroy()
        {
            // UI Service
            uiService.OnUIStateChanged -= OnUIStateChanged;
            uiService.OnGlobalInteractableChanged -= SetInteractable;

            // Buttons
            tutorialButton.onClick.RemoveListener(OnTutorialClicked);
            signInButton.onClick.RemoveListener(OnSignInClicked);
            signUpButton.onClick.RemoveListener(OnSignUpClicked);
            settingButton.onClick.RemoveListener(OnSettingClicked);
            quitButton.onClick.RemoveListener(OnQuitClicked);
            instagramButton.onClick.RemoveListener(OnInstagramClicked);
            facebookButton.onClick.RemoveListener(OnFacebookClicked);
            tiktokButton.onClick.RemoveListener(OnTiktokClicked);
            discordButton.onClick.RemoveListener(OnDiscordClicked);
            youtubeButton.onClick.RemoveListener(OnYoutubeClicked);
        }

        private void OnUIStateChanged(
            UIService service)
        {
            canvas.SetActive(service.ShowMainMenu);
        }

        private void SetInteractable(
            bool interactable)
        {
            tutorialButton.interactable = interactable;
            signInButton.interactable = interactable;
            signUpButton.interactable = interactable;
            settingButton.interactable = interactable;
            quitButton.interactable = interactable;
            instagramButton.interactable = interactable;
            facebookButton.interactable = interactable;
            tiktokButton.interactable = interactable;
            discordButton.interactable = interactable;
            youtubeButton.interactable = interactable;
        }

        private void OnTutorialClicked()
        {
            gameService.StartTutorial();
        }

        private void OnSignInClicked()
        {
            uiService.SetState(UIState.SignIn);
        }

        private void OnSignUpClicked()
        {
            uiService.SetState(UIState.SignUp);
        }

        private void OnSettingClicked()
        {
            uiService.ShowSetting();
        }

        private void OnQuitClicked()
        {
            AsyncHelper.Run(uiService, async () =>
            {
                await gameService.CloseApplication();
            });
        }

        private void OnInstagramClicked()
        {
            Application.OpenURL(Configuration.INSTAGRAM_URL);
        }

        private void OnFacebookClicked()
        {
            Application.OpenURL(Configuration.FACEBOOK_URL);
        }

        private void OnTiktokClicked()
        {
            Application.OpenURL(Configuration.TIKTOK_URL);
        }

        private void OnDiscordClicked()
        {
            Application.OpenURL(Configuration.DISCORD_URL);
        }

        private void OnYoutubeClicked()
        {
            Application.OpenURL(Configuration.YOUTUBE_URL);
        }
        #endregion
    }
}