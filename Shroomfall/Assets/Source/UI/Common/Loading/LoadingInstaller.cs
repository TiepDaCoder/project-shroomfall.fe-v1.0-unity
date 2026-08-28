using Assets.Source.Service;
using Assets.Source.UI.Abstraction;
using Assets.Source.Utilities;
using System.Collections;
using UnityEngine;

namespace Assets.Source.UI.Common.Loading
{
    public class LoadingInstaller : Installer
    {
        #region Attributes
        [SerializeField]
        private LoadingView loadingView;
        private LoadingPresenter loadingPresenter;

        private UIService uiService;
        #endregion

        #region Properties
        public override string StepName
        {
            get { return UILocalizationTable.Get("loading-binder.step-name"); }
        }
        #endregion

        #region Methods
        public override IEnumerator BindAllServices()
        {
            yield return BindWhenReady<UIService>(ui => { uiService = ui; });

            // Resolve dependencies
            loadingPresenter = new LoadingPresenter(
                uiService,
                loadingView);
        }

        private void OnDestroy()
        {
            loadingPresenter?.Dispose();
        }
        #endregion
    }
}