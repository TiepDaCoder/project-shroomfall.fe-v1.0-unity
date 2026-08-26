using Assets.Services;
using Assets.UI.Common.Loading;
using Assets.Utilities;
using System.Collections;
using UnityEngine;

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