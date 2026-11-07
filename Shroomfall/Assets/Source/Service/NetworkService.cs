using Assets.Source.Core;
using Assets.Source.Utility;
using Contract;
using Contract.DTO.Feature.Game.Response;
using Contract.DTO.Runtime.EntityDomain.Component;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Threading.Tasks;

namespace Assets.Source.Service
{
    public class NetworkService : IService
    {
        #region Attributes
        private HubConnection connection;
        private GameService gameService;
        private AuthService authService;
        #endregion

        #region Properties
        public bool IsInitialized { get; private set; } = false;
        public bool IsConnected { get; private set; } = false;
        #endregion

        public NetworkService() { }

        #region Methods
        public Task InitializeAsync()
        {
            gameService = Core.ServiceProvider.Get<GameService>();
            authService = Core.ServiceProvider.Get<AuthService>();

            gameService.OnConnected = ConnectAsync;
            gameService.OnDisconnected = DisconnectAsync;

            IsInitialized = true;
            return Task.CompletedTask;
        }

        public async Task ShutdownAsync()
        {
            gameService.OnConnected = null;
            gameService.OnDisconnected = null;

            await DisconnectAsync();
        }

        private async Task ConnectAsync()
        {
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new ComponentInstanceConverter());
            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            connection = new HubConnectionBuilder()
                .WithUrl(Configuration.HUB_URL, options => { options.AccessTokenProvider = () => authService.GetValidAccessToken(); })
                .AddNewtonsoftJsonProtocol(options => { options.PayloadSerializerSettings = settings; })
                .WithAutomaticReconnect()
                .Build();

            BindServerEvents();

            connection.Reconnecting += HandleReconnecting;
            connection.Reconnected += HandleReconnected;
            connection.Closed += HandleConnectionClosed;

            // Connect
            await connection.StartAsync();
            IsConnected = true;
        }

        private async Task DisconnectAsync()
        {
            if (connection == null)
                return;

            connection.Reconnecting -= HandleReconnecting;
            connection.Reconnected -= HandleReconnected;
            connection.Closed -= HandleConnectionClosed;

            // Disconnect
            var task = connection;
            connection = null;
            IsConnected = false;
            await task.StopAsync();
            await task.DisposeAsync();
        }

        private Task HandleReconnecting(
            Exception ex)
        {
            CoroutineRunner.Instance.Schedule(() =>
            {
                IsConnected = false;
            });

            return Task.CompletedTask;
        }

        private Task HandleReconnected(
            string connectionId)
        {
            CoroutineRunner.Instance.Schedule(() =>
            {
                IsConnected = true;
            });

            return Task.CompletedTask;
        }

        private Task HandleConnectionClosed(
            Exception ex)
        {
            CoroutineRunner.Instance.Schedule(() =>
            {
                gameService.End();
            });

            return Task.CompletedTask;
        }

        private void BindServerEvents()
        {
            // --- Entity Service ---
            connection.On<EntitySpawnedDTO>(
                NetworkMethod.OnEntitySpawned,
                dto => { Core.ServiceProvider.Get<EntityService>().OnEntitySpawned(dto); });

            connection.On<string>(
                NetworkMethod.OnEntityDespawned,
                dto => { Core.ServiceProvider.Get<EntityService>().OnEntityDespawned(dto); });

            connection.On<EntityActedDTO>(
                NetworkMethod.OnEntityActed,
                dto => { Core.ServiceProvider.Get<EntityService>().OnEntityActed(dto); });

            connection.On<EntityAppearanceChangedDTO>(
                NetworkMethod.OnEntityAppearanceChanged,
                dto => { Core.ServiceProvider.Get<EntityService>().OnEntityAppearanceChanged(dto); });

            connection.On<EntityVitalChangedDTO>(
                NetworkMethod.OnEntityVitalChanged,
                dto => { Core.ServiceProvider.Get<EntityService>().OnEntityVitalChanged(dto); Core.ServiceProvider.Get<PlayerService>().OnVitalChanged(dto); });

            // --- Player Service ---
            connection.On<CharacteristicInstanceDTO>(
                NetworkMethod.OnPlayerCharacteristicSync,
                dto => { Core.ServiceProvider.Get<PlayerService>().OnPlayerCharacteristicSync(dto); });

            connection.On<InventoryItemChangedDTO>(
                NetworkMethod.OnInventoryItemChanged,
                dto => { Core.ServiceProvider.Get<PlayerService>().OnInventoryItemChanged(dto); });

            connection.On<object>(
                NetworkMethod.OnInventoryCleared,
                _ => { Core.ServiceProvider.Get<PlayerService>().OnInventoryCleared(); });
        }

        public async Task Send(
            string method,
            params object[] args)
        {
            if (!IsConnected)
                return;

            int retryCount = 3;
            int delay = 200;

            for (int i = 0; i < retryCount; i++)
            {
                try
                {
                    await SendEvent(method, args);
                    return;
                }
                catch
                {
                    if (!IsConnected)
                        return;

                    if (i == retryCount - 1)
                        return;

                    await Task.Delay(delay);
                    delay *= 2;
                }
            }
        }

        private async Task SendEvent(
            string method,
            params object[] args)
        {
            while (!(connection != null && connection.State == HubConnectionState.Connected))
            {
                if (connection == null)
                    throw new OperationCanceledException("Network shut down");

                await Task.Delay(50);
            }

            await connection.InvokeCoreAsync(method, args);
        }
        #endregion
    }
}