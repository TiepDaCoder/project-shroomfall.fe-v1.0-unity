using Assets.Source.Core;
using Assets.Source.Storage;
using Assets.Source.Utility;
using Contract.DTO.Definition.EntityDomain.Component;
using Contract.DTO.Definition.MetaDomain;
using Contract.DTO.Feature.Design.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Assets.Source.Service
{
    public class DefinitionService : IService
    {
        #region Attributes
        private LocalizeService localizeService;
        private Dictionary<string, string> localeCache = new();
        private Dictionary<string, ItemDefinitionDTO> itemCache = new();
        private Dictionary<string, EntityDefinitionDTO> entityCache = new();
        #endregion

        #region Properties
        public bool IsInitialized { get; private set; }
        public long Version { get; private set; }
        public DefinitionSnapshotDTO Snapshot { get; private set; }

        public event Action OnChanged;
        #endregion

        public DefinitionService() { }

        #region Methods
        public Task InitializeAsync()
        {
            localizeService = ServiceProvider.Get<LocalizeService>();

            // Build cache
            Snapshot = DefinitionStorage.Load();
            if (Snapshot != null)
            {
                Version = Snapshot.Version;
                BuildCache();
            }

            // Localize
            localizeService.OnLocaleChanged += BuildLocaleCache;

            IsInitialized = true;
            return Task.CompletedTask;
        }

        public Task ShutdownAsync()
        {
            return Task.CompletedTask;
        }

        #region Build
        public void SetSnapshot(
            DefinitionSnapshotDTO snapshot)
        {
            Snapshot = snapshot ?? new DefinitionSnapshotDTO();
            Version = Snapshot.Version;

            BuildCache();
            OnChanged?.Invoke();
        }

        public void ClearDefinitions()
        {
            SetSnapshot(new DefinitionSnapshotDTO());
        }

        private void BuildCache()
        {
            BuildLocaleCache();
            BuildItemCache();
            BuildEntityCache();
        }

        private void BuildLocaleCache()
        {
            if (Snapshot == null)
                return;

            localeCache.Clear();
            var locale = Snapshot.Locales?.FirstOrDefault(x => x.Code == localizeService.CurrentLocale);
            if (locale?.LocalizationEntries != null)
            {
                foreach (var entry in locale.LocalizationEntries)
                {
                    if (entry == null || string.IsNullOrWhiteSpace(entry.Key))
                        continue;

                    if (!localeCache.ContainsKey(entry.Key))
                        localeCache.Add(entry.Key, entry.Value);
                }
            }
        }

        private void BuildItemCache()
        {
            if (Snapshot == null)
                return;

            itemCache.Clear();
            if (Snapshot.Items != null)
            {
                foreach (var item in Snapshot.Items)
                {
                    if (item == null || string.IsNullOrWhiteSpace(item.Id))
                        continue;

                    if (!itemCache.ContainsKey(item.Id))
                        itemCache.Add(item.Id, item);
                }
            }
        }

        private void BuildEntityCache()
        {
            if (Snapshot == null)
                return;

            entityCache.Clear();
            if (Snapshot.Entities != null)
            {
                foreach (var entity in Snapshot.Entities)
                {
                    if (entity == null || string.IsNullOrWhiteSpace(entity.Id))
                        continue;

                    if (!entityCache.ContainsKey(entity.Id))
                        entityCache.Add(entity.Id, entity);
                }
            }
        }
        #endregion

        #region Get
        public string GetLocalizedText(
            string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return string.Empty;

            return localeCache.TryGetValue(key, out var value) ? value : key;
        }

        public ItemDefinitionDTO GetItemDefinition(
            string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;

            itemCache.TryGetValue(id, out var definition);
            return definition;
        }

        public EntityDefinitionDTO GetEntityDefinition(
            string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;

            entityCache.TryGetValue(id, out var definition);
            return definition;
        }
        #endregion

        #region Senders
        #endregion

        #region Receivers
#nullable enable
        public async Task RefreshDefinitions()
        {
            var result = await HttpCaller.GetAsync<DefinitionSnapshotDTO?>(
                $"{Configuration.GAME_CONTROLLER}{Version}");

            if (result == null)
                return;

            DefinitionStorage.Save(result);

            Snapshot = result;
            Version = result.Version;

            BuildCache();
            OnChanged?.Invoke();
        }
        #endregion
        #endregion
    }
}