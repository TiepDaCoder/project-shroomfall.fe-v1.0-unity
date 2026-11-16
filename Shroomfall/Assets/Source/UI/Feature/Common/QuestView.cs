using Assets.Source.Core;
using Assets.Source.Service;
using Assets.Source.UI.Component.Quest;
using Assets.Source.Utility;
using Contract.DTO.Runtime.MetaDomain;
using Contract.Enum.MetaDomain.Quest;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Source.UI.Feature.Common
{
    public class QuestView : MonoBehaviour
    {
        #region Attributes
        [Header("Root")]
        [SerializeField] private GameObject canvas;

        [Header("Buttons")]
        [SerializeField] private Button closeButton;
        [SerializeField] private Button identificationTabButton;
        [SerializeField] private Button categorizingTabButton;
        [SerializeField] private Button shootingTabButton;

        [Header("Search")]
        [SerializeField] private TMP_InputField searchField;

        [Header("References")]
        [SerializeField] private Transform content;
        [SerializeField] private QuestItem questItemPrefab;

        private UIService uiService;
        private QuestService questService;

        private readonly List<QuestItem> questItems = new();
        private QuestType currentType = QuestType.Identification;
        private bool isInteractable = true;
        #endregion

        #region Properties
        #endregion

        #region Methods
        public void Awake()
        {
            uiService = ServiceProvider.Get<UIService>();
            questService = ServiceProvider.Get<QuestService>();

            // Services
            uiService.OnQuestRequested += Open;
            uiService.OnGlobalInteractableChanged += SetInteractable;

            // Buttons
            closeButton.onClick.AddListener(Close);
            identificationTabButton.onClick.AddListener(() => ShowTab(QuestType.Identification));
            categorizingTabButton.onClick.AddListener(() => ShowTab(QuestType.Categorizing));
            shootingTabButton.onClick.AddListener(() => ShowTab(QuestType.Shooting));

            // Search on submit/deselect, not per keystroke
            searchField.onEndEdit.AddListener(_ => Fetch());

            canvas.SetActive(false);
        }

        private void OnDestroy()
        {
            // Services
            uiService.OnQuestRequested -= Open;
            uiService.OnGlobalInteractableChanged -= SetInteractable;

            // Buttons
            closeButton.onClick.RemoveAllListeners();
            identificationTabButton.onClick.RemoveAllListeners();
            categorizingTabButton.onClick.RemoveAllListeners();
            shootingTabButton.onClick.RemoveAllListeners();
            searchField.onEndEdit.RemoveAllListeners();

            ClearQuestItems();
        }

        public void Open()
        {
            canvas.SetActive(true);
            ShowTab(currentType);
        }

        public void Close()
        {
            canvas.SetActive(false);
            ClearQuestItems();
        }

        private void SetInteractable(
            bool interactable)
        {
            isInteractable = interactable;
            closeButton.interactable = interactable;
            searchField.interactable = interactable;
            UpdateTabButtons();

            foreach (QuestItem item in questItems)
                item.SetInteractable(interactable);
        }

        #region Tabs
        private void ShowTab(
            QuestType type)
        {
            currentType = type;
            UpdateTabButtons();
            Fetch();
        }

        private void UpdateTabButtons()
        {
            // The active tab is disabled so it reads as selected
            identificationTabButton.interactable = isInteractable && currentType != QuestType.Identification;
            categorizingTabButton.interactable = isInteractable && currentType != QuestType.Categorizing;
            shootingTabButton.interactable = isInteractable && currentType != QuestType.Shooting;
        }
        #endregion

        #region Quest Items
        private void Fetch()
        {
            if (!canvas.activeSelf)
                return;

            QuestType type = currentType;
            string searchTerm = searchField.text;

            AsyncHelper.Run(uiService, async () =>
            {
                var result = await questService.FetchQuests(type, searchTerm);

                // Drop stale responses when the user switched tab/search mid-request
                if (type != currentType || searchTerm != searchField.text || !canvas.activeSelf)
                    return;

                Render(result?.Items);
            });
        }

        private void Render(
            List<QuestInstanceDTO> quests)
        {
            ClearQuestItems();

            if (quests == null)
                return;

            foreach (QuestInstanceDTO quest in quests)
            {
                string questId = quest.Id;
                QuestItem questItem = Instantiate(questItemPrefab, content);
                questItem.Render(quest, () => Accept(questId));
                questItem.SetInteractable(isInteractable);
                questItems.Add(questItem);
            }
        }

        private void Accept(
            string questInstanceId)
        {
            AsyncHelper.Run(uiService, async () =>
            {
                await questService.AcceptQuest(questInstanceId);
                Close();
            });
        }

        private void ClearQuestItems()
        {
            foreach (QuestItem item in questItems)
            {
                if (item != null)
                    Destroy(item.gameObject);
            }

            questItems.Clear();
        }
        #endregion
        #endregion
    }
}