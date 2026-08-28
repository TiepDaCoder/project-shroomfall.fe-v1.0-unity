using Assets.Source.Core;
using Assets.Source.Utilities;
using Contract;
using Contract.DTO.Feature.Game.Response;
using Contract.DTO.Runtime.EntityDomain.Component;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Assets.Source.Service
{
    public enum DisconnectReason
    {
        Expected,
        ConnectionLost,
        ServerShutdown
    }


    public class NetworkService : IService
    {
        #region Attributes
        private readonly Dictionary<Type, object> handlers = new();
        private readonly Dictionary<Type, Queue<Action>> pendingHandlers = new();
        private bool shuttingDown;
        private HubConnection connection;
        private const string HUB_URL = Configuration.HUB_URL;
        #endregion

        #region Properties
        public event Action<DisconnectReason> OnDisconnected;

        public bool IsReady
        {
            get { return connection != null && connection.State == HubConnectionState.Connected; }
        }
        public bool IsBinded { get; private set; } = false;
        public bool IsConnected { get; private set; } = false;
        public bool IsInitialized { get; private set; } = false;
        #endregion

        public NetworkService() { }

        #region Methods
        public Task InitializeAsync()
        {
            IsInitialized = true;
            return Task.CompletedTask;
        }

        public async Task ShutdownAsync()
        {
            handlers.Clear();
            pendingHandlers.Clear();

            if (connection != null)
            {
                await connection.StopAsync();
                await connection.DisposeAsync();
                connection = null;
            }

            IsConnected = false;
            IsBinded = false;

        }

        public async Task ConnectAsync(
            string accessToken)
        {
            shuttingDown = true;

            if (string.IsNullOrEmpty(accessToken))
                throw new InvalidOperationException("AccessToken missing");

            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new ComponentInstanceConverter());
            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            connection = new HubConnectionBuilder()
                .WithUrl(HUB_URL, options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult(accessToken);
                })
                .AddNewtonsoftJsonProtocol(options =>
                {
                    options.PayloadSerializerSettings = settings;
                })
                .WithAutomaticReconnect()
                .Build();

            BindServerEvents();

            connection.Reconnected += id =>
            {
                CoroutineRunner.Instance.Schedule(() =>
                {
                    IsConnected = true;
                });

                return Task.CompletedTask;
            };

            connection.Reconnecting += ex =>
            {
                CoroutineRunner.Instance.Schedule(() =>
                {
                    IsConnected = false;
                });

                return Task.CompletedTask;
            };

            connection.Closed += ex =>
            {
                CoroutineRunner.Instance.Schedule(() =>
                {
                    if (shuttingDown)
                    {
                        OnDisconnected?.Invoke(DisconnectReason.Expected);
                        return;
                    }

                    var reason = DisconnectReason.ConnectionLost;

                    if (ex is HubException)
                        reason = DisconnectReason.ServerShutdown;

                    OnDisconnected?.Invoke(reason);
                });

                return Task.CompletedTask;
            };

            await connection.StartAsync();

            IsConnected = true;
            shuttingDown = false;
        }

        public async Task WaitUntilReady()
        {
            while (!IsReady)
            {
                if (connection == null)
                    throw new OperationCanceledException("Network shut down");

                await Task.Delay(50);
            }
        }

        public async Task SendEvent(
            string method,
            params object[] args)
        {
            await WaitUntilReady();

            await connection.InvokeCoreAsync(method, args);

        }

        private void BindServerEvents()
        {
            if (IsBinded) return;

            // --- Entity Service ---
            connection.On<EntitySpawnedDTO>(
                NetworkMethod.OnEntitySpawned,
                dto =>
                {
                    Core.ServiceProvider
                        .Get<EntityService>()
                        .OnEntitySpawned(dto);
                });

            connection.On<string>(
                NetworkMethod.OnEntityDespawned,
                dto =>
                {
                    Core.ServiceProvider
                        .Get<EntityService>()
                        .OnEntityDespawned(dto);
                });

            connection.On<EntityActedDTO>(
                NetworkMethod.OnEntityActed,
                dto =>
                {
                    Core.ServiceProvider
                        .Get<EntityService>()
                        .OnEntityActed(dto);
                });

            connection.On<EntityAppearanceChangedDTO>(
                NetworkMethod.OnPlayerAppearanceChanged,
                dto =>
                {
                    Core.ServiceProvider
                        .Get<EntityService>()
                        .OnPlayerAppearanceChanged(dto);
                });

            connection.On<EntityVitalChangedDTO>(
                NetworkMethod.OnEntityVitalChanged,
                dto =>
                {
                    Core.ServiceProvider
                        .Get<EntityService>()
                        .OnEntityVitalChanged(dto);
                });

            // --- Player Service ---
            connection.On<CharacteristicInstanceDTO>(
                NetworkMethod.OnPlayerCharacteristicSync,
                dto =>
                {
                    Core.ServiceProvider
                        .Get<PlayerService>()
                        .OnPlayerCharacteristicSync(dto);
                });

            connection.On<InventoryItemChangedDTO>(
                NetworkMethod.OnInventoryItemChanged,
                dto =>
                {
                    Core.ServiceProvider
                        .Get<PlayerService>()
                        .OnInventoryItemChanged(dto);
                });

            connection.On<object>(
                NetworkMethod.OnInventoryCleared,
                _ =>
                {
                    Core.ServiceProvider
                        .Get<PlayerService>()
                        .OnInventoryCleared();
                });

            // --- Setting Service ---
            //connection.On<RoomSpatialDTO>(
            //    NetworkMethod.OnRoomSnapshotUpdated,
            //    dto =>
            //    {
            //        ServiceProvider
            //            .Get<GameService>()
            //            .LoadSaveGame(dto);
            //    });

            IsBinded = true;
        }

        public async Task Send(
            string method,
            params object[] args)
        {
            if (!IsConnected)
            {
                return; // silently drop
            }

            int retryCount = 3;
            int delay = 200;

            for (int i = 0; i < retryCount; i++)
            {
                try
                {
                    await SendEvent(method, args);
                    return;
                }
                catch (Exception ex)
                {
                    if (!IsConnected)
                    {
                        return;
                    }

                    if (i == retryCount - 1)
                    {
                        return;
                    }

                    await Task.Delay(delay);
                    delay *= 2;
                }
            }
        }
        #endregion
    }
}