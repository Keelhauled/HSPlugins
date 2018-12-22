using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Collections;
using UnityEngine;
using IllusionPlugin;
using System.Reflection;

namespace HideUI
{
    class HideUI : MonoBehaviour
    {
        static string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/HideUI.txt";
        Dictionary<string, CacheObject> canvasCache = new Dictionary<string, CacheObject>();
        KeyCode hotkey = KeyCode.Mouse3;

        void Awake()
        {
            LoadSettings();
            StartCoroutine(UpdateDelayed());
        }

        bool LoadSettings()
        {
            try
            {
                string keystring = ModPrefs.GetString("HideUI", "HideUIHotkey", hotkey.ToString(), true);
                hotkey = (KeyCode)Enum.Parse(typeof(KeyCode), keystring, true);
            }
            catch(Exception)
            {
                Console.WriteLine("Using default hotkey ({0})", hotkey.ToString());
                return false;
            }

            return true;
        }

        IEnumerator UpdateDelayed()
        {
            for(int i = 0; i < 3; i++) yield return null; // wait for other UI

            while(true)
            {
                if(Input.GetKeyDown(hotkey))
                {
                    var names = File.ReadAllLines(path);
                    if(names.Length > 0)
                    {
                        var gameobjects = FindObjectsOfType<GameObject>();

                        foreach(var canvasName in names)
                        {
                            CacheObject cacheobject;
                            if(!canvasCache.TryGetValue(canvasName, out cacheobject))
                            {
                                var split = canvasName.Split('|');
                                var match = gameobjects.Where(x => x.name == split[0]).ToList();

                                // Pick gameobject with RectTransform
                                GameObject gameobject = null;
                                for(int i = 0; i < match.Count; i++)
                                {
                                    if(match[i].GetComponent<RectTransform>())
                                    {
                                        gameobject = match[i];
                                        break;
                                    }
                                }

                                if(gameobject)
                                {
                                    try
                                    {
                                        cacheobject = new CacheObject();
                                        cacheobject.gameobject = gameobject;
                                        cacheobject.canvas = gameobject.GetComponent<Canvas>();
                                        cacheobject.canvasName = split[0];
                                        cacheobject.hideAction = (CacheObject.HideAction)Enum.Parse(typeof(CacheObject.HideAction), split[1], true);
                                        cacheobject.hideMethod = (CacheObject.HideMethod)Enum.Parse(typeof(CacheObject.HideMethod), split[2], true);
                                        Console.WriteLine(gameobject.name + " detected");

                                        if(cacheobject.hideMethod == CacheObject.HideMethod.Enabled && !cacheobject.canvas)
                                        {
                                            cacheobject.hideMethod = CacheObject.HideMethod.SetActive;
                                            Console.WriteLine(" Canvas not found, switching to setactive mode");
                                        }

                                        canvasCache.Add(canvasName, cacheobject);
                                    }
                                    catch(Exception ex)
                                    {
                                        Console.WriteLine(ex);
                                    }
                                }
                            }
                        }

                        if(canvasCache.Count > 0)
                        {
                            bool flag = GetMasterFlag(canvasCache.First().Value);
                            bool savedFlag = canvasCache.First().Value.savedState;

                            foreach(var cacheobject in canvasCache.Values.ToList())
                            {
                                switch(cacheobject.hideAction)
                                {
                                    case CacheObject.HideAction.Toggle:
                                    {
                                        ShowCanvas(cacheobject, !flag);
                                        break;
                                    }
                                    case CacheObject.HideAction.False:
                                    {
                                        ShowCanvas(cacheobject, false);
                                        break;
                                    }
                                    case CacheObject.HideAction.True:
                                    {
                                        ShowCanvas(cacheobject, true);
                                        break;
                                    }
                                    case CacheObject.HideAction.Save:
                                    {
                                        switch(cacheobject.hideMethod)
                                        {
                                            case CacheObject.HideMethod.SetActive:
                                            {
                                                if(cacheobject.gameobject.activeSelf)
                                                {
                                                    cacheobject.savedState = cacheobject.gameobject.activeSelf;
                                                    ShowCanvas(cacheobject, false);
                                                }
                                                else if(!cacheobject.gameobject.activeSelf && cacheobject.savedState)
                                                {
                                                    cacheobject.savedState = cacheobject.gameobject.activeSelf;
                                                    ShowCanvas(cacheobject, !flag);
                                                }
                                                else if(!cacheobject.gameobject.activeSelf && !cacheobject.savedState)
                                                {
                                                    cacheobject.savedState = cacheobject.gameobject.activeSelf;
                                                    ShowCanvas(cacheobject, false);
                                                }
                                                break;
                                            }

                                            case CacheObject.HideMethod.Enabled:
                                            {
                                                if(cacheobject.canvas.enabled)
                                                {
                                                    cacheobject.savedState = cacheobject.canvas.enabled;
                                                    ShowCanvas(cacheobject, false);
                                                }
                                                else if(!cacheobject.canvas.enabled && cacheobject.savedState)
                                                {
                                                    cacheobject.savedState = cacheobject.canvas.enabled;
                                                    ShowCanvas(cacheobject, true);
                                                }
                                                else if(!cacheobject.canvas.enabled && !cacheobject.savedState)
                                                {
                                                    cacheobject.savedState = cacheobject.canvas.enabled;
                                                    ShowCanvas(cacheobject, false);
                                                }
                                                break;
                                            }
                                        }

                                        break;
                                    }
                                }
                            }
                        }
                    }
                }

                yield return null;
            }
        }

        void ShowCanvas(CacheObject cacheobject, bool flag)
        {
            switch(cacheobject.hideMethod)
            {
                case CacheObject.HideMethod.SetActive:
                {
                    cacheobject.gameobject.SetActive(flag);
                    break;
                }
                case CacheObject.HideMethod.Enabled:
                {
                    cacheobject.canvas.enabled = flag;
                    break;
                }
            }
        }

        bool GetMasterFlag(CacheObject cacheobject)
        {
            switch(cacheobject.hideMethod)
            {
                case CacheObject.HideMethod.SetActive:
                {
                    return cacheobject.gameobject.activeSelf;
                }
                case CacheObject.HideMethod.Enabled:
                {
                    return cacheobject.canvas.enabled;
                }
            }

            return true;
        }

        class CacheObject
        {
            public GameObject gameobject;
            public Canvas canvas;
            public bool savedState = false;
            public string canvasName;
            public HideAction hideAction;
            public HideMethod hideMethod;

            public enum HideAction
            {
                Toggle,
                Save,
                False,
                True,
            }

            public enum HideMethod
            {
                SetActive,
                Enabled,
            }
        }
    }
}
