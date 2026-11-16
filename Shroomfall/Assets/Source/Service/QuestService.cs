using Assets.Source.Utility;
using Contract.DTO.Abstraction;
using Contract.DTO.Runtime.MetaDomain;
using Contract.Enum.MetaDomain.Quest;
using System;
using System.Threading.Tasks;

namespace Assets.Source.Service
{
    public class QuestService : IService
    {
        #region Attributes
        #endregion

        #region Properties
        public bool IsInitialized { get; private set; }
        #endregion

        public QuestService() { }

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

        #region Senders
        public async Task<PagedResponseDTO<QuestInstanceDTO>> FetchQuests(
            QuestType? type,
            string searchTerm,
            int pageSize = 50)
        {
            string query = $"?PageNumber=1&PageSize={pageSize}";
            if (type.HasValue)
                query += $"&Type={(int)type.Value}";
            if (!string.IsNullOrWhiteSpace(searchTerm))
                query += $"&SearchTerm={Uri.EscapeDataString(searchTerm.Trim())}";

            return await HttpCaller.GetAsync<PagedResponseDTO<QuestInstanceDTO>>(
                $"{Configuration.QUEST_CONTROLLER}{query}");
        }

        public async Task<QuestInstanceDTO> AcceptQuest(
            string questInstanceId)
        {
            return await HttpCaller.PostAsync<string, QuestInstanceDTO>(
                $"{Configuration.QUEST_CONTROLLER}accept", questInstanceId);
        }
        #endregion

        #region Receivers
        #endregion
        #endregion
    }
}