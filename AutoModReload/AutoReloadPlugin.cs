using System;
using IllusionPlugin;
using UnityEngine;

namespace AutoModReload
{
    class AutoReloadPlugin : IPlugin
    {
        public const string PLUGIN_NAME = "AutoModReload";
        public const string PLUGIN_VERSION = "1.0";
        public string Name => PLUGIN_NAME;
        public string Version => PLUGIN_VERSION;

        public void OnApplicationStart()
        {
            new GameObject(PLUGIN_NAME).AddComponent<AutoReload>();
        }

        public void OnLevelWasLoaded(int level){}
        public void OnUpdate(){}
        public void OnLateUpdate(){}
        public void OnApplicationQuit(){}
        public void OnLevelWasInitialized(int level){}
        public void OnFixedUpdate(){}
    }
}
