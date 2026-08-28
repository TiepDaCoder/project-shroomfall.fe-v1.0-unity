using Assets.Source.Service;
using Assets.Source.UI.Abstraction;
using Assets.Source.Utilities;
using Assets.UI.Features.MainMenu;
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