using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TitleShortcuts
{
    class TitleShortcuts : MonoBehaviour
    {
        bool autoStart = true;
        bool check = false;

        void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        void OnLevelWasLoaded(int level)
        {
            StartInput();
        }

        void StartInput()
        {
            if(SceneManager.GetActiveScene().name == "Title")
            {
                if(!check)
                {
                    check = true;
                    StartCoroutine(InputCheck());
                }
            }
            else
            {
                check = false;
            }
        }

        IEnumerator InputCheck()
        {
            while(check)
            {
                if(!Manager.Scene.Instance.IsNowLoadingFade)
                {
                    if(Input.GetKeyDown(KeyCode.N))
                    {
                        OnCustomFemale();
                    }
                    else if(Input.GetKeyDown(KeyCode.M))
                    {
                        OnCustomMale();
                    }
                    else if(autoStart)
                    {
                        OnCustomFemale();
                    }
                }

                yield return null;
            }
        }

        void OnCustomMale()
        {
            Singleton<TitleScene>.Instance.SelectCustomM();
            check = false;
        }

        void OnCustomFemale()
        {
            Singleton<TitleScene>.Instance.SelectCustomF();
            check = false;
        }
    }
}
