using Assets.Source.Core;
using Assets.Source.Service;
using UnityEngine;

namespace Assets.Source.UI.Feature.Common
{
    public class LoadingView : MonoBehaviour
    {
        #region Attributes
        [Header("Root")]
        [SerializeField] private GameObject canvas;

        private UIService uiService;
        #endregion

        #region Properties
        #endregion

        #region Methods
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            uiService = ServiceProvider.Get<UIService>();

            // UI Service
            uiService.OnLoadingRequested += OnLoadingRequested;
        }

        public void OnLoadingRequested(
            bool active)
        {
            canvas.SetActive(active);
        }
        #endregion
    }
}