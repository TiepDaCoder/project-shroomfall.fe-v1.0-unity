using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Source.UI.Component.Shared
{
    public class HoverTooltip :
        MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler
    {
        #region Attributes
        [Header("References")]
        [SerializeField] private GameObject tooltipPanel;

        [Header("Animation")]
        [SerializeField] private float moveDistance = 4f;
        [SerializeField] private float moveDuration = 0.6f;

        private RectTransform panelRect;
        private Vector2 originalPosition;
        private Coroutine animationCoroutine;
        #endregion

        #region Properties
        #endregion

        #region Methods
        private void Awake()
        {
            panelRect = tooltipPanel.GetComponent<RectTransform>();
            originalPosition = panelRect.anchoredPosition;
            tooltipPanel.SetActive(false);
        }

        public void OnPointerEnter(
            PointerEventData eventData)
        {
            tooltipPanel.SetActive(true);

            if (animationCoroutine != null)
                StopCoroutine(animationCoroutine);

            animationCoroutine = StartCoroutine(AnimateTooltip());
        }

        public void OnPointerExit(
            PointerEventData eventData)
        {
            HideTooltip();
        }

        private IEnumerator AnimateTooltip()
        {
            while (true)
            {
                yield return MoveTo(originalPosition + Vector2.up * moveDistance);
                yield return MoveTo(originalPosition);
            }
        }

        private IEnumerator MoveTo(Vector2 target)
        {
            Vector2 start = panelRect.anchoredPosition;
            float elapsed = 0f;

            while (elapsed < moveDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / moveDuration);
                t = Mathf.SmoothStep(0f, 1f, t);
                panelRect.anchoredPosition = Vector2.Lerp(start, target, t);
                yield return null;
            }

            panelRect.anchoredPosition = target;
        }

        private void HideTooltip()
        {
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
                animationCoroutine = null;
            }

            if (panelRect != null)
                panelRect.anchoredPosition = originalPosition;

            tooltipPanel.SetActive(false);
        }

        private void OnDisable()
        {
            HideTooltip();
        }
        #endregion
    }
}