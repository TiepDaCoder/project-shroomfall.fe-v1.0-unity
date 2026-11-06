using Assets.Source.Service;
using Assets.Source.UI.Abstraction;
using Assets.Source.Utility;
using System.Collections;
using UnityEngine;

namespace Assets.Source.UI.Feature.SignIn
{
    public class SignInInstaller : Installer
    {
        #region Attributes
        [SerializeField]
        private SignInView signInView;
        private SignInPresenter signInPresenter;

        private UIService uiService;
        private GameService gameService;
        #endregion

        #region Properties
        public override string StepName
        {
            get { return UILocalizationTable.Get("sign-in-binder.step-name"); }
        }
        #endregion

        #region Methods
        public override IEnumerator BindAllServices()
        {
            yield return BindWhenReady<UIService>(ui => { uiService = ui; });
            yield return BindWhenReady<GameService>(game => { gameService = game; });

            // Resolve dependencies
            signInPresenter = new SignInPresenter(
                uiService,
                gameService,
                signInView);
        }

        private void OnDestroy()
        {
            signInPresenter?.Dispose();
        }
        #endregion
    }
}