using Assets.Source.Core;
using Assets.Source.Service;
using Contract.DTO.Runtime.MetaDomain;
using Contract.Enum.MetaDomain.Quest;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Source.UI.Component.Quest
{
    public class QuestItem : MonoBehaviour
    {
        #region Attributes
        [Header("References")]
        [SerializeField] private Image typeIcon;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private TMP_Text expireText;
        [SerializeField] private Image timerBar;

        [Header("Rewards")]
        [SerializeField] private Transform rewardContent;
        [SerializeField] private QuestItemReward rewardPrefab;

        [Header("Buttons")]
        [SerializeField] private UnityEngine.UI.Button acceptButton;

        [Header("Type Icons")]
        [SerializeField] private Sprite identificationIcon;
        [SerializeField] private Sprite categorizingIcon;
        [SerializeField] private Sprite shootingIcon;

        private LocalizeService localizeService;
        private QuestInstanceDTO quest;
        private DateTime createdAtUtc;
        private DateTime expiredAtUtc;
        private Action onAccept;
        private readonly List<QuestItemReward> rewards = new();
        #endregion

        #region Properties
        #endregion

        #region Methods
        private void Awake()
        {
            localizeService = ServiceProvider.Get<LocalizeService>();

            // Services
            localizeService.OnLocaleChanged += RenderDescription;

            // Buttons
            acceptButton.onClick.AddListener(OnAccept);
        }

        private void OnDestroy()
        {
            // Services
            localizeService.OnLocaleChanged -= RenderDescription;

            // Buttons
            acceptButton.onClick.RemoveAllListeners();

            QuestItemReward.Clear(rewards);
        }

        private void Update()
        {
            if (quest != null)
                RenderTimer();
        }

        public void Render(
            QuestInstanceDTO quest,
            Action onAccept)
        {
            this.quest = quest;
            this.onAccept = onAccept;

            createdAtUtc = ToUtc(quest.CreatedAt);
            expiredAtUtc = ToUtc(quest.ExpiredAt);

            typeIcon.sprite = quest.Type switch
            {
                QuestType.Identification => identificationIcon,
                QuestType.Categorizing => categorizingIcon,
                QuestType.Shooting => shootingIcon,
                _ => null
            };

            expireText.text = expiredAtUtc.ToLocalTime().ToString("dd/MM/yyyy HH:mm");

            RenderDescription();
            RenderTimer();
            QuestItemReward.RenderAll(quest.QuestRewardConfig, rewardPrefab, rewardContent, rewards);
        }

        public void SetInteractable(
            bool interactable)
        {
            acceptButton.interactable = interactable;
        }

        private void RenderDescription()
        {
            if (quest == null)
                return;

            descriptionText.text = GetDescription(quest, localizeService.CurrentLocale);
        }

        internal static string GetDescription(
            QuestInstanceDTO quest,
            string locale)
        {

            LocaleQuestDTO description = quest.Type switch
            {
                QuestType.Identification => quest.IdentificationConfig?.Description,
                QuestType.Categorizing => quest.CategorizingConfig?.Description,
                QuestType.Shooting => quest.ShootingConfig?.Description,
                _ => null
            };

            var entries = description?.Entries;
            return entries?.FirstOrDefault(e => e.Key == locale)?.Value
                ?? entries?.FirstOrDefault()?.Value
                ?? string.Empty;
        }

        private void RenderTimer()
        {
            DateTime now = DateTime.UtcNow;
            TimeSpan remaining = expiredAtUtc - now;
            if (remaining < TimeSpan.Zero)
                remaining = TimeSpan.Zero;

            double total = (expiredAtUtc - createdAtUtc).TotalSeconds;
            timerBar.fillAmount = total <= 0
                ? 1f
                : Mathf.Clamp01((float)((now - createdAtUtc).TotalSeconds / total));

            timerText.text = remaining.TotalDays >= 1
                ? $"{(int)remaining.TotalDays}d {remaining:hh\\:mm\\:ss}"
                : remaining.ToString(@"hh\:mm\:ss");
        }

        // Server sends UTC; unspecified kind (no "Z") is treated as UTC too
        internal static DateTime ToUtc(
            DateTime value)
        {
            return value.Kind == DateTimeKind.Local
                ? value.ToUniversalTime()
                : DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }

        private void OnAccept()
        {
            onAccept?.Invoke();
        }
        #endregion
    }
}