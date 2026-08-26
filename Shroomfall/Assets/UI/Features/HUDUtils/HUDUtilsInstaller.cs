using Assets.Services;
using Assets.UI.Features.HUDUtils;
using Assets.Utilities;
using System.Collections;
using UnityEngine;

public class HUDUtilsInstaller : Installer
{
    #region Attributes
    [SerializeField]
    private HUDUtilsView hudUtilsView;
    private HUDUtilsPresenter hudUtilsPresenter;

    private UIService uiService;
    private GameService gameService;
    #endregion

    #region Properties
    public override string StepName
    {
        get { return UILocalizationTable.Get("hud-utils-binder.step-name"); }
    }
    #endregion

    #region Methods
    public override IEnumerator BindAllServices()
    {
        yield return BindWhenReady<UIService>(ui => { uiService = ui; });
        yield return BindWhenReady<GameService>(game => { gameService = game; });

        // Resolve dependencies
        hudUtilsPresenter = new HUDUtilsPresenter(
            uiService,
            gameService,
            hudUtilsView);
    }

    private void OnDestroy()
    {
        hudUtilsPresenter?.Dispose();
    }
    #endregion
}