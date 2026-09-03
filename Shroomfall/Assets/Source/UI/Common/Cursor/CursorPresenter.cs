using Assets.Source.Enum;
using Assets.Source.Service;
using Assets.Source.UI.Component.Shared;
using System;

namespace Assets.Source.UI.Common.Cursor
{
    public class CursorPresenter : IDisposable
    {
        #region Attributes
        private readonly UIService uiService;
        private readonly CursorView cursorView;

        private bool disposed;
        #endregion

        #region Properties
        #endregion

        public CursorPresenter(
            UIService uiService,
            CursorView cursorView)
        {
            this.uiService = uiService;
            this.cursorView = cursorView;

            Bind();

            cursorView.Apply(CursorType.Default);
        }

        #region Methods
        public void Dispose()
        {
            if (disposed) return;
            disposed = true;

            // Outbound
            uiService.OnCursorChanged -= cursorView.Apply;
        }

        private void Bind()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(CursorPresenter));

            // Outbound
            uiService.OnCursorChanged += cursorView.Apply;
        }

        public void BindTarget(
            CursorTarget target)
        {
            target.Bind(uiService.SetCursor);
        }

        public void UnbindTarget(
            CursorTarget target)
        {
            target.Bind(null);
        }
        #endregion
    }
}