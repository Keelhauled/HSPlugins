using System;
using UnityEngine;
using Studio;
using System.Reflection;
using UnityEngine.Events;

namespace TogglePOV
{
    class NeoMono : BaseMono
    {
        Studio.Studio studio = Studio.Studio.Instance;
        Studio.CameraControl camera = Studio.Studio.Instance.cameraCtrl;
        TreeNodeCtrl treeNodeCtrl = Studio.Studio.Instance.treeNodeCtrl;
        UnityAction UpdateDOF = null;

        protected override void Start()
        {
            base.Start();

            try
            {
                var field = studio.systemButtonCtrl.GetType().GetField("dofInfo", BindingFlags.NonPublic | BindingFlags.Instance);
                var method = field.FieldType.GetMethod("UpdateInfo", BindingFlags.Instance | BindingFlags.Public);
                UpdateDOF = () => method.Invoke(field.GetValue(studio.systemButtonCtrl), null);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                UpdateDOF = null;
            }
        }

        protected override bool CameraEnabled
        {
            get { return camera.enabled; }
            set { camera.enabled = value; }
        }

        protected override Vector3 CameraTargetPos
        {
            get { return camera.targetPos; }
        }

        protected override bool DepthOfField
        {
            get { return studio.sceneInfo.enableDepth; }
            set
            {
                if(UpdateDOF != null)
                {
                    studio.sceneInfo.enableDepth = value;
                    UpdateDOF.Invoke(); 
                }
            }
        }

        protected override bool Shield
        {
            get { return Manager.Config.EtcData.Shield; }
            set { Manager.Config.EtcData.Shield = value; }
        }

        protected override bool CameraStopMoving()
        {
            Studio.CameraControl.NoCtrlFunc noCtrlCondition = camera.noCtrlCondition;
            bool result = false;
            if(noCtrlCondition != null)
            {
                result = noCtrlCondition();
            }
            return result;
        }

        protected override CharInfo GetClosestChara(Vector3 targetPos)
        {
            if(treeNodeCtrl.selectNode)
            {
                ObjectCtrlInfo objectCtrlInfo = null;
                if(studio.dicInfo.TryGetValue(treeNodeCtrl.selectNode, out objectCtrlInfo))
                {
                    if(objectCtrlInfo.kind == 0)
                    {
                        OCIChar ocichar = objectCtrlInfo as OCIChar;
                        return ocichar.charInfo;
                    }
                }
            }

            return null;
        }
    }
}
