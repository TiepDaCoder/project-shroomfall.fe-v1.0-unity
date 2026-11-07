using Assets.Source.Data;
using Assets.Source.Gameplay.Component.Input;
using Assets.Source.Service;
using Assets.Source.Utility;
using Contract.DTO.Runtime.EntityDomain;
using Contract.DTO.Runtime.EntityDomain.Component;
using System.Linq;
using UnityEngine;

namespace Assets.Source.Gameplay.Factory
{
    public class EntityFactory
    {
        #region Attributes
        private readonly PlayerService playerService;
        private readonly EntityCatalogSO entityCatalog;
        private readonly LocalView localPrefab;
        private readonly EntityView entityPrefab;
        #endregion

        #region Properties
        #endregion

        public EntityFactory(
            PlayerService playerService,
            EntityCatalogSO entityCatalog,
            LocalView localPrefab,
            EntityView entityPrefab)
        {
            this.playerService = playerService;
            this.entityCatalog = entityCatalog;
            this.localPrefab = localPrefab;
            this.entityPrefab = entityPrefab;
        }

        #region Methods
        public EntityView Create(
            EntityInstanceDTO instance,
            Vector2? direction)
        {
            // Unpack Required Components
            var transformComp = instance.Components.OfType<TransformInstanceDTO>().FirstOrDefault();
            var appearanceComp = instance.Components.OfType<AppearanceInstanceDTO>().FirstOrDefault();
            var ownershipComp = instance.Components.OfType<OwnershipInstanceDTO>().FirstOrDefault();
            var collisionComp = instance.Components.OfType<CollisionInstanceDTO>().FirstOrDefault();
            var localOwnership = playerService.Runtime?.Components.OfType<OwnershipInstanceDTO>().FirstOrDefault();

            // Decide view to spawn
            EntityView view;

            bool isAuthorizedOwner = ownershipComp != null && localOwnership != null && ownershipComp.UserID == localOwnership.UserID;
            if (isAuthorizedOwner)
            {
                view = GameObject.Instantiate(localPrefab);
                view.name = $"LOCAL-VIEW.{instance.Id}-{instance.DefinitionID}";
            }
            else
            {
                view = GameObject.Instantiate(entityPrefab);
                view.name = $"{instance.Id}-{instance.DefinitionID}";
            }

            // Resolve entity asset
            if (!entityCatalog.TryGet(appearanceComp.SkinID, out var entity))
            {
                GameObject.Destroy(view.gameObject);
                return null;
            }

            view.Initialize(
                id: instance.Id,
                collision: collisionComp,
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
        #endregion
    }
}