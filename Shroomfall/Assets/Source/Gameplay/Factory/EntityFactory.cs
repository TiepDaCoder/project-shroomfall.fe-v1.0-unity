using Assets.Gameplay.Entity;
using Assets.Service;
using Assets.Source.Data;
using Assets.Source.Gameplay.Component.Input;
using Assets.Source.Utilities;
using Contract.DTO.Runtime.EntityDomain;
using Contract.DTO.Runtime.EntityDomain.Component;
using System.Linq;
using UnityEngine;

namespace Assets.Gameplay.Factory
{
    public class EntityFactory
    {
        #region Attributes
        private readonly PlayerService playerService;
        private readonly EntityCatalogSO entityCatalog;
        private readonly LocalView localView;
        private readonly EntityView entityPrefab;
        #endregion

        #region Properties
        #endregion

        public EntityFactory(
            PlayerService playerService,
            EntityCatalogSO entityCatalog,
            LocalView localView,
            EntityView entityPrefab)
        {
            this.playerService = playerService;
            this.entityCatalog = entityCatalog;
            this.localView = localView;
            this.entityPrefab = entityPrefab;
        }

        #region Methods
        public EntityView Create(
            EntityInstanceDTO instance,
            Vector2? direction)
        {
            // Unpack Required Components
            var transformComp = instance.Components
                .OfType<TransformInstanceDTO>()
                .FirstOrDefault();

            var appearanceComp = instance.Components
                .OfType<AppearanceInstanceDTO>()
                .FirstOrDefault();

            var ownershipComp = instance.Components
                .OfType<OwnershipInstanceDTO>()
                .FirstOrDefault();

            // Authorization Check (Is this character controlled by the local client?)
            var localOwnership = playerService.Runtime?.Components
                .OfType<OwnershipInstanceDTO>()
                .FirstOrDefault();

            bool isAuthorizedOwner =
                ownershipComp != null
                && localOwnership != null
                && ownershipComp.UserID == localOwnership.UserID;

            // Decide view to spawn
            EntityView view;
            if (isAuthorizedOwner)
            {
                view = localView;
                view.gameObject.SetActive(true);
            }
            else
            {
                view = GameObject.Instantiate(entityPrefab);
                view.name = instance.DefinitionID;
            }

            // Resolve entity asset
            if (entityCatalog.TryGet(appearanceComp.SkinID, out var entity))
            {
                // Initialize View
                view.Initialize(
                    id: instance.Id,
                    startPosition: new Vector2(transformComp.Position.X, transformComp.Position.Y),
                    dir: transformComp.FacingDirection,
                    action: transformComp.CurrentAction,
                    entity: appearanceComp != null ? entity : null,
                    color: appearanceComp != null ? ColorHelper.ToColor(appearanceComp.SkinColor) : Color.white,
                    name: isAuthorizedOwner ? ownershipComp.UserID : string.Empty,
                    direction: direction
                );
                return view;
            }

            return null;
        }
        #endregion
    }
}