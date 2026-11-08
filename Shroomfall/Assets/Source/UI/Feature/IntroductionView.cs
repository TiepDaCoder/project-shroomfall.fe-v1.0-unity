using Assets.Source.Core;
using Assets.Source.Enum;
using Assets.Source.Service;
using Assets.Source.Utility;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Source.UI.Feature
{
    public class IntroductionView : MonoBehaviour
    {
        #region Attributes
        [Header("Canvas")]
        [SerializeField] private GameObject canvas;

        [Header("Frame 1 - Introduction")]
        [SerializeField] private GameObject introductionFrame;
        [SerializeField] private Button introductionNextButton;

        [Header("Frame 2 - Story")]
        [SerializeField] private GameObject[] firstStoryImages;

        [Header("Frame 3 - Story")]
        [SerializeField] private GameObject[] secondStoryImages;

        [Header("Navigation")]
        [SerializeField] private Button continueButton;
        [SerializeField] private Button previousButton;
        [SerializeField] private Button skipButton;

        [Header("Audio")]
        [SerializeField] private AudioClip introductionAppearSound;
        [SerializeField] private AudioClip storyAppearSound;
        [SerializeField] private AudioClip frameChangeSound;

        [Header("Fade Out")]
        [SerializeField] private CanvasGroup fadeCanvasGroup;
        [SerializeField] private float fadeDuration = 1f;

        private UIService uiService;
        private AudioService audioService;

        private int currentStoryIndex;
        private int currentFrame;
        #endregion

        #region Properties
        #endregion

        #region Methods
        private void Awake()
        {
            uiService = ServiceProvider.Get<UIService>();
            audioService = ServiceProvider.Get<AudioService>();

            // UI Service
            uiService.OnUIStateChanged += OnUIStateChanged;
            uiService.OnGlobalInteractableChanged += SetInteractable;

            // Buttons
            introductionNextButton.onClick.AddListener(OnIntroductionNextClicked);
            continueButton.onClick.AddListener(OnContinueClicked);
            previousButton.onClick.AddListener(OnPreviousClicked);
            skipButton.onClick.AddListener(OnSkipClicked);

            Initialize();
        }

        private void OnDestroy()
        {
            // UI Service
            uiService.OnUIStateChanged -= OnUIStateChanged;
            uiService.OnGlobalInteractableChanged -= SetInteractable;

            // Buttons
            introductionNextButton.onClick.RemoveListener(OnIntroductionNextClicked);
            continueButton.onClick.RemoveListener(OnContinueClicked);
            previousButton.onClick.RemoveListener(OnPreviousClicked);
            skipButton.onClick.RemoveListener(OnSkipClicked);
        }

        private void Initialize()
        {
            currentFrame = 0;
            currentStoryIndex = 0;

            HideAllFrames();
            ShowIntroductionFrame();

            UpdateButtons();
        }

        private void OnUIStateChanged(
            UIService service)
        {
            bool visible = service.ShowIntroduction;
            canvas.SetActive(visible);

            if (visible)
                Initialize();
        }

        private void SetInteractable(
            bool interactable)
        {
            introductionNextButton.interactable = interactable;
            continueButton.interactable = interactable;
            previousButton.interactable = interactable;
            skipButton.interactable = interactable;
        }

        private void OnIntroductionNextClicked()
        {
            currentFrame = 1;
            currentStoryIndex = 0;
            HideAllFrames();
            audioService.PlayUI(frameChangeSound);
            ShowNextStoryImage(firstStoryImages);
            UpdateButtons();
        }

        private void OnContinueClicked()
        {
            if (currentFrame == 1)
            {
                if (currentStoryIndex < firstStoryImages.Length)
                {
                    ShowNextStoryImage(firstStoryImages);
                    return;
                }

                currentFrame = 2;
                currentStoryIndex = 0;
                HideAllFrames();
                audioService.PlayUI(frameChangeSound);
                ShowNextStoryImage(secondStoryImages);
                UpdateButtons();
                return;
            }

            if (currentFrame == 2)
            {
                if (currentStoryIndex < secondStoryImages.Length)
                {
                    ShowNextStoryImage(secondStoryImages);
                    return;
                }

                FinishIntroduction();
            }
        }

        private void OnPreviousClicked()
        {
            // Previous always goes back to the very beginning.
            currentFrame = 0;
            currentStoryIndex = 0;
            HideAllFrames();
            ShowIntroductionFrame();
            audioService.PlayUI(frameChangeSound);
            UpdateButtons();
        }

        private void OnSkipClicked()
        {
            FinishIntroduction();
        }

        private void ShowIntroductionFrame()
        {
            if (introductionFrame != null)
            {
                introductionFrame.SetActive(true);
                audioService.PlayUI(introductionAppearSound);
            }
        }

        private void ShowNextStoryImage(
            GameObject[] storyImages)
        {
            if (currentStoryIndex >= storyImages.Length)
                return;

            GameObject storyImage = storyImages[currentStoryIndex];
            if (storyImage != null)
            {
                storyImage.SetActive(true);
                audioService.PlayUI(storyAppearSound);
            }

            currentStoryIndex++;

            UpdateButtons();
        }

        private void FinishIntroduction()
        {
            StartCoroutine(FadeOutAndFinish());
        }

        private void UpdateButtons()
        {
            bool isIntroduction = currentFrame == 0;

            // Introduction frame has its own Next button.
            introductionNextButton.gameObject.SetActive(isIntroduction);

            // Hide story navigation buttons during the introduction.
            continueButton.gameObject.SetActive(!isIntroduction);
            previousButton.gameObject.SetActive(!isIntroduction);
            skipButton.gameObject.SetActive(!isIntroduction);
        }

        private void HideAllFrames()
        {
            if (introductionFrame != null)
                introductionFrame.SetActive(false);

            foreach (GameObject storyImage in firstStoryImages)
                if (storyImage != null)
                    storyImage.SetActive(false);

            foreach (GameObject storyImage in secondStoryImages)
                if (storyImage != null)
                    storyImage.SetActive(false);
        }

        private IEnumerator FadeOutAndFinish()
        {
            // Prevent further interaction during the fade.
            SetInteractable(false);
            fadeCanvasGroup.blocksRaycasts = true;
            float startAlpha = fadeCanvasGroup.alpha;
            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, elapsed / fadeDuration);
                yield return null;
            }

            fadeCanvasGroup.alpha = 1f;
            HideAllFrames();

            // Change to main menu
            audioService.PlayMusic();
            SceneManager.LoadScene($"{Configuration.GAME_SCENE}");
            uiService.SetState(UIState.MainMenu);
        }
        #endregion
    }
}