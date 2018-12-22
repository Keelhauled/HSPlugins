using IllusionPlugin;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Reflection;
using Harmony;

namespace NeckSettings
{
    public class NeckSettingsPlugin : IEnhancedPlugin
    {
        public const string PLUGIN_NAME = "NeckSettings";
        public const string PLUGIN_VERSION = "1.0";
        public string Name { get; } = PLUGIN_NAME;
        public string Version { get; } = PLUGIN_VERSION;

        public string[] Filter { get; } = new string[]
        {
            "HoneySelect_32",
            "HoneySelect_64",
            "StudioNEO_32",
            "StudioNEO_64",
        };

        public void OnLevelWasLoaded(int level)
        {
            
        }

        public void OnApplicationStart()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("NeckSettings.HarmonyPatches");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        public void OnUpdate(){}
        public void OnLateUpdate(){}
        public void OnApplicationQuit(){}
        public void OnLevelWasInitialized(int level){}
        public void OnFixedUpdate(){}
    }
}
