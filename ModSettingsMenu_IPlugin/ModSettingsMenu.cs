using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UILib;
using IllusionPlugin;
using System.IO;
using UnityEngine.EventSystems;
using System.Xml.Linq;
using System.Reflection;
using IllusionInjector;
using System.Linq;

// support modifiers with KeyListener
// set Kisama.Kisama.IsInputBlocked in KeyListener

namespace ModSettingsMenu
{
    class ModSettingsMenu : MonoBehaviour
    {
        string menuFolder = "/Plugins/InterfaceSuite/ModSettingsMenu";
        float buttonSize = 10f;
        float marginSizeBig = 2f;
        float marginSizeSmall = 2f;
        float headerSize = 20f;
        float UIScale = 1.2f;
        float elementSize = 24f;
        float scrollOffsetX = -15f;

        Color dragColor = new Color(0.4f, 0.4f, 0.4f, 1f);
        Color backgroundColor = new Color(1f, 1f, 1f, 1f);
        Color outlineColor = new Color(0f, 0f, 0f, 1f);
        Color listColor1 = new Color(0.95f, 0.95f, 0.95f, 1f);
        Color listColor2 = new Color(1f, 1f, 1f, 1f);
        Color separatorColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        Color sliderFieldColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        Color textColor = Color.black;

        Canvas UISystem;
        Dictionary<string, Dictionary<string, string>> changes = new Dictionary<string, Dictionary<string, string>>();

        void Awake()
        {
            UIUtility.Init();
            ModMenuUI();
        }

        void Update()
        {
            Kisama.Kisama.IsInputBlocked = EventSystem.current.currentSelectedGameObject && EventSystem.current.currentSelectedGameObject.GetComponent<KeyListener>() != null;
        }

        void OnDestroy()
        {
            DestroyImmediate(UISystem.gameObject);
        }

        void ModMenuUI()
        {
            int itemLimit = 10;
            float width = 200f / 2f;
            string[] files = Directory.GetFiles(Environment.CurrentDirectory + menuFolder, "*.xml", SearchOption.TopDirectoryOnly);
            float height = (elementSize * Mathf.Clamp(files.Length, 1, itemLimit) + headerSize + marginSizeBig) / 2f;

            UISystem = UIUtility.CreateNewUISystem("ModSettingsMenuCanvas");
            UISystem.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920f / UIScale, 1080f / UIScale);
            UISystem.gameObject.SetActive(false);

            var panel = UIUtility.CreatePanel("Panel", UISystem.transform);
            panel.transform.SetRect(1f, 0.4f, 1f, 0.4f, -width * 3f, -height, -width, height);
            panel.color = backgroundColor;
            UIUtility.AddOutlineToObject(panel.transform, outlineColor);

            var modlist = UIUtility.CreateScrollView("Modlist", panel.transform);
            modlist.transform.SetRect(0f, 0f, 1f, 1f, marginSizeBig, marginSizeBig, -marginSizeBig, -headerSize);
            modlist.gameObject.AddComponent<Mask>();
            modlist.content.gameObject.AddComponent<VerticalLayoutGroup>();
            modlist.content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            modlist.verticalScrollbar.GetComponent<RectTransform>().offsetMin = new Vector2(scrollOffsetX, 0f);
            modlist.viewport.offsetMax = new Vector2(scrollOffsetX, 0f);
            modlist.movementType = ScrollRect.MovementType.Clamped;
            modlist.scrollSensitivity = elementSize;

            var drag = UIUtility.CreatePanel("Draggable", panel.transform);
            drag.transform.SetRect(0f, 1f, 1f, 1f, 0f, -headerSize);
            drag.color = dragColor;
            UIUtility.MakeObjectDraggable(drag.rectTransform, panel.rectTransform);

            var nametext = UIUtility.CreateText("Nametext", drag.transform, "Mod Settings");
            nametext.transform.SetRect(0f, 0f, 1f, 1f);
            nametext.alignment = TextAnchor.MiddleCenter;
            nametext.resizeTextMinSize = 10;
            nametext.resizeTextMaxSize = 16;

            var close = UIUtility.CreateButton("CloseButton", drag.transform, "");
            close.transform.SetRect(1f, 0f, 1f, 1f, -buttonSize * 2f);
            close.onClick.AddListener(() => UISystem.gameObject.SetActive(false));
            AddCloseSymbol(close);

