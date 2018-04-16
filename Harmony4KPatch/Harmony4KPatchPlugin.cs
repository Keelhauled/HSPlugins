using System;
using IllusionPlugin;
using Harmony;
using System.Reflection;

namespace Harmony4KPatch
{
    public class Harmony4KPatchPlugin : IEnhancedPlugin
    {
        public const string PLUGIN_NAME = "Harmony4KPatch";
        public const string PLUGIN_VERSION = "1.0";
        public string Name => PLUGIN_NAME;
        public string Version => PLUGIN_VERSION;

        public string[] Filter => new string[]
        {
            "HoneySelect_32",
            "HoneySelect_64",
            "StudioNEO_32",
            "StudioNEO_64",
        };

        public void OnApplicationStart()
        {
            //HarmonyInstance.DEBUG = true;
            HarmonyInstance harmony = HarmonyInstance.Create("Harmony4KPatch.HarmonyPatches");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        public void OnLevelWasLoaded(int level){}
        public void OnUpdate(){}
        public void OnLateUpdate(){}
        public void OnApplicationQuit(){}
        public void OnLevelWasInitialized(int level){}
        public void OnFixedUpdate(){}
    }
}
