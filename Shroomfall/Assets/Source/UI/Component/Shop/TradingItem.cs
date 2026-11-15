using Assets.Source.Core;
using Assets.Source.Data;
using Assets.Source.Service;
using Contract.DTO.Definition.MetaDomain;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Source.UI.Component.Shop
{
    public class TradingItem :
        MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler
    {
        #region Attributes
        [Header("References")]
        [SerializeField] private Image itemIcon;
        [SerializeField] private TMP_Text itemNameText;
        [SerializeField] private TMP_Text priceText;

        [Header("Buttons")]
        [SerializeField] private UnityEngine.UI.Button tradeButton;

        [Header("Price Colors")]
        [SerializeField] private Color sellPriceColor = Color.green;
        [SerializeField] private Color buyPriceColor = Color.red;

        [Header("Data")]
        [SerializeField] private ItemCatalogSO itemCatalog;

        private DefinitionService definitionService;
        private ItemDefinitionDTO definition;
        private Action onTrade;
        #endregion

        #region Properties
        public event Action<ItemDefinitionDTO> OnHovered;
        public event Action OnUnhovered;
        #endregion

        #region Methods
        private void Awake()
        {
            definitionService = ServiceProvider.Get<DefinitionService>();

            // Buttons
            tradeButton.onClick.AddListener(OnTrade);
        }

        private void OnDestroy()
        {
            // Buttons
            tradeButton.onClick.RemoveAllListeners();
        }

        public void Render(
            ItemDefinitionDTO definition,
            int price,
            bool isSell,
            Action onTrade)
        {
            this.definition = definition;
            this.onTrade = onTrade;

            itemNameText.text = definitionService.GetLocalizedText(definition.Presentation.LocalizedText.NameKey);

            // Sign shows the effect on the player's money: selling adds, buying spends
            priceText.text = isSell ? $"+{price}" : $"-{price}";
            priceText.color = isSell ? sellPriceColor : buyPriceColor;

            if (itemCatalog.TryGet(definition.Id, out ItemAsset asset))
                itemIcon.sprite = asset.icon;
            else
                itemIcon.sprite = null;
        }

        public void SetInteractable(
            bool interactable)
        {
            tradeButton.interactable = interactable;
        }

        public void OnPointerEnter(
            PointerEventData eventData)
        {
            OnHovered?.Invoke(definition);
        }

        public void OnPointerExit(
            PointerEventData eventData)
        {
            OnUnhovered?.Invoke();
        }

        private void OnTrade()
        {
            onTrade?.Invoke();
        }
        #endregion
    }
}