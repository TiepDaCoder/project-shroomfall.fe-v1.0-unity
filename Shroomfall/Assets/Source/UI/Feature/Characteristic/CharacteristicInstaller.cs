using Assets.Source.Service;
using Assets.Source.UI.Abstraction;
using Assets.Source.Utility;
using System.Collections;
using UnityEngine;

namespace Assets.Source.UI.Feature.Characteristic
{
    public class CharacteristicInstaller : Installer
    {
        #region Attributes
        [SerializeField]
        private CharacteristicView characteristicView;
        private CharacteristicPresenter characteristicPresenter;

        private PlayerService playerService;
        #endregion

        #region Properties
        public override string StepName
        {
            get { return UILocalizationTable.Get("characteristic-binder.step-name"); }
        }
        #endregion

        #region Methods
        public override IEnumerator BindAllServices()
        {
            yield return BindWhenReady<PlayerService>(player => { playerService = player; });

            // Resolve dependencies
            characteristicPresenter = new CharacteristicPresenter(
                playerService,
                characteristicView);
        }

        private void OnDestroy()
        {
            characteristicPresenter?.Dispose();
        }
        #endregion
    }
}