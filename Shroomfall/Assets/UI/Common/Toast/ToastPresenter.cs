using Assets.Enums;
using Assets.Services;
using Assets.UI.Models;
using System;

namespace Assets.UI.Common.Toast
{
    public class ToastPresenter : IDisposable
    {
        #region Attributes
        private readonly UIService uiService;
        private readonly ToastView toastView;

        private ToastModel? current;

        private bool disposed;
        #endregion

        #region Properties
        #endregion

        public ToastPresenter(
            UIService uiService,
            ToastView toastView)
        {
            this.uiService = uiService;
            this.toastView = toastView;

            Bind();
        }

        #region Methods
        public void Dispose()
        {
            if (disposed) return;
            disposed = true;

            // Inbound
            toastView.OnOkClicked -= OnOkClicked;

            // Outbound
            uiService.OnToastRequested -= OnToastRequested;
        }

        private void Bind()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(ToastPresenter));

            // Inbound
            toastView.OnOkClicked += OnOkClicked;

            // Outbound
            uiService.OnToastRequested += OnToastRequested;
        }

        private void OnOkClicked()
        {
            current = null;
            toastView.Hide();
        }

        private void OnToastRequested(
            (ToastType type, string message) request)
        {
            current = new ToastModel(request.type, request.message);

            switch (current.Value.Type)
            {
                case ToastType.Information:
                    toastView.ShowInformation(current.Value.Message);
                    break;

                case ToastType.Error:
                    toastView.ShowError(current.Value.Message);
                    break;
            }
        }
        #endregion
    }
}