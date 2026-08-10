using Assets.Core;
using Assets.Enums;
using Assets.Storages;
using Contract;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Services
{
    public class SettingService : IService
    {
        #region Attributes
        // Visual
        public float AnimationSpeedMultiplier { get; private set; }

        // Input / Networking
        public float MoveSendRate { get; private set; }

        // Screen
        public ScreenPreset ScreenPreset { get; private set; }

        // Events
        public event Action<SettingService> OnChanged;

        public string CurrentLocale { get; private set; }

        private DefinitionService definitionService;
        #endregion

        #region Properties
        public bool IsInitialized { get; private set; } = false;
        #endregion

        public SettingService()
        {
            var profile = UserProfileStorage.Load();

            // Locale
            if (profile != null && !string.IsNullOrEmpty(profile.PreferredLocale))
            {
                SetCurrentLocale(profile.PreferredLocale);
            }
            else
            {
                SetCurrentLocale(Constraint.DEFAULT_LOCALE);
            }

            // Screen preset
            if (profile != null && !string.IsNullOrEmpty(profile.ScreenPresent) && System.Enum.TryParse<ScreenPreset>(profile.ScreenPresent, ignoreCase: true, out var screenPreset))
            {
                SetScreen(screenPreset);
            }
            else
            {
                SetScreen(ScreenPreset.Full);
            }
        }

        #region Methods

        public Task InitializeAsync()
        {
            // Resolve setting service
            definitionService = ServiceProvider.Get<DefinitionService>();

            IsInitialized = true;
            return Task.CompletedTask;
        }

        public Task ShutdownAsync()
        {
            return Task.CompletedTask;
        }

        public void SetScreenPreset(
            ScreenPreset screenPreset)
        {
            SetScreen(screenPreset);
            SaveToStorage();

        }

        public void ChangeLocale(
            string locale)
        {
            SetCurrentLocale(locale);
            SaveToStorage();

            // Clear locale cache
            definitionService.ClearLocaleCache();
        }

        public string GetCurrentLocale()
        {
            return CurrentLocale;
        }

        private void SaveToStorage()
        {
            UserProfileStorage.Save(new UserProfileDTO
            {
                PreferredLocale = CurrentLocale,
                ScreenPresent = ScreenPreset.ToString(),
            });
        }

        private void SetScreen(
            ScreenPreset preset)
        {
            // Apply resolution
            switch (preset)
            {
                case ScreenPreset.Small:
                    Screen.SetResolution(1280, 720, false);
                    break;

                case ScreenPreset.Medium:
                    Screen.SetResolution(1600, 900, false);
                    break;

                case ScreenPreset.Full:
                    Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, true);
                    break;
            }

            // Update UI reference resolution
            Canvas.ForceUpdateCanvases();

            ScreenPreset = preset;
            OnChanged?.Invoke(this);
        }

        private void SetCurrentLocale(
            string locale)
        {
            CurrentLocale = locale;
        }
        #endregion
    }
}