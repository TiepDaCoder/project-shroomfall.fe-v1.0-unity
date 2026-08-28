using Assets.Source.Data;
using Assets.Source.Service;
using Assets.Source.UI.Abstraction;
using Assets.Source.Utilities;
using Assets.UI.Features.Customize;
using System.Collections;
using UnityEngine;

public class CustomizeInstaller : Installer
{
    #region Attributes
    [Header("Asset Catalogs")]
    [SerializeField] private EntityCatalogSO skinCatalog;

    [SerializeField]
    private CustomizeView customizeView;
    private CustomizePresenter customizePresenter;

    private UIService uiService;
    private PlayerService playerService;
    private AuthService authService;
    #endregion

    #region Properties
    public override string StepName
    {
        get { return UILocalizationTable.Get("customize-binder.step-name"); }
    }
    #endregion

    #region Methods
    public override IEnumerator BindAllServices()
    {
        yield return BindWhenReady<UIService>(ui => { uiService = ui; });
        yield return BindWhenReady<PlayerService>(player => { playerService = player; });
        yield return BindWhenReady<AuthService>(auth => { authService = auth; });

        // Resolve dependencies
        customizePresenter = new CustomizePresenter(
            uiService,
            playerService,
            authService,
            customizeView,
            skinCatalog
        );
    }

    private void OnDestroy()
    {
        customizePresenter?.Dispose();
    }
    #endregion
}