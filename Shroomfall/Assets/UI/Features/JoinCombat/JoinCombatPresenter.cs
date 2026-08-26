using Assets.Enums;
using Assets.Services;
using System;

namespace Assets.UI.Features.JoinCombat
{
    public class JoinCombatPresenter : IDisposable
    {
        #region Attributes
        private readonly UIService uiService;
        private readonly GameService gameService;
        private readonly JoinCombatView joinCombatView;

        private string code;
        private bool disposed;
        #endregion

        #region Properties
        #endregion

        public JoinCombatPresenter(
            UIService uiService,
            GameService gameService,
            JoinCombatView joinCombatView)
        {
            this.uiService = uiService;
            this.gameService = gameService;
            this.joinCombatView = joinCombatView;

            Bind();
        }

        #region Methods
        public void Dispose()
        {
            if (disposed) return;
            disposed = true;

            // Outbound
            uiService.OnUIStateChanged -= OnUIStateChanged;
            uiService.OnGlobalInteractableChanged -= joinCombatView.SetInteractable;

            // Inbound
            joinCombatView.OnBackClicked -= OnBackClicked;
            joinCombatView.OnHostClicked -= OnHostClicked;
            joinCombatView.OnJoinClicked -= OnJoinClicked;
            joinCombatView.OnCodeChanged -= OnCodeChanged;
        }

        private void Bind()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(JoinCombatPresenter));

            // Outbound
            uiService.OnUIStateChanged += OnUIStateChanged;
            uiService.OnGlobalInteractableChanged += joinCombatView.SetInteractable;

            // Inbound
            joinCombatView.OnBackClicked += OnBackClicked;
            joinCombatView.OnHostClicked += OnHostClicked;
            joinCombatView.OnJoinClicked += OnJoinClicked;
            joinCombatView.OnCodeChanged += OnCodeChanged;
        }

        private void OnUIStateChanged(
            UIService service)
        {
            joinCombatView.SetVisible(service.ShowJoinCombat);
        }

        private void OnBackClicked()
        {
            gameService.PopPhase();
        }

        private void OnHostClicked()
        {
            gameService.PushPhase(GamePhase.HostCombat);
        }

        private void OnJoinClicked()
        {
            if (string.IsNullOrEmpty(code))
                return;

            // TODO: add api calling here
        }

        private void OnCodeChanged(
            string code)
        {
            this.code = code;
        }
        #endregion
    }
}