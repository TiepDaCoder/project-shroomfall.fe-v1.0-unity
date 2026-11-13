using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Source.UI.Component.Inventory
{
    public class Slot :
        MonoBehaviour,
        IDropHandler,
        IPointerEnterHandler,
        IPointerExitHandler
    {
        #region Attributes
        [SerializeField] private UnityEngine.UI.Button button;

        [Header("Slot Appearance")]
        [SerializeField] private Image slotImage;
        [SerializeField] private Sprite normalSprite;
        [SerializeField] private Sprite hoverSprite;
        #endregion

        #region Properties
        public int Index { get; private set; }
        public SlotItem CurrentItem { get; private set; }

        public Action<int> OnSlotClicked;
        public Action<int, int> OnItemDropped;

        private bool isSelected;
        private bool isHovered;
        #endregion

        #region Methods
        public void Initialize(
            int index)
        {
            Index = index;

            if (button != null)
                button.onClick.AddListener(
                    () => OnSlotClicked?.Invoke(Index));

            UpdateAppearance();
        }

        public void SetItemView(
            SlotItem itemView)
        {
            if (CurrentItem != null)
                CurrentItem.OnItemClicked -= HandleItemClicked;

            CurrentItem = itemView;

            if (itemView == null)
                return;

            itemView.OnItemClicked += HandleItemClicked;

            var rect = itemView.GetComponent<RectTransform>();
            rect.SetParent(transform, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.pivot = new Vector2(0f, 0f);
        }

        private void HandleItemClicked(
            int index)
        {
            OnSlotClicked?.Invoke(index);
        }

        public void Clear()
        {
            if (CurrentItem != null)
                Destroy(CurrentItem.gameObject);

            CurrentItem = null;
        }

        public void OnDrop(
            PointerEventData eventData)
        {
            var draggedItem = eventData.pointerDrag?.GetComponent<SlotItem>();

            if (draggedItem == null)
                return;

            OnItemDropped?.Invoke(draggedItem.SlotIndex, Index);
        }

        public void SetSelected(
            bool selected)
        {
            isSelected = selected;
            UpdateAppearance();
        }

        public void OnPointerEnter(
            PointerEventData eventData)
        {
            isHovered = true;
            UpdateAppearance();
        }

        public void OnPointerExit(
            PointerEventData eventData)
        {
            isHovered = false;
            UpdateAppearance();
        }

        private void UpdateAppearance()
        {
            if (slotImage == null)
                return;

            slotImage.sprite = (isSelected || isHovered) ? hoverSprite : normalSprite;
        }
        #endregion
    }
}