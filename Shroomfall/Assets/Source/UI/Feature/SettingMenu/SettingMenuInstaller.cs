using Assets.Source.Data;
using Assets.Source.Service;
using Assets.Source.UI.Abstraction;
using Assets.Source.Utility;
using System.Collections;
using UnityEngine;

namespace Assets.Source.UI.Feature.SettingMenu
{
    public class SettingMenuInstaller : Installer
    {
        #region Attributes
        [Header("Asset Catalogs")]
        [SerializeField] private LocaleCatalogSO localeCatalog;

        [SerializeField]
        private SettingMenuView settingMenuView;
        private SettingMenuPresenter settingMenuPresenter;

        private GameService gameService;
        private UIService uiService;
        private SettingService settingService;
        #endregion

        #region Properties
        public override string StepName
        {
            get { return UILocalizationTable.Get("setting-menu-binder.step-name"); }
        }
        #endregion

        #region Methods
        public override IEnumerator BindAllServices()
        {
            yield return BindWhenReady<GameService>(game => { gameService = game; });
            yield return BindWhenReady<UIService>(ui => { uiService = ui; });
            yield return BindWhenReady<SettingService>(setting => { settingService = setting; });

            // Resolve dependencies
            settingMenuPresenter = new SettingMenuPresenter(
                gameService,
                uiService,
                settingService,
                settingMenuView,
                localeCatalog);
        }

        private void OnDestroy()
        {
            settingMenuPresenter?.Dispose();
        }
        #endregion
    }
}