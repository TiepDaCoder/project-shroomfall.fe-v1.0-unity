using Assets.Source.Core;
using Assets.Source.Enum;
using Assets.Source.Service;
using UnityEngine;

namespace Assets.Source.Gameplay.Component.Input
{
    public class TriggerView : MonoBehaviour
    {
        #region Attributes
        [SerializeField] private TriggerPlateType type;
        [SerializeField] private string hubRoomSpatialId;

        private SessionService sessionService;
        private UIService uiService;
        #endregion

        #region Properties
        #endregion

        #region Methods
        private void Awake()
        {
            sessionService = ServiceProvider.Get<SessionService>();
            uiService = ServiceProvider.Get<UIService>();
        }

        public async void Interact()
        {
            switch (type)
            {
                case TriggerPlateType.BackHome:
                    await sessionService.BackHomeAsync();
                    break;

                case TriggerPlateType.EnterHub:
                    await sessionService.EnterHubAsync(hubRoomSpatialId);
                    break;

                case TriggerPlateType.TravelBoard:
                    uiService.ShowTravelBoard();
                    break;
            }
        }
        #endregion
    }
}