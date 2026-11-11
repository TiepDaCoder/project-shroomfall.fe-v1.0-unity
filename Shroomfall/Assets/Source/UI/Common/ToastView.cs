using Assets.Source.Core;
using Assets.Source.Enum;
using Assets.Source.Service;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Source.UI.Feature.Common
{
    public class ToastView : MonoBehaviour
    {
        #region Attributes
        [Header("Icons")]
        [SerializeField] private Sprite errorIcon;
        [SerializeField] private Sprite informationIcon;
        [SerializeField] private Image icon;

        [Header("Buttons")]
        [SerializeField] private Button okButton;

        [Header("Text Fields")]
        [SerializeField] private TMP_Text text;

        [Header("Animation")]
        [SerializeField] private RectTransform panel;
        [SerializeField] private float animationDuration = 0.25f;
        [SerializeField] private float showOffsetY = -600f;
        [SerializeField] private float autoHideDelay = 3f;

        private UIService uiService;

        private Vector2 initialPosition;
        private Coroutine animateCoroutine;
        private Coroutine autoHideCoroutine;
        #endregion

        #region Properties
        #endregion

        #region Methods
        void Awake()
        {
            DontDestroyOnLoad(gameObject);
            initialPosition = panel.anchoredPosition;

            uiService = ServiceProvider.Get<UIService>();

            // UI Service
            uiService.OnToastRequested += OnToastRequested;

            // Buttons
            okButton.onClick.AddListener(Hide);
        }

        private void OnToastRequested(
            (ToastType type, string message) request)
        {
            switch (request.type)
            {
                case ToastType.Information:
                    icon.sprite = informationIcon;
                    break;
                case ToastType.Error:
                    icon.sprite = errorIcon;
                    break;
            }

            text.text = request.message;
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
        #endregion
    }
}