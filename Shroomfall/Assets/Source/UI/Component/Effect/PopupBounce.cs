using System.Collections;
using UnityEngine;

namespace Assets.Source.UI.Component.Effect
{
    public class PopupBounce : MonoBehaviour
    {
        #region Attributes
        [Header("Animation")]
        [SerializeField] private float duration = 0.4f;
        [SerializeField] private float startScale = 0.5f;
        [SerializeField] private float bounceScale = 1.1f;

        private RectTransform rectTransform;
        private Coroutine animationCoroutine;
        private Vector3 targetScale;
        #endregion

        #region Properties
        #endregion

        #region Methods
        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            targetScale = Vector3.one;
        }

        private void OnEnable()
        {
            if (rectTransform == null)
                return;

            targetScale = Vector3.one;

            if (animationCoroutine != null)
                StopCoroutine(animationCoroutine);

            animationCoroutine = StartCoroutine(PlayBounce());
        }

        private IEnumerator PlayBounce()
        {
            float halfDuration = duration / 2f;
            float elapsed = 0f;

            rectTransform.localScale = targetScale * startScale;

            // Phase 1: Scale up with overshoot.
            while (elapsed < halfDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / halfDuration);
                float easedT = 1f - Mathf.Pow(1f - t, 3f);
                rectTransform.localScale = Vector3.Lerp(targetScale * startScale, targetScale * bounceScale, easedT);
                yield return null;
            }

            elapsed = 0f;

            // Phase 2: Settle back to the original scale.
            while (elapsed < halfDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / halfDuration);
                float easedT = 1f - Mathf.Pow(1f - t, 3f);
                rectTransform.localScale = Vector3.Lerp(targetScale * bounceScale, targetScale, easedT);
                yield return null;
            }

            rectTransform.localScale = targetScale;
            animationCoroutine = null;
        }
        #endregion
    }
}