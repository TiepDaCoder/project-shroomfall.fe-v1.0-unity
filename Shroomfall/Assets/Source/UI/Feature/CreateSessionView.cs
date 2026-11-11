using Assets.Source.Core;
using Assets.Source.Data;
using Assets.Source.Enum;
using Assets.Source.Service;
using Assets.Source.UI.Component.Button;
using Assets.Source.UI.Model;
using Assets.Source.Utility;
using Contract.Enum.WorldDomain;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Source.UI.Feature
{
    public class CreateSessionView : MonoBehaviour
    {
        #region Attributes
        [Header("Root")]
        [SerializeField] private GameObject canvas;

        [Header("Selectors")]
        [SerializeField] private Transform selectionRoot;
        [SerializeField] private ImageButton selectionPrefab;

        [Header("Buttons")]
        [SerializeField] private Button backButton;

        [Header("Data")]
        [SerializeField] private RoomCatalogSO roomCatalog;

        private UIService uiService;
        private SessionService sessionService;
        private DefinitionService definitionService;

        private readonly List<ImageButton> selections = new();
        #endregion

        #region Properties
        #endregion

        #region Methods
        private void Awake()
        {
            uiService = ServiceProvider.Get<UIService>();
            sessionService = ServiceProvider.Get<SessionService>();
            definitionService = ServiceProvider.Get<DefinitionService>();

            // UI Service
            uiService.OnUIStateChanged += OnUIStateChanged;
            uiService.OnGlobalInteractableChanged += SetInteractable;

            // Buttons
            backButton.onClick.AddListener(OnBackClicked);
        }

        private void OnDestroy()
        {
            // UI Service
            uiService.OnUIStateChanged -= OnUIStateChanged;
            uiService.OnGlobalInteractableChanged -= SetInteractable;

            // Buttons
            backButton.onClick.RemoveListener(OnBackClicked);

            // Selections
            foreach (var selection in selections)
                selection.OnClicked -= OnCreateClicked;
        }

        private void OnUIStateChanged(
            UIService service)
        {
            var visible = service.ShowCreateSession;
            canvas.SetActive(visible);

            if (visible)
                BindRooms();
        }

        public void SetInteractable(
            bool interactable)
        {
            backButton.interactable = interactable;

            foreach (var selection in selections)
                selection.SetInteractable(interactable);
        }

        private void OnBackClicked()
        {
            uiService.SetState(UIState.ListSession);
        }

        private void BindRooms()
        {
            var models = new List<IconModel>();

            foreach (var room in definitionService.Snapshot.Rooms)
            {
                if (room.Type != RoomType.PersonalSpace)
                    continue;

                if (!roomCatalog.TryGet(room.Id, out var asset))
                    continue;

                models.Add(new IconModel
                {
                    Id = room.Id,
                    Icon = asset.icon,
                    Name = definitionService.GetLocalizedText(room.Presentation.LocalizedText.NameKey),
                    Description = definitionService.GetLocalizedText(room.Presentation.LocalizedText.DescriptionKey)
                });
            }

            foreach (Transform child in selectionRoot)
                Destroy(child.gameObject);

            selections.Clear();
            foreach (var model in models)
            {
                var selection = Instantiate(selectionPrefab, selectionRoot);
                selection.Bind(model);
                selection.OnClicked += OnCreateClicked;
                selections.Add(selection);
            }
        }

        private void OnCreateClicked(
            string id)
        {
            if (string.IsNullOrEmpty(id))
                return;

            AsyncHelper.Run(uiService, async () =>
            {
                await sessionService.CreateSession(id);
                uiService.ShowToast(ToastType.Information, "Session created successfully!");
            });
        }
        #endregion
    }
}