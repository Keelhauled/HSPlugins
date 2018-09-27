using System;
using System.Linq;
using UnityEngine;
using Manager;
using IllusionUtility.GetUtility;
using System.Collections.Generic;

namespace TogglePOV
{
    internal class HSceneMono : BaseMono
    {
        private CameraControl_Ver2 camera => Singleton<CameraControl_Ver2>.Instance;
        private Character charaManager => Character.Instance;

        List<string> targets = new List<string>()
        {
            "_J_FaceUp_tz",
            "_J_Mune00",
            "_J_Spine01",
            "_J_Kokan",
        };

        protected override bool CameraEnabled
        {
            get { return camera.enabled; }
            set { camera.enabled = value; }
        }

        protected override Vector3 CameraTargetPos
        {
            get { return camera.TargetPos; }
        }

        protected override bool DepthOfField
        {
            get { return Manager.Config.EtcData.DepthOfField; }
            set { Manager.Config.EtcData.DepthOfField = value; }
        }

        protected override bool Shield
        {
            get { return Manager.Config.EtcData.Shield; }
            set { Manager.Config.EtcData.Shield = value; }
        }

        protected override bool CameraStopMoving()
        {
            var noCtrlCondition = camera.NoCtrlCondition;
            bool result = false;
            if(noCtrlCondition != null)
            {
                result = noCtrlCondition();
            }
            return result;
        }

        protected override CharInfo GetClosestChara(Vector3 targetPos)
        {
            var females = charaManager.dictFemale.Values.Where(x => x.animBody != null).Select(x => x as CharInfo);
            var males = charaManager.dictMale.Values.Where(x => x.animBody != null).Select(x => x as CharInfo);
            var characters = females.Concat(males);

            CharInfo closestChara = null;
            float smallestMagnitude = 0f;
            foreach(var chara in characters)
            {
                string prefix = chara is CharFemale ? "cf" : "cm";
                float magnitude = 0f;
                foreach(var targetname in targets)
                {
                    var target = chara.chaBody.objBone.transform.FindLoop(prefix + targetname);
                    float distance = Vector3.Distance(targetPos, camera.transBase.InverseTransformPoint(target.transform.position));
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
    }
}
