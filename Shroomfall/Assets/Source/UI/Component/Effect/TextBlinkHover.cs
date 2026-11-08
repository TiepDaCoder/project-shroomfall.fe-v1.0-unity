using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Source.UI.Component.Effect
{
    public class TextBlinkHover :
        MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerClickHandler
    {
        #region Attributes
        [SerializeField] private TMP_Text targetText;
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color darkColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        [SerializeField] private float blinkSpeed = 4f;
        [SerializeField] private int blinkSteps = 3;
        [SerializeField] private float clickDuration = 0.15f;
        [SerializeField] private int clickSteps = 3;

        private bool isHovered;
        private Coroutine clickCoroutine;
        #endregion

        #region Properties
        #endregion

        #region Methods
        private void Update()
        {
            if (isHovered || targetText == null) return;
            float sine = (Mathf.Sin(Time.time * blinkSpeed) + 1f) / 2f;

            // Convert smooth value into discrete steps.
            float stepped = Mathf.Floor(sine * blinkSteps) / blinkSteps;
            targetText.color = Color.Lerp(darkColor, normalColor, stepped);
        }

        public void OnPointerEnter(
            PointerEventData eventData)
        {
            isHovered = true;
            if (targetText != null)
                targetText.color = darkColor;
        }

        public void OnPointerExit(
            PointerEventData eventData)
        {
            isHovered = false;
            if (targetText != null)
                targetText.color = normalColor;
        }

        public void OnPointerClick(
            PointerEventData eventData)
        {
            if (targetText == null)
                return;

            // The button may have been disabled by another click listener.
            if (!isActiveAndEnabled || !gameObject.activeInHierarchy)
                return;

            if (clickCoroutine != null)
                StopCoroutine(clickCoroutine);

            clickCoroutine = StartCoroutine(PlayClickFeedback());
        }

        private IEnumerator PlayClickFeedback()
        {
            float stepDuration = clickDuration / (clickSteps * 2f);

            for (int i = 1; i <= clickSteps; i++)
            {
                float t = (float)i / clickSteps;
                targetText.color = Color.Lerp(normalColor, darkColor, t);
                yield return new WaitForSecondsRealtime(stepDuration);
                if (!isActiveAndEnabled || !gameObject.activeInHierarchy)
                    yield break;
            }

            for (int i = clickSteps - 1; i >= 0; i--)
            {
                float t = (float)i / clickSteps;
                targetText.color = Color.Lerp(normalColor, darkColor, t);
                yield return new WaitForSecondsRealtime(stepDuration);
                if (!isActiveAndEnabled || !gameObject.activeInHierarchy)
                    yield break;
            }

            targetText.color = isHovered ? darkColor : normalColor;
            clickCoroutine = null;
        }

        private void OnDisable()
        {
            if (clickCoroutine != null)
            {
                StopCoroutine(clickCoroutine);
                clickCoroutine = null;
            }

            isHovered = false;
        }
        #endregion
    }
}