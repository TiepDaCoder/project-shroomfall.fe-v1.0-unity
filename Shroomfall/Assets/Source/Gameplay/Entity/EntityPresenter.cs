using Assets.Gameplay.Factory;
using Assets.Service;
using Assets.Source.Data;
using Assets.Source.Gameplay.Component.Input;
using Assets.Source.Utilities;
using Contract.DTO.Runtime.EntityDomain;
using Contract.DTO.Runtime.EntityDomain.Component;
using Contract.Enum.EntityDomain;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Gameplay.Entity
{
    public class EntityPresenter : IDisposable
    {
        #region Attributes
        private readonly EntityService entityService;

        private readonly EntityFactory entityViewFactory;
        private readonly InputBinder inputBinder;

        private readonly EntityCatalogSO entityCatalog;
        private readonly ItemCatalogSO itemCatalog;

        private readonly Dictionary<string, EntityView> entityViews = new();
        protected bool disposed = false;
        #endregion

        #region Properties
        #endregion

        public EntityPresenter(
            EntityService entityService,
            PlayerService playerService,

            EntityCatalogSO entityCatalog,
            ItemCatalogSO itemCatalog,

            LocalView localView,
            EntityView entityPrefab)
        {
            this.entityService = entityService;

            entityViewFactory = new EntityFactory(
                playerService,
                entityCatalog,
                localView,
                entityPrefab);

            inputBinder = new InputBinder(
                playerService);

            this.entityCatalog = entityCatalog;
            this.itemCatalog = itemCatalog;

            Bind();
        }

        #region Methods
        public void Dispose()
        {
            if (disposed) return;
            disposed = true;

            // Outbound
            entityService.OnEventEntityAdded -= OnEntityAdded;
            entityService.OnEventEntityRemoved -= OnEntityRemoved;
            entityService.OnEventEntityActed -= OnEntityActed;
            entityService.OnEventEntityAppearanceChanged -= OnEntityAppearanceChanged;

            entityViews.Clear();
        }

        private void Bind()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(EntityPresenter));

            // Outbound
            entityService.OnEventEntityAdded += OnEntityAdded;
            entityService.OnEventEntityRemoved += OnEntityRemoved;
            entityService.OnEventEntityActed += OnEntityActed;
            entityService.OnEventEntityAppearanceChanged += OnEntityAppearanceChanged;
        }

        public bool TryGetView(
            string id,
            out EntityView view)
        {
            return entityViews.TryGetValue(id, out view);
        }

        private void OnEntityAdded(
            EntityInstanceDTO runtime,
            Vector2? direction)
        {
            var view = entityViewFactory.Create(runtime, direction);

            entityViews[runtime.Id] = view;

            if (view is LocalView localView)
                inputBinder.Bind(localView);
        }

        private void OnEntityRemoved(
            string id,
            EntityInstanceDTO runtime)
        {
            if (!entityViews.TryGetValue(id, out var view))
                return;

            if (view is LocalView localView)
            {
                inputBinder.Unbind(localView);
                localView.gameObject.SetActive(false);
            }
            else
            {
                GameObject.Destroy(view.gameObject);
            }

            entityViews.Remove(id);
        }

        private void OnEntityActed(
            string id,
            Vector2 newPosition,
            EntityDirection dir,
            EntityAction action,
            string itemDefinitionId = null)
        {
            if (!TryGetView(id, out var view))
                return;

            if (!string.IsNullOrEmpty(itemDefinitionId) && itemCatalog.TryGet(itemDefinitionId, out var item))
                view.HoldItem(item);

            view.SetAction(action);
            view.SetDirection(dir);
            view.ApplyPosition(newPosition);
        }

        private void OnEntityAppearanceChanged(
            string id,
            AppearanceInstanceDTO dto)
        {
            if (!TryGetView(id, out var view))
                return;

            if (!entityCatalog.TryGet(dto.SkinID, out var entity))
                return;

            view.ApplyAppearance(
                entity: entity,
                color: ColorHelper.ToColor(dto.SkinColor),
                name: null);
        }
        #endregion
    }
}