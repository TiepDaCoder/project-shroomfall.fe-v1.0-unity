using Assets.Source.Data;
using Assets.Source.Enum;
using Assets.Source.Service;
using Assets.Source.UI.Model;
using Assets.Source.Utility;
using System;
using System.Collections.Generic;

namespace Assets.Source.UI.Feature.HostCombat
{
    public class HostCombatPresenter : IDisposable
    {
        #region Attributes
        private readonly UIService uiService;
        private readonly GameService gameService;
        private readonly DefinitionService definitionService;
        private readonly HostCombatView hostCombatView;

        private readonly CombatRunCatalogSO combatRunCatalog;

        private bool disposed;
        #endregion

        #region Properties
        #endregion

        public HostCombatPresenter(
            UIService uiService,
            GameService gameService,
            DefinitionService definitionService,
            HostCombatView hostCombatView,
            CombatRunCatalogSO combatRunCatalog)
        {
            this.uiService = uiService;
            this.gameService = gameService;
            this.definitionService = definitionService;
            this.hostCombatView = hostCombatView;

            Bind();
            this.combatRunCatalog = combatRunCatalog;
        }

        #region Methods
        public void Dispose()
        {
            if (disposed) return;
            disposed = true;

            // Outbound
            uiService.OnUIStateChanged -= OnUIStateChanged;
            uiService.OnGlobalInteractableChanged -= hostCombatView.SetInteractable;

            // Inbound
            hostCombatView.OnBackClicked -= OnBackClicked;
            hostCombatView.OnCreateClicked -= OnCreateClicked;
            hostCombatView.OnOpened -= OnOpened;
        }

        private void Bind()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(HostCombatPresenter));

            // Outbound
            uiService.OnUIStateChanged += OnUIStateChanged;
            uiService.OnGlobalInteractableChanged += hostCombatView.SetInteractable;

            // Inbound
            hostCombatView.OnBackClicked += OnBackClicked;
            hostCombatView.OnCreateClicked += OnCreateClicked;
            hostCombatView.OnOpened += OnOpened;
        }

        private void OnUIStateChanged(
            UIService service)
        {
            hostCombatView.SetVisible(service.ShowHostCombat);
        }

        private void OnBackClicked()
        {
            gameService.PopPhase();
        }

        private void OnCreateClicked(
            string id)
        {
            if (string.IsNullOrEmpty(id))
                return;

            AsyncHelper.Run(uiService, async () =>
            {
                await gameService.EnterCombat(id);
                uiService.ShowToast(ToastType.Information, "Run created successfully!");
            });
        }

        private void OnOpened()
        {
            var models = new List<IconModel>();

            foreach (var combatRun in definitionService.Snapshot.CombatRuns)
            {
                if (!combatRunCatalog.TryGet(combatRun.Id, out var asset))
                    continue;

                models.Add(new IconModel
                {
                    Id = combatRun.Id,
                    Icon = asset.icon,
                    Name = string.Empty,
                    Description = string.Empty,
                });
            }

            hostCombatView.BindCombatIcons(models);
        }
        #endregion
    }
}