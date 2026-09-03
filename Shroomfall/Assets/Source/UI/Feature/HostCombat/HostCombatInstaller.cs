using Assets.Source.Data;
using Assets.Source.Service;
using Assets.Source.UI.Abstraction;
using Assets.Source.Utility;
using System.Collections;
using UnityEngine;

namespace Assets.Source.UI.Feature.HostCombat
{
    public class HostCombatInstaller : Installer
    {
        #region Attributes
        [SerializeField]
        private HostCombatView hostCombatView;
        private HostCombatPresenter hostCombatPresenter;

        [SerializeField]
        private CombatRunCatalogSO combatRunCatalog;

        private UIService uiService;
        private GameService gameService;
        private DefinitionService definitionService;
        #endregion

        #region Properties
        public override string StepName
        {
            get { return UILocalizationTable.Get("host-combat-binder.step-name"); }
        }
        #endregion

        #region Methods
        public override IEnumerator BindAllServices()
        {
            yield return BindWhenReady<UIService>(ui => { uiService = ui; });
            yield return BindWhenReady<GameService>(game => { gameService = game; });
            yield return BindWhenReady<DefinitionService>(definition => { definitionService = definition; });

            // Resolve dependencies
            hostCombatPresenter = new HostCombatPresenter(
                uiService,
                gameService,
                definitionService,
                hostCombatView,
                combatRunCatalog);
        }

        private void OnDestroy()
        {
            hostCombatPresenter?.Dispose();
        }
        #endregion
    }
}