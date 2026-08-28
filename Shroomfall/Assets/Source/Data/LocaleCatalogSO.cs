using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Source.Data
{
    [Serializable]
    public class LocaleAsset
    {
        public string id;
        public Sprite icon;
        public AnimatorOverrideController animatorOverride;
    }

    [CreateAssetMenu(menuName = "UI/Locale Catalog")]
    public class LocaleCatalogSO : ScriptableObject
    {
        #region Attributes
        [SerializeField]
        private List<LocaleAsset> locales;
        private Dictionary<string, LocaleAsset> lookup;
        #endregion

        #region Properties
        #endregion

        #region Methods
        private void OnEnable()
        {
            BuildLookup();
        }

        public bool TryGet(
            string id,
            out LocaleAsset asset)
        {
            return lookup.TryGetValue(id, out asset);
        }

        public IReadOnlyList<LocaleAsset> GetAll()
        {
            return locales;
        }

        private void BuildLookup()
        {
            lookup = new Dictionary<string, LocaleAsset>();

            foreach (var locale in locales)
            {
                if (locale == null)
                    continue;

                if (string.IsNullOrEmpty(locale.id))
                    continue;

                lookup[locale.id] = locale;
            }
        }
        #endregion
    }
}