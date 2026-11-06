using Assets.Source.Core;
using Assets.Source.Enum;
using Assets.Source.Service.Abstraction;
using Assets.Source.Utility;
using Contract.DTO.Feature.Connection.Response;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Assets.Source.Service
{
    public class GameService : IService
    {
        #region Attributes
        private UIService uiService;
        private PlayerService playerService;
        private SessionService sessionService;
        private AuthService authService;
        private NetworkService networkService;
        private EntityService entityService;
        private DefinitionService definitionService;

        private readonly Stack<GamePhase> phaseStack = new();
        #endregion

        #region Properties
        public bool IsInitialized { get; private set; } = false;
        public GamePhase Phase { get; private set; } = GamePhase.Paused;
        public GamePhase? PendingPhase { get; private set; }
        public bool IsLoading { get; private set; }

        public event Action<GameService> OnRequestedNewScene;
        public event Action<GameService> OnChangedPhase;
        public event Action<string> OnWorldLoaded;
        #endregion

        public GameService()
        {
        }

        #region Methods
        public Task InitializeAsync()
        {
            // Resolve dependecies
            uiService = ServiceProvider.Get<UIService>();
            playerService = ServiceProvider.Get<PlayerService>();
            sessionService = ServiceProvider.Get<SessionService>();
            authService = ServiceProvider.Get<AuthService>();
            networkService = ServiceProvider.Get<NetworkService>();
            entityService = ServiceProvider.Get<EntityService>();
            definitionService = ServiceProvider.Get<DefinitionService>();

            OnChangedPhase += uiService.ApplyGameState;
            networkService.OnDisconnected += OnDisconnected;

            IsInitialized = true;
            return Task.CompletedTask;
        }

        public Task ShutdownAsync()
        {
            OnChangedPhase -= uiService.ApplyGameState;
            networkService.OnDisconnected -= OnDisconnected;

            return Task.CompletedTask;
        }

        public void NotifySceneReady()
        {
            if (!IsLoading || PendingPhase == null)
                return;

            var next = PendingPhase.Value;

            PendingPhase = null;
            IsLoading = false;

            if (Phase == next)
                return;

            phaseStack.Clear();
            Phase = next;

            OnChangedPhase?.Invoke(this);
        }

        public void PushPhase(
            GamePhase overlay)
        {
            if (IsLoading)
            {
                return;
            }

            if (Phase == overlay || phaseStack.Contains(overlay))
                return;

            phaseStack.Push(Phase);
            Phase = overlay;

            OnChangedPhase?.Invoke(this);
        }

        public void PopPhase()
        {
            if (IsLoading)
            {
                return;
            }

            if (phaseStack.Count == 0)
                return;

            Phase = phaseStack.Pop();
            OnChangedPhase?.Invoke(this);
        }

        #region App Life-cycle
        public async Task StartApplication()
        {
            BackToMenu();
        }

        public async Task QuitGame()
        {
            await ServiceProvider.ShutdownAll();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
                UnityEngine.Application.Quit(); 
#endif
        }

        public void BackToMenu()
        {
            RequestNewScene(GamePhase.MainMenu);
        }
        #endregion

        #region Player Life-cycle
        public async Task PlayerSignIn(
            string email,
            string password)
        {
            await ExecuteAuthPipeline(async () =>
            {
                await authService.Login(email, password);
            });
        }

        public async Task PlayerSignUp(
            string email,
            string password,
            string name)
        {
            await ExecuteAuthPipeline(async () =>
            {
                await authService.Register(email, password, name);
            });
        }

        public async Task PlayerSteamAuthenticate()
        {
            await ExecuteAuthPipeline(async () =>
            {
                await authService.SteamAuth();
            });
        }

        public async Task PlayerLogout()
        {
            await HandleInvalidateSession();
        }

        public async Task LoadSessionAndWorld(
            string sessionId)
        {
            var session = await sessionService.LoadSession(sessionId);
            LoadSaveGame(session);

            PushPhase(GamePhase.InGame);
        }

        public async Task BackHome()
        {
            var room = await playerService.BackHomeAsync();
            LoadSaveGame(room);
        }

        public async Task EnterHub(
            string hubRoomSpatialId)
        {
            var room = await playerService.EnterHubAsync(hubRoomSpatialId);
            LoadSaveGame(room);
        }

        public async Task EnterCombat(
            string combatRunDefinitionId)
        {
            var run = await playerService.EnterCombatAsync(combatRunDefinitionId);
            LoadSaveGame(run.SaveGame);
        }
        #endregion

        private async Task ExecuteAuthPipeline(
            Func<Task> flow)
        {
            try
            {
                await flow();
                await HandleConnectionEstablish(authService.Token.AccessToken);
            }
            finally
            {
                PopPhase();
            }

            RequestNewScene(GamePhase.ListSession);
        }

        private void OnDisconnected(
            DisconnectReason reason)
        {
            switch (reason)
            {
                case DisconnectReason.Expected:
                    // Ignore.
                    return;

                case DisconnectReason.ConnectionLost:
                    AsyncHelper.Run(uiService, TryReconnect);
                    break;

                case DisconnectReason.ServerShutdown:
                    AsyncHelper.Run(uiService, async () =>
                    {
                        await HandleInvalidateSession();
                    });
                    break;
            }
        }

        private async Task HandleConnectionEstablish(
            string token)
        {
            await networkService.ConnectAsync(token);
            await definitionService.RefreshDefinitions();
        }

        private async Task HandleInvalidateSession(
            bool goToMainMenu = true)
        {
            try
            {
                await networkService.ShutdownAsync();
                playerService.UnloadPlayerData();
                entityService.UnloadEntitiesData();
            }
            finally
            {
                PopPhase();
            }

            if (goToMainMenu)
                BackToMenu();
        }

        private async Task TryReconnect()
        {
            // Try refreshing the access token
            var success = await authService.Refresh();

            if (!success)
            {
                await HandleInvalidateSession();
                return;
            }

            await HandleConnectionEstablish(authService.Token.AccessToken);
        }

        public void LoadSaveGame(
            SaveGameDTO saveGame)
        {
            playerService.LoadPlayerData(saveGame.PlayerData);

            saveGame.RoomData.Entities.Add(saveGame.PlayerData);

            entityService.UnloadEntitiesData();
            entityService.LoadEntitiesData(saveGame.RoomData.Entities);
            LoadWorld(saveGame.RoomData.Room.DefinitionID);
        }

        private bool RequestNewScene(
            GamePhase target)
        {
            if (Phase == target || IsLoading)
                return false;

            PendingPhase = target;
            IsLoading = true;

            OnRequestedNewScene?.Invoke(this);
            return true;
        }

        private void LoadWorld(
            string roomDefinitionId)
        {
            OnWorldLoaded?.Invoke(roomDefinitionId);
        }
        #endregion
    }
}