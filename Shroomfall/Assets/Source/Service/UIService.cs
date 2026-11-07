using Assets.Source.Core;
using Assets.Source.Enum;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Source.Service
{
    public class UIService : IService
    {
        #region Attributes
        #endregion

        #region Properties
        public bool IsInitialized { get; private set; } = false;
        public bool ShowMainMenu { get; private set; }
        public bool ShowCreateSession { get; private set; }
        public bool ShowListSession { get; private set; }
        public bool ShowSignIn { get; private set; }
        public bool ShowSignUp { get; private set; }
        public bool ShowHUD { get; private set; }
        public bool ShowInGame { get; private set; }
        public bool ShowIntroduction { get; private set; }
        public UIState State { get; private set; }
        public CursorType CurrentCursor { get; private set; }
        public bool IsBackpackOpen { get; private set; }

        public event Action<UIService> OnUIStateChanged;
        public event Action OnBackpackToggle;
        public event Action<(ToastType type, string message)> OnToastRequested;
        public event Action<bool> OnLoadingRequested;
        public event Action OnSettingRequested;
        public event Action OnTravelBoardRequested;
        public event Action<bool> OnGlobalInteractableChanged;
        public event Action<CursorType> OnCursorChanged;
        public event Action<(string message, Vector2 screenPosition)> OnCursorMessageRequested;

        private LocalizeService localizeService;
        #endregion

        public UIService() { }

        #region Methods
        public Task InitializeAsync()
        {
            localizeService = ServiceProvider.Get<LocalizeService>();

            IsInitialized = true;
            return Task.CompletedTask;
        }

        public Task ShutdownAsync()
        {
            return Task.CompletedTask;
        }

        public void SetState(
            UIState state)
        {
            if (State == state)
                return;

            ShowMainMenu = state == UIState.MainMenu;
            ShowCreateSession = state == UIState.CreateSession;
            ShowListSession = state == UIState.ListSession;
            ShowSignIn = state == UIState.SignIn;
            ShowSignUp = state == UIState.SignUp;
            ShowHUD = state == UIState.HUD;
            ShowIntroduction = state == UIState.Introduction;

            State = state;
            OnUIStateChanged?.Invoke(this);
        }

        public void ToggleBackpack()
        {
            IsBackpackOpen = !IsBackpackOpen;
            OnBackpackToggle?.Invoke();
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
            OnLoadingRequested?.Invoke(isShown);
        }

        public void ShowSetting()
        {
            OnSettingRequested?.Invoke();
        }

        public void ShowTravelBoard()
        {
            OnTravelBoardRequested?.Invoke();
        }

        public void SetGlobalInteractable(
            bool interactable)
        {
            OnGlobalInteractableChanged?.Invoke(interactable);
        }

        public void SetCursor(
            CursorType type)
        {
            if (CurrentCursor == type)
                return;

            CurrentCursor = type;
            OnCursorChanged?.Invoke(type);
        }

        public void ShowCursorMessage(
            string localizeKey,
            Vector2 screenPosition)
        {
            var message = localizeService.Get(localizeKey);
            if (string.IsNullOrEmpty(message))
                return;

            OnCursorMessageRequested?.Invoke((message, screenPosition));
        }

        #region Senders
        #endregion

        #region Receivers
        #endregion
        #endregion
    }
}