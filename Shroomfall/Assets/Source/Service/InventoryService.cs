using Assets.Source.Core;
using Assets.Source.Utility;
using Contract.DTO.Feature.Game.Command;
using Contract.DTO.Runtime.EntityDomain.Component;
using Contract.DTO.Runtime.MetaDomain;
using Contract.Enum.MetaDomain.Effect;
using Contract.Enum.MetaDomain.Item;
using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Source.Service
{
    public class InventoryService : IService
    {
        #region Attributes
        private const int MAX_INVENTORY_SLOTS = 96;
        private PlayerService playerService;
        #endregion

        #region Properties
        public bool IsInitialized { get; private set; } = false;
        public string SelectedItemID { get; private set; }
        public ItemInstanceDTO[] IndexedInventory { get; private set; } = new ItemInstanceDTO[MAX_INVENTORY_SLOTS];

        public event Action OnChanged;
        public event Action<string> OnSelectedItemChanged;
        #endregion

        public InventoryService() { }

        #region Methods
        public Task InitializeAsync()
        {
            playerService = ServiceProvider.Get<PlayerService>();

            IsInitialized = true;
            return Task.CompletedTask;
        }

        public Task ShutdownAsync()
        {
            return Task.CompletedTask;
        }

        public void SyncInventory()
        {
            ReindexInventory();
            OnChanged?.Invoke();
        }

        public ItemInstanceDTO GetInventoryItem(
            int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= MAX_INVENTORY_SLOTS)
                return null;

            return IndexedInventory[slotIndex];
        }

        public void SwapInventorySlots(
            int fromIndex,
            int toIndex)
        {
            if (fromIndex == toIndex)
                return;

            if (fromIndex < 0 || fromIndex >= MAX_INVENTORY_SLOTS || toIndex < 0 || toIndex >= MAX_INVENTORY_SLOTS)
                return;

            // Simple local swap in the array view until backend handles slot positions
            var temp = IndexedInventory[fromIndex];
            IndexedInventory[fromIndex] = IndexedInventory[toIndex];
            IndexedInventory[toIndex] = temp;

            OnChanged?.Invoke();
        }

        private void ReindexInventory()
        {
            Array.Clear(IndexedInventory, 0, IndexedInventory.Length);

            var inventory = playerService.Runtime?.Components?.OfType<InventoryInstanceDTO>().FirstOrDefault();
            if (inventory?.Items == null)
                return;

            for (int i = 0; i < inventory.Items.Count && i < MAX_INVENTORY_SLOTS; i++)
                IndexedInventory[i] = inventory.Items[i];
        }

        public void SetSelectedItem(
            string itemInstanceID)
        {
            SelectedItemID = itemInstanceID;
            if (string.IsNullOrEmpty(itemInstanceID))
            {
                OnSelectedItemChanged?.Invoke(null);
                return;
            }

            var item = IndexedInventory.FirstOrDefault(x => x?.ID == itemInstanceID);
            if (item == null)
            {
                SelectedItemID = null;
                OnSelectedItemChanged?.Invoke(null);
                return;
            }

            OnSelectedItemChanged?.Invoke(item.DefinitionID);
        }

        private void ShowEnoughEnergy()
        {
            var characteristic = playerService.Runtime?.Components?.OfType<CharacteristicInstanceDTO>().FirstOrDefault();
            if (characteristic == null)
                return;

            float energy = characteristic.Vitals.FirstOrDefault(x => x.AttributeType == AttributeType.Energy).Value;
            if (energy <= 0f)
            {
                var uiService = ServiceProvider.Get<UIService>();
                uiService.ShowCursorMessage("cursor-message.not-enough-energy", Mouse.current.position.ReadValue());
            }
        }
        #region Senders
        public async Task CraftItem(
            string itemDefinitionId)
        {
            if (string.IsNullOrEmpty(itemDefinitionId))
                return;

            var dto = new CraftItemDTO
            {
                ItemDefinitionID = itemDefinitionId,
            };

            await HttpCaller.PostAsync<CraftItemDTO, VoidResponse>(
                $"{Configuration.GAME_CONTROLLER}craft-item", dto);
        }

        public async Task UseItem(
            Vector2 targetDirection)
        {
            ShowEnoughEnergy();

            if (string.IsNullOrEmpty(SelectedItemID))
                return;

            var dto = new UseItemDTO
            {
                ItemInstanceID = SelectedItemID,
                ItemUsageAction = ItemUsageAction.Use,
                UnequippedSlot = null,
                TargetPositionX = targetDirection.x,
                TargetPositionY = targetDirection.y
            };

            await HttpCaller.PostAsync<UseItemDTO, VoidResponse>(
                $"{Configuration.GAME_CONTROLLER}use-item", dto);
        }

        public async Task UseEquipmentItem(
            string itemInstanceId,
            EquipmentSlot? unequippedSlot)
        {
            ShowEnoughEnergy();

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
                $"{Configuration.GAME_CONTROLLER}use-item", dto);
        }
        #endregion

        #region Receivers
        #endregion
        #endregion
    }
}