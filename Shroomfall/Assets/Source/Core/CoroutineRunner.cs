using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Source.Core
{
    public class CoroutineRunner : MonoBehaviour
    {
        #region Attributes
        private static CoroutineRunner instance;
        private readonly Queue<Action> actions = new();
        #endregion

        #region Properties
        public static CoroutineRunner Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GameObject("[CoroutineRunner]").AddComponent<CoroutineRunner>();
                    DontDestroyOnLoad(instance.gameObject);
                }
                return instance;
            }
        }
        #endregion

        #region Methods
        private void Update()
        {
            while (true)
            {
                Action a = null;
                lock (actions)
                {
                    if (actions.Count > 0)
                        a = actions.Dequeue();
                }
                if (a == null)
                    break;

                a.Invoke();
            }
        }

        private void OnApplicationQuit()
        {
            _ = ServiceProvider.ShutdownAll();
        }

        public void Schedule(
            Action action)
        {
            lock (actions)
            {
                actions.Enqueue(action);
            }
        }
        #endregion
    }
}
