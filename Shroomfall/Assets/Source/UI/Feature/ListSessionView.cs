using Assets.Source.Core;
using Assets.Source.Data;
using Assets.Source.Enum;
using Assets.Source.Service;
using Assets.Source.UI.Component.Session;
using Assets.Source.UI.Model;
using Assets.Source.Utility;
using Contract.DTO.Feature.Connection.Response;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Source.UI.Feature
{
    public class ListSessionView : MonoBehaviour
    {
        #region Attributes
        [Header("Root")]
        [SerializeField] private GameObject canvas;

        [Header("Prefabs")]
        [SerializeField] private SessionEntry entryPrefab;

        [Header("Scroller Content")]
        [SerializeField] private Transform content;

        [Header("Buttons")]
        [SerializeField] private Button backButton;
        [SerializeField] private Button createButton;

        [Header("Data")]
        [SerializeField] private EntityCatalogSO entityCatalog;

        private UIService uiService;
        private SessionService sessionService;
        #endregion

        #region Properties
        #endregion

        #region Methods
        private void Awake()
        {
            uiService = ServiceProvider.Get<UIService>();
            sessionService = ServiceProvider.Get<SessionService>();

            // UI Service
            uiService.OnUIStateChanged += OnUIStateChanged;
            uiService.OnGlobalInteractableChanged += SetInteractable;

            // Buttons
            backButton.onClick.AddListener(OnBackClicked);
            createButton.onClick.AddListener(OnCreateClicked);
        }

        private void OnDestroy()
        {
            // UI Service
            uiService.OnUIStateChanged -= OnUIStateChanged;
            uiService.OnGlobalInteractableChanged -= SetInteractable;

            // Buttons
            backButton.onClick.RemoveListener(OnBackClicked);
            createButton.onClick.RemoveListener(OnCreateClicked);
        }

        private void OnUIStateChanged(
            UIService service)
        {
            var visible = service.ShowListSession;
            canvas.SetActive(visible);

            if (visible)
                AsyncHelper.Run(uiService, async () =>
                {
                    var session = await sessionService.FetchSessions();
                    SetSessions(session.Sessions);
                });
        }

        private void SetInteractable(
            bool interactable)
        {
            for (int i = 0; i < content.childCount; i++)
            {
                SessionEntry entry = content.GetChild(i).GetComponent<SessionEntry>();
                if (entry != null) entry.SetInteractable(interactable);
            }
        }

        private void OnBackClicked()
        {
            uiService.SetState(UIState.MainMenu);
        }

        private void OnCreateClicked()
        {
            uiService.SetState(UIState.CreateSession);
        }

        private void SetSessions(
            List<ExistedSessionEntryDTO> dto)
        {
            // Clear old list
            for (int i = content.childCount - 1; i >= 0; i--)
                Destroy(content.GetChild(i).gameObject);

            // Instantiate new items
            var models = dto.Select(MapToViewModel).ToList();
            foreach (var model in models)
            {
                var entry = Instantiate(entryPrefab, content);
                entry.Bind(model);
            }
        }

        private SessionEntryModel MapToViewModel(
            ExistedSessionEntryDTO dto)
        {
            var entry = new SessionEntryModel
            {
                SessionId = dto.PlayerInstanceID,
                SkinColor = ColorHelper.ToColor(dto.PlayerAppearance.SkinColor),
            };

            if (entityCatalog.TryGet(dto.PlayerAppearance.SkinID, out var entityAsset))
            {
                entry.Entity = entityAsset;
            }

            return entry;
        }
        #endregion
    }
}