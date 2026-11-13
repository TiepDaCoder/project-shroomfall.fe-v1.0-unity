using TMPro;
using UnityEngine;

namespace Assets.Source.UI.Component.Shared
{
    public class CursorMessage : MonoBehaviour
    {
        #region Attributes
        [Header("References")]
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TMP_Text text;

        [Header("Animation")]
        [SerializeField] private float duration = 0.8f;
        [SerializeField] private float riseDistance = 35f;

        private float elapsed;
        private Vector2 startPosition;
        #endregion

        #region Properties
        #endregion

        #region Methods
        private void Update()
        {
            elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(elapsed / duration);

            // Move upward.
            rectTransform.anchoredPosition = startPosition + Vector2.up * (riseDistance * progress);

            // Fade out.
            canvasGroup.alpha = 1f - progress;

            if (progress >= 1f)
                Destroy(gameObject);
        }

        public void Play(
            string message,
            Vector2 position)
        {
            text.text = message;
            startPosition = position;
            rectTransform.anchoredPosition = startPosition;

            canvasGroup.alpha = 1f;
            elapsed = 0f;

            enabled = true;
        }
        #endregion
    }
}