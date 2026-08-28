using Assets.Source.Service;
using Assets.Source.UI.Abstraction;
using Assets.Source.UI.Common.Toast;
using Assets.Source.Utilities;
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