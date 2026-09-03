using Assets.Source.Service;
using Assets.Source.Utility;
using System;

namespace Assets.Source.UI.Feature.HUDUtils
{
    public class HUDUtilsPresenter : IDisposable
    {
        #region Attributes
        private readonly UIService uiService;
        private readonly GameService gameService;
        private readonly HUDUtilsView hudUtilsView;

        private bool disposed;
        #endregion

        #region Properties
        #endregion

        public HUDUtilsPresenter(
            UIService uiService,
            GameService gameService,
            HUDUtilsView hudUtilsView)
        {
            this.uiService = uiService;
            this.gameService = gameService;
            this.hudUtilsView = hudUtilsView;

            Bind();
        }

        #region Methods
        public void Dispose()
        {
            if (disposed) return;
            disposed = true;

            // Outbound
            uiService.OnUIStateChanged -= hudUtilsView.HandleUIState;

            // Inbound
            hudUtilsView.OnQuitClicked -= OnQuitClicked;
        }

        private void Bind()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(HUDUtilsPresenter));

            // Outbound
            uiService.OnUIStateChanged += hudUtilsView.HandleUIState;

            // Inbound
            hudUtilsView.OnQuitClicked += OnQuitClicked;
        }

        private void OnQuitClicked()
        {
            AsyncHelper.Run(uiService, async () =>
            {
                await gameService.QuitGame();
            });
        }
        #endregion
    }
}