            var images = new List<Image>();
            for(int i = 0; i < files.Length; i++)
            {
                ModInfo modinfo = new ModInfo(files[i]);
                Image image = CreateSettingsUI(panel.transform, modinfo);
                images.Add(image);

                var button = UIUtility.CreateButton("Button", modlist.content.transform, modinfo.Name);
                button.gameObject.AddComponent<LayoutElement>().preferredHeight = elementSize;
                button.transform.SetRect(0f, 0f, 1f, 1f, marginSizeSmall, marginSizeSmall, -marginSizeSmall, -marginSizeSmall);
                button.onClick.AddListener(() =>
                {
                    if(image.gameObject.activeSelf)
                    {
                        image.gameObject.SetActive(false);
                    }
                    else
                    {
                        foreach(var item in images)
                        {
                            item.gameObject.SetActive(false);
                        }

                        image.gameObject.SetActive(true);
                    }
                });

                var text = button.gameObject.GetComponentInChildren<Text>(true);
                text.resizeTextMinSize = 10;
                text.resizeTextMaxSize = 18;
                text.color = Color.black;
            }
        }

        Image CreateSettingsUI(Transform parent, ModInfo modinfo)
        {
            int itemlimit = 15;
            float width = 360f;
            float height = elementSize * Mathf.Clamp(modinfo.settings.Count, 1, itemlimit) + headerSize + marginSizeBig;

            var panel = UIUtility.CreatePanel("Panel", parent.transform);
            panel.gameObject.SetActive(false);
            panel.color = backgroundColor;
            panel.rectTransform.SetRect(0f, 1f, 0f, 1f, -width, -height, 0f, 0f);
            UIUtility.AddOutlineToObject(panel.transform, outlineColor);

            var optionlist = UIUtility.CreateScrollView("Optionlist", panel.transform);
            optionlist.transform.SetRect(0f, 0f, 1f, 1f, marginSizeBig, marginSizeBig, -marginSizeBig, -headerSize);
            optionlist.gameObject.AddComponent<Mask>();
            optionlist.content.gameObject.AddComponent<VerticalLayoutGroup>();
            optionlist.content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            optionlist.scrollSensitivity = elementSize;
            optionlist.movementType = ScrollRect.MovementType.Clamped;
            optionlist.verticalScrollbar.GetComponent<RectTransform>().offsetMin = new Vector2(scrollOffsetX, 0f);
            optionlist.viewport.offsetMax = new Vector2(scrollOffsetX, 0f);

            var drag = UIUtility.CreatePanel("Draggable", panel.transform);
            drag.rectTransform.SetRect(0f, 1f, 1f, 1f, 0f, -headerSize);
            drag.color = dragColor;
            UIUtility.MakeObjectDraggable(drag.rectTransform, parent as RectTransform);

            var nametext = UIUtility.CreateText("Nametext", drag.transform, modinfo.Name);
            nametext.transform.SetRect(0f, 0f, 1f, 1f, 100f, 0f, -buttonSize * 2f);
            nametext.alignment = TextAnchor.MiddleCenter;
            nametext.resizeTextMinSize = 10;
            nametext.resizeTextMaxSize = 16;

            var close = UIUtility.CreateButton("CloseButton", drag.transform, "");
            close.transform.SetRect(1f, 0f, 1f, 1f, -buttonSize * 2f);
            close.onClick.AddListener(() => panel.gameObject.SetActive(false));
            AddCloseSymbol(close);

            var apply = UIUtility.CreateButton("ApplyButton", drag.transform, "Apply");
            apply.transform.SetRect(0f, 0f, 0f, 1f, 0f, 0f, 50f);
            apply.onClick.AddListener(() =>
            {
                foreach(var item in changes[modinfo.IniName])
                {
                    Console.WriteLine(modinfo.IniName + " " + item.Key + " " + item.Value);
                    ModPrefs.SetString(modinfo.IniName, item.Key, item.Value);
                }

                changes[modinfo.IniName] = new Dictionary<string, string>();

                if(modinfo.MethodPath != null)
                {
                    var returnval = InvokePluginMethod(modinfo.TypeName, modinfo.Invoke);
                    bool updated = returnval != null && returnval is bool && (bool)returnval == true;
                    Console.WriteLine("{0} settings {1}updated", modinfo.Name, updated ? "" : "may not have been "); 
                }
                else
                {
                    Console.WriteLine("{0} settings not updated", modinfo.Name);
                }
            });
            
            var presetbutton = UIUtility.CreateButton("PresetButton", drag.transform, "Preset");
            presetbutton.transform.SetRect(0f, 0f, 0f, 1f, 50f, 0f, 100f);

            if(modinfo.presets.Count > 0)
            {
                float elementsize = 20f;
                float listmargin = 4f;
                int scrollLimit = 5;
                float listwidth = 120f / 2f;
                float listheight = Mathf.Clamp(modinfo.presets.Count, 1, scrollLimit) * elementsize + listmargin;

                var presetbg = UIUtility.CreatePanel("PresetBackground", presetbutton.transform);
                presetbg.gameObject.SetActive(false);
                presetbg.transform.SetRect(0.5f, 0f, 0.5f, 0f, -listwidth, -listheight, listwidth, 0f);
                presetbg.color = backgroundColor;
                UIUtility.AddOutlineToObject(presetbg.transform, outlineColor);

                presetbutton.onClick.AddListener(() =>
                {
                    presetbg.gameObject.SetActive(!presetbg.gameObject.activeSelf);
                    // CHECK FOR NEW PRESET FILES HERE OR WHEN OPENING MOD SPECIFIC MENU
                });

                var presetlist = UIUtility.CreateScrollView("PresetList", presetbg.transform);
                presetlist.transform.SetRect(0f, 0f, 1f, 1f, marginSizeSmall, marginSizeSmall, -marginSizeSmall, -marginSizeSmall);
                presetlist.gameObject.AddComponent<Mask>();
                presetlist.content.gameObject.AddComponent<VerticalLayoutGroup>();
                presetlist.content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                presetlist.scrollSensitivity = elementSize;
                presetlist.movementType = ScrollRect.MovementType.Clamped;
                presetlist.verticalScrollbar.GetComponent<RectTransform>().offsetMin = new Vector2(scrollOffsetX, 0f);
                presetlist.viewport.offsetMax = new Vector2(scrollOffsetX, 0f);

                foreach(var preset in modinfo.presets)
                {
                    var button = UIUtility.CreateButton(preset.Name, presetlist.content.transform, preset.Name);
                    button.transform.SetRect(0f, 0f, 1f, 1f);
                    button.gameObject.AddComponent<LayoutElement>().preferredHeight = elementsize;

                    button.onClick.AddListener(() =>
                    {
                        foreach(var setting in preset.PresetSettings)
                        {
                            ModPrefs.SetString(modinfo.IniName, setting.Setting, setting.Value);
                        }

                        foreach(var item in optionlist.content.GetComponentsInChildren<Image>())
                        {
                            Destroy(item.gameObject);
                        }

                        changes[modinfo.IniName] = new Dictionary<string, string>();
                        CreateOptions(optionlist, modinfo);
                    });
                }
            }
            else
            {
                presetbutton.interactable = false;
            }

            changes[modinfo.IniName] = new Dictionary<string, string>();
            CreateOptions(optionlist, modinfo);
            return panel;
        }

