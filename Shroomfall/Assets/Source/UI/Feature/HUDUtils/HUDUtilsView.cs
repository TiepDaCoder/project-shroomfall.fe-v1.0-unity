using Assets.Source.Service;
using Assets.Source.UI.Abstraction;
using Assets.Source.UI.Component.Button;
using Assets.Source.UI.Feature.Characteristic;
using Assets.Source.UI.Feature.Customize;
using Assets.Source.UI.Feature.Inventory;
using Assets.Source.Utility;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Source.UI.Feature.HUDUtils
{
    public class HUDUtilsView : MonoBehaviour
    {
        #region Attributes
        [Header("Buttons")]
        [SerializeField] private TextButton quitButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button customizeButton;
        [SerializeField] private Button inventoryViewButton;
        [SerializeField] private Button characteristicButton;

        [Header("Panels")]
        [SerializeField] private CustomizeView customizeView;
        [SerializeField] private InventoryView inventoryView;
        [SerializeField] private CharacteristicView characteristicView;

        private IHUDView currentView;
        #endregion

        #region Properties
        public bool IsHUDOpen => closeButton.gameObject.activeSelf;

        public event Action OnQuitClicked;
        #endregion

        #region Methods
        private void Awake()
        {
            quitButton.onClick.AddListener(() => OnQuitClicked?.Invoke());
            closeButton.onClick.AddListener(() => Close());
            customizeButton.onClick.AddListener(() => Open(customizeView, customizeButton));
            inventoryViewButton.onClick.AddListener(() => Open(inventoryView, inventoryViewButton));
            characteristicButton.onClick.AddListener(() => Open(characteristicView, characteristicButton));
        }

        private void OnEnable()
        {
            RefreshLocalizedText();
        }

        public void HandleUIState(
            UIService service)
        {
            gameObject.SetActive(service.ShowHUD);
        }

        private void Open(
            IHUDView view,
            Button activeButton)
        {
            Close();

            currentView = view;
            currentView.Show();

            UpdateButtons(activeButton);
            SetButtonsVisible(true);
        }

        private void Close()
        {
            currentView?.Hide();
            currentView = null;

            customizeButton.interactable = true;
            inventoryViewButton.interactable = true;
            characteristicButton.interactable = true;

            SetButtonsVisible(false);
        }

        private void UpdateButtons(
            Button activeButton)
        {
            customizeButton.interactable = customizeButton != activeButton;
            inventoryViewButton.interactable = inventoryViewButton != activeButton;
            characteristicButton.interactable = characteristicButton != activeButton;
        }

        public void ToggleHUD()
        {
            if (closeButton.gameObject.activeSelf)
            {
                Close();
            }
            else
            {
                Open(inventoryView, inventoryViewButton);
            }
        }

        private void SetButtonsVisible(
            bool visible)
        {
            closeButton.gameObject.SetActive(visible);
            customizeButton.gameObject.SetActive(visible);
            inventoryViewButton.gameObject.SetActive(visible);
            characteristicButton.gameObject.SetActive(visible);
        }

        private void RefreshLocalizedText()
        {
            // Button
            quitButton.SetText(UILocalizationTable.Get("hud-utils.btn-quit"));
        }
        #endregion
    }
}