using Assets.Source.Core;
using Assets.Source.Service;
using TMPro;
using UnityEngine;

namespace Assets.Source.UI.Component.Shared
{
    public class Localize : MonoBehaviour
    {
        #region Attributes
        [SerializeField] private TMP_Text text;
        [SerializeField] private string key;

        private LocalizeService localizeService;
        #endregion

        #region Properties
        #endregion

        #region Methods
        private void Start()
        {
            localizeService = ServiceProvider.Get<LocalizeService>();
            localizeService.OnLocaleChanged += Refresh;

            Refresh();
        }

        private void OnDestroy()
        {
            if (localizeService != null)
                localizeService.OnLocaleChanged -= Refresh;
        }

        private void Refresh()
        {
            text.text = localizeService.Get(key);
        }
        #endregion
    }
}