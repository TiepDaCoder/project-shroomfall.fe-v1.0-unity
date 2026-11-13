using Assets.Source.Core;
using Assets.Source.Service;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Source.UI.Feature.HUD
{
    public class HUDView : MonoBehaviour
    {
        #region 
        [Header("Canvas")]
        [SerializeField] private GameObject canvas;

        [Header("Buttons")]
        [SerializeField] private Button closeButton;
        [SerializeField] private Button customizeButton;
        [SerializeField] private Button inventoryViewButton;
        [SerializeField] private Button characteristicButton;
        [SerializeField] private Button craftingButton;
        [SerializeField] private Button settingButton;

        [Header("Views")]
        [SerializeField] private CustomizeView customizeView;
        [SerializeField] private InventoryView inventoryView;
        [SerializeField] private CharacteristicView characteristicView;
        [SerializeField] private CraftingView craftingView;

        [Header("Backgrounds")]
        [SerializeField] private GameObject tabBackground;

        private UIService uiService;

        private IHUDView currentView;
        #endregion

        #region Properties
        #endregion

        #region Methods
        private void Awake()
        {
            uiService = ServiceProvider.Get<UIService>();

            // UI Service
            uiService.OnUIStateChanged += OnUIStateChanged;
            uiService.OnGlobalInteractableChanged += SetInteractable;
            uiService.OnBackpackToggle += ToggleBackpack;

            // Buttons
            closeButton.onClick.AddListener(() => uiService.ToggleBackpack());
            customizeButton.onClick.AddListener(() => Open(customizeView, customizeButton));
            inventoryViewButton.onClick.AddListener(() => Open(inventoryView, inventoryViewButton));
            characteristicButton.onClick.AddListener(() => Open(characteristicView, characteristicButton));
            craftingButton.onClick.AddListener(() => Open(craftingView, craftingButton));
            settingButton.onClick.AddListener(() => uiService.ShowSetting());
        }

        private void OnDestroy()
        {
            // UI Service
            uiService.OnUIStateChanged -= OnUIStateChanged;
            uiService.OnGlobalInteractableChanged -= SetInteractable;
            uiService.OnBackpackToggle -= ToggleBackpack;

            // Buttons
            closeButton.onClick.RemoveAllListeners();
            customizeButton.onClick.RemoveAllListeners();
            inventoryViewButton.onClick.RemoveAllListeners();
            characteristicButton.onClick.RemoveAllListeners();
            craftingButton.onClick.RemoveAllListeners();
            settingButton.onClick.RemoveAllListeners();
        }

        private void OnUIStateChanged(
            UIService service)
        {
            bool visible = service.ShowHUD;
            canvas.SetActive(visible);

            if (visible)
                Open(inventoryView, inventoryViewButton);
        }

        private void SetInteractable(
            bool interactable)
        {
            closeButton.interactable = interactable;
            customizeButton.interactable = interactable;
            inventoryViewButton.interactable = interactable;
            characteristicButton.interactable = interactable;
            craftingButton.interactable = interactable;
        }

        private void ToggleBackpack()
        {
            if (uiService.IsBackpackOpen)
                Open(inventoryView, inventoryViewButton);
            else
                Close();
        }

        private void Close()
        {
            currentView?.Hide();
            currentView = null;

            customizeButton.interactable = true;
            inventoryViewButton.interactable = true;
            characteristicButton.interactable = true;
            craftingButton.interactable = true;

            tabBackground.SetActive(false);

            SetButtonsVisible(false);
        }

        private void Open(
            IHUDView view,
            Button activeButton)
        {
            Close();

            currentView = view;
            currentView.Show();

            tabBackground.SetActive(true);

            UpdateButtons(activeButton);
            SetButtonsVisible(true);
        }

        private void UpdateButtons(
            Button activeButton)
        {
            customizeButton.interactable = customizeButton != activeButton;
            inventoryViewButton.interactable = inventoryViewButton != activeButton;
            characteristicButton.interactable = characteristicButton != activeButton;
            craftingButton.interactable = craftingButton != activeButton;
        }

        private void SetButtonsVisible(
            bool visible)
        {
            closeButton.gameObject.SetActive(visible);
            customizeButton.gameObject.SetActive(visible);
            inventoryViewButton.gameObject.SetActive(visible);
            characteristicButton.gameObject.SetActive(visible);
            craftingButton.gameObject.SetActive(visible);
        }
        #endregion
    }
}