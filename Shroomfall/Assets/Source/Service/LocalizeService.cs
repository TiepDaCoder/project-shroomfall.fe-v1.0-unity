using Assets.Source.Storage;
using Contract;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Source.Service
{
    public class LocalizeService : IService
    {
        #region Attributes
        private readonly Dictionary<string, string> staticLocales = new();
        #endregion

        #region Properties
        public bool IsInitialized { get; private set; } = false;
        public string CurrentLocale { get; private set; }

        public event Action OnLocaleChanged;
        #endregion

        public LocalizeService() { }

        #region Methods
        public Task InitializeAsync()
        {
            // Reload profile
            var profile = UserProfileStorage.Load();
            if (profile != null && !string.IsNullOrEmpty(profile.PreferredLocale))
                SetLocale(profile.PreferredLocale);
            else
                SetLocale(Constraint.DEFAULT_LOCALE);

            IsInitialized = true;
            return Task.CompletedTask;
        }

        public Task ShutdownAsync()
        {
            return Task.CompletedTask;
        }

        public void SetLocale(
            string locale)
        {
            if (CurrentLocale == locale)
                return;

            // Load file
            TextAsset file = Resources.Load<TextAsset>($"localization/{locale}");
            if (file == null)
                return;

            // Build cache
            staticLocales.Clear();
            LocalizationData data = JsonUtility.FromJson<LocalizationData>(file.text);
            foreach (LocalizationEntry entry in data.entries)
                staticLocales[entry.key] = entry.value;

            // Update profile and state
            CurrentLocale = locale;
            UserProfileStorage.UpdatePreferredLocale(locale);

            // Trigger state change event
            OnLocaleChanged?.Invoke();
        }

        public string Get(
            string key)
        {
            if (staticLocales.TryGetValue(key, out string value))
                return value;

            return key;
        }

        #region Senders
        #endregion

        #region Receivers
        #endregion
        #endregion
    }

    [Serializable]
    public class LocalizationEntry
    {
        public string key;
        public string value;
    }

    [Serializable]
    public class LocalizationData
    {
        public LocalizationEntry[] entries;
    }
}