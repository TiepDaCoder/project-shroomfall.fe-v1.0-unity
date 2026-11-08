using Assets.Source.Core;
using Assets.Source.Enum;
using Assets.Source.Service;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Source.UI.Component.Shared
{
    public class CursorInteractable :
        MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerClickHandler,
        IBeginDragHandler,
        IEndDragHandler
    {
        #region Attributes
        [SerializeField] private CursorType hoverCursor = CursorType.Hover;
        [SerializeField] private CursorType dragCursor = CursorType.Drag;
        [SerializeField] private float clickDuration = 0.1f;

        private CursorService cursorService;
        private float clickEndTime;
        private bool isHovered;
        private bool isDragging;
        private bool isClicking;
        #endregion

        #region Properties
        #endregion

        #region Methods
        private void Awake()
        {
            cursorService = ServiceProvider.Get<CursorService>();
        }

        private void Update()
        {
            if (!isClicking || Time.unscaledTime < clickEndTime)
                return;

            isClicking = false;
            if (!isDragging)
                cursorService.SetCursor(isHovered ? hoverCursor : CursorType.Default);
        }

        public void OnPointerEnter(
            PointerEventData eventData)
        {
            isHovered = true;
            if (!isDragging && !isClicking)
                cursorService.SetCursor(hoverCursor);
        }

        public void OnPointerExit(
            PointerEventData eventData)
        {
            isHovered = false;
            if (!isDragging && !isClicking)
                cursorService.SetCursor(CursorType.Default);
        }

        public void OnPointerClick(
            PointerEventData eventData)
        {
            isClicking = true;
            clickEndTime = Time.unscaledTime + clickDuration;
            cursorService.SetCursor(CursorType.Click);
        }

        public void OnBeginDrag(
            PointerEventData eventData)
        {
            isDragging = true;
            isClicking = false;
            cursorService.SetCursor(dragCursor);
        }

        public void OnEndDrag(
            PointerEventData eventData)
        {
            isDragging = false;
            cursorService.SetCursor(isHovered ? hoverCursor : CursorType.Default);
        }

        private void OnDisable()
        {
            isHovered = false;
            isDragging = false;
            isClicking = false;

            if (cursorService != null)
                cursorService.SetCursor(CursorType.Default);
        }
        #endregion
    }
}