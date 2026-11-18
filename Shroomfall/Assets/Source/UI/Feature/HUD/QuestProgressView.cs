using Assets.Source.Core;
using Assets.Source.Service;
using Assets.Source.UI.Component.QuestProgress;
using Assets.Source.Utility;
using Contract.DTO.Runtime.EntityDomain.Component;
using Contract.DTO.Runtime.MetaDomain;
using Contract.Enum.EntityDomain;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Source.UI.Feature.HUD
{
    public class QuestProgressView : MonoBehaviour
    {
        #region Attributes
        [Header("References")]
        [SerializeField] private Transform content;
        [SerializeField] private QuestProgressItem questProgressItemPrefab;

        [Header("Toggle")]
        [SerializeField] private Button toggleButton;
        [SerializeField] private RectTransform listRoot;
        [SerializeField] private bool startShown = true;
        [Tooltip("How far left the list slides when hidden. 0 = the list's own width.")]
        [SerializeField] private float slideDistance = 0f;
        [SerializeField] private float slideDuration = 0.35f;

        [Header("Update Notify")]
        [SerializeField] private MushroomParticle mushroomParticlePrefab;
        [Tooltip("Parent the particles spawn under; put it above the list so they draw on top.")]
        [SerializeField] private RectTransform particleRoot;
        [SerializeField] private int particleBurstCount = 30;
        [SerializeField] private GameObject updateIndicator;

        private PlayerService playerService;
        private QuestService questService;

        private readonly List<QuestProgressItem> items = new();
        private int renderVersion;

        private Vector2 shownPosition;
        private Vector2 hiddenPosition;
        private bool isShown;
        private Coroutine slideCoroutine;
        #endregion

        #region Properties
        #endregion

        #region Methods
        private void Awake()
        {
            playerService = ServiceProvider.Get<PlayerService>();
            questService = ServiceProvider.Get<QuestService>();

            // Shown position is wherever the list is placed in the editor
            shownPosition = listRoot.anchoredPosition;
            float distance = slideDistance > 0f ? slideDistance : listRoot.rect.width;
            hiddenPosition = shownPosition - new Vector2(distance, 0f);

            isShown = startShown;
            listRoot.anchoredPosition = isShown ? shownPosition : hiddenPosition;
            updateIndicator.SetActive(false);

            // Player Service
            playerService.OnQuestProgressSync += QuestProgressSync;

            // Buttons
            toggleButton.onClick.AddListener(Toggle);

            QuestProgressSync(false);
        }

        private void OnDestroy()
        {
            // Player Service
            playerService.OnQuestProgressSync -= QuestProgressSync;

            // Buttons
            toggleButton.onClick.RemoveAllListeners();

            ClearItems();
        }

        #region Toggle
        private void Toggle()
        {
            isShown = !isShown;

            // Opening the list means the player has seen the update
            if (isShown)
                updateIndicator.SetActive(false);

            if (slideCoroutine != null)
                StopCoroutine(slideCoroutine);

            slideCoroutine = StartCoroutine(Slide(isShown ? shownPosition : hiddenPosition));
        }

        private IEnumerator Slide(
            Vector2 target)
        {
            Vector2 start = listRoot.anchoredPosition;
            float elapsed = 0f;

            while (elapsed < slideDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / slideDuration);
                float easedT = 1f - Mathf.Pow(1f - t, 3f);
                listRoot.anchoredPosition = Vector2.Lerp(start, target, easedT);
                yield return null;
            }

            listRoot.anchoredPosition = target;
            slideCoroutine = null;
        }
        #endregion

        #region Quest Progress
        private void QuestProgressSync(
            bool fromServer)
        {
            if (fromServer)
                NotifyUpdate();

            // Only active quests and ones waiting for reward claim
            var entries = playerService?.Runtime?.Components?
                .OfType<QuestProgressInstanceDTO>().FirstOrDefault()?.Entries?
                .Where(x => x.Status == QuestProgressStatus.InProgress || x.Status == QuestProgressStatus.Completed)
                .ToList();

            int version = ++renderVersion;

            AsyncHelper.Run(async () =>
            {
                var rows = new List<(QuestProgressEntryDTO Entry, QuestInstanceDTO Quest)>();
                foreach (var entry in entries ?? new List<QuestProgressEntryDTO>())
                {
                    var quest = await questService.FetchQuest(entry.QuestInstanceId);
                    if (quest != null)
                        rows.Add((entry, quest));
                }

                // Drop stale renders when a newer sync arrived mid-fetch
                if (version != renderVersion || this == null)
                    return;

                ClearItems();
                foreach (var (entry, quest) in rows)
                {
                    var item = Instantiate(questProgressItemPrefab, content);
                    item.Render(entry, quest);
                    items.Add(item);
                }
            });
        }

        private void NotifyUpdate()
        {
            // Burst from the toggle button's center
            Vector3 origin = toggleButton.transform.position;
            for (int i = 0; i < particleBurstCount; i++)
            {
                var particle = Instantiate(mushroomParticlePrefab, particleRoot);
                particle.transform.position = origin;
            }

            // The dot only matters while the list is hidden
            if (!isShown)
                updateIndicator.SetActive(true);
        }

        private void ClearItems()
        {
            foreach (var item in items)
            {
                if (item != null)
                    Destroy(item.gameObject);
            }

            items.Clear();
        }
        #endregion
        #endregion
    }
}