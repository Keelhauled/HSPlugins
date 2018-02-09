using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Manager;
using Studio;
using IllusionUtility.GetUtility;
using UILib;
using UnityEngine.UI;

namespace NeckLookMod
{
    class NeckLookMod : MonoBehaviour
    {
        Color dragColor = new Color(0.4f, 0.4f, 0.4f, 1f);
        Color backgroundColor = new Color(1f, 1f, 1f, 1f);
        Color outlineColor = new Color(0f, 0f, 0f, 1f);
        
        //float marginSize = 5f;
        float headerSize = 20f;
        float UIScale = 1.0f;
        //float scrollOffsetX = -15f;
        //float windowMargin = 130f;

        Canvas UISystem;
        CharInfo target = null;
        bool autoTarget = false;

        void Start()
        {
            UIUtility.Init();
            MakeUI();
            StartCoroutine(ChangeTarget());
        }

        void OnDestroy()
        {
            DestroyImmediate(UISystem.gameObject);
        }

        IEnumerator ChangeTarget()
        {
            while(true)
            {
                if(autoTarget)
                {
                    var activeChara = GetActiveChara();
                    if(activeChara != null)
                    {
                        var newtarget = GetClosestChara(activeChara.charInfo.transform.position, true);
                        if(target != newtarget)
                        {
                            target = newtarget;
                            SetTarget(target, "_J_NoseBridge_t");
                        }
                    }
                }

                yield return new WaitForSeconds(1f); 
            }
        }

        void MakeUI()
        {
            float width = 100f;
            float height = 150f;

            UISystem = UIUtility.CreateNewUISystem("NeckLookModCanvas");
            UISystem.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920f / UIScale, 1080f / UIScale);
            UISystem.gameObject.SetActive(false);

            var mainPanel = UIUtility.CreatePanel("Panel", UISystem.transform);
            mainPanel.color = backgroundColor;
            mainPanel.transform.SetRect(0.9f, 0.15f, 0.9f, 0.15f, -width, -height, width, height);
            UIUtility.AddOutlineToObject(mainPanel.transform, outlineColor);

            var drag = UIUtility.CreatePanel("Draggable", mainPanel.transform);
            drag.transform.SetRect(0f, 1f, 1f, 1f, 0f, -headerSize);
            drag.color = dragColor;
            UIUtility.MakeObjectDraggable(drag.rectTransform, mainPanel.rectTransform);

            var nametext = UIUtility.CreateText("Nametext", drag.transform, "NeckLookMod");
            nametext.transform.SetRect(0f, 0f, 1f, 1f);
            nametext.alignment = TextAnchor.MiddleCenter;

            var close = UIUtility.CreateButton("CloseButton", drag.transform, "");
            close.transform.SetRect(1f, 0f, 1f, 1f, -10f * 2f);
            close.onClick.AddListener(() => UISystem.gameObject.SetActive(false));
            AddCloseSymbol(close);

            var choose = UIUtility.CreateButton("TargetButton", mainPanel.transform, "NoTarget");
            choose.transform.SetRect(0.2f, 0.83f, 0.8f, 0.93f);
            choose.onClick.AddListener(() =>
            {
                //target = GetClosestChara(Studio.Studio.Instance.cameraCtrl.targetPos);
                target = GetActiveChara().charInfo;
                choose.GetComponentInChildren<Text>().text = target.customInfo.name;
            });

            {
                var head = UIUtility.CreateButton("HeadButton", mainPanel.transform, "Eyes");
                head.transform.SetRect(0.3f, 0.7f, 0.7f, 0.8f);
                head.onClick.AddListener(() => SetTarget(target, "_J_NoseBridge_t"));

                var chest = UIUtility.CreateButton("ChestButton", mainPanel.transform, "Chest");
                chest.transform.SetRect(0.3f, 0.6f, 0.7f, 0.7f);
                chest.onClick.AddListener(() => SetTarget(target, "_J_Mune00"));

                var crotch = UIUtility.CreateButton("CrotchButton", mainPanel.transform, "Crotch");
                crotch.transform.SetRect(0.3f, 0.5f, 0.7f, 0.6f);
                crotch.onClick.AddListener(() => SetTarget(target, "_J_Kokan"));
            }
        }

