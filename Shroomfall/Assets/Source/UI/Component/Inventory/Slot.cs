using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Source.UI.Component.Inventory
{
    public class Slot : MonoBehaviour, IDropHandler
    {
        #region Attributes
        [SerializeField] private UnityEngine.UI.Button button;
        [SerializeField] private UnityEngine.UI.Image selectionBorder;
        #endregion

        #region Properties
        public int Index { get; private set; }
        public SlotItem CurrentItem { get; private set; }

        public Action<int, int> OnItemDropped;
        public Action<int> OnClicked;
        #endregion

        #region Methods
        public void Init(
            int index)
        {
            Index = index;

            if (button != null)
                button.onClick.AddListener(HandleClick);

            if (selectionBorder != null)
                selectionBorder.enabled = false;
        }

        public void SetItemView(
            SlotItem itemView)
        {
            CurrentItem = itemView;

            if (itemView != null)
            {
                var rect = itemView.GetComponent<RectTransform>();

                rect.SetParent(transform, false);
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                rect.pivot = new Vector2(0f, 0f);
            }
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

            // Fire the event so the Presenter can swap the data arrays and trigger a RefreshView()
            OnItemDropped?.Invoke(draggedItem.SlotIndex, Index);
        }

        public void SetSelected(
            bool selected)
        {
            if (selectionBorder != null)
                selectionBorder.enabled = selected;
        }

        private void HandleClick()
        {
            OnClicked?.Invoke(Index);
        }
        #endregion
    }
}