using Assets.Source.Data;
using Assets.Source.Enum;
using Assets.Source.Service;
using Assets.UI.Models;
using Contract;
using System;
using System.Collections.Generic;

namespace Assets.UI.Features.SettingMenu
{
    public class SettingMenuPresenter
    {
        #region Attributes
        private readonly GameService gameService;
        private readonly UIService uiService;
        private readonly SettingService settingService;
        private readonly SettingMenuView settingMenuView;

        private readonly LocaleCatalogSO localeCatalog;

        private bool disposed;
        #endregion

        #region Properties
        #endregion

        public SettingMenuPresenter(
            GameService gameService,
            UIService uiService,
            SettingService settingService,
            SettingMenuView settingMenuView,

            LocaleCatalogSO localeCatalog)
        {
            this.gameService = gameService;
            this.uiService = uiService;
            this.settingService = settingService;
            this.settingMenuView = settingMenuView;

            this.localeCatalog = localeCatalog;

            Bind();
        }

        #region Methods
        public void Dispose()
        {
            if (disposed) return;
            disposed = true;

            // Outbound
            uiService.OnUIStateChanged -= OnUIStateChanged;
            uiService.OnGlobalInteractableChanged -= settingMenuView.SetInteractable;

            // Inbound
            settingMenuView.OnBackClicked -= OnBackClicked;
            settingMenuView.OnScreenSelectionChanged -= OnScreenSelectionChanged;
            settingMenuView.OnLocaleSelectionChanged -= OnLocaleSelectionChanged;
            settingMenuView.OnSettingMenuShown -= OnSettingMenuShown;
        }

        private void Bind()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(SettingMenuPresenter));

            // Outbound
            uiService.OnUIStateChanged += OnUIStateChanged;
            uiService.OnGlobalInteractableChanged += settingMenuView.SetInteractable;

            // Inbound
            settingMenuView.OnBackClicked += OnBackClicked;
            settingMenuView.OnScreenSelectionChanged += OnScreenSelectionChanged;
            settingMenuView.OnLocaleSelectionChanged += OnLocaleSelectionChanged;
            settingMenuView.OnSettingMenuShown += OnSettingMenuShown;
        }

        private void OnUIStateChanged(
            UIService service)
        {
            settingMenuView.SetVisible(service.ShowSettingMenu);
        }

        private void OnBackClicked()
        {
            gameService.PopPhase();
        }

        private void OnScreenSelectionChanged(
            string value,
            bool isSelected)
        {
            if (!isSelected) return;
            if (System.Enum.TryParse<ScreenPreset>(value, true, out var preset)) settingService.SetScreenPreset(preset);
        }

        private void OnLocaleSelectionChanged(
            string value,
            bool isSelected)
        {
            if (!isSelected) return;
            settingService.ChangeLocale(value);
        }

        private void OnSettingMenuShown()
        {
            List<IconModel> selectorItems = new List<IconModel>();

            foreach (var locale in Constraint.SUPPORTED_LOCALES)
            {
                if (localeCatalog.TryGet(locale.code, out var asset))
                {
                    selectorItems.Add(
                        new IconModel()
                        {
                            Description = locale.name,
                            Id = locale.code,
                            Name = locale.name,
                            Icon = asset.icon
                        });
                }
            }

            settingMenuView.BindLocaleIcons(selectorItems, settingService.CurrentLocale);
            settingMenuView.BindScreenSelector(settingService.ScreenPreset.ToString());
        }
        #endregion
    }
}