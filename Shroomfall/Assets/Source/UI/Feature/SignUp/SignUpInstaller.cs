using Assets.Source.Service;
using Assets.Source.UI.Abstraction;
using Assets.Source.Utility;
using System.Collections;
using UnityEngine;

namespace Assets.Source.UI.Feature.SignUp
{
    public class SignUpInstaller : Installer
    {
        #region Attributes
        [SerializeField]
        private SignUpView signUpView;
        private SignUpPresenter signUpPresenter;

        private UIService uiService;
        private GameService gameService;
        #endregion

        #region Properties
        public override string StepName
        {
            get { return UILocalizationTable.Get("sign-up-binder.step-name"); }
        }
        #endregion

        #region Methods
        public override IEnumerator BindAllServices()
        {
            yield return BindWhenReady<UIService>(ui => { uiService = ui; });
            yield return BindWhenReady<GameService>(game => { gameService = game; });

            // Resolve dependencies
            signUpPresenter = new SignUpPresenter(
                uiService,
                gameService,
                signUpView);
        }

        private void OnDestroy()
        {
            signUpPresenter?.Dispose();
        }
        #endregion
    }
}