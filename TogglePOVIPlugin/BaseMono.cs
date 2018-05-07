using System;
using IllusionPlugin;
using UnityEngine;
using UnityEngine.EventSystems;
using IllusionUtility.GetUtility;

namespace TogglePOV
{
    internal abstract class BaseMono : MonoBehaviour
    {
        protected abstract bool CameraEnabled { get; set; }
        protected abstract Vector3 CameraTargetPos { get; }
        protected abstract bool DepthOfField { get; set; }
        protected abstract bool Shield { get; set; }
        protected abstract bool CameraStopMoving();
        protected abstract CharInfo GetClosestChara(Vector3 targetPos);

        private KeyCode hotkey = KeyCode.Backspace;
        private float sensitivityX = 0.5f;
        private float sensitivityY = 0.5f;
        private float DEFAULT_FOV = 70f;
        private float MAXFOV = 120f;
        private float MALE_OFFSET = 0.042f;
        private float FEMALE_OFFSET = 0.0315f;
        private bool SHOW_HAIR = false;

        private float currentfov;
        private bool currentHairState = true;
        protected CharBody currentBody;
        private float targetRotDistance;
        private float currentRotDistance;
        private Vector2 angle;
        private Vector2 rot;
        private float offset;
        private Transform neckBone;
        private Transform leftEye;
        private Transform rightEye;
        private float nearClip = 0.005f;
        private float lastFOV;
        private float lastNearClip;
        private Quaternion lastRotation;
        private Vector3 lastPosition;
        private bool lastDOF;
        private bool hideObstacle;
        protected bool povActive = false;

        protected virtual void Awake()
        {
            LoadSettings();
            gameObject.AddComponent<DragManager>();
        }

        protected virtual void Start()
        {

        }

        void OnDestroy()
        {
            if(currentBody)
            {
                Restore();
            }
        }

        protected bool LoadSettings()
        {
            if(ModPrefs.GetString("TogglePOV", "Version", TogglePOVPlugin.PLUGIN_VERSION, true) != TogglePOVPlugin.PLUGIN_VERSION)
            {
                ModPrefs.SetString("TogglePOV", "Version", TogglePOVPlugin.PLUGIN_VERSION);
                ModPrefs.SetFloat("TogglePOV", "fFOV", DEFAULT_FOV);
                ModPrefs.SetBool("TogglePOV", "bShowHair", SHOW_HAIR);
                ModPrefs.SetFloat("TogglePOV", "fMaleOffset", MALE_OFFSET);
                ModPrefs.SetFloat("TogglePOV", "fFemaleOffset", FEMALE_OFFSET);
            }
            else
            {
                currentfov = DEFAULT_FOV = Mathf.Clamp(ModPrefs.GetFloat("TogglePOV", "fFOV", DEFAULT_FOV, true), 1f, MAXFOV);
                SHOW_HAIR = ModPrefs.GetBool("TogglePOV", "bShowHair", SHOW_HAIR, true);
                MALE_OFFSET = ModPrefs.GetFloat("TogglePOV", "fMaleOffset", MALE_OFFSET, true);
                FEMALE_OFFSET = ModPrefs.GetFloat("TogglePOV", "fFemaleOffset", FEMALE_OFFSET, true);
            }

            try
            {
                string keystring = ModPrefs.GetString("TogglePOV", "POVHotkey", hotkey.ToString(), true);
                hotkey = (KeyCode)Enum.Parse(typeof(KeyCode), keystring, true);
            }
            catch(Exception)
            {
                Console.WriteLine("Using default hotkey ({0})", hotkey.ToString());
                return false;
            }

            return true;
        }

        protected virtual void Update()
        {
            if(!currentBody && povActive)
            {
                Restore();
                povActive = false;
                currentHairState = true;
                Console.WriteLine("TogglePOV reset");
            }

            if(Input.GetKeyDown(hotkey))
            {
                TogglePOV();
            }

            if(currentBody)
            {
                EyesNeckUpdate();
                UpdateCamera();
                UpdateCamPos();
            }
        }

