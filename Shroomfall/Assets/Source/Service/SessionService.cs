using Assets.Source.Core;
using Assets.Source.Utility;
using Contract.DTO.Feature.Connection.Command;
using Contract.DTO.Feature.Connection.Response;
using System.Threading.Tasks;

namespace Assets.Source.Service
{
    public class SessionService : IService
    {
        #region Attributes
        private GameService gameService;
        #endregion

        #region Properties
        public bool IsInitialized { get; private set; }
        #endregion

        public SessionService() { }

        #region Methods
        public Task InitializeAsync()
        {
            gameService = ServiceProvider.Get<GameService>();

            IsInitialized = true;
            return Task.CompletedTask;
        }

        public async Task ShutdownAsync()
        {
            await UnloadSession();
        }

        public void EnterTutorial(
            SaveGameDTO tutorialSave)
        {
            gameService.ConstructWorld(tutorialSave);
        }

        #region Senders
        public async Task<ExistedSessionDTO> FetchSessions()
        {
            return await HttpCaller.GetAsync<ExistedSessionDTO>(
                $"{Configuration.CONNECTION_CONTROLLER}sessions");
        }

        public async Task BackHomeAsync()
        {
            var data = await HttpCaller.PostAsync<object, SaveGameDTO>(
                $"{Configuration.GAME_CONTROLLER}back-home", new { });

            gameService.ConstructWorld(data);
        }

        public async Task EnterHubAsync(
            string hubRoomSpatialId)
        {
            var data = await HttpCaller.PostAsync<object, SaveGameDTO>(
                $"{Configuration.GAME_CONTROLLER}enter-hub/{hubRoomSpatialId}", new { });

            gameService.ConstructWorld(data);
        }

        public async Task LoadSession(
            string playerInstanceId)
        {
            var dto = new LoadSessionDTO
            {
                PlayerInstanceID = playerInstanceId
            };

            var data = await HttpCaller.PostAsync<LoadSessionDTO, SaveGameDTO>(
                $"{Configuration.CONNECTION_CONTROLLER}session/load", dto);

            gameService.ConstructWorld(data);
        }

        public async Task CreateSession(
            string roomDefinitionId)
        {
            var dto = new CreateSessionDTO
            {
                RoomDefinitionID = roomDefinitionId
            };

            await HttpCaller.PostAsync<object, CreateSessionDTO>(
                $"{Configuration.CONNECTION_CONTROLLER}session", dto);
        }

        public async Task UnloadSession()
        {
            await HttpCaller.PostAsync<object, VoidResponse>(
                $"{Configuration.CONNECTION_CONTROLLER}session/unload",
                new { });
        }
        #endregion

        #region Receivers
        #endregion
        #endregion
    }
}