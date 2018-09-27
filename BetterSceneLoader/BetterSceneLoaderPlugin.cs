using IllusionPlugin;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BetterSceneLoader
{
    public class BetterSceneLoaderPlugin : IEnhancedPlugin
    {
        public const string PLUGIN_NAME = "BetterSceneLoader";
        public const string PLUGIN_VERSION = "1.1.1";
        public string Name => PLUGIN_NAME;
        public string Version => PLUGIN_VERSION;

        public string[] Filter => new string[]
        {
            "StudioNEO_32",
            "StudioNEO_64",
        };

        public static string[] SceneFilter = new string[]
        {
            "Studio",
        };

        public void OnLevelWasLoaded(int level)
        {
            StartMod();
        }

        public static void StartMod()
        {
            if(SceneFilter.Contains(SceneManager.GetActiveScene().name)) new GameObject(PLUGIN_NAME).AddComponent<BetterSceneLoader>();
        }

        public static void Bootstrap()
        {
            var gameobject = GameObject.Find(PLUGIN_NAME);
            if(gameobject != null) GameObject.DestroyImmediate(gameobject);
            StartMod();
        }

        public void OnApplicationStart(){}
        public void OnUpdate(){}
        public void OnLateUpdate(){}
        public void OnApplicationQuit(){}
        public void OnLevelWasInitialized(int level){}
        public void OnFixedUpdate(){}
    }
}
