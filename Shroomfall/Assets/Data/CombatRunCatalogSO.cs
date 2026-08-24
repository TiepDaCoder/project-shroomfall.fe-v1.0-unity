using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CombatRunAsset
{
    public string id;
    public Sprite icon;
}

[CreateAssetMenu(menuName = "Run/Combat Run Catalog")]
public class CombatRunCatalogSO : ScriptableObject
{
    #region Attributes
    [SerializeField]
    private List<CombatRunAsset> combatRuns;
    private Dictionary<string, CombatRunAsset> lookup;
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
        out CombatRunAsset asset)
    {
        return lookup.TryGetValue(id, out asset);
    }

    public IReadOnlyList<CombatRunAsset> GetAll()
    {
        return combatRuns;
    }

    private void BuildLookup()
    {
        lookup = new Dictionary<string, CombatRunAsset>();

        foreach (var combatRun in combatRuns)
        {
            if (combatRun == null)
                continue;

            if (string.IsNullOrEmpty(combatRun.id))
                continue;

            lookup[combatRun.id] = combatRun;
        }
    }
    #endregion
}