        void CreateOptions(ScrollRect parent, ModInfo modinfo)
        {
            for(int i = 0; i < modinfo.settings.Count; i++)
            {
                var option = UIUtility.CreatePanel("Option" + i, parent.content.transform);
                option.color = i % 2 != 0 ? listColor1 : listColor2;
                option.gameObject.AddComponent<LayoutElement>().preferredHeight = elementSize;

                var setting = modinfo.settings[i];
                switch(setting.Type)
                {
                    case "hotkey":
                    {
                        var text = UIUtility.CreateText(setting.Setting, option.transform, setting.Text);
                        text.alignment = TextAnchor.MiddleCenter;
                        text.transform.SetRect(0.01f, 0f, 0.55f, 1f);
                        text.resizeTextMinSize = 10;
                        text.resizeTextMaxSize = 18;
                        text.color = textColor;

                        var button = UIUtility.CreateButton(setting.Setting, option.transform, ModPrefs.GetString(modinfo.IniName, setting.Setting, "None"));
                        button.transform.SetRect(0.66f, 0f, 0.86f, 1f);
                        var listener = button.gameObject.AddComponent<KeyListener>();
                        listener.RegisterKeyDownCallback((x, y) =>
                        {
                            button.GetComponentInChildren<Text>().text = x.ToString();
                            changes[modinfo.IniName][setting.Setting] = x.ToString();
                        });

                        var reset = UIUtility.CreateButton("ResetButton", option.transform, "");
                        reset.transform.SetRect(0.89f, 0.5f, 0.89f, 0.5f, -buttonSize, -buttonSize, buttonSize, buttonSize);
                        AddCloseSymbol(reset);
                        reset.onClick.AddListener(() =>
                        {
                            button.GetComponentInChildren<Text>().text = "None";
                            changes[modinfo.IniName][setting.Setting] = "None";
                        });
                        break;
                    }

                    case "toggle":
                    {
                        var text = UIUtility.CreateText(setting.Setting, option.transform, setting.Text);
                        text.alignment = TextAnchor.MiddleCenter;
                        text.transform.SetRect(0.01f, 0f, 0.55f, 0.94f);
                        text.resizeTextMinSize = 10;
                        text.resizeTextMaxSize = 18;
                        text.color = textColor;

                        var container = UIUtility.CreateNewUIObject(option.transform);
                        var toggle = UIUtility.AddCheckboxToObject(container);
                        toggle.transform.SetRect(0.75f, 0.5f, 0.75f, 0.5f, -buttonSize, -buttonSize, buttonSize, buttonSize);
                        toggle.isOn = ModPrefs.GetBool(modinfo.IniName, setting.Setting);
                        toggle.onValueChanged.AddListener((x) => changes[modinfo.IniName][setting.Setting] = Convert.ToInt32(x).ToString());
                        break;
                    }

                    case "slider":
                    {
                        bool wholenumbers = (bool)setting.Int;

                        var text = UIUtility.CreateText(setting.Setting, option.transform, setting.Text);
                        text.alignment = TextAnchor.MiddleCenter;
                        text.transform.SetRect(0.01f, 0f, 0.55f, 0.94f);
                        text.resizeTextMinSize = 10;
                        text.resizeTextMaxSize = 18;
                        text.color = textColor;

                        var field = UIUtility.CreateInputField(setting.Setting, option.transform, "");
                        field.transform.SetRect(0.9f, 0f, 1f, 1f);
                        field.interactable = false;
                        float value = ModPrefs.GetFloat(modinfo.IniName, setting.Setting);
                        field.text = value.ToString("F" + (wholenumbers ? "0" : ""));
                        field.textComponent.alignment = TextAnchor.MiddleCenter;
                        var colors = field.colors;
                        colors.disabledColor = sliderFieldColor;
                        field.colors = colors;

                        var slider = UIUtility.CreateSlider(setting.Setting, option.transform);
                        slider.transform.SetRect(0.58f, 0.1f, 0.89f, 0.9f);
                        slider.minValue = (float)setting.Min;
                        slider.maxValue = (float)setting.Max;
                        slider.wholeNumbers = wholenumbers;
                        slider.value = value;
                        slider.onValueChanged.AddListener((x) =>
                        {
                            field.text = x.ToString("F" + (wholenumbers ? "0" : ""));
                            changes[modinfo.IniName][setting.Setting] = x.ToString();
                        });
                        break;
                    }

                    case "title":
                    {
                        var separator = UIUtility.CreatePanel("Separator", option.transform);
                        separator.transform.SetRect(0f, 0f, 1f, 1f);
                        separator.color = separatorColor;

                        var text = UIUtility.CreateText("Title", option.transform, setting.Text);
                        text.alignment = TextAnchor.MiddleCenter;
                        text.fontStyle = FontStyle.Bold;
                        text.transform.SetRect(0f, 0f, 1f, 1f);
                        text.resizeTextMinSize = 10;
                        text.resizeTextMaxSize = 18;
                        break;
                    }

                    case "fullinputfield":
                    {
                        var input = UIUtility.CreateInputField(setting.Setting, option.transform, setting.Setting);
                        input.transform.SetRect(0f, 0f, 1f, 1f);
                        input.text = ModPrefs.GetString(modinfo.IniName, setting.Setting);
                        input.textComponent.alignment = TextAnchor.MiddleCenter;
                        input.onValueChanged.AddListener((x) => changes[modinfo.IniName][setting.Setting] = x);
                        break;
                    }

                    case "inputfield":
                    {
                        var text = UIUtility.CreateText(setting.Setting, option.transform, setting.Text);
                        text.alignment = TextAnchor.MiddleCenter;
                        text.transform.SetRect(0.01f, 0f, 0.55f, 1f);
                        text.resizeTextMinSize = 10;
                        text.resizeTextMaxSize = 18;
                        text.color = textColor;

                        var input = UIUtility.CreateInputField(setting.Setting, option.transform, setting.Setting);
                        input.transform.SetRect(0.56f, 0f, 1f, 1f);
                        input.text = ModPrefs.GetString(modinfo.IniName, setting.Setting);
                        input.textComponent.alignment = TextAnchor.MiddleCenter;
                        input.onValueChanged.AddListener((x) => changes[modinfo.IniName][setting.Setting] = x);
                        break;
                    }
                }
            }
        }

