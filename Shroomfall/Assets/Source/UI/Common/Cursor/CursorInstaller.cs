using Assets.Source.Service;
using Assets.Source.UI.Abstraction;
using Assets.Source.UI.Component.Shared;
using Assets.Source.Utility;
using System.Collections;
using UnityEngine;

namespace Assets.Source.UI.Common.Cursor
{
    public class CursorInstaller : Installer
    {
        #region Attributes
        [SerializeField]
        private CursorView cursorView;
        private CursorPresenter cursorPresenter;

        private UIService uiService;
        #endregion

        #region Properties
        public override string StepName
        {
            get { return UILocalizationTable.Get("cursor-binder.step-name"); }
        }
        #endregion

        #region Methods
        public override IEnumerator BindAllServices()
        {
            yield return BindWhenReady<UIService>(ui => { uiService = ui; });

            // Resolve dependencies
            cursorPresenter = new CursorPresenter(
                uiService,
                cursorView);

            // Bind targets
            CursorTarget.OnTargetEnabled += cursorPresenter.BindTarget;
            CursorTarget.OnTargetDisabled += cursorPresenter.UnbindTarget;
            foreach (var target in FindObjectsByType<CursorTarget>(FindObjectsSortMode.None))
            {
                if (target.isActiveAndEnabled)
                {
                    cursorPresenter.BindTarget(target);
                }
            }
        }

        private void OnDestroy()
        {
            cursorPresenter?.Dispose();
        }
        #endregion
    }
}