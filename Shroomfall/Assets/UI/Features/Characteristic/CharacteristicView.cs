using Assets.UI.Abstraction;
using Assets.Utilities;
using Contract.Enum.MetaDomain.Effect;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[Serializable]
public struct AttributeUIBinding
{
    public AttributeType Type;
    public TMP_Text ValueText;
}

public class CharacteristicView : MonoBehaviour, IHUDView
{
    #region Attributes
    [Header("Root")]
    [SerializeField] private GameObject root;

    [Header("Labels")]
    [SerializeField] private List<AttributeUIBinding> attributeBindings;
    [SerializeField] private TMP_Text resistanceLabel;
    [SerializeField] private TMP_Text powerLabel;
    [SerializeField] private TMP_Text penetrationLabel;
    [SerializeField] private TMP_Text utilityLabel;

    private readonly Dictionary<AttributeType, TMP_Text> bindingMap = new();
    #endregion

    #region Methods
    private void Awake()
    {
        bindingMap.Clear();
        if (attributeBindings == null)
            return;

        foreach (var binding in attributeBindings)
        {
            if (binding.ValueText == null)
                continue;

            bindingMap.TryAdd(binding.Type, binding.ValueText);
        }
    }

    public void Show()
    {
        root.SetActive(true);
        RefreshLocalizedText();
    }

    public void Hide()
    {
        root.SetActive(false);
    }

    public void UpdateAllAttributes(
        IReadOnlyDictionary<AttributeType, float> attributeValues)
    {
        foreach (var (type, value) in attributeValues)
        {
            if (bindingMap.TryGetValue(type, out var textComponent))
            {
                textComponent.text = value.ToString();
            }
        }
    }

    private void RefreshLocalizedText()
    {
        // Labels
        resistanceLabel.text = UILocalizationTable.Get("characteristic.label-resistance");
        powerLabel.text = UILocalizationTable.Get("characteristic.label-power");
        penetrationLabel.text = UILocalizationTable.Get("characteristic.label-penetration");
        utilityLabel.text = UILocalizationTable.Get("characteristic.label-utility");
    }
    #endregion
}