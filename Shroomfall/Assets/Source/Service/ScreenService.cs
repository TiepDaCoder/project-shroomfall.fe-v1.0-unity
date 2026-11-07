using Assets.Source.Enum;
using Assets.Source.Storage;
using Assets.Source.Utility;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Source.Service
{
    public class ScreenService : IService
    {
        #region Attributes
        private readonly ScreenEffectSpawner screenEffectSpawner;
        #endregion

        #region Properties
        public bool IsInitialized { get; private set; } = false;
        public ScreenPreset ScreenSize { get; private set; }
        public int ScreenEffect { get; private set; }
        #endregion

        public ScreenService(
            ScreenEffectSpawner screenEffectSpawner)
        {
            this.screenEffectSpawner = screenEffectSpawner;
        }

        #region Methods
        public Task InitializeAsync()
        {
            // Reload profile
            var profile = UserProfileStorage.Load();
            if (profile != null
                && !string.IsNullOrEmpty(profile.PreferredScreenSize)
                && System.Enum.TryParse<ScreenPreset>(profile.PreferredScreenSize, ignoreCase: true, out var screenPreset))
                SetScreenSize(screenPreset);
            else
                SetScreenSize(ScreenPreset.Full);

            if (profile != null)
                SetScreenEffect(profile.PreferredScreenEffect);
            else
                SetScreenEffect(0);

            IsInitialized = true;
            return Task.CompletedTask;
        }

        public Task ShutdownAsync()
        {
            return Task.CompletedTask;
        }

        public void SetScreenSize(
            ScreenPreset preset)
        {
            // Update UI reference resolution
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
            Canvas.ForceUpdateCanvases();

            // Update profile and state
            ScreenSize = preset;
            UserProfileStorage.UpdatePreferredScreenSize(preset);
        }

        public void SetScreenEffect(
            int index)
        {
            // Update UI reference effect
            screenEffectSpawner.SetEffect(index);

            // Update profile and state
            ScreenEffect = index;
            UserProfileStorage.UpdatePreferredScreenEffect(index);
        }

        #region Senders
        #endregion

        #region Receivers
        #endregion
        #endregion
    }
}