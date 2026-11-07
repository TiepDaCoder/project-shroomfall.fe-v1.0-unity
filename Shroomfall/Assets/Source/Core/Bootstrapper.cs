using Assets.Source.Data;
using Assets.Source.Gameplay.Component.Input;
using Assets.Source.Gameplay.Factory;
using Assets.Source.Service;
using Assets.Source.Storage;
using Assets.Source.Utility;
using Assets.Source.Utility.Map;
using UnityEngine;

namespace Assets.Source.Core
{
    public class Bootstrapper : MonoBehaviour
    {
        #region Attributes
        [Header("Audio")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource ambientSource;
        [SerializeField] private AudioSource uiSource;

        [Header("Data")]
        [SerializeField] private CursorCatalogSO cursorCatalog;
        [SerializeField] private EntityCatalogSO entityCatalog;
        [SerializeField] private ItemCatalogSO itemCatalog;

        [Header("Prefabs")]
        [SerializeField] private LocalView localPrefab;
        [SerializeField] private EntityView entityPrefab;

        [Header("Utility")]
        [SerializeField] private MusicPlayer musicPlayer;
        [SerializeField] private ScreenEffectSpawner screenEffectSpawner;
        [SerializeField] private WorldBuilder worldBuilder;
        #endregion

        #region Properties
        #endregion

        #region Methods
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private async void Start()
        {
            // --------------------------------------------------------------------------------
            // Initialize storages
            // --------------------------------------------------------------------------------
            DefinitionStorage.Initialize();
            UserProfileStorage.Initialize();

            // --------------------------------------------------------------------------------
            // Register services
            // --------------------------------------------------------------------------------
            // Auth service
            var authService = new AuthService();
            ServiceProvider.Register<AuthService>(authService);

            // Network service
            var networkService = new NetworkService();
            ServiceProvider.Register<NetworkService>(networkService);

            // Session service
            var sessionService = new SessionService();
            ServiceProvider.Register<SessionService>(sessionService);

            // Audio service
            var audioService = new AudioService(musicSource, ambientSource, uiSource, musicPlayer);
            ServiceProvider.Register<AudioService>(audioService);

            // Cursor service
            var cursorService = new CursorService(cursorCatalog);
            ServiceProvider.Register<CursorService>(cursorService);

            // Definition service
            var definitionService = new DefinitionService();
            ServiceProvider.Register<DefinitionService>(definitionService);

            // Entity service
            var entityService = new EntityService(entityCatalog, itemCatalog, localPrefab, entityPrefab);
            ServiceProvider.Register<EntityService>(entityService);

            // Game service
            var gameService = new GameService(worldBuilder);
            ServiceProvider.Register<GameService>(gameService);

            // Inventory service
            var inventoryService = new InventoryService();
            ServiceProvider.Register<InventoryService>(inventoryService);

            // Localize service
            var localizeService = new LocalizeService();
            ServiceProvider.Register<LocalizeService>(localizeService);

            // Player service
            var playerService = new PlayerService();
            ServiceProvider.Register<PlayerService>(playerService);

            // Screen service
            var screenService = new ScreenService(screenEffectSpawner);
            ServiceProvider.Register<ScreenService>(screenService);

            // Tutorial service
            var tutorialService = new TutorialService();
            ServiceProvider.Register<TutorialService>(tutorialService);

            // UI service
            var uiService = new UIService();
            ServiceProvider.Register<UIService>(uiService);

            // --------------------------------------------------------------------------------
            // Initialize services
            // --------------------------------------------------------------------------------
            await ServiceProvider.InitializeAll();

            // --------------------------------------------------------------------------------
            // Initialize utilities
            // --------------------------------------------------------------------------------
            musicPlayer.Initialize(localizeService);

            // --------------------------------------------------------------------------------
            // Start game
            // --------------------------------------------------------------------------------
            gameService.StartApplication();
        }
        #endregion
    }
}