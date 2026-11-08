using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Source.UI.Component.Effect
{
    public class HoverBounce :
        MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerClickHandler
    {
        #region Attributes
        [Header("Hover")]
        [SerializeField] private float hoverScale = 1.5f;
        [SerializeField] private float hoverDuration = 0.15f;

        [Header("Click Bounce")]
        [SerializeField] private float bounceScale = 2.5f;
        [SerializeField] private float bounceDuration = 0.25f;

        private RectTransform rectTransform;
        private Vector3 originalScale;
        private Coroutine scaleCoroutine;
        private bool isHovered;
        private bool isBouncing;
        #endregion

        #region Properties
        #endregion

        #region Methods
        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            originalScale = Vector3.one;
            rectTransform.localScale = originalScale;
        }

        public void OnPointerEnter(
            PointerEventData eventData)
        {
            isHovered = true;
            if (!isBouncing)
                AnimateScale(originalScale * hoverScale, hoverDuration);
        }

        public void OnPointerExit(
            PointerEventData eventData)
        {
            isHovered = false;
            if (!isBouncing)
                AnimateScale(originalScale, hoverDuration);
        }

        public void OnPointerClick(
            PointerEventData eventData)
        {
            if (!isActiveAndEnabled || isBouncing)
                return;

            StopScaleCoroutine();
            scaleCoroutine = StartCoroutine(PlayBounce());
        }

        private void AnimateScale(
            Vector3 targetScale,
            float duration)
        {
            StopScaleCoroutine();
            scaleCoroutine = StartCoroutine(ScaleTo(targetScale, duration));
        }

        private void StopScaleCoroutine()
        {
            if (scaleCoroutine == null)
                return;

            StopCoroutine(scaleCoroutine);
            scaleCoroutine = null;
        }

        private IEnumerator ScaleTo(
            Vector3 targetScale,
            float duration)
        {
            Vector3 startScale = rectTransform.localScale;
            if (duration <= 0f)
            {
                rectTransform.localScale = targetScale;
                scaleCoroutine = null;
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                t = t * t * (3f - 2f * t);
                rectTransform.localScale = Vector3.Lerp(startScale, targetScale, t);
                yield return null;
            }

            rectTransform.localScale = targetScale;
            scaleCoroutine = null;
        }

        private IEnumerator PlayBounce()
        {
            isBouncing = true;
            Vector3 startScale = rectTransform.localScale;
            Vector3 peakScale = originalScale * bounceScale;
            float halfDuration = bounceDuration / 2f;
            float elapsed = 0f;

            // Scale up.
            while (elapsed < halfDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / halfDuration);
                t = 1f - Mathf.Pow(1f - t, 3f);
                rectTransform.localScale = Vector3.Lerp(startScale, peakScale, t);
                yield return null;
            }

            rectTransform.localScale = peakScale;

            // Return to the appropriate scale.
            Vector3 targetScale = isHovered ? originalScale * hoverScale : originalScale;
            elapsed = 0f;

            while (elapsed < halfDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / halfDuration);
                t = 1f - Mathf.Pow(1f - t, 3f);
                rectTransform.localScale = Vector3.Lerp(peakScale, targetScale, t);
                yield return null;
            }

            // Guarantee the exact final scale.
            rectTransform.localScale = targetScale;
            isBouncing = false;
            scaleCoroutine = null;
        }

        private void OnDisable()
        {
            StopScaleCoroutine();

            isHovered = false;
            isBouncing = false;

            if (rectTransform != null)
                rectTransform.localScale = originalScale;
        }
        #endregion
    }
}