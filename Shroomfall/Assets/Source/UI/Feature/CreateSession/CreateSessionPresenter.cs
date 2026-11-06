using Assets.Source.Data;
using Assets.Source.Enum;
using Assets.Source.Service;
using Assets.Source.UI.Model;
using Assets.Source.Utility;
using Contract.Enum.WorldDomain;
using System;
using System.Collections.Generic;

namespace Assets.Source.UI.Feature.CreateSession
{
    public class CreateSessionPresenter : IDisposable
    {
        #region Attributes
        private readonly UIService uiService;
        private readonly SessionService sessionService;
        private readonly GameService gameService;
        private readonly DefinitionService definitionService;
        private readonly CreateSessionView createSessionView;

        private readonly RoomCatalogSO roomCatalog;

        private bool disposed;
        #endregion

        #region Properties
        #endregion

        public CreateSessionPresenter(
            UIService uiService,
            SessionService sessionService,
            GameService gameService,
            DefinitionService definitionService,
            CreateSessionView createSessionView,
            RoomCatalogSO roomCatalog)
        {
            this.uiService = uiService;
            this.sessionService = sessionService;
            this.gameService = gameService;
            this.definitionService = definitionService;
            this.createSessionView = createSessionView;

            Bind();
            this.roomCatalog = roomCatalog;
        }

        #region Methods
        public void Dispose()
        {
            if (disposed) return;
            disposed = true;

            // Outbound
            uiService.OnUIStateChanged -= OnUIStateChanged;
            uiService.OnGlobalInteractableChanged -= createSessionView.SetInteractable;

            // Inbound
            createSessionView.OnBackClicked -= OnBackClicked;
            createSessionView.OnCreateClicked -= OnCreateClicked;
            createSessionView.OnOpened -= OnOpened;
        }

        private void Bind()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(CreateSessionPresenter));

            // Outbound
            uiService.OnUIStateChanged += OnUIStateChanged;
            uiService.OnGlobalInteractableChanged += createSessionView.SetInteractable;

            // Inbound
            createSessionView.OnBackClicked += OnBackClicked;
            createSessionView.OnCreateClicked += OnCreateClicked;
            createSessionView.OnOpened += OnOpened;
        }

        private void OnUIStateChanged(
            UIService service)
        {
            createSessionView.SetVisible(service.ShowCreateSession);
        }

        private void OnBackClicked()
        {
            gameService.PopPhase();
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

        private void OnOpened()
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

            createSessionView.BindRoomIcons(models);
        }
        #endregion
    }
}