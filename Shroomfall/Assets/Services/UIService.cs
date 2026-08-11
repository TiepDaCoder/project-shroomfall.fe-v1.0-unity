using Assets.Enums;
using System;
using System.Threading.Tasks;

namespace Assets.Services
{
    public class UIService : IService
    {
        #region Attributes
        #endregion

        #region Properties
        public bool IsInitialized { get; private set; } = false;
        public bool ShowMainMenu { get; private set; }
        public bool ShowHostCombat { get; private set; }
        public bool ShowJoinCombat { get; private set; }
        public bool ShowCreateSession { get; private set; }
        public bool ShowListSession { get; private set; }
        public bool ShowSettingMenu { get; private set; }
        public bool ShowSignIn { get; private set; }
        public bool ShowSignUp { get; private set; }
        public bool ShowHUD { get; private set; }
        public CursorType CurrentCursor { get; private set; }

        public event Action<UIService> OnUIStateChanged;
        public event Action<(ToastType type, string message)> OnToastRequested;
        public event Action<bool> OnLoadingChanged;
        public event Action<bool> OnGlobalInteractableChanged;
        public event Action<CursorType> OnCursorChanged;
        #endregion

        public UIService()
        {
        }

        #region Methods
        public Task InitializeAsync()
        {
            IsInitialized = true;
            return Task.CompletedTask;
        }

        public Task ShutdownAsync()
        {
            return Task.CompletedTask;
        }

        public void ApplyGameState(
            GameService game)
        {
            ShowMainMenu = game.Phase == GamePhase.MainMenu;
            ShowHostCombat = game.Phase == GamePhase.HostCombat;
            ShowJoinCombat = game.Phase == GamePhase.JoinCombat;
            ShowCreateSession = game.Phase == GamePhase.CreateSession;
            ShowListSession = game.Phase == GamePhase.ListSession;
            ShowSettingMenu = game.Phase == GamePhase.SettingMenu;
            ShowSignIn = game.Phase == GamePhase.SignIn;
            ShowSignUp = game.Phase == GamePhase.SignUp;
            ShowHUD = game.Phase == GamePhase.InGame;
            OnUIStateChanged?.Invoke(this);
        }

        public void ShowToast(
            ToastType type,
            string message)
        {
            OnToastRequested?.Invoke((type, message));
        }

        public void ShowLoading(
            bool isShown)
        {
            OnLoadingChanged?.Invoke(isShown);
        }

        public void SetGlobalInteractable(
            bool interactable)
        {
            OnGlobalInteractableChanged?.Invoke(interactable);
        }

        public void SetCursor(
            CursorType type)
        {
            if (CurrentCursor == type) return;

            CurrentCursor = type;
            OnCursorChanged?.Invoke(type);
        }
        #endregion
    }
}