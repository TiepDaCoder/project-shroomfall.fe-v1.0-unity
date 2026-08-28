using Assets.Source.Service;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Assets.Source.Core
{
    public static class ServiceProvider
    {
        #region Attributes
        private static readonly Dictionary<Type, IService> services = new();
        #endregion

        #region Properties
        #endregion

        #region Methods
        public static void Register<T>(T service) where T : IService
        {
            services[typeof(T)] = service;
        }

        public static T Get<T>() where T : IService
        {
            return (T)services[typeof(T)];
        }

        public static async Task ShutdownAll()
        {
            foreach (var service in services.Values)
            {
                await service.ShutdownAsync();
            }

            services.Clear();
        }

        public static bool IsRegistered<T>() where T : IService
        {
            return services.ContainsKey(typeof(T));
        }
        #endregion
    }
}
