using Assets.Services;
using Assets.UI.Features.CreateSession;
using Assets.Utilities;
using System.Collections;
using UnityEngine;

public class CreateSessionInstaller : Installer
{
    #region Attributes
    [SerializeField]
    private CreateSessionView createSessionView;
    private CreateSessionPresenter createSessionPresenter;

    [SerializeField]
    private RoomCatalogSO roomCatalogSO;

    private UIService uiService;
    private SessionService sessionService;
    private GameService gameService;
    private DefinitionService definitionService;
    #endregion

    #region Properties
    public override string StepName
    {
        get { return UILocalizationTable.Get("session-registry-binder.step-name"); }
    }
    #endregion

    #region Methods
    public override IEnumerator BindAllServices()
    {
        yield return BindWhenReady<UIService>(ui => { uiService = ui; });
        yield return BindWhenReady<SessionService>(session => { sessionService = session; });
        yield return BindWhenReady<GameService>(game => { gameService = game; });
        yield return BindWhenReady<DefinitionService>(definition => { definitionService = definition; });

        // Resolve dependencies
        createSessionPresenter = new CreateSessionPresenter(
            uiService,
            sessionService,
            gameService,
            definitionService,
            createSessionView,
            roomCatalogSO);
    }

    private void OnDestroy()
    {
        createSessionPresenter?.Dispose();
    }
    #endregion
}