        public void TogglePOV()
        {
            if(!currentBody)
            {
                var chara = GetClosestChara(CameraTargetPos);
                if(chara)
                {
                    Backup();
                    Apply(chara.chaBody);
                }
            }
            else
            {
                Restore();
            }
        }

        protected void ToggleHair()
        {
            SHOW_HAIR = !SHOW_HAIR;
            if(currentBody && currentHairState != SHOW_HAIR)
            {
                ShowHair(SHOW_HAIR);
            }
        }

        private void UpdateCamera()
        {
            if(leftEye == null || rightEye == null)
            {
                Restore();
                return;
            }
            
            if(!CameraEnabled)
            {
                if(Input.GetMouseButton(1))
                {
                    GameCursor.Instance.SetCursorLock(true);
                    currentfov = Mathf.Clamp(currentfov + Input.GetAxis("Mouse X") * Time.deltaTime * 30f, 1f, MAXFOV);
                }
                else if(Input.GetMouseButton(0) && DragManager.allowCamera)
                {
                    GameCursor.Instance.SetCursorLock(true);
                    float rateaddspeed = 2.5f;
                    float num = Input.GetAxis("Mouse X") * rateaddspeed;
                    float num2 = Input.GetAxis("Mouse Y") * rateaddspeed;
                    rot += new Vector2(-num2, num) * new Vector2(sensitivityX, sensitivityY).magnitude;
                }
                else
                {
                    GameCursor.Instance.SetCursorLock(false);
                }
            }

            if(Input.GetKeyDown(KeyCode.Semicolon))
            {
                currentfov = DEFAULT_FOV;
            }

            if(Input.GetKey(KeyCode.Equals))
            {
                currentfov = Mathf.Max(currentfov - Time.deltaTime * 15f, 1f);
            }
            else if(Input.GetKey(KeyCode.RightBracket))
            {
                currentfov = Mathf.Min(currentfov + Time.deltaTime * 15f, 100f);
            }

            if(Input.GetKeyDown(KeyCode.UpArrow))
            {
                offset = Mathf.Min(offset + 0.0005f, 2f);

                if(currentBody.chaInfo.Sex == 0)
                {
                    MALE_OFFSET = offset;
                }
                else
                {
                    FEMALE_OFFSET = offset;
                }
            }
            else if(Input.GetKeyDown(KeyCode.DownArrow))
            {
                offset = Mathf.Max(offset - 0.0005f, -2f);

                if(currentBody.chaInfo.Sex == 0)
                {
                    MALE_OFFSET = offset;
                }
                else
                {
                    FEMALE_OFFSET = offset;
                }
            }

            Camera.main.fieldOfView = currentfov;
            Camera.main.nearClipPlane = nearClip;
            DepthOfField = false;
            Shield = false;

            NeckLookControllerVer2 neckLookCtrl = currentBody.neckLookCtrl;
            NeckLookCalcVer2 neckLookScript = neckLookCtrl.neckLookScript;
            NeckTypeStateVer2 param = neckLookScript.neckTypeStates[neckLookCtrl.ptnNo];
            angle = new Vector2(rot.x, rot.y);
            for(int i = neckLookScript.aBones.Length - 1; i > -1; i--)
            {
                NeckObjectVer2 bone = neckLookScript.aBones[i];
                RotateToAngle(param, i, bone);
            }
        }

        private void UpdateCamPos()
        {
            Camera.main.transform.rotation = neckBone.rotation;
            Camera.main.transform.position = (leftEye.position + rightEye.position) / 2f;
            Camera.main.transform.Translate(Vector3.forward * offset);
            float y = Mathf.Clamp(angle.y, -40f, 40f);
            float x = Mathf.Clamp(angle.x, -60f, 60f);
            angle -= new Vector2(x, y);
            Camera.main.transform.rotation *= Quaternion.Euler(x, y, 0f);
            rot = new Vector2(rot.x - angle.x, rot.y - angle.y);
        }

