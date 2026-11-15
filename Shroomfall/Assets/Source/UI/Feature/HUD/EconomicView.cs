using Assets.Source.Core;
using Assets.Source.Service;
using Contract.DTO.Runtime.EntityDomain.Component;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Assets.Source.UI.Feature.HUD
{
    public class EconomicView : MonoBehaviour
    {
        #region Attributes
        [Header("References")]
        [SerializeField] private TMP_Text moneyText;

        private PlayerService playerService;
        #endregion

        #region Properties
        #endregion

        #region Methods
        private void Awake()
        {
            playerService = ServiceProvider.Get<PlayerService>();

            // Player Service
            playerService.OnEconomicSync += EconomicSync;

            EconomicSync();
        }

        private void OnDestroy()
        {
            // Player Service
            playerService.OnEconomicSync -= EconomicSync;
        }

        private void EconomicSync()
        {
            var economic = playerService?.Runtime?.Components?.OfType<EconomicInstanceDTO>().FirstOrDefault();
            moneyText.text = (economic?.CurrentMoney ?? 0).ToString();
        }
        #endregion
    }
}