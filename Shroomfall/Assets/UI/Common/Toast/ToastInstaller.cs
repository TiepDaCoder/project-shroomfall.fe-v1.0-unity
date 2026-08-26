using Assets.Services;
using Assets.UI.Common.Toast;
using Assets.Utilities;
using System.Collections;
using UnityEngine;

public class ToastInstaller : Installer
{
    #region Attributes
    [SerializeField]
    private ToastView toastView;
    private ToastPresenter toastPresenter;

    private UIService uiService;
    #endregion

    #region Properties
    public override string StepName
    {
        get { return UILocalizationTable.Get("toast-binder.step-name"); }
    }
    #endregion

    #region Methods
    public override IEnumerator BindAllServices()
    {
        yield return BindWhenReady<UIService>(ui => { uiService = ui; });

        // Resolve dependencies
        toastPresenter = new ToastPresenter(
            uiService,
            toastView);
    }

    private void OnDestroy()
    {
        toastPresenter?.Dispose();
    }
    #endregion
}