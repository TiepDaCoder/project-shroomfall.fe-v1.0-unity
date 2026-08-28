using Assets.Source.Core;
using Contract.DTO.Feature.Game.Response;
using Contract.DTO.Runtime.EntityDomain;
using Contract.DTO.Runtime.EntityDomain.Component;
using Contract.Enum.EntityDomain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Source.Service
{
    public class EntityService : IService
    {
        #region Attributes
        private readonly Dictionary<string, EntityInstanceDTO> entities = new();
        #endregion

        #region Properties
        public bool IsInitialized { get; private set; } = false;
        public event Action<EntityInstanceDTO, Vector2?> OnEventEntityAdded;
        public event Action<string, EntityInstanceDTO> OnEventEntityRemoved;
        public event Action<string, Vector2, EntityDirection, EntityAction, string> OnEventEntityActed;
        public event Action<string, AppearanceInstanceDTO> OnEventEntityAppearanceChanged;
        #endregion

        public EntityService()
        {
        }

        #region Methods
        public Task InitializeAsync()
        {
            IsInitialized = true;
            return Task.CompletedTask;
        }

        public Task ShutdownAsync()
        {
            return Task.CompletedTask;
        }

        public void UnloadEntitiesData()
        {
            foreach (var kvp in new Dictionary<string, EntityInstanceDTO>(entities))
            {
                RemoveEntity(kvp.Key);
                OnEventEntityRemoved?.Invoke(kvp.Key, kvp.Value);
            }
        }

        public void LoadEntitiesData(
            IEnumerable<EntityInstanceDTO> entityList)
        {
            // Clear existing data
            UnloadEntitiesData();

            // Load into dictionary
            int count = 0;
            foreach (var entity in entityList)
            {
                if (entity == null || string.IsNullOrEmpty(entity.Id)) continue;

                entities[entity.Id] = entity;
                count++;
            }

            // Fire events safely
            foreach (var entity in entities.Values)
            {
                OnEventEntityAdded?.Invoke(entity, null);
            }
        }

        #region Senders
        #endregion

        #region Receivers
        public void OnEntitySpawned(
            EntitySpawnedDTO dto)
        {
            CoroutineRunner.Instance.Schedule(() =>
                {
                    if (entities.ContainsKey(dto.EntityInstance.Id)) return;
                    entities[dto.EntityInstance.Id] = dto.EntityInstance;

                    Vector2? direction = null;
                    if (dto.Direction != null)
                        direction = new Vector2(dto.Direction.X, dto.Direction.Y);

                    OnEventEntityAdded?.Invoke(dto.EntityInstance, direction);
                });
        }

        public void OnEntityDespawned(
            string id)
        {
            CoroutineRunner.Instance.Schedule(() =>
                    RemoveEntity(id)
            );
        }

        public void OnEntityActed(
            EntityActedDTO dto)
        {
            CoroutineRunner.Instance.Schedule(() =>
            {
                if (TryGetEntity(dto.EntityInstanceID, out var entity))
                {
                    // Update visual
                    OnEventEntityActed?.Invoke(dto.EntityInstanceID, new Vector2(dto.X, dto.Y), dto.Direction, dto.Action, dto.UsedItemDefinitionID);

                    var transformComp = entity.Components.OfType<TransformInstanceDTO>().FirstOrDefault();

                    if (transformComp != null)
                    {
                        transformComp.Position.X = dto.X;
                        transformComp.Position.Y = dto.Y;
                        transformComp.FacingDirection = dto.Direction;
                        transformComp.CurrentAction = dto.Action;
                    }
                }
            });
        }

        public void OnPlayerAppearanceChanged(
            EntityAppearanceChangedDTO dto)
        {
            CoroutineRunner.Instance.Schedule(() =>
            {
                if (!TryGetEntity(dto.EntityInstanceID, out var entity))
                    return;

                var existingAppearance = entity.Components.OfType<AppearanceInstanceDTO>().FirstOrDefault();

                if (existingAppearance != null)
                {
                    existingAppearance.SkinID = dto.Appearance.SkinID;
                    existingAppearance.SkinColor = dto.Appearance.SkinColor;
                }
                else
                {
                    entity.Components.Add(dto.Appearance);
                }

                // Notify presenter/view
                OnEventEntityAppearanceChanged?.Invoke(dto.EntityInstanceID, dto.Appearance);
            });
        }

        public void OnEntityVitalChanged(
            EntityVitalChangedDTO dto)
        {

        }
        #endregion

        private bool TryGetEntity(
            string id, out
            EntityInstanceDTO entity)
        {
            return entities.TryGetValue(id, out entity);
        }

        private void RemoveEntity(
            string id)
        {
            if (!entities.TryGetValue(id, out var entity)) return;
            entities.Remove(id);

            OnEventEntityRemoved?.Invoke(id, entity);
        }
        #endregion
    }
}