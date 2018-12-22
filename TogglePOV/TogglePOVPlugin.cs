using IllusionPlugin;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TogglePOV
{
    public class TogglePOVPlugin : IEnhancedPlugin
    {
        public const string PLUGIN_NAME = "TogglePOV";
        public const string PLUGIN_VERSION = "1.0.1";
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
            StartMod();
        }

        public static void StartMod()
        {
            switch(SceneManager.GetActiveScene().name)
            {
                case "Studio":
                {
                    new GameObject(PLUGIN_NAME).AddComponent<NeoMono>();
                    break;
                }

                case "HScene":
                {
                    new GameObject(PLUGIN_NAME).AddComponent<HSceneMono>();
                    break;
                }
            }
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