        void SetTarget(CharInfo target, string name)
        {
            if(target)
            {
                var activeChara = GetActiveChara();
                if(activeChara != null)
                {
                    string prefix = target is CharFemale ? "cf" : "cm";
                    var targetbone = target.chaBody.objBone.transform.FindLoop(prefix + name).transform;
                    activeChara.charBody.neckLookCtrl.target = targetbone;
                    activeChara.charBody.eyeLookCtrl.target = targetbone;
                    SetEyeLook(activeChara.charBody, EYE_LOOK_TYPE.TARGET);
                    SetNeckLook(activeChara.charBody, NECK_LOOK_TYPE_VER2.TARGET);
                }
            }
        }

        private void SetEyeLook(CharBody currentBody, EYE_LOOK_TYPE eyetype)
        {
            for(int i = 0; i < currentBody.eyeLookCtrl.eyeLookScript.eyeTypeStates.Length; i++)
            {
                if(currentBody.eyeLookCtrl.eyeLookScript.eyeTypeStates[i].lookType == eyetype)
                {
                    currentBody.eyeLookCtrl.ptnNo = i;
                    return;
                }
            }
        }

        private void SetNeckLook(CharBody currentBody, NECK_LOOK_TYPE_VER2 necktype)
        {
            for(int i = 0; i < currentBody.neckLookCtrl.neckLookScript.neckTypeStates.Length; i++)
            {
                if(currentBody.neckLookCtrl.neckLookScript.neckTypeStates[i].lookType == necktype)
                {
                    currentBody.neckLookCtrl.ptnNo = i;
                    return;
                }
            }
        }

        CharInfo GetClosestChara(Vector3 targetPos, bool notSelf = false)
        {
            var females = Character.Instance.dictFemale.Values.Where(x => x.animBody != null).Select(x => x as CharInfo);
            var males = Character.Instance.dictMale.Values.Where(x => x.animBody != null).Select(x => x as CharInfo);
            var characters = females.Concat(males);
            if(notSelf) characters = characters.Where(x => x != GetActiveChara().charInfo);

            CharInfo closestChara = null;
            float smallestMagnitude = 0f;
            foreach(var chara in characters)
            {
                var targets = new List<string>()
                {
                    "_J_FaceUp_tz",
                    "_J_Mune00",
                    "_J_Spine01",
                    "_J_Kokan",
                };

                string prefix = chara is CharFemale ? "cf" : "cm";
                float magnitude = 0f;
                foreach(var item in targets)
                {
                    var distance = Vector3.Distance(targetPos, chara.chaBody.objBone.transform.FindLoop(prefix + item).transform.position);
                    magnitude += distance;
                }

                if(closestChara == null)
                {
                    closestChara = chara;
                    smallestMagnitude = magnitude;
                }
                else
                {
                    if(magnitude < smallestMagnitude)
                    {
                        closestChara = chara;
                        smallestMagnitude = magnitude;
                    }
                }
            }

            return closestChara;
        }

        OCIChar GetActiveChara()
        {
            if(Studio.Studio.Instance.treeNodeCtrl.selectNode)
            {
                ObjectCtrlInfo objectCtrlInfo = null;
                if(Studio.Studio.Instance.dicInfo.TryGetValue(Studio.Studio.Instance.treeNodeCtrl.selectNode, out objectCtrlInfo))
                {
                    if(objectCtrlInfo.kind == 0)
                    {
                        Console.WriteLine((objectCtrlInfo as OCIChar).charInfo.customInfo.name);
                        return objectCtrlInfo as OCIChar;
                    }
                }
            }

            return null;
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
    }
}
