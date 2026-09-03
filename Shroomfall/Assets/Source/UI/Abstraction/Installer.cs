using Assets.Source.Core;
using Assets.Source.Service.Abstraction;
using System;
using System.Collections;
using UnityEngine;

namespace Assets.Source.UI.Abstraction
{
    public abstract class Installer : MonoBehaviour
    {
        #region Attributes
        #endregion

        #region Properties
        public virtual string StepName
        {
            get { return name; }
        }
        #endregion

        #region Methods
        protected IEnumerator BindWhenReady<T>(
            Action<T> onReady) where T : class, IService
        {
            // Wait for registration
            while (!ServiceProvider.IsRegistered<T>())
                yield return null;

            var service = ServiceProvider.Get<T>();

            // Wait for initialization
            while (!service.IsInitialized)
                yield return null;

            // Safe to bind
            onReady(service);
        }

        /// <summary>
        /// Each binder overrides this to bind its services.
        /// Called by LoadingUI to orchestrate all binders.
        /// </summary>
        public abstract IEnumerator BindAllServices();
        #endregion
    }
}