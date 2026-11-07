using Assets.Source.Core;
using Assets.Source.Data;
using Assets.Source.Gameplay.Component.Input;
using Assets.Source.Gameplay.Factory;
using Assets.Source.Utility;
using Contract.DTO.Feature.Game.Response;
using Contract.DTO.Runtime.EntityDomain;
using Contract.DTO.Runtime.EntityDomain.Component;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Source.Service
{
    public class EntityService : IService
    {
        #region Attributes
        private Dictionary<string, EntityInstanceDTO> entities = new();
        private Dictionary<string, EntityView> entityViews = new();

        private EntityFactory entityFactory;

        private EntityCatalogSO entityCatalog;
        private ItemCatalogSO itemCatalog;
        private LocalView localPrefab;
        private EntityView entityPrefab;
        #endregion

        #region Properties
        public bool IsInitialized { get; private set; } = false;
        #endregion

        public EntityService(
            EntityCatalogSO entityCatalog,
            ItemCatalogSO itemCatalog,
            LocalView localPrefab,
            EntityView entityPrefab)
        {
            this.entityCatalog = entityCatalog;
            this.itemCatalog = itemCatalog;
            this.localPrefab = localPrefab;
            this.entityPrefab = entityPrefab;
        }

        #region Methods
        public Task InitializeAsync()
        {
            var playerService = ServiceProvider.Get<PlayerService>();

            entityFactory = new EntityFactory(playerService, entityCatalog, localPrefab, entityPrefab);

            IsInitialized = true;
            return Task.CompletedTask;
        }

        public Task ShutdownAsync()
        {
            return Task.CompletedTask;
        }

        public void LoadEntitiesData(
            IEnumerable<EntityInstanceDTO> entityList)
        {
            foreach (var entity in entityList)
                CreateEntity(entity, null);
        }

        public void UnloadEntitiesData()
        {
            foreach (var id in new List<string>(entities.Keys))
                DestroyEntity(id);
        }

        private void CreateEntity(
            EntityInstanceDTO entity,
            Vector2? direction)
        {
            if (entity == null || string.IsNullOrEmpty(entity.Id))
                return;

            if (entities.ContainsKey(entity.Id))
                return;

            entities[entity.Id] = entity;
            var view = entityFactory.Create(entity, direction);
            entityViews[entity.Id] = view;
        }

        private void DestroyEntity(
            string id)
        {
            if (string.IsNullOrEmpty(id))
                return;

            entities?.Remove(id);

            if (entityViews == null || !entityViews.TryGetValue(id, out var view))
                return;

            entityViews.Remove(id);

            if (view != null && view.gameObject != null)
                GameObject.Destroy(view.gameObject);
        }

        private void UpdateEntityActed(
            EntityInstanceDTO entity,
            EntityActedDTO dto)
        {
            var transform = entity.Components.OfType<TransformInstanceDTO>().FirstOrDefault();
            if (transform == null)
                return;

            transform.Position.X = dto.X;
            transform.Position.Y = dto.Y;
            transform.FacingDirection = dto.Direction;
            transform.CurrentAction = dto.Action;
        }

        private void UpdateEntityAppearance(
            EntityInstanceDTO entity,
            EntityAppearanceChangedDTO dto)
        {
            var appearance = entity.Components.OfType<AppearanceInstanceDTO>().FirstOrDefault();
            if (appearance != null)
            {
                appearance.SkinID = dto.Appearance.SkinID;
                appearance.SkinColor = dto.Appearance.SkinColor;
                return;
            }

            entity.Components.Add(dto.Appearance);
        }

        private void UpdateEntityActedView(
            EntityActedDTO dto)
        {
            if (!entityViews.TryGetValue(dto.EntityInstanceID, out var view))
                return;

            if (!string.IsNullOrEmpty(dto.UsedItemDefinitionID))
            {
                if (itemCatalog.TryGet(dto.UsedItemDefinitionID, out var item))
                {
                    view.HoldItem(item);
                }
            }

            view.SetAction(dto.Action);
            view.SetDirection(dto.Direction);
            view.ApplyPosition(new Vector2(dto.X, dto.Y));
        }

        private void UpdateEntityAppearanceView(
            EntityAppearanceChangedDTO dto)
        {
            if (!entityViews.TryGetValue(dto.EntityInstanceID, out var view))
                return;

            if (!entityCatalog.TryGet(dto.Appearance.SkinID, out var entity))
                return;

            view.ApplyAppearance(entity, ColorHelper.ToColor(dto.Appearance.SkinColor), null);
        }

        #region Senders
        #endregion

        #region Receivers
        public void OnEntitySpawned(
            EntitySpawnedDTO dto)
        {
            CoroutineRunner.Instance.Schedule(() =>
            {
                if (entities.ContainsKey(dto.EntityInstance.Id))
                    return;

                Vector2? direction = null;
                if (dto.Direction != null)
                    direction = new Vector2(dto.Direction.X, dto.Direction.Y);

                CreateEntity(dto.EntityInstance, direction);
            });
        }

        public void OnEntityDespawned(
            string id)
        {
            CoroutineRunner.Instance.Schedule(() =>
            {
                DestroyEntity(id);
            });
        }

        public void OnEntityActed(
            EntityActedDTO dto)
        {
            CoroutineRunner.Instance.Schedule(() =>
            {
                if (!entities.TryGetValue(dto.EntityInstanceID, out var entity))
                    return;

                UpdateEntityActed(entity, dto);
                UpdateEntityActedView(dto);
            });
        }

        public void OnEntityAppearanceChanged(
            EntityAppearanceChangedDTO dto)
        {
            CoroutineRunner.Instance.Schedule(() =>
            {
                if (!entities.TryGetValue(dto.EntityInstanceID, out var entity))
                    return;

                UpdateEntityAppearance(entity, dto);
                UpdateEntityAppearanceView(dto);
            });
        }

        public void OnEntityVitalChanged(
            EntityVitalChangedDTO dto)
        {
            CoroutineRunner.Instance.Schedule(() =>
            {
                if (!entities.TryGetValue(dto.EntityInstanceID, out var entity))
                    return;
            });
        }
        #endregion
        #endregion
    }
}