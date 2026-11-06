using Assets.Source.UI.Component.Button;
using Assets.Source.Utility;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Source.UI.Common.Toast
{
    public class ToastView : MonoBehaviour
    {
        #region Attributes
        [Header("Icons")]
        [SerializeField] private Sprite errorIcon;
        [SerializeField] private Sprite informationIcon;
        [SerializeField] private Image icon;

        [Header("Buttons")]
        [SerializeField] private TextButton okButton;

        [Header("TextFields")]
        [SerializeField] private TMP_Text text;

        [Header("Animation")]
        [SerializeField] private RectTransform panel;
        [SerializeField] private float animationDuration = 0.25f;
        [SerializeField] private float showOffsetY = -600f;
        [SerializeField] private float autoHideDelay = 3f;

        private Vector2 initialPosition;
        private Coroutine animateCoroutine;
        private Coroutine autoHideCoroutine;
        #endregion

        #region Properties
        public event Action OnOkClicked;
        #endregion

        #region Methods
        void Awake()
        {
            DontDestroyOnLoad(gameObject);

            initialPosition = panel.anchoredPosition;

            // Buttons
            okButton.onClick.AddListener(() => OnOkClicked?.Invoke());
        }

        public void ShowInformation(
            string message)
        {
            text.text = message;

            icon.sprite = informationIcon;
            RefreshLocalizedText();

            PlayAnimation(initialPosition + Vector2.up * showOffsetY);
            RestartAutoHide();
        }

        public void ShowError(
            string message)
        {
            text.text = message;

            icon.sprite = errorIcon;
            RefreshLocalizedText();

            PlayAnimation(initialPosition + Vector2.up * showOffsetY);
            RestartAutoHide();
        }

        public void Hide()
        {
            if (autoHideCoroutine != null)
            {
                StopCoroutine(autoHideCoroutine);
                autoHideCoroutine = null;
            }

            PlayAnimation(initialPosition);
        }

        private void RestartAutoHide()
        {
            if (autoHideCoroutine != null)
                StopCoroutine(autoHideCoroutine);

            autoHideCoroutine = StartCoroutine(AutoHide());
        }

        private IEnumerator AutoHide()
        {
            yield return new WaitForSecondsRealtime(autoHideDelay);

            Hide();

            autoHideCoroutine = null;
        }

        private void PlayAnimation(
            Vector2 target,
            Action onComplete = null)
        {
            if (animateCoroutine != null)
                StopCoroutine(animateCoroutine);

            animateCoroutine = StartCoroutine(Animate(target, onComplete));
        }

        private IEnumerator Animate(
            Vector2 target,
            Action onComplete)
        {
            Vector2 start = panel.anchoredPosition;

            float t = 0f;

            while (t < animationDuration)
            {
                t += Time.unscaledDeltaTime;

                float p = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / animationDuration));

                panel.anchoredPosition = Vector2.Lerp(start, target, p);

                yield return null;
            }

            panel.anchoredPosition = target;

            animateCoroutine = null;
            onComplete?.Invoke();
        }

        private void RefreshLocalizedText()
        {
            okButton.SetText(UILocalizationTable.Get("toast.btn-ok"));
        }
        #endregion
    }
}