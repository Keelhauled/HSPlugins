using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using UILib;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using System.Collections;
using UnityEngine.EventSystems;

namespace ModMenuManager
{
    class ModMenuManager : MonoBehaviour
    {
        string menuFolder = "/Plugins/InterfaceSuite/ModMenuManager/";
        string menuFile = "ModMenuManager.xml";
        float marginSizeBig = 0f;
        float marginSizeSmall = 0f;
        float headerSize = 15f;
        float UIScale = 1.04f;
        float spacingY = 2f;

        Color dragColor = new Color(0.4f, 0.4f, 0.4f, 0.3f);
        Color transparentColor = new Color(1f, 1f, 1f, 0.01f);

        Canvas UISystem;
        static List<ModInfo> modinfolist;

        void Awake()
        {
            UIUtility.Init();
            StartCoroutine(MakeUI());
        }

        //void Update()
        //{
        //    if(Input.GetMouseButtonDown(0))
        //    {
        //        PointerEventData pointer = new PointerEventData(EventSystem.current);
        //        pointer.position = Input.mousePosition;

        //        List<RaycastResult> raycastResults = new List<RaycastResult>();
        //        EventSystem.current.RaycastAll(pointer, raycastResults);

        //        if(raycastResults.Count > 0)
        //        {
        //            var parent = FindParentWithComponent<Canvas>(raycastResults[0].gameObject);
        //            if(parent)
        //            {
        //                parent.transform.SetAsLastSibling();
        //                parent.GetComponent<Canvas>().sortingOrder = 5;
        //            }
        //        }
        //    }
        //}

        //static GameObject FindParentWithComponent<T>(GameObject childObject)
        //{
        //    Transform t = childObject.transform;
        //    while(t.parent != null)
        //    {
        //        if(t.parent.GetComponent<T>() != null)
        //        {
        //            return t.parent.gameObject;
        //        }
        //        t = t.parent.transform;
        //    }
        //    return null; // Could not find a parent with given tag.
        //}

        void OnDestroy()
        {
            if(UISystem)
                DestroyImmediate(UISystem.gameObject); 
        }

        IEnumerator MakeUI()
        {
            for(int i = 0; i < 2; i++) yield return null; // wait for other UI

            var elements = XElement.Load(Environment.CurrentDirectory + menuFolder + menuFile).Elements();
            var gameObjects = Resources.FindObjectsOfTypeAll<GameObject>().Where(x => x.GetComponent<RectTransform>()).ToList();
            modinfolist = new List<ModInfo>();
            foreach(var item in elements)
            {
                string itemName = (string)item.Element("name");
                var match = gameObjects.Where(x => itemName == x.name).ToList();
                if(match.Count > 0)
                {
                    if(match.Count > 1) Console.WriteLine("[{0}] More than one GameObject found with the name \"{1}\"", GetType().Name, itemName);
                    modinfolist.Add(new ModInfo(itemName, (string)item.Element("icon"), match[0]));
                }
            }

            int itemLimit = 4;
            float width = 59f;
            float height = ((width + spacingY / 2f) * Mathf.Clamp(modinfolist.Count, 1, itemLimit) + headerSize + marginSizeBig) / 2f + 1f;

            UISystem = UIUtility.CreateNewUISystem("ModMenuManagerCanvas");
            UISystem.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920f / UIScale, 1080f / UIScale);
            UISystem.gameObject.SetActive(false);
            UISystem.transform.SetAsLastSibling();
            UISystem.GetComponent<Canvas>().sortingOrder = 5;

            var panel = UIUtility.CreatePanel("Panel", UISystem.transform);
            panel.transform.SetRect(1f, 0.4f, 1f, 0.4f, -width, -height, 0f, height);
            panel.color = transparentColor;

            var drag = UIUtility.CreatePanel("Draggable", panel.transform);
            drag.transform.SetRect(0f, 1f, 1f, 1f, 0f, -headerSize);
            drag.color = dragColor;
            UIUtility.MakeObjectDraggable(drag.rectTransform, panel.rectTransform);

            var modlist = UIUtility.CreateScrollView("Modlist", panel.transform);
            modlist.transform.SetRect(0f, 0f, 1f, 1f, marginSizeBig, marginSizeBig, -marginSizeBig, -headerSize - marginSizeBig);
            modlist.gameObject.AddComponent<Mask>();
            modlist.content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            modlist.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.01f);
            modlist.movementType = ScrollRect.MovementType.Clamped;
            Destroy(modlist.verticalScrollbar.gameObject);
            modlist.scrollSensitivity = 50f;

            var autogrid = modlist.content.gameObject.AddComponent<AutoGridLayout>();
            autogrid.aspectRatio = 1;
            autogrid.m_IsColumn = true;
            autogrid.m_Column = 1;
            autogrid.spacing = new Vector2(0f, spacingY);

            foreach(var modinfo in modinfolist)
            {
                var button = UIUtility.CreateButton("Button", modlist.content.transform, "");
                button.transform.SetRect(0f, 0f, 1f, 1f, marginSizeSmall, marginSizeSmall, -marginSizeSmall, -marginSizeSmall);
                button.gameObject.AddComponent<Image>();

                string iconPath = Environment.CurrentDirectory + menuFolder + "icons/" + modinfo.Icon;
                if(File.Exists(iconPath))
                {
                    var texture = PngAssist.LoadTexture(iconPath);
                    var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    button.transform.gameObject.AddComponent<Image>();
                    var image = button.transform.gameObject.GetComponent<Image>();
                    image.sprite = sprite;
                }
                else
                {
                    button.GetComponentInChildren<Text>().text = modinfo.Name.Substring(0, 1);
                }

                //modinfo.Canvas.AddComponent<ManageSortingOrder>();

                button.onClick.AddListener(() =>
                {
                    modinfo.Canvas.SetActive(!modinfo.Canvas.activeSelf);
                    SetSortingOrder(modinfo);
                });
            }

            // hack for AutoGridLayout
            yield return null;
            UISystem.gameObject.SetActive(true);
        }

        void SetSortingOrder(ModInfo modinfo)
        {
            foreach(var item in modinfolist)
            {
                if(item == modinfo)
                {
                    modinfo.Canvas.transform.SetAsLastSibling();
                    modinfo.Canvas.GetComponent<Canvas>().sortingOrder = 6;
                }
                else
                {
                    modinfo.Canvas.transform.SetAsLastSibling();
                    modinfo.Canvas.GetComponent<Canvas>().sortingOrder = 5;
                }
            }
        }

        //static void SortHigh(GameObject canvas)
        //{
        //    canvas.transform.SetAsLastSibling();
        //    canvas.GetComponent<Canvas>().sortingOrder = 6;
        //}

        //static void SortLow()
        //{
        //    foreach(var item in modinfolist)
        //    {
        //        item.Canvas.transform.SetAsLastSibling();
        //        item.Canvas.GetComponent<Canvas>().sortingOrder = 5;
        //    }
        //}

        //class ManageSortingOrder : MonoBehaviour, IPointerClickHandler
        //{
        //    public void OnPointerClick(PointerEventData eventData)
        //    {
        //        Console.WriteLine("TEST");
        //        //SortLow();
        //        //SortHigh(eventData.pointerEnter);
        //    }
        //}

        class ModInfo
        {
            public string Name { get; set; }
            public string Icon { get; set; }
            public GameObject Canvas { get; set; }

            public ModInfo(string name, string icon, GameObject canvas)
            {
                Name = name;
                Icon = icon;
                Canvas = canvas;
            }
        }
    }
}
