using Assets.Source.Service;
using Assets.Source.UI.Abstraction;
using Assets.Source.Utility;
using System.Collections;
using UnityEngine;

namespace Assets.Source.UI.Feature.JoinCombat
{
    public class JoinCombatInstaller : Installer
    {
        #region Attributes
        [SerializeField]
        private JoinCombatView joinCombatView;
        private JoinCombatPresenter joinCombatPresenter;

        private UIService uiService;
        private GameService gameService;
        #endregion

        #region Properties
        public override string StepName
        {
            get { return UILocalizationTable.Get("join-combat-binder.step-name"); }
        }
        #endregion

        #region Methods
        public override IEnumerator BindAllServices()
        {
            yield return BindWhenReady<UIService>(ui => { uiService = ui; });
            yield return BindWhenReady<GameService>(game => { gameService = game; });

            // Resolve dependencies
            joinCombatPresenter = new JoinCombatPresenter(
                uiService,
                gameService,
                joinCombatView);
        }

        private void OnDestroy()
        {
            joinCombatPresenter?.Dispose();
        }
        #endregion
    }
}