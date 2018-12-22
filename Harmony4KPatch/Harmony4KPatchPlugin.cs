using IllusionPlugin;
using Harmony;
using System.Reflection;

namespace Harmony4KPatch
{
    public class Harmony4KPatchPlugin : IEnhancedPlugin
    {
        public string Name { get; } = "Harmony4KPatch";
        public string Version { get; } = "1.0";

        public string[] Filter { get; } = new string[]
        {
            "HoneySelect_32",
            "HoneySelect_64",
            "StudioNEO_32",
            "StudioNEO_64",
        };

        public void OnApplicationStart()
        {
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