        public static void AddCloseSymbol(Button button)
        {
            var x1 = UIUtility.CreatePanel("x1", button.transform);
            x1.transform.SetRect(0f, 0f, 1f, 1f, 8f, 0f, -8f);
            x1.rectTransform.eulerAngles = new Vector3(0f, 0f, 45f);
            x1.color = new Color(0f, 0f, 0f, 1f);

            var x2 = UIUtility.CreatePanel("x2", button.transform);
            x2.transform.SetRect(0f, 0f, 1f, 1f, 8f, 0f, -8f);
            x2.rectTransform.eulerAngles = new Vector3(0f, 0f, -45f);
            x2.color = new Color(0f, 0f, 0f, 1f);
        }

        public static object InvokePluginMethod(string typeName, string methodName, params object[] parameters)
        {
            Type type = FindTypeIPlugin(typeName);

            if(type != null)
            {
                var instance = FindObjectOfType(type);

                if(instance != null)
                {
                    MethodInfo methodInfo = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

                    if(methodInfo != null)
                    {
                        object returnval;
                        if(methodInfo.GetParameters().Length == 0)
                        {
                            returnval = methodInfo.Invoke(instance, null);
                        }
                        else
                        {
                            returnval = methodInfo.Invoke(instance, parameters);
                        }

                        return returnval;
                    }
                    else
                    {
                        Console.WriteLine("Method {0}.{1} not found", typeName, methodInfo);
                    }
                }
                else
                {
                    Console.WriteLine("Instance of {0} not found", typeName);
                }
            }
            else
            {
                Console.WriteLine("Type {0} not found", typeName);
            }

