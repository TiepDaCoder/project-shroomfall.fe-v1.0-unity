using Assets.Source.Data;
using Contract.DTO.Definition.MetaDomain;
using Contract.DTO.Runtime.MetaDomain;
using Contract.Enum.MetaDomain.Item;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Source.UI.Component.Inventory
{
    public class SlotEquipment :
            MonoBehaviour,
            IDropHandler,
            IPointerEnterHandler,
            IPointerExitHandler
    {
        #region Attributes
        [Header("Configuration")]
        [SerializeField] private EquipmentSlot slotType;
        [SerializeField] private ItemCatalogSO itemCatalog;

        [Header("UI References")]
        [SerializeField] private Image itemIcon;
        [SerializeField] private GameObject unequipButton;
        [SerializeField] private UnityEngine.UI.Button unequipButtonComp;

        private ItemInstanceDTO currentEquippedItem;
        #endregion

        #region Properties
        public event Action<string> OnEquipRequested;
        public event Action<string, EquipmentSlot> OnUnequipRequested;
        public EquipmentSlot SlotType { get { return slotType; } }
        #endregion

        #region Methods
        private void Awake()
        {
            unequipButton.SetActive(false);
            unequipButtonComp.onClick.AddListener(() => OnUnequipButtonClicked());
        }

        public void Bind(
            ItemInstanceDTO item,
            ItemDefinitionDTO definition)
        {
            currentEquippedItem = item;

            if (currentEquippedItem != null && definition != null)
            {
                if (itemCatalog.TryGet(definition.Id, out var asset) && asset.icon != null)
                {
                    itemIcon.sprite = asset.icon;
                    itemIcon.enabled = true;
                }
                else
                {
                    itemIcon.enabled = false;
                }
            }
            else
            {
                itemIcon.enabled = false;
            }

            if (currentEquippedItem == null && unequipButton != null)
                unequipButton.SetActive(false);
        }

        public void OnDrop(
            PointerEventData eventData)
        {
            if (eventData.pointerDrag == null)
                return;

            // Assuming your dragged prefab has the SlotItem component attached
            var slotItem = eventData.pointerDrag.GetComponent<SlotItem>();

            if (slotItem != null)
                OnEquipRequested?.Invoke(slotItem.Item.ID);
        }

        public void OnPointerEnter(
            PointerEventData eventData)
        {
            if (currentEquippedItem != null && unequipButton != null)
                unequipButton.SetActive(true);
        }

        public void OnPointerExit(
            PointerEventData eventData)
        {
            if (unequipButton != null)
                unequipButton.SetActive(false);
        }

        private void OnUnequipButtonClicked()
        {
            if (currentEquippedItem != null)
                OnUnequipRequested?.Invoke(currentEquippedItem.ID, slotType);
        }
        #endregion
    }
}