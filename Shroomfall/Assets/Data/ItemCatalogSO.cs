using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ItemAsset
{
    public string id;
    public Sprite icon;
    public AnimatorOverrideController animatorOverride;
}

[CreateAssetMenu(menuName = "Item/Item Catalog")]
public class ItemCatalogSO : ScriptableObject
{
    #region Attributes
    [SerializeField]
    private List<ItemAsset> items;
    private Dictionary<string, ItemAsset> lookup;
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
        out ItemAsset asset)
    {
        return lookup.TryGetValue(id, out asset);
    }

    public IReadOnlyList<ItemAsset> GetAll()
    {
        return items;
    }

    private void BuildLookup()
    {
        lookup = new Dictionary<string, ItemAsset>();

        foreach (var item in items)
        {
            if (item == null)
                continue;

            if (string.IsNullOrEmpty(item.id))
                continue;

            lookup[item.id] = item;
        }
    }
    #endregion
}