using Assets.Source.Core;
using Assets.Source.Service;
using Assets.Source.UI.Model;
using Contract.DTO.Definition.EntityDomain.Component;
using Contract.DTO.Runtime.EntityDomain.Component;
using Contract.Enum.MetaDomain.Effect;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Source.UI.Feature.HUD
{
    public class CharacteristicView : MonoBehaviour, IHUDView
    {
        #region Attributes
        [Header("Root")]
        [SerializeField] private GameObject root;

        [Header("Attributes")]
        [SerializeField] private List<AttributeUIModel> attributeUis;

        [Header("Health")]
        [SerializeField] private Image healthBar;
        [SerializeField] private TMP_Text healthText;

        [Header("Energy")]
        [SerializeField] private Image energyBar;
        [SerializeField] private TMP_Text energyText;

        private PlayerService playerService;
        private DefinitionService definitionService;

        private readonly Dictionary<AttributeType, TMP_Text> bindingMap = new();
        #endregion

        #region Properties
        #endregion

        #region Methods
        private void Awake()
        {
            playerService = ServiceProvider.Get<PlayerService>();
            definitionService = ServiceProvider.Get<DefinitionService>();

            // Player Service
            playerService.OnCharacteristicSync += CharacteristicSync;

            // Definition Service
            definitionService.OnChanged += CharacteristicSync;

            InitializeBindingMap();
            CharacteristicSync();

            root.SetActive(false);
        }

        private void OnDestroy()
        {
            // Player Service
            playerService.OnCharacteristicSync -= CharacteristicSync;

            // Definition Service
            definitionService.OnChanged -= CharacteristicSync;

        }

        #region HUD Action
        public void Show()
        {
            root.SetActive(true);
            CharacteristicSync();
        }

        public void Hide()
        {
            root.SetActive(false);
        }
        #endregion

        #region Initialization
        private void InitializeBindingMap()
        {
            if (attributeUis == null)
                return;

            bindingMap.Clear();
            foreach (var binding in attributeUis)
            {
                if (binding.ValueText == null)
                    continue;

                bindingMap.TryAdd(binding.Type, binding.ValueText);
            }
        }
        #endregion

        private void CharacteristicSync()
        {
            var runtime = playerService?.Runtime;
            if (runtime?.Components == null)
                return;

            // Get the runtime characteristic component.
            var characteristic = runtime.Components.OfType<CharacteristicInstanceDTO>().FirstOrDefault();
            if (characteristic == null)
                return;

            // Get the entity definition.
            var entityDefinition = definitionService.GetEntityDefinition(runtime.DefinitionID.ToString());
            if (entityDefinition == null)
                return;

            // Match the runtime component to its definition component.
            var characteristicDefinition = entityDefinition.Components?.OfType<CharacteristicDefinitionDTO>().FirstOrDefault();
            if (characteristicDefinition == null)
                return;

            // Collect current runtime values.
            var currentValues = new Dictionary<AttributeType, float>();
            if (characteristic.Cores != null)
            {
                foreach (var attribute in characteristic.Cores)
                    currentValues[attribute.AttributeType] = attribute.Value;
            }

            if (characteristic.Vitals != null)
            {
                foreach (var attribute in characteristic.Vitals)
                    currentValues[attribute.AttributeType] = attribute.Value;
            }

            // Update existing characteristic text bindings.
            foreach (var (type, value) in currentValues)
            {
                if (bindingMap.TryGetValue(type, out var text))
                    text.text = value.ToString("0.##");
            }

            // Read maximum values from the definition.
            var maxValues = characteristicDefinition.AttributeValues?
                .GroupBy(x => x.Type)
                .ToDictionary(g => g.Key, g => g.First().Max) ?? new Dictionary<AttributeType, float>();

            // Update health and energy.
            UpdateBar(AttributeType.Health, currentValues, maxValues, healthBar, healthText);
            UpdateBar(AttributeType.Energy, currentValues, maxValues, energyBar, energyText);
        }

        private void UpdateBar(
            AttributeType type,
            Dictionary<AttributeType, float> currentValues,
            Dictionary<AttributeType, float> maxValues,
            Image bar,
            TMP_Text valueText)
        {
            if (bar == null)
                return;

            float current = currentValues.TryGetValue(type, out var value) ? value : 0f;
            float max = maxValues.TryGetValue(type, out var maximum) ? maximum : 0f;
            bar.fillAmount = max > 0f ? Mathf.Clamp01(current / max) : 0f;

            if (valueText != null)
                valueText.text = $"{current:0.##} / {max:0.##}";
        }
        #endregion
    }
}