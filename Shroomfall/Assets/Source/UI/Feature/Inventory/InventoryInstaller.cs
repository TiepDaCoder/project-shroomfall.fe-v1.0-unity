using Assets.Source.Service;
using Assets.Source.UI.Abstraction;
using Assets.Source.Utility;
using System.Collections;
using UnityEngine;

namespace Assets.Source.UI.Feature.Inventory
{
    public class InventoryInstaller : Installer
    {
        #region Attributes
        [SerializeField]
        private InventoryView inventoryView;
        private InventoryPresenter inventoryPresenter;

        private PlayerService playerService;
        private DefinitionService definitionService;
        #endregion

        #region Properties
        public override string StepName
        {
            get { return UILocalizationTable.Get("inventory-binder.step-name"); }
        }
        #endregion

        #region Methods
        public override IEnumerator BindAllServices()
        {
            yield return BindWhenReady<PlayerService>(player => { playerService = player; });
            yield return BindWhenReady<DefinitionService>(definition => { definitionService = definition; });

            // Resolve dependencies
            inventoryPresenter = new InventoryPresenter(
                playerService,
                definitionService,
                inventoryView);
        }

        private void OnDestroy()
        {
            inventoryPresenter?.Dispose();
        }
        #endregion
    }
}