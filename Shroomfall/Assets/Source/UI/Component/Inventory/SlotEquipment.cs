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
    public class SlotEquipment : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
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

        /// <summary>
        /// Binds the currently equipped item data to this visual indicator.
        /// </summary>
        public void Bind(
            ItemInstanceDTO item,
            ItemDefinitionDTO definition)
        {
            currentEquippedItem = item;

            if (currentEquippedItem != null && definition != null)
            {
                if (itemCatalog.TryGet(item.DefinitionID, out var asset))
                {
                    // Update item icon
                    itemIcon.sprite = asset.icon;
                }

                itemIcon.enabled = true;
            }
            else
            {
                itemIcon.enabled = false;
            }

            // Ensure the button hides if the item is unequipped while hovering
            if (currentEquippedItem == null && unequipButton != null)
            {
                unequipButton.SetActive(false);
            }
        }

        /// <summary>
        /// Triggered natively by Unity UI when a draggable object is dropped onto this RectTransform.
        /// </summary>
        public void OnDrop(PointerEventData eventData)
        {
            if (eventData.pointerDrag == null)
                return;

            // Assuming your dragged prefab has the SlotItem component attached
            var slotItem = eventData.pointerDrag.GetComponent<SlotItem>();

            if (slotItem != null)
            {
                OnEquipRequested?.Invoke(slotItem.Item.ID);
            }
        }

        /// <summary>
        /// Triggered when the mouse enters the RectTransform of this UI element.
        /// </summary>
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (currentEquippedItem != null && unequipButton != null)
            {
                unequipButton.SetActive(true);
            }
        }

        /// <summary>
        /// Triggered when the mouse leaves the RectTransform of this UI element.
        /// </summary>
        public void OnPointerExit(PointerEventData eventData)
        {
            if (unequipButton != null)
            {
                unequipButton.SetActive(false);
            }
        }

        /// <summary>
        /// Call this method from the OnClick() UnityEvent on your Unequip UI Button.
        /// </summary>
        public void OnUnequipButtonClicked()
        {
            if (currentEquippedItem != null)
            {
                OnUnequipRequested?.Invoke(currentEquippedItem.ID, slotType);
            }
        }
        #endregion
    }
}