            return null;
        }

        public static Type FindTypeIPlugin(string qualifiedTypeName)
        {
            Type t = null;

            foreach(Assembly asm in PluginManager.Plugins.Select(x => x.GetType().Assembly))
            {
                t = asm.GetType(qualifiedTypeName);
                if(t != null)
                {
                    //Console.WriteLine("{0} belongs to an IPlugin", qualifiedTypeName);
                    return t;
                }
            }

            t = Type.GetType(qualifiedTypeName);
            if(t != null) return t;

            foreach(Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                t = asm.GetType(qualifiedTypeName);
                if(t != null)
                {
                    return t;
                }
            }

            return null;
        }

        class ModInfo
        {
            public string Name { get; set; }
            public string IniName { get; set; }
            public string Namespace { get; set; }
            public string Classname { get; set; }
            public string Invoke { get; set; }
            public string TypeName { get; set; }
            public string MethodPath { get; set; }
            public List<ModSetting> settings { get; set; }
            public List<PresetInfo> presets { get; set; }

            public ModInfo(string xmlPath)
            {
                var xElement = XElement.Load(xmlPath);
                
                Name = (string)xElement.Element("name");
                IniName = (string)xElement.Element("ininame");
                Namespace = (string)xElement.Element("namespace");
                Classname = (string)xElement.Element("classname");
                Invoke = (string)xElement.Element("invoke");

                if(Namespace != null && Classname != null && Invoke != null)
                {
                    TypeName = string.Format("{0}.{1}", Namespace, Classname);
                    MethodPath = string.Format("{0}.{1}.{2}", Namespace, Classname, Invoke);
                }

                settings = new List<ModSetting>();
                foreach(var item in xElement.Element("settings").Elements())
                {
                    var setting = new ModSetting();
                    setting.Type = (string)item.Element("type");
                    setting.Text = (string)item.Element("text");
                    setting.Setting = (string)item.Element("setting");
                    setting.Int = (bool?)item.Element("int");
                    setting.Min = (float?)item.Element("min");
                    setting.Max = (float?)item.Element("max");
                    settings.Add(setting);
                }

                presets = new List<PresetInfo>();
                var presetElement = xElement.Element("presets");
                if(presetElement != null)
                {
                    foreach(var item in presetElement.Elements())
                    {
                        var preset = new PresetInfo();
                        preset.Name = (string)item.Element("name");

                        preset.PresetSettings = new List<PresetInfo.PresetSetting>();
                        foreach(var item2 in item.Element("values").Elements())
                        {
                            var presetSetting = new PresetInfo.PresetSetting();
                            presetSetting.Setting = (string)item2.Element("setting");
                            presetSetting.Value = (string)item2.Element("value");
                            preset.PresetSettings.Add(presetSetting);
                        }

                        presets.Add(preset);
                    } 
                }
            }

            public class ModSetting
            {
                public string Type { get; set; }
                public string Text { get; set; }
                public string Setting { get; set; }
                public float? Min { get; set; }
                public float? Max { get; set; }
                public bool? Int { get; set; }
            }

            public class PresetInfo
            {
                public string Name { get; set; }
                public List<PresetSetting> PresetSettings { get; set; }

                public class PresetSetting
                {
                    public string Setting { get; set; }
                    public string Value { get; set; }
                }
            }
        }
    }
}
