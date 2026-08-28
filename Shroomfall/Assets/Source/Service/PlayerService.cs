using Assets.Source.Core;
using Assets.Source.Utilities;
using Contract;
using Contract.DTO.Feature.Connection.Response;
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
        #endregion

        #region Properties
        public bool IsInitialized { get; private set; } = false;

        public bool HasJoined { get { return Runtime != null; } }
        public string SelectedInventoryItemID { get; set; }

        public EntityInstanceDTO Runtime { get; private set; }

        public event Action<CharacteristicInstanceDTO> OnCharacteristicSync;
        public event Action OnInventoryChanged;
        public event Action<ItemInventorySyncEvent, ItemInstanceDTO> OnInventoryItemUpdated;
        #endregion

        public PlayerService()
        {
        }

        #region Methods
        public Task InitializeAsync()
        {
            IsInitialized = true;
            networkService = ServiceProvider.Get<NetworkService>();
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
            OnInventoryChanged?.Invoke();
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

        public async Task UseItem(
            Vector2 targetDirection)
        {
            var itemInstanceId = SelectedInventoryItemID;

            if (string.IsNullOrEmpty(itemInstanceId))
                return;

            var dto = new UseItemDTO
            {
                ItemInstanceID = itemInstanceId,
                ItemUsageAction = ItemUsageAction.Use,
                UnequippedSlot = null,
                TargetPositionX = targetDirection.x,
                TargetPositionY = targetDirection.y
            };

            await HttpCaller.PostAsync<UseItemDTO, VoidResponse>(
                $"{Configuration.GAME_CONTROLLER}use-item",
                dto
            );
        }

        public async Task UseEquipmentItem(
            string itemInstanceId,
            EquipmentSlot? unequippedSlot)
        {
            if (string.IsNullOrEmpty(itemInstanceId))
                return;

            var dto = new UseItemDTO
            {
                ItemInstanceID = itemInstanceId,
                ItemUsageAction = unequippedSlot != null ? ItemUsageAction.Unequip : ItemUsageAction.Use,
                UnequippedSlot = unequippedSlot,
                TargetPositionX = 0f,
                TargetPositionY = 0f
            };

            await HttpCaller.PostAsync<UseItemDTO, VoidResponse>(
                $"{Configuration.GAME_CONTROLLER}use-item",
                dto
            );
        }

        public async Task<SaveGameDTO> BackHomeAsync()
        {
            return await HttpCaller.PostAsync<object, SaveGameDTO>(
                $"{Configuration.GAME_CONTROLLER}back-home",
                new { }
            );
        }

        public async Task<SaveGameDTO> EnterHubAsync(
            string hubRoomSpatialId)
        {
            return await HttpCaller.PostAsync<object, SaveGameDTO>(
                $"{Configuration.GAME_CONTROLLER}enter-hub/{hubRoomSpatialId}",
                new { }
            );
        }

        public async Task<CombatRunDTO> EnterCombatAsync(
            string combatRunDefinitionId)
        {
            var dto = new CreateCombatRunDTO() { CombatDefinitionID = combatRunDefinitionId };

            return await HttpCaller.PostAsync<CreateCombatRunDTO, CombatRunDTO>(
                $"{Configuration.GAME_CONTROLLER}combat-run",
                dto
            );
        }
        #endregion

        #region Receivers
        public void OnPlayerCharacteristicSync(
            CharacteristicInstanceDTO dto)
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

            OnCharacteristicSync?.Invoke(dto);
        }

        public void OnInventoryItemChanged(
            InventoryItemChangedDTO dto)
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
                    if (!inventory.Items.Any(i => i != null && i.ID == dto.Item.ID))
                    {
                        inventory.Items.Add(dto.Item);
                    }
                    affectedItem = dto.Item;
                    break;

                case ItemInventorySyncEvent.Updated:
                    var existingItem = inventory.Items.FirstOrDefault(i => i != null && i.ID == dto.Item.ID);
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
                OnInventoryItemUpdated?.Invoke(dto.EventType, affectedItem);
        }

        public void OnInventoryCleared()
        {
            if (Runtime == null)
                return;

            var inventory = Runtime.Components?.OfType<InventoryInstanceDTO>().FirstOrDefault();
            if (inventory?.Items == null || inventory.Items.Count == 0)
                return;

            inventory.Items.Clear();

            SelectedInventoryItemID = null;

            OnInventoryChanged?.Invoke();
        }
        #endregion
        #endregion
    }
}