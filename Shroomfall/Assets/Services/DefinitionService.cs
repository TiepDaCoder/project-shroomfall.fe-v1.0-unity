using Assets.Core;
using Assets.Storages;
using Assets.Utilities;
using Contract.DTO.Definition.MetaDomain;
using Contract.DTO.Feature.Design.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Assets.Services
{
    public class DefinitionService : IService
    {
        #region Attributes
        private SettingService settingService;
        private Dictionary<string, string> localeCache = new();
        #endregion

        #region Properties
        public long Version { get; private set; }

        public DefinitionSnapshotDTO Snapshot { get; private set; }

        public event Action OnChanged;

        public bool IsInitialized { get; private set; }
        #endregion

        public DefinitionService()
        {
        }

        #region Methods
        public Task InitializeAsync()
        {
            // Resolve setting service
            settingService = ServiceProvider.Get<SettingService>();

            // Reload current definition from local file
            var cached = DefinitionStorage.Load();

            // Load definition on state
            if (cached != null)
                SetSnapshot(cached);

            IsInitialized = true;
            return Task.CompletedTask;
        }

        public Task ShutdownAsync()
        {
            return Task.CompletedTask;
        }

        public string GetLocalizedText(
            string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return string.Empty;

            // fast path (cached)
            if (localeCache.TryGetValue(key, out var cached))
                return cached;

            var locale = Snapshot.Locales
                .FirstOrDefault(x => x.Code == settingService.GetCurrentLocale());

            if (locale == null || locale.LocalizationEntries == null)
                return key;

            var entry = locale.LocalizationEntries
                .FirstOrDefault(e => e.Key == key);

            if (entry != null)
            {
                localeCache[key] = entry.Value;
                return entry.Value;
            }

            return key;
        }

        public void ClearLocaleCache()
        {
            localeCache.Clear();
        }

        public List<ItemDefinitionDTO> GetItemDefinitions()
        {
            return Snapshot.Items;
        }
        #region Senders

#nullable enable
        public async Task RefreshDefinitions()
        {
            var result = await HttpCaller.GetAsync<DefinitionSnapshotDTO?>(
                $"{Configuration.DESIGN_CONTROLLER}{Version}"
            );

            // Already latest version
            if (result == null)
            {
                return;
            }

            // Pernamently save to local file
            DefinitionStorage.Save(result);

            // Load on state
            SetSnapshot(result);
        }

        private void SetSnapshot(
            DefinitionSnapshotDTO snapshot)
        {
            Snapshot = snapshot;
            Version = snapshot.Version;
            OnChanged?.Invoke();
        }
        #endregion

        #region Receivers
        #endregion
        #endregion
    }
}