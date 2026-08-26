using Assets.Services;
using System;

namespace Assets.UI.Common.Loading
{
    public class LoadingPresenter : IDisposable
    {
        #region Attributes
        private readonly UIService uiService;
        private readonly LoadingView loadingView;

        private bool disposed;
        #endregion

        #region Properties
        #endregion

        public LoadingPresenter(
            UIService uiService,
            LoadingView loadingView)
        {
            this.uiService = uiService;
            this.loadingView = loadingView;

            Bind();
        }

        #region Methods
        public void Dispose()
        {
            if (disposed) return;
            disposed = true;

            // Outbound
            uiService.OnLoadingChanged -= OnLoadingChanged;
        }

        private void Bind()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(LoadingPresenter));

            // Outbound
            uiService.OnLoadingChanged += OnLoadingChanged;
        }

        private void OnLoadingChanged(
            bool visible)
        {
            loadingView.SetActive(visible);
        }
        #endregion
    }
}