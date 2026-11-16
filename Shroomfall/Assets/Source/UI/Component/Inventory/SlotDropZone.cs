using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Source.UI.Component.Inventory
{
    public class SlotDropZone :
        MonoBehaviour,
        IDropHandler
    {
        #region Properties
        public event Action<string> OnDropRequested;
        #endregion

        #region Methods
        public void OnDrop(
            PointerEventData eventData)
        {
            var slotItem = eventData.pointerDrag?.GetComponent<SlotItem>();
            if (slotItem?.Item == null)
                return;

            // Server rejects equipped items; skip the request instead of surfacing an error
            if (slotItem.Item.EquippedSlot.HasValue)
                return;

            OnDropRequested?.Invoke(slotItem.Item.ID);
        }
        #endregion
    }
}