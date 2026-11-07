using Assets.Source.Core;
using Assets.Source.Enum;
using Assets.Source.Utility;
using Assets.Source.Utility.Map;
using Contract.DTO.Feature.Connection.Response;
using Contract.DTO.Runtime.EntityDomain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace Assets.Source.Service
{
    public class GameService : IService
    {
        #region Attributes
        private UIService uiService;
        private PlayerService playerService;
        private EntityService entityService;
        private DefinitionService definitionService;

        private WorldBuilder worldBuilder;
        #endregion

        #region Properties
        public bool IsInitialized { get; private set; } = false;

        public Func<Task> OnConnected { get; set; }
        public Func<Task> OnDisconnected { get; set; }
        #endregion

        public GameService(
            WorldBuilder worldBuilder)
        {
            this.worldBuilder = worldBuilder;
        }

        #region Methods
        public Task InitializeAsync()
        {
            uiService = ServiceProvider.Get<UIService>();
            playerService = ServiceProvider.Get<PlayerService>();
            entityService = ServiceProvider.Get<EntityService>();
            definitionService = ServiceProvider.Get<DefinitionService>();

            IsInitialized = true;
            return Task.CompletedTask;
        }

        public Task ShutdownAsync()
        {
            return Task.CompletedTask;
        }

        #region App
        public void StartApplication()
        {
            SceneManager.LoadScene($"{Configuration.INTRODUCTION_SCENE}");
            uiService.SetState(UIState.Introduction);
        }

        public void StartTutorial()
        {
            SceneManager.LoadScene($"{Configuration.TUTORIAL_SCENE}");
            uiService.SetState(UIState.HUD);
        }

        public async Task CloseApplication()
        {
            await ServiceProvider.ShutdownAll();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
                UnityEngine.Application.Quit(); 
#endif
        }
        #endregion

        #region Authentication & Session
        public void Start()
        {
            AsyncHelper.Run(uiService, async () =>
            {
                if (OnConnected != null) await OnConnected();
                await definitionService.RefreshDefinitions();
                uiService.SetState(UIState.ListSession);
            });
        }

        public void End()
        {
            AsyncHelper.Run(uiService, async () =>
            {
                if (OnDisconnected != null) await OnDisconnected();
                DestructWorld();
            });
        }
        #endregion

        #region World
        public void DestructWorld()
        {
            playerService.UnloadPlayerData();
            entityService.UnloadEntitiesData();
        }

        public void ConstructWorld(
            SaveGameDTO saveGame)
        {
            playerService.UnloadPlayerData();
            playerService.LoadPlayerData(saveGame.PlayerData);

            var entities = new List<EntityInstanceDTO>(saveGame.RoomData.Entities)
            {
                saveGame.PlayerData
            };

            entityService.UnloadEntitiesData();
            entityService.LoadEntitiesData(entities);
            uiService.SetState(UIState.HUD);

            worldBuilder.LoadRoom(saveGame.RoomData.Room.DefinitionID);
        }
        #endregion

        #region Senders
        #endregion

        #region Receivers
        #endregion
        #endregion
    }
}