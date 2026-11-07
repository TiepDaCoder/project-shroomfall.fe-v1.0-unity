using Assets.Source.Core;
using Assets.Source.Utility;
using Contract;
using Contract.DTO.Feature.Game.Command;
using Contract.DTO.Feature.Game.Response;
using Contract.DTO.Runtime.EntityDomain;
using Contract.DTO.Runtime.EntityDomain.Component;
using Contract.DTO.Runtime.MetaDomain;
using Contract.Enum.MetaDomain.Item;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Source.Service
{
    public class PlayerService : IService
    {
        #region Attributes
        private NetworkService networkService;
        private InventoryService inventoryService;
        #endregion

        #region Properties
        public bool IsInitialized { get; private set; } = false;
        public EntityInstanceDTO Runtime { get; private set; }

        public event Action OnCharacteristicSync;
        #endregion

        public PlayerService() { }

        #region Methods
        public Task InitializeAsync()
        {
            networkService = ServiceProvider.Get<NetworkService>();
            inventoryService = ServiceProvider.Get<InventoryService>();

            IsInitialized = true;
            return Task.CompletedTask;
        }

        public Task ShutdownAsync()
        {
            return Task.CompletedTask;
        }

        public void LoadPlayerData(
            EntityInstanceDTO runtime)
        {
            Runtime = runtime;
            inventoryService.SyncInventory();
            OnCharacteristicSync?.Invoke();
        }

        public void UnloadPlayerData()
        {
            Runtime = null;
        }

        #region Senders
        public async Task MoveAsync(
            Vector2 direction)
        {
            var dto = new MoveDTO
            {
                X = direction.x,
                Y = direction.y
            };

            await networkService.Send(NetworkMethod.Move, dto);
        }

        public async Task UpdateAppearanceAsync(
            string skinId,
            Color skinColor)
        {
            var dto = new UpdatePlayerAppearanceDTO
            {
                SkinID = skinId,
                SkinColor = ColorHelper.ToHSV(skinColor),
            };

            await HttpCaller.PutAsync<UpdatePlayerAppearanceDTO, VoidResponse>(
                $"{Configuration.GAME_CONTROLLER}appearance", dto);
        }
        #endregion

        #region Receivers
        public void OnPlayerCharacteristicSync(
            CharacteristicInstanceDTO dto)
        {
            CoroutineRunner.Instance.Schedule(() =>
            {
                if (Runtime == null || dto == null)
                    return;

                var existingComp = Runtime.Components
                    .OfType<CharacteristicInstanceDTO>()
                    .FirstOrDefault();

                if (existingComp != null)
                {
                    // Update properties directly
                    existingComp.Cores = dto.Cores ?? new List<AttributeValueInstanceDTO>();
                    existingComp.Vitals = dto.Vitals ?? new List<AttributeValueInstanceDTO>();
                    existingComp.CurrentLevel = dto.CurrentLevel;
                }
                else
                {
                    Runtime.Components.Add(dto);
                }

                OnCharacteristicSync?.Invoke();
            });
        }

        public void OnInventoryItemChanged(
            InventoryItemChangedDTO dto)
        {
            CoroutineRunner.Instance.Schedule(() =>
            {
                if (Runtime == null || dto?.Item == null)
                    return;

                var inventory = Runtime.Components.OfType<InventoryInstanceDTO>().FirstOrDefault();
                if (inventory == null)
                    return;

                ItemInstanceDTO affectedItem = null;
                switch (dto.EventType)
                {
                    case ItemInventorySyncEvent.Added:
                        if (!inventory.Items.Any(i => i.ID == dto.Item.ID))
                            inventory.Items.Add(dto.Item);

                        affectedItem = dto.Item;
                        break;

                    case ItemInventorySyncEvent.Updated:
                        var existingItem = inventory.Items.FirstOrDefault(i => i.ID == dto.Item.ID);
                        if (existingItem == null)
                            return;

                        existingItem.DefinitionID = dto.Item.DefinitionID;
                        existingItem.Amount = dto.Item.Amount;
                        existingItem.Quality = dto.Item.Quality;
                        existingItem.Durability = dto.Item.Durability;
                        existingItem.EquippedSlot = dto.Item.EquippedSlot;

                        affectedItem = existingItem;
                        break;

                    case ItemInventorySyncEvent.Removed:
                        inventory.Items.RemoveAll(i => i != null && i.ID == dto.Item.ID);
                        affectedItem = dto.Item;
                        break;
                }

                if (affectedItem != null)
                    inventoryService.SyncInventory();
            });
        }

        public void OnInventoryCleared()
        {
            CoroutineRunner.Instance.Schedule(() =>
            {
                if (Runtime == null)
                    return;

                var inventory = Runtime.Components?.OfType<InventoryInstanceDTO>().FirstOrDefault();
                if (inventory?.Items == null || inventory.Items.Count == 0)
                    return;

                inventory.Items.Clear();
                inventoryService.SyncInventory();
            });
        }


        public void OnVitalChanged(
            EntityVitalChangedDTO dto)
        {
            CoroutineRunner.Instance.Schedule(() =>
            {
                if (Runtime == null || dto == null)
                    return;

                // Only handle the local player's vital changes.
                if (Runtime.Id != dto.EntityInstanceID)
                    return;

                var characteristic = Runtime.Components?.OfType<CharacteristicInstanceDTO>().FirstOrDefault();
                if (characteristic == null)
                    return;

                var attribute = characteristic.Vitals?.FirstOrDefault(x => x.AttributeType == dto.AttributeType);
                if (attribute == null)
                    return;

                // Update the runtime value.
                attribute.Value = dto.NewValue;

                // Notify HUD subscribers.
                OnCharacteristicSync?.Invoke();
            });
        }
        #endregion
        #endregion
    }
}