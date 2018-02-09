using System;
using Studio;
using UnityEngine;
using IllusionUtility.GetUtility;
using UnityEngine.UI;
using UILib;
using HSStudioNEOExtSave;
using System.Collections.Generic;
using System.Linq;

namespace LightManager
{
    class LightManager : MonoBehaviour
    {
        LightManagerSaveLoadHandler handler;
        Action<TreeNodeObject> selectWorkDel;

        Image spotlightUI;
        Image mainPanel;
        Text targetText;
        InputField speedField;

        void Start()
        {
            var uiTransform = Studio.Studio.Instance.transform.Find("Canvas Main Menu/02_Manipulate/02_Light/Image Spot");
            spotlightUI = uiTransform.GetComponent<Image>();

            UIUtility.Init();
            ExtraLightUI(uiTransform);

            handler = new LightManagerSaveLoadHandler();
            StudioNEOExtendSaveMgr.Instance.RegisterHandler(handler);

            selectWorkDel = new Action<TreeNodeObject>(OnSelectWork);
            Studio.Studio.Instance.treeNodeCtrl.onSelect += selectWorkDel;
        }

        void OnSelectWork(TreeNodeObject node)
        {
            ObjectCtrlInfo objectCtrlInfo;
            if(Studio.Studio.Instance.dicInfo.TryGetValue(node, out objectCtrlInfo))
            {
                if(objectCtrlInfo is OCILight)
                {
                    var ocilight = objectCtrlInfo as OCILight;
                    var tracker = ocilight.light.gameObject.GetComponent<TrackTransform>();
                    if(tracker)
                    {
                        targetText.text = tracker.targetName;
                        speedField.text = tracker.rotationSpeed.ToString();
                    }
                    else
                    {
                        targetText.text = "None";
                        speedField.text = "1";
                    }
                }
            }
        }

        void OnDestroy()
        {
            DestroyImmediate(mainPanel.gameObject);

            foreach(var item in Resources.FindObjectsOfTypeAll<TrackTransform>())
            {
                DestroyImmediate(item);
            }

            StudioNEOExtendSaveMgr.Instance.Unregisterhandler(handler);
            Studio.Studio.Instance.treeNodeCtrl.onSelect -= selectWorkDel;
        }

        void Update()
        {
            mainPanel.gameObject.SetActive(spotlightUI.isActiveAndEnabled);
        }

        void ExtraLightUI(Transform parent)
        {
            Color backgroundColor = new Color(0.41f, 0.42f, 0.43f, 1f);
            float width = 50f;
            float height = 50f;

            mainPanel = UIUtility.CreatePanel("LightManagerPanel", parent);
            mainPanel.color = backgroundColor;
            mainPanel.transform.SetRect(1f, 1f, 1f, 1f, 0f, -height * 2f, width * 2f, 0f);

            var targetTextPanel = UIUtility.CreatePanel("TargetTextPanel", mainPanel.transform);
            targetTextPanel.transform.SetRect(0.1f, 0.68f, 0.9f, 0.92f);
            targetText = UIUtility.CreateText("TargetText", targetTextPanel.transform, "None");
            targetText.transform.SetRect(0.01f, 0.01f, 0.99f, 0.99f);
            targetText.alignment = TextAnchor.MiddleCenter;

            var button = UIUtility.CreateButton("RetargetButton", mainPanel.transform, "Apply");
            button.transform.SetRect(0.1f, 0.38f, 0.9f, 0.62f);
            button.onClick.AddListener(() => SetTargetsForSelected(targetText));

            var speedTextPanel = UIUtility.CreatePanel("SpeedTextPanel", mainPanel.transform);
            speedTextPanel.transform.SetRect(0.1f, 0.08f, 0.6f, 0.32f);
            var speedText = UIUtility.CreateText("SpeedText", speedTextPanel.transform, "Speed");
            speedText.transform.SetRect(0.05f, 0.01f, 0.95f, 0.99f);
            speedText.alignment = TextAnchor.MiddleCenter;

            speedField = UIUtility.CreateInputField("RotationSpeedInput", mainPanel.transform, "");
            speedField.transform.SetRect(0.6f, 0.08f, 0.9f, 0.32f);
            speedField.text = "1";
            speedField.textComponent.alignment = TextAnchor.MiddleCenter;
            speedField.onEndEdit.AddListener((input) => UpdateSelectedTrackers(input));
        }

        void UpdateSelectedTrackers(string input)
        {
            float parsedSpeed;
            if(!float.TryParse(input, out parsedSpeed))
            {
                parsedSpeed = 1f;
                speedField.text = "1";
            }

            foreach(var objectCtrl in Studio.Studio.Instance.treeNodeCtrl.selectObjectCtrl)
            {
                if(objectCtrl is OCILight)
                {
                    var tracker = (objectCtrl as OCILight).light.gameObject.GetComponent<TrackTransform>();
                    if(tracker)
                    {
                        tracker.rotationSpeed = parsedSpeed;
                    }
                }
            }
        }

        void SetTargetsForSelected(Text targetText)
        {
            var lightlist = new List<OCILight>();
            var charalist = new List<OCIChar>();

            foreach(var objectCtrl in Studio.Studio.Instance.treeNodeCtrl.selectObjectCtrl)
            {
                if(objectCtrl is OCILight)
                {
                    var light = objectCtrl as OCILight;
                    if(light.lightType == LightType.Spot)
                    {
                        lightlist.Add(light);
                    }
                }
                else if(objectCtrl is OCIChar)
                {
                    charalist.Add(objectCtrl as OCIChar);
                }
            }
            
            if(charalist.Count > 0)
            {
                targetText.text = charalist[0].charInfo.customInfo.name;
                string prefix = charalist[0].charInfo is CharFemale ? "cf" : "cm";
                var targetTransform = charalist[0].charBody.transform.FindLoop(prefix + "_J_Mune00").transform;

                float parsedSpeed;
                if(!float.TryParse(speedField.text, out parsedSpeed))
                {
                    parsedSpeed = 1f;
                }

                foreach(var ocilight in lightlist)
                {
                    var tracker = ocilight.light.gameObject.GetComponent<TrackTransform>();
                    if(!tracker) tracker = ocilight.light.gameObject.AddComponent<TrackTransform>();
                    tracker.target = targetTransform;
                    tracker.targetKey = charalist[0].objectInfo.dicKey;
                    tracker.targetName = charalist[0].charInfo.customInfo.name;
                    tracker.rotationSpeed = parsedSpeed;
                }
            }
            else
            {
                targetText.text = "None";

                foreach(var ocilight in lightlist)
                {
                    var tracker = ocilight.light.gameObject.GetComponent<TrackTransform>();
                    if(tracker) DestroyImmediate(tracker);
                }
            }
        }
    }
}
