using Assets.Source.Core;
using Assets.Source.Service;
using Assets.Source.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Source.UI.Feature.Common
{
    public class TravelBoardView : MonoBehaviour
    {
        #region Attributes
        [Header("Root")]
        [SerializeField] private GameObject canvas;

        [Header("Buttons")]
        [SerializeField] private Button closeButton;
        [SerializeField] private Button forestButton;

        private UIService uiService;
        private SessionService sessionService;
        #endregion

        #region Properties
        #endregion

        #region Methods
        public void Awake()
        {
            uiService = ServiceProvider.Get<UIService>();
            sessionService = ServiceProvider.Get<SessionService>();

            // UI Service
            uiService.OnTravelBoardRequested += Open;
            uiService.OnGlobalInteractableChanged += SetInteractable;

            // Buttons
            closeButton.onClick.AddListener(Close);
            forestButton.onClick.AddListener(TravelToForest);

            canvas.SetActive(false);
        }

        private void OnDestroy()
        {
            // UI Service
            uiService.OnTravelBoardRequested -= Open;
            uiService.OnGlobalInteractableChanged -= SetInteractable;

            // Buttons
            closeButton.onClick.RemoveAllListeners();
            forestButton.onClick.RemoveAllListeners();
        }

        public void Open()
        {
            canvas.SetActive(true);
        }

        private void SetInteractable(
            bool interactable)
        {
            closeButton.interactable = interactable;
            forestButton.interactable = interactable;
        }

        public void Close()
        {
            canvas.SetActive(false);
        }

        private void TravelToForest()
        {
            AsyncHelper.Run(uiService, async () =>
            {
                await sessionService.EnterHubAsync("forest");
            });
        }
        #endregion
    }
}