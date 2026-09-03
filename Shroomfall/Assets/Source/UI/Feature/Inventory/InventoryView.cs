using Assets.Source.Data;
using Assets.Source.UI.Abstraction;
using Assets.Source.UI.Component.Inventory;
using Contract.DTO.Definition.MetaDomain;
using Contract.DTO.Runtime.MetaDomain;
using Contract.Enum.MetaDomain.Item;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Source.UI.Feature.Inventory
{
    public class InventoryView : MonoBehaviour, IHUDView
    {
        #region Attributes
        [Header("Root")]
        [SerializeField] private GameObject root;

        [SerializeField] private Slot[] slots;
        [SerializeField] private SlotItem slotItemPrefab;
        [SerializeField] private int toolbarSlotCount = 6;

        [Header("Equipment Indicators")]
        [SerializeField] private SlotEquipment[] equipments;

        [Header("Inspected Item Display")]
        [SerializeField] private TMP_Text inspectedNameText;
        [SerializeField] private TMP_Text inspectedDescriptionText;
        [SerializeField] private Image inspectedCategoryImage;
        [SerializeField] private Image inspectedIconImage;

        [Header("Category Icons")]
        [SerializeField] private Sprite equippableCategoryIcon;
        [SerializeField] private Sprite consumableCategoryIcon;
        [SerializeField] private Sprite materialCategoryIcon;
        [SerializeField] private Sprite placeableCategoryIcon;
        [SerializeField] private Sprite rangedCategoryIcon;
        [SerializeField] private Sprite meleeCategoryIcon;

        [Header("Item Assets")]
        [SerializeField] private ItemCatalogSO itemCatalog;

        private int selectedIndex = 0;

        // Cache definitions once to avoid garbage collection during Bind calls
        private Dictionary<string, ItemDefinitionDTO> cachedDefinitions = new();

        // Quick lookup dictionary for equipment slot indicators
        private Dictionary<EquipmentSlot, SlotEquipment> equipmentIndicators = new();
        #endregion

        #region Properties
        public event Action<int> OnSelectedItemChanged;
        public event Action<int> OnInspectedItemChanged;
        public event Action<string> OnEquipRequested;
        public event Action<string, EquipmentSlot> OnUnequipRequested;
        #endregion

        #region Methods
        private void Awake()
        {
            for (int i = 0; i < slots.Length; i++)
            {
                int index = i;
                slots[i].Init(index);
                slots[i].OnClicked += OnSlotClicked;
            }

            // Initialize equipment indicators directly from the array
            if (equipments != null)
            {
                foreach (var indicator in equipments)
                {
                    if (indicator == null)
                        continue;

                    equipmentIndicators[indicator.SlotType] = indicator;

                    // Forward events upward
                    indicator.OnEquipRequested += (itemId) => OnEquipRequested?.Invoke(itemId);
                    indicator.OnUnequipRequested += (itemId, slot) => OnUnequipRequested?.Invoke(itemId, slot);
                }
            }

            UpdateSelectionVisual();
        }

        public void Show()
        {
            root.SetActive(true);
        }

        public void Hide()
        {
            root.SetActive(false);
        }

        private void Update()
        {
            HandleScrollInput();
        }

        public void SetDefinitions(
            IEnumerable<ItemDefinitionDTO> definitions)
        {
            if (definitions == null)
            {
                cachedDefinitions.Clear();
                return;
            }

            cachedDefinitions = definitions
                .Where(d => d != null && !string.IsNullOrEmpty(d.Id))
                .ToDictionary(d => d.Id, d => d);
        }

        public void DisplayInspectedItem(
            ItemDefinitionDTO item,
            string localizedName,
            string localizedDescription)
        {
            // 1. Update Name & Description
            if (inspectedNameText != null)
                inspectedNameText.text = localizedName;

            if (inspectedDescriptionText != null)
                inspectedDescriptionText.text = localizedDescription;

            // 2. Update Category Image via Switch Case
            if (inspectedCategoryImage != null)
            {
                Sprite categorySprite = GetCategoryIcon(item.Category);
                inspectedCategoryImage.sprite = categorySprite;
                inspectedCategoryImage.gameObject.SetActive(categorySprite != null);
            }

            // 3. Update Item Icon Image
            if (inspectedIconImage != null)
            {
                if (itemCatalog != null && itemCatalog.TryGet(item.Id, out var itemAsset) && itemAsset.icon != null)
                {
                    inspectedIconImage.sprite = itemAsset.icon;
                }
                else
                {
                    inspectedIconImage.sprite = null;
                }
            }
        }

        private Sprite GetCategoryIcon(
            ItemCategory category)
        {
            switch (category)
            {
                case ItemCategory.Equippable:
                    return equippableCategoryIcon;

                case ItemCategory.Material:
                    return materialCategoryIcon;

                case ItemCategory.Consumable:
                    return consumableCategoryIcon;

                case ItemCategory.Placeable:
                    return placeableCategoryIcon;

                case ItemCategory.Ranged:
                    return rangedCategoryIcon;

                case ItemCategory.Melee:
                    return meleeCategoryIcon;

                default:
                    return meleeCategoryIcon;
            }
        }

        public void Bind(ItemInstanceDTO[] items)
        {
            // Clear all inventory slots first
            for (int i = 0; i < slots.Length; i++)
            {
                slots[i].Clear();
            }

            if (items == null)
                return;

            for (int i = 0; i < slots.Length && i < items.Length; i++)
            {
                var item = items[i];
                if (item == null)
                    continue;

                ItemDefinitionDTO definition = null;
                if (!string.IsNullOrEmpty(item.DefinitionID))
                {
                    cachedDefinitions.TryGetValue(item.DefinitionID, out definition);
                }

                // Bind to inventory slot
                var itemView = Instantiate(slotItemPrefab, slots[i].transform, false);
                itemView.Bind(item, definition, i);
                slots[i].SetItemView(itemView);
            }

            // Synchronize equipment indicators based on the newly bound inventory source of truth
            RefreshEquipmentIndicators();
        }

        public void UpdateSlot(
            int visualIndex,
            ItemInstanceDTO item)
        {
            if (visualIndex < 0 || visualIndex >= slots.Length)
                return;

            var slot = slots[visualIndex];

            // If the updated item is null, simply clear the slot
            if (item == null)
            {
                slot.Clear();
            }
            else
            {
                ItemDefinitionDTO definition = null;
                if (!string.IsNullOrEmpty(item.DefinitionID))
                {
                    cachedDefinitions.TryGetValue(item.DefinitionID, out definition);
                }

                // If the slot already has a visual item, update it in-place (Performance boost)
                if (slot.CurrentItem != null)
                {
                    slot.CurrentItem.Bind(item, definition, visualIndex);
                }
                else
                {
                    // Otherwise, it's a new item placed into an empty slot
                    slot.Clear();
                    var itemView = Instantiate(slotItemPrefab, slot.transform, false);
                    itemView.Bind(item, definition, visualIndex);
                    slot.SetItemView(itemView);
                }
            }

            // Synchronize equipment indicators based on the updated inventory source of truth
            RefreshEquipmentIndicators();
        }

        private void RefreshEquipmentIndicators()
        {
            // Reset all indicators first
            foreach (var indicator in equipmentIndicators.Values)
            {
                indicator.Bind(null, null);
            }

            // Scan all inventory slots to find items that are currently equipped
            foreach (var slot in slots)
            {
                if (slot.CurrentItem == null || slot.CurrentItem.Item == null)
                    continue;

                var itemDTO = slot.CurrentItem.Item;
                if (itemDTO.EquippedSlot.HasValue)
                {
                    if (equipmentIndicators.TryGetValue(itemDTO.EquippedSlot.Value, out var indicator))
                    {
                        ItemDefinitionDTO definition = null;
                        if (!string.IsNullOrEmpty(itemDTO.DefinitionID))
                        {
                            cachedDefinitions.TryGetValue(itemDTO.DefinitionID, out definition);
                        }

                        indicator.Bind(itemDTO, definition);
                    }
                }
            }
        }

        private void OnSlotClicked(int index)
        {
            if (index < toolbarSlotCount)
            {
                SelectIndex(index);
            }
            else
            {
                InspectIndex(index);
            }
        }

        private void HandleScrollInput()
        {
            float scroll = Input.mouseScrollDelta.y;
            if (scroll == 0)
                return;

            int direction = scroll > 0 ? -1 : 1;

            int nextIndex = (selectedIndex + direction + toolbarSlotCount) % toolbarSlotCount;
            SelectIndex(nextIndex);
        }

        public void SubscribeToDrop(Action<int, int> callback)
        {
            foreach (var slot in slots)
                slot.OnItemDropped += callback;
        }

        public void UnsubscribeToDrop(Action<int, int> callback)
        {
            foreach (var slot in slots)
                slot.OnItemDropped -= callback;
        }

        private void SelectIndex(int index)
        {
            selectedIndex = index;
            UpdateSelectionVisual();
            OnSelectedItemChanged?.Invoke(selectedIndex);
        }

        private void InspectIndex(int index)
        {
            if (slots[index].CurrentItem != null)
            {
                OnInspectedItemChanged?.Invoke(index);
            }
        }

        private void UpdateSelectionVisual()
        {
            for (int i = 0; i < slots.Length; i++)
            {
                slots[i].SetSelected(i == selectedIndex && i < toolbarSlotCount);
            }
        }
        #endregion
    }
}