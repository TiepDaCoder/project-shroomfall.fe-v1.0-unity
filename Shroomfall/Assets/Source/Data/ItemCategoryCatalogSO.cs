using Contract.Enum.MetaDomain.Item;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Source.Data
{
    [Serializable]
    public class ItemCategoryAsset
    {
        public ItemCategory Category;
        public Sprite Icon;
    }

    [CreateAssetMenu(fileName = "ItemCategoryCatalog", menuName = "Data/Item Category Catalog")]
    public class ItemCategoryCatalogSO : ScriptableObject
    {
        #region Attributes
        [SerializeField]
        private List<ItemCategoryAsset> itemCategories;
        private Dictionary<ItemCategory, Sprite> lookup;
        #endregion

        #region Properties
        #endregion

        #region Methods
        private void OnEnable()
        {
            BuildLookup();
        }

        public bool TryGet(
            ItemCategory category,
            out Sprite sprite)
        {
            return lookup.TryGetValue(category, out sprite);
        }

        public IReadOnlyList<ItemCategoryAsset> GetAll()
        {
            return itemCategories;
        }

        private void BuildLookup()
        {
            lookup = new Dictionary<ItemCategory, Sprite>();

            foreach (var itemCategory in itemCategories)
            {
                if (itemCategory == null)
                    continue;

                lookup[itemCategory.Category] = itemCategory.Icon;
            }
        }
        #endregion
    }
}