using PlayFabTests.Utils;
using PlayFabTests.Utils.Singleton;
using Spades.Managers.GeneralUtils;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PlayFabTests
{
    public class AppHandler : Singleton<AppHandler>
    {
        public static UnityMainThreadDispatcher MainThreadDispatcher { get; private set; }
        public static TaskUtilitiesManager TaskManager { get; private set; }

        protected override void InitSingleton()
        {
            base.InitSingleton();

            MainThreadDispatcher = gameObject.AddComponent<UnityMainThreadDispatcher>();
            TaskManager = TaskUtilitiesManager.CreateInstance(transform);
        }
    }
}
