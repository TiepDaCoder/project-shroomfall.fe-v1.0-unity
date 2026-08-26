using Assets.Services;
using Assets.UI.Features.MainMenu;
using Assets.Utilities;
using System.Collections;
using UnityEngine;

public class MainMenuInstaller : Installer
{
    #region Attributes
    [SerializeField]
    private MainMenuView mainMenuView;
    private MainMenuPresenter mainMenuPresenter;

    private GameService gameService;
    private UIService uiService;
    #endregion

    #region Properties
    public override string StepName
    {
        get { return UILocalizationTable.Get("main-menu-binder.step-name"); }
    }
    #endregion

    #region Methods
    public override IEnumerator BindAllServices()
    {
        yield return BindWhenReady<GameService>(game => { gameService = game; });
        yield return BindWhenReady<UIService>(ui => { uiService = ui; });

        // Resolve dependencies
        mainMenuPresenter = new MainMenuPresenter(
            gameService,
            uiService,
            mainMenuView);
    }

    private void OnDestroy()
    {
        mainMenuPresenter?.Dispose();
    }
    #endregion
}