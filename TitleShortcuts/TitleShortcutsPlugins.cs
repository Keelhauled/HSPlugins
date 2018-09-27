using System;
using UnityEngine;
using IllusionPlugin;

namespace TitleShortcuts
{
    public class TitleShortcutsPlugins : IEnhancedPlugin
    {
        public const string PLUGIN_NAME = "TitleShortcuts";
        public const string PLUGIN_VERSION = "1.0";
        public string Name => PLUGIN_NAME;
        public string Version => PLUGIN_VERSION;

        public string[] Filter => new string[]
        {
            "HoneySelect_32",
            "HoneySelect_64",
        };

        public void OnApplicationStart()
        {
            new GameObject(PLUGIN_NAME).AddComponent<TitleShortcuts>();
        }

        public void OnUpdate(){}
        public void OnLateUpdate(){}
        public void OnApplicationQuit(){}
        public void OnLevelWasInitialized(int level){}
        public void OnLevelWasLoaded(int level){}
        public void OnFixedUpdate(){}
    }
}
