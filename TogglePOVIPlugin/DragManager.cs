using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TogglePOV
{
    public class DragManager : MonoBehaviour
    {
        public static bool allowCamera = false;
        bool mouseButtonDown0 = false;
        bool mouseButtonDown1 = false;

        void Update()
        {
            if(!allowCamera)
            {
                if(GUIUtility.hotControl == 0 && !EventSystem.current.IsPointerOverGameObject())
                {
                    if(Input.GetMouseButtonDown(0))
                    {
                        mouseButtonDown0 = true;
                        allowCamera = true;
                    }

                    if(Input.GetMouseButtonDown(1))
                    {
                        mouseButtonDown1 = true;
                        allowCamera = true;
                    }
                }
            }

            if(allowCamera)
            {
                bool mouseUp0 = Input.GetMouseButtonUp(0);
                bool mouseUp1 = Input.GetMouseButtonUp(1);

                if((mouseButtonDown0 || mouseButtonDown1) && (mouseUp0 || mouseUp1))
                {
                    if(mouseUp0) mouseButtonDown0 = false;
                    if(mouseUp1) mouseButtonDown1 = false;

                    if(!mouseButtonDown0 && !mouseButtonDown1)
                    {
                        allowCamera = false;
                    }
                }
            }
        }
    }
}
