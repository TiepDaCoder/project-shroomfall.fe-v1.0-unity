using UnityEngine;

namespace Assets.Source.UI.Common.Loading
{
    public class LoadingView : MonoBehaviour
    {
        #region Attributes
        [SerializeField] private GameObject canvas;
        #endregion

        #region Properties
        #endregion

        #region Methods
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void SetActive(
            bool active)
        {
            canvas.SetActive(active);
        }
        #endregion
    }
}