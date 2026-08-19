using Assets.Services;
using Assets.Utilities;
using Contract.DTO.Runtime.EntityDomain.Component;
using Contract.DTO.Runtime.MetaDomain;
using Contract.Enum.MetaDomain.Item;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.UI.Inventory
{
    public class InventoryPresenter : IDisposable
    {
        #region Attributes
        private readonly PlayerService playerService;
        private readonly DefinitionService definitionService;
        private readonly InventoryView inventoryView;

        private const int MAX_SLOTS = 36;
        private readonly Dictionary<string, int> itemToVisualIndex = new Dictionary<string, int>();
        private bool disposed;
        #endregion

        #region Properties
        #endregion

        public InventoryPresenter(
            PlayerService playerService,
            DefinitionService definitionService,
            InventoryView inventoryView)
        {
            this.playerService = playerService;
            this.definitionService = definitionService;
            this.inventoryView = inventoryView;

            inventoryView.SetDefinitions(definitionService.GetItemDefinitions());

            Bind();
            OnInventoryChanged();
        }

        #region Methods
        private void Bind()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(InventoryPresenter));

            // Outbound
            playerService.OnInventoryChanged += OnInventoryChanged;
            playerService.OnInventoryItemUpdated += OnInventoryItemUpdated;

            // Inbound
            inventoryView.SubscribeToDrop(OnItemDropped);
            inventoryView.OnSelectedItemChanged += OnSelectedItemChanged;
            inventoryView.OnInspectedItemChanged += OnInspectedItemChanged;
            inventoryView.OnEquipRequested += OnEquipRequested;
            inventoryView.OnUnequipRequested += OnUnequipRequested;
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;

            // Outbound
            playerService.OnInventoryChanged -= OnInventoryChanged;
            playerService.OnInventoryItemUpdated -= OnInventoryItemUpdated;

            // Inbound
            inventoryView.UnsubscribeToDrop(OnItemDropped);
            inventoryView.OnSelectedItemChanged -= OnSelectedItemChanged;
            inventoryView.OnInspectedItemChanged -= OnInspectedItemChanged;
            inventoryView.OnEquipRequested -= OnEquipRequested;
            inventoryView.OnUnequipRequested -= OnUnequipRequested;
        }

        // ==========================================
        // STATE UPDATES
        // ==========================================
        private void OnInventoryItemUpdated(
            ItemInventorySyncEvent eventType,
            ItemInstanceDTO item)
        {
            if (item == null || string.IsNullOrEmpty(item.ID))
                return;

            switch (eventType)
            {
                case ItemInventorySyncEvent.Added:
                    if (!itemToVisualIndex.ContainsKey(item.ID))
                    {
                        int emptySlot = FindFirstEmptySlot();
                        if (emptySlot != -1)
                        {
                            itemToVisualIndex[item.ID] = emptySlot;
                            inventoryView.UpdateSlot(emptySlot, item);
                        }
                    }
                    break;

                case ItemInventorySyncEvent.Updated:
                    if (itemToVisualIndex.TryGetValue(item.ID, out int visualIndex))
                    {
                        // Standard fast update
                        inventoryView.UpdateSlot(visualIndex, item);
                    }
                    else
                    {
                        OnInventoryChanged();
                    }
                    break;

                case ItemInventorySyncEvent.Removed:
                    if (itemToVisualIndex.TryGetValue(item.ID, out int removeIndex))
                    {
                        itemToVisualIndex.Remove(item.ID);
                        inventoryView.UpdateSlot(removeIndex, null);

                        if (playerService.SelectedInventoryItemID == item.ID)
                        {
                            playerService.SelectedInventoryItemID = null;
                        }
                    }
                    else
                    {
                        OnInventoryChanged();
                    }
                    break;
            }
        }

        private void OnInventoryChanged()
        {
            var inventoryComp = playerService.Runtime?.Components
                .OfType<InventoryInstanceDTO>()
                .FirstOrDefault();

            if (inventoryComp?.Items == null) return;

            var items = inventoryComp.Items;
            var orderedItems = new ItemInstanceDTO[MAX_SLOTS];

            // 1. Clean up removed items: Remove keys that no longer exist in the backend
            var validIds = new HashSet<string>(items.Select(i => i.ID));
            var idsToRemove = itemToVisualIndex.Keys.Where(id => !validIds.Contains(id)).ToList();
            foreach (var id in idsToRemove)
                itemToVisualIndex.Remove(id);

            // 2. Map items to their slots, assigning new slots to new items
            foreach (var item in items)
            {
                if (item == null || string.IsNullOrEmpty(item.ID)) continue;

                if (!itemToVisualIndex.ContainsKey(item.ID))
                {
                    int emptySlot = FindFirstEmptySlot();
                    if (emptySlot != -1)
                        itemToVisualIndex[item.ID] = emptySlot;
                    else
                        continue; // Inventory is visually full
                }

                int slotIndex = itemToVisualIndex[item.ID];
                if (slotIndex >= 0 && slotIndex < MAX_SLOTS)
                {
                    orderedItems[slotIndex] = item;
                }
            }

            // 3. Bind the cleanly organized array to the view
            inventoryView.Bind(orderedItems);
        }

        // ==========================================
        // UI INTERACTIONS
        // ==========================================
        private void OnItemDropped(int fromIndex, int toIndex)
        {
            if (fromIndex == toIndex) return;

            // Reverse lookup: Find which Item IDs are currently sitting in these two slots
            string itemAtFrom = itemToVisualIndex.FirstOrDefault(x => x.Value == fromIndex).Key;
            string itemAtTo = itemToVisualIndex.FirstOrDefault(x => x.Value == toIndex).Key;

            // Swap them in the dictionary
            if (itemAtFrom != null) itemToVisualIndex[itemAtFrom] = toIndex;
            if (itemAtTo != null) itemToVisualIndex[itemAtTo] = fromIndex;

            RefreshSingleSlot(fromIndex);
            RefreshSingleSlot(toIndex);
        }

        private void OnSelectedItemChanged(int visualIndex)
        {
            // Reverse lookup the Item ID from the slot index
            string itemId = itemToVisualIndex.FirstOrDefault(x => x.Value == visualIndex).Key;
            playerService.SelectedInventoryItemID = itemId;
        }

        private void OnInspectedItemChanged(int visualIndex)
        {
            string itemId = itemToVisualIndex.FirstOrDefault(x => x.Value == visualIndex).Key;

            var inventoryComp = playerService.Runtime?.Components
                .OfType<InventoryInstanceDTO>()
                .FirstOrDefault();

            var item = inventoryComp?.Items.FirstOrDefault(i => i.ID == itemId);
            if (item != null)
            {
                // Find matching definition
                var definitions = definitionService.GetItemDefinitions();
                var definition = definitions?.FirstOrDefault(d => d.Id == item.DefinitionID);

                if (definition != null)
                {
                    // Fetch localized text using DefinitionService
                    string localizedName = definitionService.GetLocalizedText(definition.Presentation.LocalizedText.NameKey);
                    string localizedDesc = definitionService.GetLocalizedText(definition.Presentation.LocalizedText.DescriptionKey);

                    // Render inspected item in View
                    inventoryView.DisplayInspectedItem(definition, localizedName, localizedDesc);
                }
            }
        }

        private void OnEquipRequested(
            string itemId)
        {
            AsyncHelper.Run(async () =>
            {
                await playerService.UseEquipmentItem(itemId, null);
            });
        }

        private void OnUnequipRequested(
            string itemId,
            EquipmentSlot equipmentSlot)
        {
            AsyncHelper.Run(async () =>
            {
                await playerService.UseEquipmentItem(itemId, equipmentSlot);
            });
        }

        // ==========================================
        // HELPER METHODS
        // ==========================================

        private void RefreshSingleSlot(int slotIndex)
        {
            var inventoryComp = playerService.Runtime?.Components
                .OfType<InventoryInstanceDTO>()
                .FirstOrDefault();

            if (inventoryComp?.Items == null) return;

            string itemId = itemToVisualIndex.FirstOrDefault(x => x.Value == slotIndex).Key;
            var item = inventoryComp.Items.FirstOrDefault(i => i.ID == itemId);

            inventoryView.UpdateSlot(slotIndex, item);
        }

        private int FindFirstEmptySlot()
        {
            var occupiedSlots = new HashSet<int>(itemToVisualIndex.Values);
            for (int i = 0; i < MAX_SLOTS; i++)
            {
                if (!occupiedSlots.Contains(i))
                    return i;
            }
            return -1;
        }
        #endregion
    }
}