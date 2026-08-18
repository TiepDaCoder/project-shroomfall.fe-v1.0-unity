using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EntityAsset
{
    public string id;
    public Sprite icon;
    public AnimatorOverrideController animatorOverride;
}

[CreateAssetMenu(menuName = "Entity/Entity Catalog")]
public class EntityCatalogSO : ScriptableObject
{
    #region Attributes
    [SerializeField]
    private List<EntityAsset> entities;
    private Dictionary<string, EntityAsset> lookup;
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
        out EntityAsset asset)
    {
        return lookup.TryGetValue(id, out asset);
    }

    public IReadOnlyList<EntityAsset> GetAll()
    {
        return entities;
    }

    private void BuildLookup()
    {
        lookup = new Dictionary<string, EntityAsset>();

        foreach (var entity in entities)
        {
            if (entity == null)
                continue;

            if (string.IsNullOrEmpty(entity.id))
                continue;

            lookup[entity.id] = entity;
        }
    }
    #endregion
}