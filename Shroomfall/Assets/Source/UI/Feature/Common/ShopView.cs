using Assets.Source.Core;
using Assets.Source.Service;
using Assets.Source.UI.Component.Shop;
using Assets.Source.Utility;
using Contract;
using Contract.DTO.Definition.MetaDomain;
using Contract.DTO.Runtime.MetaDomain;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Source.UI.Feature.Common
{
    public class ShopView : MonoBehaviour
    {
        #region Attributes
        [Header("Root")]
        [SerializeField] private GameObject canvas;

        [Header("Buttons")]
        [SerializeField] private Button closeButton;
        [SerializeField] private Button buyTabButton;
        [SerializeField] private Button sellTabButton;

        [Header("References")]
        [SerializeField] private Transform content;
        [SerializeField] private TradingItem tradingItemPrefab;

        [Header("Description")]
        [SerializeField] private TMP_Text descriptionText;

        private UIService uiService;
        private DefinitionService definitionService;
        private InventoryService inventoryService;

        private readonly List<TradingItem> tradingItems = new();
        private bool isSellMode;
        private bool isInteractable = true;
        #endregion

        #region Properties
        #endregion

        #region Methods
        public void Awake()
        {
            uiService = ServiceProvider.Get<UIService>();
            definitionService = ServiceProvider.Get<DefinitionService>();
            inventoryService = ServiceProvider.Get<InventoryService>();

            // Services
            uiService.OnShopRequested += Open;
            uiService.OnCloseAllPopupsRequested += Close;
            uiService.OnGlobalInteractableChanged += SetInteractable;
            definitionService.OnChanged += Render;
            inventoryService.OnChanged += Render;

            // Buttons
            closeButton.onClick.AddListener(Close);
            buyTabButton.onClick.AddListener(ShowBuy);
            sellTabButton.onClick.AddListener(ShowSell);

            canvas.SetActive(false);
        }

        private void OnDestroy()
        {
            // Services
            uiService.OnShopRequested -= Open;
            uiService.OnCloseAllPopupsRequested -= Close;
            uiService.OnGlobalInteractableChanged -= SetInteractable;
            definitionService.OnChanged -= Render;
            inventoryService.OnChanged -= Render;

            // Buttons
            closeButton.onClick.RemoveAllListeners();
            buyTabButton.onClick.RemoveAllListeners();
            sellTabButton.onClick.RemoveAllListeners();

            ClearTradingItems();
        }

        public void Open()
        {
            canvas.SetActive(true);
            ShowBuy();
        }

        public void Close()
        {
            canvas.SetActive(false);
            ClearTradingItems();
        }

        private void SetInteractable(
            bool interactable)
        {
            isInteractable = interactable;
            closeButton.interactable = interactable;
            UpdateTabButtons();

            foreach (TradingItem item in tradingItems)
                item.SetInteractable(interactable);
        }

        #region Tabs
        private void ShowBuy()
        {
            isSellMode = false;
            Render();
        }

        private void ShowSell()
        {
            isSellMode = true;
            Render();
        }

        private void UpdateTabButtons()
        {
            // The active tab is disabled so it reads as selected
            buyTabButton.interactable = isInteractable && isSellMode;
            sellTabButton.interactable = isInteractable && !isSellMode;
        }
        #endregion

        #region Trading Items
        private void Render()
        {
            if (!canvas.activeSelf)
                return;

            ClearTradingItems();
            UpdateTabButtons();
            ClearDescription();

            if (isSellMode)
                RenderSellItems();
            else
                RenderBuyItems();
        }

        private void RenderBuyItems()
        {
            var items = definitionService.Snapshot?.Items;
            if (items == null)
                return;

            foreach (ItemDefinitionDTO definition in items
                .Where(item => item?.TradingConfig != null)
                .OrderBy(item => item.Id))
            {
                string definitionId = definition.Id;
                AddTradingItem(definition, definition.TradingConfig.Price, () =>
                    AsyncHelper.Run(uiService, async () => await inventoryService.BuyItem(definitionId)));
            }
        }

        private void RenderSellItems()
        {
            if (inventoryService.IndexedInventory == null)
                return;

            // Equipped items can't be sold, same rule as the server
            foreach (ItemInstanceDTO item in inventoryService.IndexedInventory
                .Where(item => item != null && !item.EquippedSlot.HasValue))
            {
                ItemDefinitionDTO definition = definitionService.GetItemDefinition(item.DefinitionID);
                if (definition?.TradingConfig == null)
                    continue;

                string itemInstanceId = item.ID;
                AddTradingItem(definition, GetSellPrice(definition.TradingConfig.Price), () =>
                    AsyncHelper.Run(uiService, async () => await inventoryService.SellItem(itemInstanceId)));
            }
        }

        // Must match the server's ItemService.Sell rounding (floor)
        private static int GetSellPrice(
            int price)
        {
            return Mathf.FloorToInt(price * (1f - Constraint.SELL_PRICE_REDUCTION));
        }

        private void AddTradingItem(
            ItemDefinitionDTO definition,
            int price,
            System.Action onTrade)
        {
            TradingItem tradingItem = Instantiate(tradingItemPrefab, content);
            tradingItem.Render(definition, price, isSellMode, onTrade);
            tradingItem.SetInteractable(isInteractable);
            tradingItem.OnHovered += ShowDescription;
            tradingItem.OnUnhovered += ClearDescription;
            tradingItems.Add(tradingItem);
        }

        private void ClearTradingItems()
        {
            foreach (TradingItem item in tradingItems)
            {
                if (item == null)
                    continue;

                item.OnHovered -= ShowDescription;
                item.OnUnhovered -= ClearDescription;
                Destroy(item.gameObject);
            }

            tradingItems.Clear();
        }
        #endregion

        #region Description
        private void ShowDescription(
            ItemDefinitionDTO definition)
        {
            descriptionText.text = definitionService.GetLocalizedText(definition.Presentation.LocalizedText.DescriptionKey);
        }

        private void ClearDescription()
        {
            descriptionText.text = string.Empty;
        }
        #endregion
        #endregion
    }
}