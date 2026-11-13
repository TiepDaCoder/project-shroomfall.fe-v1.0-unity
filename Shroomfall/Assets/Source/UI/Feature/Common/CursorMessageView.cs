using Assets.Source.Core;
using Assets.Source.Service;
using Assets.Source.UI.Component.Shared;
using UnityEngine;

namespace Assets.Source.UI.Feature.Common
{
    public class CursorMessageView : MonoBehaviour
    {
        #region Attributes
        [Header("References")]
        [SerializeField] private Canvas canvas;
        [SerializeField] private RectTransform container;
        [SerializeField] private CursorMessage prefab;

        private UIService uiService;
        #endregion

        #region Properties
        #endregion

        #region Methods
        private void Awake()
        {
            uiService = ServiceProvider.Get<UIService>();
            uiService.OnCursorMessageRequested += OnCursorMessageRequested;
        }

        private void OnDestroy()
        {
            if (uiService != null)
                uiService.OnCursorMessageRequested -= OnCursorMessageRequested;
        }

        private void OnCursorMessageRequested(
            (string message, Vector2 screenPosition) request)
        {
            Camera eventCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : canvas.worldCamera;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                container,
                request.screenPosition,
                eventCamera,
                out Vector2 localPosition))
                return;

            var instance = Instantiate(prefab, container);
            instance.Play(request.message, localPosition);
        }
        #endregion
    }
}