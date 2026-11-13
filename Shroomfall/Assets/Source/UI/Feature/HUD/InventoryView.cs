using Assets.Source.Core;
using Assets.Source.Data;
using Assets.Source.Service;
using Assets.Source.UI.Component.Inventory;
using Assets.Source.Utility;
using Contract.Enum.MetaDomain.Item;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Assets.Source.UI.Feature.HUD
{
    public class InventoryView : MonoBehaviour, IHUDView
    {
        #region Attributes
        [Header("Root")]
        [SerializeField] private GameObject root;

        [Header("Slots")]
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

        [Header("Data")]
        [SerializeField] private ItemCatalogSO itemCatalog;
        [SerializeField] private ItemCategoryCatalogSO itemCategoryCatalog;

        [Header("Audio")]
        [SerializeField] private AudioClip slotSelectionSound;

        private InventoryService inventoryService;
        private DefinitionService definitionService;
        private AudioService audioService;

        private int selectedIndex;
        private Dictionary<EquipmentSlot, SlotEquipment> equipmentIndicators = new();
        #endregion

        #region Properties
        #endregion

        #region Methods
        private void Awake()
        {
            inventoryService = ServiceProvider.Get<InventoryService>();
            definitionService = ServiceProvider.Get<DefinitionService>();
            audioService = ServiceProvider.Get<AudioService>();

            InitializeSlots();
            InitializeEquipmentIndicators();

            inventoryService.OnChanged += OnInventoryChanged;
        }

        private void OnDestroy()
        {
            // Inventory Service
            inventoryService.OnChanged -= OnInventoryChanged;

            // Slots
            foreach (var slot in slots)
            {
                slot.OnSlotClicked -= OnSlotClicked;
                slot.OnItemDropped -= OnItemDropped;
            }

            // Equipment Indicators
            foreach (var indicator in equipments)
            {
                if (indicator == null)
                    continue;

                indicator.OnEquipRequested -= OnEquipRequested;
                indicator.OnUnequipRequested -= OnUnequipRequested;
            }
        }

        private void Update()
        {
            HandleScrollInput();
        }

        #region HUD Action
        public void Show()
        {
            root.SetActive(true);
            OnInventoryChanged();
            UpdateSelectionVisual();
        }

        public void Hide()
        {
            root.SetActive(false);
        }
        #endregion

        #region Initialization
        private void InitializeSlots()
        {
            for (int i = 0; i < slots.Length; i++)
            {
                int index = i;

                slots[i].Initialize(index);
                slots[i].OnSlotClicked += OnSlotClicked;
                slots[i].OnItemDropped += OnItemDropped;
            }
        }

        private void InitializeEquipmentIndicators()
        {
            if (equipments == null)
                return;

            foreach (var indicator in equipments)
            {
                if (indicator == null)
                    continue;

                equipmentIndicators[indicator.SlotType] = indicator;

                indicator.OnEquipRequested += OnEquipRequested;
                indicator.OnUnequipRequested += OnUnequipRequested;
            }
        }
        #endregion

        #region Inventory Binding
        private void OnInventoryChanged()
        {
            for (int i = 0; i < slots.Length; i++)
                slots[i].Clear();

            if (inventoryService.IndexedInventory == null)
                return;

            for (int i = 0; i < slots.Length && i < inventoryService.IndexedInventory.Length; i++)
            {
                var item = inventoryService.IndexedInventory[i];
                if (item == null)
                    continue;

                var definition = definitionService.GetItemDefinition(item.DefinitionID);
                var itemView = Instantiate(slotItemPrefab, slots[i].transform, false);
                itemView.Bind(item, definition, i);
                slots[i].SetItemView(itemView);
            }

            RefreshEquipmentIndicators();
        }

        private void OnSlotClicked(
            int index)
        {
            InspectIndex(index);

            if (index < toolbarSlotCount)
                SelectIndex(index);
        }

        private void OnItemDropped(
            int fromIndex,
            int toIndex)
        {
            inventoryService.SwapInventorySlots(fromIndex, toIndex);
        }

        private void OnEquipRequested(
            string itemId)
        {
            AsyncHelper.Run(async () =>
            {
                await inventoryService.UseEquipmentItem(itemId, null);
            });
        }

        private void OnUnequipRequested(
            string itemId,
            EquipmentSlot equipmentSlot)
        {
            AsyncHelper.Run(async () =>
            {
                await inventoryService.UseEquipmentItem(itemId, equipmentSlot);
            });
        }

        private void SelectIndex(
            int index)
        {
            if (selectedIndex != index)
                audioService.PlayUI(slotSelectionSound);

            selectedIndex = index;
            UpdateSelectionVisual();
            var item = inventoryService.GetInventoryItem(index);
            inventoryService.SetSelectedItem(item?.ID);
        }

        private void InspectIndex(
            int index)
        {
            var item = inventoryService.GetInventoryItem(index);
            if (item == null)
            {
                ClearInspection();
                return;
            }

            var definition = definitionService.GetItemDefinition(item.DefinitionID);
            if (definition == null)
            {
                ClearInspection();
                return;
            }

            // Display the inspected item information
            if (inspectedNameText != null)
            {
                inspectedNameText.gameObject.SetActive(true);
                inspectedNameText.text = definitionService.GetLocalizedText(definition.Presentation.LocalizedText.NameKey);
            }

            if (inspectedDescriptionText != null)
            {
                inspectedDescriptionText.gameObject.SetActive(true);
                inspectedDescriptionText.text = definitionService.GetLocalizedText(definition.Presentation.LocalizedText.DescriptionKey);
            }

            // Display inspected item category
            if (inspectedCategoryImage != null)
            {
                bool hasCategory = itemCategoryCatalog.TryGet(definition.Category, out Sprite categoryIcon);
                inspectedCategoryImage.sprite = categoryIcon;
                inspectedCategoryImage.gameObject.SetActive(hasCategory && categoryIcon != null);
            }

            // Display inspected item icon
            if (inspectedIconImage != null)
            {
                bool hasIcon = itemCatalog.TryGet(definition.Id, out var itemAsset) && itemAsset.icon != null;
                inspectedIconImage.sprite = hasIcon ? itemAsset.icon : null;
                inspectedIconImage.gameObject.SetActive(hasIcon);
            }
        }

        private void ClearInspection()
        {
            if (inspectedNameText != null)
            {
                inspectedNameText.text = string.Empty;
                inspectedNameText.gameObject.SetActive(false);
            }

            if (inspectedDescriptionText != null)
            {
                inspectedDescriptionText.text = string.Empty;
                inspectedDescriptionText.gameObject.SetActive(false);
            }

            if (inspectedCategoryImage != null)
            {
                inspectedCategoryImage.sprite = null;
                inspectedCategoryImage.gameObject.SetActive(false);
            }

            if (inspectedIconImage != null)
            {
                inspectedIconImage.sprite = null;
                inspectedIconImage.gameObject.SetActive(false);
            }
        }

        private void RefreshEquipmentIndicators()
        {
            foreach (var indicator in equipmentIndicators.Values)
                indicator.Bind(null, null);

            foreach (var slot in slots)
            {
                if (slot.CurrentItem == null || slot.CurrentItem.Item == null)
                    continue;

                var item = slot.CurrentItem.Item;
                if (!item.EquippedSlot.HasValue)
                    continue;

                if (!equipmentIndicators.TryGetValue(item.EquippedSlot.Value, out var indicator))
                    continue;

                var definition = definitionService.GetItemDefinition(item.DefinitionID);
                indicator.Bind(item, definition);
            }
        }
        #endregion

        // ==========================================
        // TOOLBAR
        // ==========================================
        private void HandleScrollInput()
        {
            if (Mouse.current == null)
                return;

            float scroll = Mouse.current.scroll.ReadValue().y;
            if (scroll == 0)
                return;

            int direction = scroll > 0 ? -1 : 1;
            int nextIndex = (selectedIndex + direction + toolbarSlotCount) % toolbarSlotCount;
            SelectIndex(nextIndex);
        }

        private void UpdateSelectionVisual()
        {
            for (int i = 0; i < slots.Length; i++)
                slots[i].SetSelected(i == selectedIndex && i < toolbarSlotCount);
        }
        #endregion
    }
}