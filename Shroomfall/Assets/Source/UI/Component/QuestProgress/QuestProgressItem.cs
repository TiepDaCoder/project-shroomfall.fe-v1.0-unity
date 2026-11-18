using Assets.Source.Core;
using Assets.Source.Service;
using Assets.Source.UI.Component.Quest;
using Contract.DTO.Runtime.EntityDomain.Component;
using Contract.DTO.Runtime.MetaDomain;
using Contract.Enum.EntityDomain;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Source.UI.Component.QuestProgress
{
    public class QuestProgressItem : MonoBehaviour
    {
        #region Attributes
        [Header("References")]
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private TMP_Text createdText;
        [SerializeField] private TMP_Text expireText;

        [Header("Timer")]
        [SerializeField] private Image timerBar;
        [SerializeField] private TMP_Text timerText;

        [Header("Progress")]
        [SerializeField] private Image progressBar;
        [SerializeField] private TMP_Text progressText;

        private LocalizeService localizeService;
        private QuestInstanceDTO quest;
        private QuestProgressEntryDTO entry;
        private DateTime createdAtUtc;
        private DateTime expiredAtUtc;
        #endregion

        #region Properties
        #endregion

        #region Methods
        private void Awake()
        {
            localizeService = ServiceProvider.Get<LocalizeService>();

            // Services
            localizeService.OnLocaleChanged += RenderDescription;
        }

        private void OnDestroy()
        {
            // Services
            localizeService.OnLocaleChanged -= RenderDescription;
        }

        private void Update()
        {
            if (quest != null && entry?.Status == QuestProgressStatus.InProgress)
                RenderTimer();
        }

        public void Render(
            QuestProgressEntryDTO entry,
            QuestInstanceDTO quest)
        {
            this.entry = entry;
            this.quest = quest;

            createdAtUtc = QuestItem.ToUtc(quest.CreatedAt);
            expiredAtUtc = QuestItem.ToUtc(quest.ExpiredAt);

            createdText.text = createdAtUtc.ToLocalTime().ToString("dd/MM/yyyy HH:mm");
            expireText.text = expiredAtUtc.ToLocalTime().ToString("dd/MM/yyyy HH:mm");

            RenderDescription();
            RenderTimer();
            RenderProgress();
        }

        private void RenderDescription()
        {
            if (quest == null)
                return;

            descriptionText.text = QuestItem.GetDescription(quest, localizeService.CurrentLocale);
        }

        private void RenderTimer()
        {
            DateTime now = DateTime.UtcNow;
            TimeSpan remaining = expiredAtUtc - now;
            if (remaining < TimeSpan.Zero)
                remaining = TimeSpan.Zero;

            // Bar drains as the quest approaches expiry
            double total = (expiredAtUtc - createdAtUtc).TotalSeconds;
            timerBar.fillAmount = total <= 0
                ? 0f
                : Mathf.Clamp01((float)(remaining.TotalSeconds / total));

            timerText.text = remaining.TotalDays >= 1
                ? $"{(int)remaining.TotalDays}d {remaining:hh\\:mm\\:ss}"
                : remaining.ToString(@"hh\:mm\:ss");
        }

        private void RenderProgress()
        {
            var identification = entry.IdentificationProgress;
            bool completed = entry.Status == QuestProgressStatus.Completed || entry.Status == QuestProgressStatus.Rewarded;

            if (identification != null)
            {
                progressBar.fillAmount = identification.Percentage / 100f;
                progressText.text = $"{identification.CorrectDefeatCount}/{identification.RequiredDefeatCount}";
            }
            else
            {
                // Categorizing/Shooting have no partial progress: done or not
                progressBar.fillAmount = completed ? 1f : 0f;
                progressText.text = entry.Status.ToString();
            }
        }
        #endregion
    }
}