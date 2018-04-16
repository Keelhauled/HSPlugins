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
        public string Name => PLUGIN_NAME;
        public string Version => PLUGIN_VERSION;

        public string[] Filter => new string[]
        {
            "HoneySelect_32",
            "HoneySelect_64",
            "StudioNEO_32",
            "StudioNEO_64",
        };

        //public static string[] SceneFilter = new string[]
        //{
        //    "Studio",
        //    "HScene",
        //    "CustomScene",
        //};

        public void OnLevelWasLoaded(int level)
        {
            //StartMod();
        }

        //public static void StartMod()
        //{
        //    //if(SceneFilter.Contains(SceneManager.GetActiveScene().name)) new GameObject(PLUGIN_NAME).AddComponent<HSTesting>();
        //}

        //public static void Bootstrap()
        //{
        //    var gameobject = GameObject.Find(PLUGIN_NAME);
        //    if(gameobject != null) GameObject.DestroyImmediate(gameobject);
        //    StartMod();
        //}

        public void OnApplicationStart()
        {
            //HarmonyInstance.DEBUG = true;
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