        private void Backup()
        {
            lastFOV = Camera.main.fieldOfView;
            lastNearClip = Camera.main.nearClipPlane;
            lastRotation = Camera.main.transform.rotation;
            lastPosition = Camera.main.transform.position;
            lastDOF = DepthOfField;
            hideObstacle = Shield;
        }

        protected void Restore()
        {
            if(currentBody && !currentHairState)
            {
                ShowHair(true);
            }

            currentBody = null;

            if(Camera.main)
            {
                Camera.main.fieldOfView = lastFOV;
                Camera.main.nearClipPlane = lastNearClip;
                Camera.main.transform.rotation = lastRotation;
                Camera.main.transform.position = lastPosition;
            }
            
            CameraEnabled = true;

            DepthOfField = lastDOF;
            Shield = hideObstacle;

            povActive = false;
        }

        private void ShowHair(bool show)
        {
            currentHairState = show;
            string sex = currentBody.chaInfo.Sex == 0 ? "cm" : "cf";
            currentBody.transform.FindLoop(sex + "_J_FaceBase")?.SetActive(show);
            currentBody.transform.FindLoop(sex + "_O_mayuge")?.SetActive(show);
        }

        private void Apply(CharBody body)
        {
            CameraEnabled = false;

            if(!currentHairState)
            {
                ShowHair(true);
            }

            currentBody = body;
            FindNLR();
            angle = Vector2.zero;
            rot = Vector3.zero;
            offset = currentBody.chaInfo.Sex == 0 ? MALE_OFFSET : FEMALE_OFFSET;
            UpdateCamPos();

            if(!SHOW_HAIR)
            {
                ShowHair(false);
            }

            povActive = true;
        }

        private void RotateToAngle(NeckTypeStateVer2 param, int boneNum, NeckObjectVer2 bone)
        {
            Vector2 b = default(Vector2);
            b.x = Mathf.DeltaAngle(0f, bone.neckBone.localEulerAngles.x);
            b.y = Mathf.DeltaAngle(0f, bone.neckBone.localEulerAngles.y);
            angle += b;
            float y = Mathf.Clamp(angle.y, param.aParam[boneNum].minBendingAngle, param.aParam[boneNum].maxBendingAngle);
            float x = Mathf.Clamp(angle.x, param.aParam[boneNum].upBendingAngle, param.aParam[boneNum].downBendingAngle);
            angle -= new Vector2(x, y);
            float z = bone.neckBone.localEulerAngles.z;
            bone.neckBone.localRotation = Quaternion.Euler(x, y, z);
        }

        private void EyesNeckUpdate()
        {
            //SetEyeLook(EYE_LOOK_TYPE.FORWARD);
            SetNeckLook(NECK_LOOK_TYPE_VER2.ANIMATION);
        }

        private void SetEyeLook(EYE_LOOK_TYPE eyetype)
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

        private void SetNeckLook(NECK_LOOK_TYPE_VER2 necktype)
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

        private void FindNLR()
        {
            foreach(NeckObjectVer2 neckObjectVer in currentBody.neckLookCtrl.neckLookScript.aBones)
            {
                if(neckObjectVer.neckBone.name.ToLower().Contains("head"))
                {
                    neckBone = neckObjectVer.neckBone;
                    break;
                }
            }

            foreach(EyeObject eyeObject in currentBody.eyeLookCtrl.eyeLookScript.eyeObjs)
            {
                switch(eyeObject.eyeLR)
                {
                    case EYE_LR.EYE_L:
                    leftEye = eyeObject.eyeTransform;
                    break;
                    case EYE_LR.EYE_R:
                    rightEye = eyeObject.eyeTransform;
                    break;
                }
            }
        }
    }
}
