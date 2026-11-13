using Assets.Source.Core;
using Assets.Source.Data;
using Assets.Source.Service;
using Contract.DTO.Definition.MetaDomain;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Source.UI.Component.Crafting
{
    public class RequiredItem : MonoBehaviour
    {
        #region Attributes
        [Header("References")]
        [SerializeField] private Image itemIcon;
        [SerializeField] private TMP_Text itemNameText;
        [SerializeField] private TMP_Text quantityText;

        [Header("Tooltip")]
        [SerializeField] private GameObject tooltipContainer;

        [Header("Data")]
        [SerializeField] private ItemCatalogSO itemCatalog;

        private DefinitionService definitionService;
        #endregion

        #region Properties
        public string ItemDefinitionID { get; private set; } = string.Empty;
        #endregion

        #region Methods
        private void Awake()
        {
            definitionService = ServiceProvider.Get<DefinitionService>();
        }

        public void SetTooltipRoot(
            Transform root)
        {
            if (tooltipContainer == null || root == null)
                return;

            tooltipContainer.transform.SetParent(root, false);
            tooltipContainer.transform.SetAsLastSibling();
        }

        public void Render(
            RequiredMaterialDTO material)
        {
            if (material == null)
                return;

            string itemId = material.MaterialItemDefinitionID;
            ItemDefinitionID = itemId;
            ItemDefinitionDTO itemDef = definitionService.GetItemDefinition(itemId);

            // Set quantity
            quantityText.text = material.Quantity.ToString();

            // Set name fallback
            if (itemDef == null)
            {
                itemIcon.sprite = null;
                itemNameText.text = itemId;
                return;
            }

            // Set icon
            if (itemCatalog.TryGet(itemId, out ItemAsset asset))
                itemIcon.sprite = asset.icon;
            else
                itemIcon.sprite = null;

            // Set localized name
            itemNameText.text = definitionService.GetLocalizedText(itemDef.Presentation?.LocalizedText?.NameKey);
        }
        #endregion
    }
}