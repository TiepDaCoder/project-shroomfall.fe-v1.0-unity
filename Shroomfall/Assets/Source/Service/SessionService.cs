using Assets.Source.Service.Abstraction;
using Assets.Source.Utility;
using Contract.DTO.Feature.Connection.Command;
using Contract.DTO.Feature.Connection.Response;
using System.Threading.Tasks;

namespace Assets.Source.Service
{
    public class SessionService : IService
    {
        #region Attributes
        #endregion

        #region Properties
        public bool IsInitialized { get; private set; }
        #endregion

        public SessionService()
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

        public async Task<ExistedSessionDTO> FetchSessions()
        {
            return await HttpCaller.GetAsync<ExistedSessionDTO>(
                            $"{Configuration.CONNECT_CONTROLLER}sessions"
                        );
        }

        public async Task<SaveGameDTO> LoadSession(
            string playerInstanceId)
        {
            var dto = new LoadSessionDTO
            {
                PlayerInstanceID = playerInstanceId
            };

            return await HttpCaller.PostAsync<LoadSessionDTO, SaveGameDTO>(
                $"{Configuration.CONNECT_CONTROLLER}session/load",
                dto
            );
        }

        public async Task CreateSession(
            string roomDefinitionId)
        {
            var dto = new CreateSessionDTO
            {
                RoomDefinitionID = roomDefinitionId
            };

            await HttpCaller.PostAsync<object, CreateSessionDTO>(
                $"{Configuration.CONNECT_CONTROLLER}session",
                dto
            );
        }
        #endregion
    }
}