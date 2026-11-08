using Assets.Source.Core;
using Assets.Source.Enum;
using Assets.Source.Service;
using UnityEngine;

namespace Assets.Source.UI.Component.Shared
{
    public class MenuBackground : MonoBehaviour
    {
        #region Attributes
        [Header("Background")]
        [SerializeField] private GameObject root;

        private UIService uiService;
        #endregion

        #region Properties
        #endregion

        #region Methods
        private void Awake()
        {
            uiService = ServiceProvider.Get<UIService>();

            // UI Service
            uiService.OnUIStateChanged += OnUIStateChanged;
        }

        private void OnDestroy()
        {
            // UI Service
            uiService.OnUIStateChanged -= OnUIStateChanged;
        }

        private void OnUIStateChanged(
            UIService uiService)
        {
            root.SetActive(uiService.State != UIState.HUD);
        }
        #endregion
    }
}