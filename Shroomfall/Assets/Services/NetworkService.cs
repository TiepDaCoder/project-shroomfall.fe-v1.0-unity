using Assets.Core;
using Assets.Utilities;
using Contract;
using Contract.DTO.Feature.Game.Response;
using Contract.DTO.Runtime.EntityDomain.Component;
using Contract.DTO.Runtime.WorldDomain;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Assets.Services
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

        #region Handlers Registration
        public void Register<T>(T handler) where T : class
        {
            var type = typeof(T);
            handlers[type] = handler;

            if (pendingHandlers.TryGetValue(type, out var queue))
            {
                while (queue.Count > 0)
                    queue.Dequeue().Invoke();
                pendingHandlers.Remove(type);
            }
        }

        public void Unregister<T>()
        {
            handlers.Remove(typeof(T));
        }
        #endregion
        #endregion

        #region Private Helpers
        private void BindServerEvents()
        {
            if (IsBinded) return;

            // --- Entity Service ---
            connection.On<EntitySpawnedDTO>(
                NetworkMethod.OnEntitySpawned, dto =>
                {
                    //Dispatch<IEntityNetworkReceiver, EntitySpawnedDTO>(
                    //    dto, (r, d) => r.OnEntitySpawned(d));
                });

            connection.On<string>(
                NetworkMethod.OnEntityDespawned, dto =>
                {
                    //Dispatch<IEntityNetworkReceiver, string>(
                    //    dto, (r, d) => r.OnEntityDespawned(d));
                });

            connection.On<EntityActedDTO>(
                NetworkMethod.OnEntityActed, dto =>
                {
                    //Dispatch<IEntityNetworkReceiver, EntityActedDTO>(
                    //    dto, (r, d) => r.OnEntityActed(d));
                });

            connection.On<EntityAppearanceChangedDTO>(
                NetworkMethod.OnPlayerAppearanceChanged, dto =>
                {
                    //Dispatch<IEntityNetworkReceiver, EntityAppearanceChangedDTO>(
                    //    dto, (r, d) => r.OnPlayerAppearanceChanged(d));
                });

            connection.On<EntityVitalChangedDTO>(
                NetworkMethod.OnEntityVitalChanged, dto =>
                {
                    //Dispatch<IEntityNetworkReceiver, EntityVitalChangedDTO>(
                    //    dto, (r, d) => r.OnEntityVitalChanged(d));
                });

            // --- Player Service ---
            connection.On<CharacteristicInstanceDTO>(
                NetworkMethod.OnPlayerCharacteristicSync, dto =>
                {
                    //Dispatch<IPlayerNetworkReceiver, CharacteristicInstanceDTO>(
                    //    dto, (r, d) => r.OnPlayerCharacteristicSync(dto));
                });

            connection.On<InventoryItemChangedDTO>(
                NetworkMethod.OnInventoryItemChanged, dto =>
                {
                    //Dispatch<IPlayerNetworkReceiver, InventoryItemChangedDTO>(
                    //    dto, (r, d) => r.OnInventoryItemChanged(dto));
                });

            connection.On<object>(
                NetworkMethod.OnInventoryCleared, dto =>
                {
                    //Dispatch<IPlayerNetworkReceiver, object>(
                    //    dto, (r, d) => r.OnInventoryCleared());
                });

            // --- Setting Service ---
            connection.On<RoomSpatialDTO>(
                NetworkMethod.OnRoomSnapshotUpdated, dto =>
                {
                    //Dispatch<ISettingNetworkReceiver, RoomSpatialDTO>(
                    //    dto, (r, d) => r.OnRoomSnapshotUpdated(dto));
                });

            IsBinded = true;

        }

        private void Dispatch<TReceiver, TData>(
            TData data,
            Action<TReceiver, TData> call,
            string group = null)
            where TReceiver : class
        {
            CoroutineRunner.Instance.Schedule(() =>
            {
                bool calledAny = false;

                foreach (var receiver in handlers.Values)
                {
                    // Call methods
                    if (receiver is TReceiver r)
                    {
                        if (group == null || (r is INetworkBase gr && gr.Group == group))
                        {
                            call(r, data);
                            calledAny = true;
                        }
                    }
                }

                if (!calledAny)
                {
                    // Fallback for pending events
                    var type = typeof(TReceiver);
                    if (!pendingHandlers.TryGetValue(type, out var queue))
                        queue = new Queue<Action>();

                    queue.Enqueue(() =>
                    {
                        if (handlers.TryGetValue(type, out var later))
                            call((TReceiver)later, data);
                    });

                    pendingHandlers[type] = queue;
                }
            });
        }
        #endregion
    }
}