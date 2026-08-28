using Assets.Source.Core;
using Assets.Source.Enum;
using Assets.Service;
using UnityEngine;

namespace Assets.Source.Gameplay.Component.Input
{
    public class TriggerPlateView : MonoBehaviour
    {
        #region Attributes
        [SerializeField] private TriggerPlateType type;
        [SerializeField] private string hubRoomSpatialId;

        private GameService gameService;
        #endregion

        #region Properties
        #endregion

        #region Methods
        private void Awake()
        {
            gameService = ServiceProvider.Get<GameService>();
        }

        public async void Interact()
        {
            switch (type)
            {
                case TriggerPlateType.BackHome:
                    await gameService.BackHome();
                    break;

                case TriggerPlateType.EnterHub:
                    await gameService.EnterHub(hubRoomSpatialId);
                    break;

                case TriggerPlateType.EnterCombat:
                    gameService.PushPhase(GamePhase.JoinCombat);
                    break;
            }
        }
        #endregion
    }
}