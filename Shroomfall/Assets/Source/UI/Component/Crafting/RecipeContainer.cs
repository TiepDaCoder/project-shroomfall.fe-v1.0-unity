using Assets.Source.Core;
using Assets.Source.Data;
using Assets.Source.Service;
using Assets.Source.Utility;
using Contract.DTO.Definition.MetaDomain;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Source.UI.Component.Crafting
{
    public class RecipeContainer : MonoBehaviour
    {
        #region Attributes
        [Header("References")]
        [SerializeField] private TMP_Text recipeNameText;
        [SerializeField] private Image outputItemIcon;

        [Header("Buttons")]
        [SerializeField] private UnityEngine.UI.Button craftButton;

        [Header("Required Materials")]
        [SerializeField] private Transform requiredItemsContent;
        [SerializeField] private RequiredItem requiredItemPrefab;

        [Header("Data")]
        [SerializeField] private ItemCatalogSO itemCatalog;

        private DefinitionService definitionService;
        private InventoryService inventoryService;
        private UIService uiService;
        private string itemDefinitionId = string.Empty;
        private readonly List<RequiredItem> requiredItems = new();
        #endregion

        #region Properties
        #endregion

        #region Methods
        private void Awake()
        {
            definitionService = ServiceProvider.Get<DefinitionService>();
            inventoryService = ServiceProvider.Get<InventoryService>();
            uiService = ServiceProvider.Get<UIService>();

            // Buttons
            craftButton.onClick.AddListener(OnCraft);
        }

        private void OnDestroy()
        {
            ClearRequiredItems();

            // Buttons
            craftButton.onClick.RemoveAllListeners();
        }

        public void Render(
            Transform tooltipRoot,
            ItemDefinitionDTO definition)
        {
            if (definitionService?.Snapshot == null)
                return;

            // Clear old screens
            ClearRequiredItems();

            // Set definition ID
            itemDefinitionId = definition.Id;

            // Render recipe name and output icon.
            recipeNameText.text = definitionService.GetLocalizedText(definition.Presentation.LocalizedText.NameKey);

            if (itemCatalog.TryGet(definition.Id, out ItemAsset asset))
                outputItemIcon.sprite = asset.icon;
            else
                outputItemIcon.sprite = null;

            // Render required materials.
            foreach (RequiredMaterialDTO material in definition.RequiredMaterials)
            {
                if (material == null)
                    continue;

                RequiredItem requiredItem = Instantiate(requiredItemPrefab, requiredItemsContent);
                requiredItem.Render(material);
                requiredItem.SetTooltipRoot(tooltipRoot);
                requiredItems.Add(requiredItem);
            }
        }

        private void OnCraft()
        {
            AsyncHelper.Run(uiService, async () =>
            {
                await inventoryService.CraftItem(itemDefinitionId);
            });
        }

        private void ClearRequiredItems()
        {
            itemDefinitionId = string.Empty;
            foreach (RequiredItem item in requiredItems)
                if (item != null)
                    Destroy(item.gameObject);

            requiredItems.Clear();
        }
        #endregion
    }
}