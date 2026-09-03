using Assets.Source.UI.Component.Button;
using Assets.Source.Utility;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Source.UI.Feature.MainMenu
{
    public class MainMenuView : MonoBehaviour
    {
        #region Attributes
        [Header("Buttons")]
        [SerializeField] private TextButton steamAuthButton;
        [SerializeField] private TextButton signInButton;
        [SerializeField] private TextButton signUpButton;
        [SerializeField] private TextButton settingButton;
        [SerializeField] private TextButton quitButton;
        [SerializeField] private Button instagramButton;
        [SerializeField] private Button facebookButton;
        [SerializeField] private Button tiktokButton;
        [SerializeField] private Button discordButton;
        [SerializeField] private Button youtubeButton;
        #endregion

        #region Properties
        public event Action OnSteamAuthClicked;
        public event Action OnSignInClicked;
        public event Action OnSignUpClicked;
        public event Action OnSettingClicked;
        public event Action OnQuitClicked;
        public event Action OnInstagramClicked;
        public event Action OnFacebookClicked;
        public event Action OnTiktokClicked;
        public event Action OnDiscordClicked;
        public event Action OnYoutubeClicked;
        #endregion

        #region Methods
        private void Awake()
        {
            // Buttons
            steamAuthButton.onClick.AddListener(() => { OnSteamAuthClicked?.Invoke(); });
            signInButton.onClick.AddListener(() => { OnSignInClicked?.Invoke(); });
            signUpButton.onClick.AddListener(() => { OnSignUpClicked?.Invoke(); });
            settingButton.onClick.AddListener(() => { OnSettingClicked?.Invoke(); });
            quitButton.onClick.AddListener(() => { OnQuitClicked?.Invoke(); });
            instagramButton.onClick.AddListener(() => { OnInstagramClicked?.Invoke(); });
            facebookButton.onClick.AddListener(() => { OnFacebookClicked?.Invoke(); });
            tiktokButton.onClick.AddListener(() => { OnTiktokClicked?.Invoke(); });
            discordButton.onClick.AddListener(() => { OnDiscordClicked?.Invoke(); });
            youtubeButton.onClick.AddListener(() => { OnYoutubeClicked?.Invoke(); });
        }

        public void SetVisible(
            bool visible)
        {
            gameObject.SetActive(visible);
            if (visible) RefreshLocalizedText();
        }

        public void SetInteractable(
            bool interactable)
        {
            steamAuthButton.interactable = interactable;
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

        private void RefreshLocalizedText()
        {
            // Buttons
            steamAuthButton.SetText(UILocalizationTable.Get("main-menu.btn-steamauth"));
            signInButton.SetText(UILocalizationTable.Get("main-menu.btn-signin"));
            signUpButton.SetText(UILocalizationTable.Get("main-menu.btn-signup"));
            settingButton.SetText(UILocalizationTable.Get("main-menu.btn-setting"));
            quitButton.SetText(UILocalizationTable.Get("main-menu.btn-quit"));
        }
        #endregion
    }
}