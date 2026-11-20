using Assets.Source.Core;
using Assets.Source.Data;
using Assets.Source.Service;
using Assets.Source.UI.Component.QuestProgress;
using Assets.Source.Utility;
using Contract.DTO.Runtime.EntityDomain.Component;
using Contract.DTO.Runtime.MetaDomain;
using Contract.Enum.EntityDomain;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
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

        [Header("Gacha Popup")]
        [SerializeField] private RectTransform gachaPopupRoot;
        [SerializeField] private Image gachaItemIcon;
        [SerializeField] private TMP_Text gachaResultText;
        [SerializeField] private Button gachaCloseButton;
        [SerializeField] private Animator gachaMachineAnimator;
        [SerializeField] private ItemCatalogSO itemCatalog;
        [SerializeField] private float machineFallDuration = 0.5f;
        [SerializeField] private float rollDuration = 3f;
        [SerializeField] private float rollIconInterval = 0.08f;

        private PlayerService playerService;
        private QuestService questService;
        private UIService uiService;

        private readonly List<QuestProgressItem> items = new();
        private int renderVersion;

        private Vector2 shownPosition;
        private Vector2 hiddenPosition;
        private bool isShown;
        private Coroutine slideCoroutine;

        private RectTransform machineRect;
        private Vector2 machineCenter;
        private Coroutine gachaCoroutine;
        #endregion

        #region Properties
        #endregion

        #region Methods
        private void Awake()
        {
            playerService = ServiceProvider.Get<PlayerService>();
            questService = ServiceProvider.Get<QuestService>();
            uiService = ServiceProvider.Get<UIService>();

            // Shown position is wherever the list is placed in the editor
            shownPosition = listRoot.anchoredPosition;
            float distance = slideDistance > 0f ? slideDistance : listRoot.rect.width;
            hiddenPosition = shownPosition - new Vector2(distance, 0f);

            isShown = startShown;
            listRoot.anchoredPosition = isShown ? shownPosition : hiddenPosition;
            updateIndicator.SetActive(false);

            // Machine lands wherever it is placed in the editor
            machineRect = (RectTransform)gachaMachineAnimator.transform;
            machineCenter = machineRect.anchoredPosition;
            gachaPopupRoot.gameObject.SetActive(false);

            // Player Service
            playerService.OnQuestProgressSync += QuestProgressSync;

            // Buttons
            toggleButton.onClick.AddListener(Toggle);
            gachaCloseButton.onClick.AddListener(CloseGachaPopup);

            QuestProgressSync(false);
        }

        private void OnDestroy()
        {
            // Player Service
            playerService.OnQuestProgressSync -= QuestProgressSync;

            // Buttons
            toggleButton.onClick.RemoveAllListeners();
            gachaCloseButton.onClick.RemoveAllListeners();

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
                    item.Render(entry, quest, () => Claim(item, quest));
                    items.Add(item);
                }
            });
        }

        private void Claim(
            QuestProgressItem item,
            QuestInstanceDTO quest)
        {
            // Block double claims; the Rewarded sync removes the row on success
            item.SetClaimInteractable(false);
            OpenGachaPopup();

            AsyncHelper.Run(uiService, async () =>
            {
                try
                {
                    var reward = await questService.ClaimReward(quest.Id);
                    gachaCoroutine = StartCoroutine(Roll(quest.QuestRewardConfig, reward));
                }
                catch
                {
                    CloseGachaPopup();
                    if (item != null)
                        item.SetClaimInteractable(true);
                    throw;
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

        #region Gacha Popup
        private void OpenGachaPopup()
        {
            gachaPopupRoot.gameObject.SetActive(true);

            // Park the machine above the top edge, still, until the reward arrives
            gachaMachineAnimator.enabled = false;
            machineRect.anchoredPosition = MachineStart();
            gachaItemIcon.enabled = false;
            gachaResultText.text = string.Empty;
            gachaCloseButton.gameObject.SetActive(false);
        }

        private void CloseGachaPopup()
        {
            if (gachaCoroutine != null)
                StopCoroutine(gachaCoroutine);
            gachaCoroutine = null;

            gachaPopupRoot.gameObject.SetActive(false);
        }

        private Vector2 MachineStart()
        {
            return machineCenter + new Vector2(0f, gachaPopupRoot.rect.height / 2f + machineRect.rect.height / 2f);
        }

        // Result is already decided by the server; the roll is only a visual
        private IEnumerator Roll(
            QuestRewardConfig config,
            ItemInstanceDTO result)
        {
            // Fall straight down to center
            Vector2 start = MachineStart();
            float elapsed = 0f;
            while (elapsed < machineFallDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / machineFallDuration);
                machineRect.anchoredPosition = Vector2.LerpUnclamped(start, machineCenter, t * t);
                yield return null;
            }
            machineRect.anchoredPosition = machineCenter;

            // Landed: restart the default clip from frame 0, then wait it out
            gachaMachineAnimator.enabled = true;
            gachaMachineAnimator.Rebind();
            yield return null;
            AnimatorStateInfo state = gachaMachineAnimator.GetCurrentAnimatorStateInfo(0);
            yield return new WaitForSecondsRealtime(state.length / Mathf.Max(state.speed, 0.01f));

            var icons = new List<Sprite>();
            foreach (GachaRewardEntryDTO entry in config?.Entries ?? new List<GachaRewardEntryDTO>())
            {
                if (itemCatalog.TryGet(entry.ItemDefinitionId, out ItemAsset asset) && asset.icon != null)
                    icons.Add(asset.icon);
            }

            gachaItemIcon.enabled = true;
            elapsed = 0f;
            int index = 0;
            while (elapsed < rollDuration && icons.Count > 0)
            {
                gachaItemIcon.sprite = icons[index++ % icons.Count];
                yield return new WaitForSecondsRealtime(rollIconInterval);
                elapsed += rollIconInterval;
            }

            gachaItemIcon.sprite = itemCatalog.TryGet(result.DefinitionID, out ItemAsset resultAsset)
                ? resultAsset.icon
                : null;
            gachaResultText.text = $"x{result.Amount} ({result.Quality})";
            gachaCloseButton.gameObject.SetActive(true);
            gachaCoroutine = null;
        }
        #endregion
        #endregion
    }
}