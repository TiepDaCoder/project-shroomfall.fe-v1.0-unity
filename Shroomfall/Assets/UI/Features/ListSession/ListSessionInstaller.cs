using Assets.Services;
using Assets.UI.Features.ListSession;
using Assets.Utilities;
using System.Collections;
using UnityEngine;

public class ListSessionInstaller : Installer
{
    #region Attributes
    [SerializeField]
    private ListSessionView listSessionView;
    private ListSessionPresenter listSessionPresenter;

    private UIService uiService;
    private SessionService sessionService;
    private GameService gameService;
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

        // Resolve dependencies
        listSessionPresenter = new ListSessionPresenter(
            uiService,
            sessionService,
            gameService,
            listSessionView);
    }

    private void OnDestroy()
    {
        listSessionPresenter?.Dispose();
    }
    #endregion
}