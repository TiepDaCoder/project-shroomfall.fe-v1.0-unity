using Assets.Source.Enum;
using Assets.Source.Service;
using Assets.Source.Utility;
using System;

namespace Assets.Source.UI.Feature.ListSession
{
    public class ListSessionPresenter : IDisposable
    {
        #region Attributes
        private readonly UIService uiService;
        private readonly SessionService sessionService;
        private readonly GameService gameService;
        private readonly ListSessionView listSessionView;

        private bool disposed;
        #endregion

        #region Properties
        #endregion

        public ListSessionPresenter(
            UIService uiService,
            SessionService sessionService,
            GameService gameService,
            ListSessionView listSessionView)
        {
            this.uiService = uiService;
            this.sessionService = sessionService;
            this.gameService = gameService;
            this.listSessionView = listSessionView;

            Bind();
        }

        #region Methods
        public void Dispose()
        {
            if (disposed) return;
            disposed = true;

            // Outbound
            uiService.OnUIStateChanged -= OnUIStateChanged;
            uiService.OnGlobalInteractableChanged -= listSessionView.SetInteractable;

            // Inbound
            listSessionView.OnBackClicked -= OnBackClicked;
            listSessionView.OnCreateClicked -= OnCreateClicked;
            listSessionView.OnLoadClicked -= OnLoadClicked;
        }

        private void Bind()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(ListSessionPresenter));

            // Outbound
            uiService.OnUIStateChanged += OnUIStateChanged;
            uiService.OnGlobalInteractableChanged += listSessionView.SetInteractable;

            // Inbound
            listSessionView.OnBackClicked += OnBackClicked;
            listSessionView.OnCreateClicked += OnCreateClicked;
            listSessionView.OnLoadClicked += OnLoadClicked;
        }

        private void OnUIStateChanged(
            UIService service)
        {
            listSessionView.SetVisible(service.ShowListSession);

            AsyncHelper.Run(uiService, async () =>
            {
                var session = await sessionService.FetchSessions();
                listSessionView.SetSessions(session.Sessions);
            });
        }

        private void OnBackClicked()
        {
            gameService.BackToMenu();
        }

        private void OnCreateClicked()
        {
            gameService.PushPhase(GamePhase.CreateSession);
        }

        private void OnLoadClicked(
            string sessionId)
        {
            AsyncHelper.Run(uiService, async () =>
            {
                await gameService.LoadSessionAndWorld(sessionId);
            });
        }
        #endregion
    }
}