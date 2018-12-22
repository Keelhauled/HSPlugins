using System;
using System.Collections.Generic;
using Harmony;
using UnityEngine;
using Config;
using UnityStandardAssets.CinematicEffects;
using Utility.Xml;
using System.Reflection.Emit;
using System.Linq;

namespace Harmony4KPatch
{
    public static class HarmonyPatch_ConfigEffector
    {
        [HarmonyPatch(typeof(ConfigEffector))]
        [HarmonyPatch("Refresh")]
        public static class HarmonyPatch_ConfigEffector_Refresh
        {
            static bool Prefix(ConfigEffector __instance)
            {
                if(__instance.SSAOPro && Manager.Config.EtcData.SSAO != __instance.SSAOPro.enabled)
                {
                    __instance.SSAOPro.enabled = Manager.Config.EtcData.SSAO;
                }
                if(__instance.GlobalFog && Manager.Config.EtcData.Fog != __instance.GlobalFog.enabled)
                {
                    __instance.GlobalFog.enabled = Manager.Config.EtcData.Fog;
                }
                if(__instance.BloomAndFlares && Manager.Config.EtcData.Bloom != __instance.BloomAndFlares.enabled)
                {
                    __instance.BloomAndFlares.enabled = Manager.Config.EtcData.Bloom;
                }
                if(__instance.ScreenSpaceReflection && Manager.Config.EtcData.SSR != __instance.ScreenSpaceReflection.enabled)
                {
                    __instance.ScreenSpaceReflection.enabled = Manager.Config.EtcData.SSR;
                }
                if(__instance.SunShafts && Manager.Config.EtcData.SunShafts != __instance.SunShafts.enabled)
                {
                    __instance.SunShafts.enabled = Manager.Config.EtcData.SunShafts;
                }
                if(__instance.VignetteAndChromaticAberration && Manager.Config.EtcData.Vignette != __instance.VignetteAndChromaticAberration.enabled)
                {
                    __instance.VignetteAndChromaticAberration.enabled = Manager.Config.EtcData.Vignette;
                }
                if(__instance.DepthOfField && Manager.Config.EtcData.DepthOfField != __instance.DepthOfField.enabled)
                {
                    __instance.DepthOfField.enabled = Manager.Config.EtcData.DepthOfField;
                }
                if(__instance.Antialiasing && Manager.Config.EtcData.AntiAliasing != __instance.Antialiasing.enabled)
                {
                    __instance.Antialiasing.enabled = Manager.Config.EtcData.AntiAliasing;
                }

                return false;
            }
        }

        [HarmonyPatch(typeof(ConfigEffector))]
        [HarmonyPatch("Start")]
        public static class HarmonyPatch_ConfigEffector_Start
        {
            static void Postfix(ConfigEffector __instance)
            {
                if(HarmonyPatch_Config.CurveSettings.Curve == -1)
                {
                    __instance.ColorCorrectionCurves.enabled = false;
                }
            }
        }
    }

    [HarmonyPatch(typeof(EtceteraSystem))]
    [HarmonyPatch("SetSelfShadow")]
    public static class HarmonyPatch_EtceteraSystem_SetSelfShadow
    {
        static bool Prefix(ref bool _flag, EtceteraSystem __instance)
        {
            __instance.SelfShadow = _flag;
            QualitySettings.SetQualityLevel(QualitySettings.GetQualityLevel() / 2 * 2 + ((!_flag) ? 1 : 0), false);

            if(_flag)
            {
                QualitySettings.shadowNearPlaneOffset = HarmonyPatch_Config.ShadowSettings.ShadowNearPlaneOffset;
                QualitySettings.shadowDistance = HarmonyPatch_Config.ShadowSettings.ShadowDistance;
                QualitySettings.shadowCascades = HarmonyPatch_Config.ShadowSettings.ShadowCascades;
                QualitySettings.shadowCascade2Split = HarmonyPatch_Config.ShadowSettings.ShadowCascade2Split;
                QualitySettings.shadowCascade4Split = new Vector3(HarmonyPatch_Config.ShadowSettings.ShadowCascade4Split_x, HarmonyPatch_Config.ShadowSettings.ShadowCascade4Split_y, HarmonyPatch_Config.ShadowSettings.ShadowCascade4Split_z);

                if(HarmonyPatch_Config.ShadowSettings.ShadowProjection == 0)
                {
                    QualitySettings.shadowProjection = 0;
                    return false;
                }

                if(HarmonyPatch_Config.ShadowSettings.ShadowProjection == 1)
                {
                    QualitySettings.shadowProjection = ShadowProjection.StableFit;
                }
            }

            return false;
        }
    }

    public static class HarmonyPatch_Config
    {
        public static BasicSetting GraphicSettings { get; set; }
        public static BasicSetting BasicSettings { get; set; }
        public static CurveSetting CurveSettings { get; set; }
        public static BloomSetting BloomSettings { get; set; }
        public static SSAOSetting SSAOSettings { get; set; }
        public static SSRSetting SSRSettings { get; set; }
        public static ShadowSetting ShadowSettings { get; set; }

        public static Control xmlGraphicsSetting;
        public static Control xmlSSAO;
        public static Control xmlSSR;
        public static Control xmlShadow;
        public static Control xmlStyle;

        [HarmonyPatch(typeof(Manager.Config))]
        [HarmonyPatch("Save")]
        public static class HarmonyPatch_Config_Save
        {
            static void Postfix()
            {
                xmlGraphicsSetting.Write();
            }
        }

        [HarmonyPatch(typeof(Manager.Config))]
        [HarmonyPatch("Awake")]
        public static class HarmonyPatch_Config_Awake
        {
            static bool Prefix(Manager.Config __instance)
            {
                var traverse = Traverse.Create(__instance);
                if(!traverse.Method("CheckInstance").GetValue<bool>()) return false;
                GameObject.DontDestroyOnLoad(__instance.gameObject);

                traverse.Property("SoundData").SetValue(new SoundSystem("Sound"));
                traverse.Property("TextData").SetValue(new TextSystem("Text"));
                traverse.Property("EtcData").SetValue(new EtceteraSystem("Etc"));
                traverse.Property("DebugStatus").SetValue(new DebugSystem("Debug"));
                BasicSettings = new BasicSetting("GraphicBasic");
                BloomSettings = new BloomSetting("Bloom");
                CurveSettings = new CurveSetting("ColorCorrectionCurve");
                SSAOSettings = new SSAOSetting("SSAO");
                SSRSettings = new SSRSetting("SSR");
                ShadowSettings = new ShadowSetting("Shadow");

                var xmlCtrl = new Control("Config", "Config.xml", "Config", new List<Data>{ Manager.Config.SoundData, Manager.Config.TextData, Manager.Config.EtcData, Manager.Config.DebugStatus });
                traverse.Field("xmlCtrl").SetValue(xmlCtrl);

                __instance.Load();
                xmlGraphicsSetting = new Control("GraphicSetting", "Config.xml", "GraphicSetting", new List<Data>{ BasicSettings });
                xmlGraphicsSetting.Read();

                string stylePreset = BasicSettings.StylePreset;
                string ssaopreset = BasicSettings.SSAOPreset;
                string ssrpreset = BasicSettings.SSRPreset;
                string shadowPreset = BasicSettings.ShadowPreset;

                var xmlStyle = new Control("GraphicSetting/Style", stylePreset + ".xml", "Config", new List<Data>{ BloomSettings, CurveSettings });
                traverse.Field("xmlStyle").SetValue(xmlStyle);

                xmlSSAO = new Control("GraphicSetting/SSAO", ssaopreset + ".xml", "Config", new List<Data>{ SSAOSettings });
                xmlSSR = new Control("GraphicSetting/SSR", ssrpreset + ".xml", "Config", new List<Data>{ SSRSettings });
                xmlShadow = new Control("GraphicSetting/Shadow", shadowPreset + ".xml", "Config", new List<Data>{ ShadowSettings });
                xmlShadow.Read();
                xmlSSAO.Read();
                xmlSSR.Read();
                xmlStyle.Read();

                Manager.Config.ScrollBarValue = 1f;
                if(ShadowSettings.ShadowProjection == 0)
                {
                    QualitySettings.shadowProjection = ShadowProjection.CloseFit;
                }
                else if(ShadowSettings.ShadowProjection == 1)
                {
                    QualitySettings.shadowProjection = ShadowProjection.StableFit;
                }

                QualitySettings.shadowNearPlaneOffset = ShadowSettings.ShadowNearPlaneOffset;
                QualitySettings.shadowDistance = ShadowSettings.ShadowDistance;
                QualitySettings.shadowCascades = ShadowSettings.ShadowCascades;
                QualitySettings.shadowCascade2Split = ShadowSettings.ShadowCascade2Split;
                QualitySettings.shadowCascade4Split = new Vector3(ShadowSettings.ShadowCascade4Split_x, ShadowSettings.ShadowCascade4Split_y, ShadowSettings.ShadowCascade4Split_z);
                QualitySettings.antiAliasing = BasicSettings.MSAA;
                HarmonyPatch_BloomAndFlares.BlmIntst = BloomSettings.Bloomintensity;
                HarmonyPatch_BloomAndFlares.BlmSprd = BloomSettings.BloomBlurSpread;
                HarmonyPatch_BloomAndFlares.BlmThrshd = BloomSettings.BloomThreshold;
                HarmonyPatch_ColorCorrectionCurves.CCCCustom = CurveSettings.Curve;
                HarmonyPatch_ColorCorrectionCurves.CCCName = CurveSettings.CurveName;
                HarmonyPatch_ColorCorrectionCurves.CCCSaturation = CurveSettings.CurveSaturation;
                HarmonyPatch_ScreenSpaceReflection.SSRPresets = SSRSettings.SSRpresets;
                if(HarmonyPatch_ScreenSpaceReflection.SSRPresets != 0) return false;

                HarmonyPatch_ScreenSpaceReflection.MySSRPresets.basicSettings.screenEdgeFading = SSRSettings.SSRscreenEdgeFading;
                HarmonyPatch_ScreenSpaceReflection.MySSRPresets.basicSettings.maxDistance = SSRSettings.SSRmaxDistance;
                HarmonyPatch_ScreenSpaceReflection.MySSRPresets.basicSettings.fadeDistance = SSRSettings.SSRfadeDistance;
                HarmonyPatch_ScreenSpaceReflection.MySSRPresets.basicSettings.reflectionMultiplier = SSRSettings.SSRreflectionMultiplier;
                HarmonyPatch_ScreenSpaceReflection.MySSRPresets.basicSettings.enableHDR = SSRSettings.SSRenableHDR;
                HarmonyPatch_ScreenSpaceReflection.MySSRPresets.basicSettings.additiveReflection = SSRSettings.SSRadditiveReflection;
                HarmonyPatch_ScreenSpaceReflection.MySSRPresets.reflectionSettings.maxSteps = SSRSettings.SSRmaxSteps;
                HarmonyPatch_ScreenSpaceReflection.MySSRPresets.reflectionSettings.rayStepSize = SSRSettings.SSRrayStepSize;
                HarmonyPatch_ScreenSpaceReflection.MySSRPresets.reflectionSettings.widthModifier = SSRSettings.SSRwidthModifier;
                HarmonyPatch_ScreenSpaceReflection.MySSRPresets.reflectionSettings.smoothFallbackThreshold = SSRSettings.SSRsmoothFallbackThreshold;
                HarmonyPatch_ScreenSpaceReflection.MySSRPresets.reflectionSettings.distanceBlur = SSRSettings.SSRdistanceBlur;
                HarmonyPatch_ScreenSpaceReflection.MySSRPresets.reflectionSettings.fresnelFade = SSRSettings.SSRfresnelFade;
                HarmonyPatch_ScreenSpaceReflection.MySSRPresets.reflectionSettings.fresnelFadePower = SSRSettings.SSRfresnelFadePower;
                HarmonyPatch_ScreenSpaceReflection.MySSRPresets.reflectionSettings.smoothFallbackDistance = SSRSettings.SSRsmoothFallbackDistance;
                HarmonyPatch_ScreenSpaceReflection.MySSRPresets.advancedSettings.useTemporalConfidence = SSRSettings.SSRuseTemporalConfidence;
                HarmonyPatch_ScreenSpaceReflection.MySSRPresets.advancedSettings.temporalFilterStrength = SSRSettings.SSRtemporalFilterStrength;
                HarmonyPatch_ScreenSpaceReflection.MySSRPresets.advancedSettings.treatBackfaceHitAsMiss = SSRSettings.SSRtreatBackfaceHitAsMiss;
                HarmonyPatch_ScreenSpaceReflection.MySSRPresets.advancedSettings.allowBackwardsRays = SSRSettings.SSRallowBackwardsRays;
                HarmonyPatch_ScreenSpaceReflection.MySSRPresets.advancedSettings.traceBehindObjects = SSRSettings.SSRtraceBehindObjects;
                HarmonyPatch_ScreenSpaceReflection.MySSRPresets.advancedSettings.highQualitySharpReflections = SSRSettings.SSRhighQualitySharpReflections;
                HarmonyPatch_ScreenSpaceReflection.MySSRPresets.advancedSettings.traceEverywhere = SSRSettings.SSRtraceEverywhere;
                HarmonyPatch_ScreenSpaceReflection.MySSRPresets.advancedSettings.bilateralUpsample = SSRSettings.SSRbilateralUpsample;
                HarmonyPatch_ScreenSpaceReflection.MySSRPresets.advancedSettings.improveCorners = SSRSettings.SSRimproveCorners;
                HarmonyPatch_ScreenSpaceReflection.MySSRPresets.advancedSettings.reduceBanding = SSRSettings.SSRreduceBanding;
                HarmonyPatch_ScreenSpaceReflection.MySSRPresets.advancedSettings.highlightSuppression = SSRSettings.SSRhighlightSuppression;

                switch(SSRSettings.SSRresolution)
                {
                    case 0:
                        HarmonyPatch_ScreenSpaceReflection.MySSRPresets.advancedSettings.resolution = ScreenSpaceReflection.SSRResolution.FullResolution;
                        break;
                    case 1:
                        HarmonyPatch_ScreenSpaceReflection.MySSRPresets.advancedSettings.resolution = ScreenSpaceReflection.SSRResolution.HalfTraceFullResolve;
                        break;
                    case 2:
                        HarmonyPatch_ScreenSpaceReflection.MySSRPresets.advancedSettings.resolution = ScreenSpaceReflection.SSRResolution.HalfResolution;
                        break;
                }

                switch(SSRSettings.SSRdebugMode)
                {
                    case 0:
                        HarmonyPatch_ScreenSpaceReflection.MySSRPresets.debugSettings.debugMode = ScreenSpaceReflection.SSRDebugMode.None;
                        return false;
                    case 1:
                        HarmonyPatch_ScreenSpaceReflection.MySSRPresets.debugSettings.debugMode = ScreenSpaceReflection.SSRDebugMode.IncomingRadiance;
                        return false;
                    case 2:
                        HarmonyPatch_ScreenSpaceReflection.MySSRPresets.debugSettings.debugMode = ScreenSpaceReflection.SSRDebugMode.SSRResult;
                        return false;
                    case 3:
                        HarmonyPatch_ScreenSpaceReflection.MySSRPresets.debugSettings.debugMode = ScreenSpaceReflection.SSRDebugMode.FinalGlossyTerm;
                        return false;
                    case 4:
                        HarmonyPatch_ScreenSpaceReflection.MySSRPresets.debugSettings.debugMode = ScreenSpaceReflection.SSRDebugMode.SSRMask;
                        return false;
                    case 5:
                        HarmonyPatch_ScreenSpaceReflection.MySSRPresets.debugSettings.debugMode = ScreenSpaceReflection.SSRDebugMode.Roughness;
                        return false;
                    case 6:
                        HarmonyPatch_ScreenSpaceReflection.MySSRPresets.debugSettings.debugMode = ScreenSpaceReflection.SSRDebugMode.BaseColor;
                        return false;
                    case 7:
                        HarmonyPatch_ScreenSpaceReflection.MySSRPresets.debugSettings.debugMode = ScreenSpaceReflection.SSRDebugMode.SpecColor;
                        return false;
                    case 8:
                        HarmonyPatch_ScreenSpaceReflection.MySSRPresets.debugSettings.debugMode = ScreenSpaceReflection.SSRDebugMode.Reflectivity;
                        return false;
                    case 9:
                        HarmonyPatch_ScreenSpaceReflection.MySSRPresets.debugSettings.debugMode = ScreenSpaceReflection.SSRDebugMode.ReflectionProbeOnly;
                        return false;
                    case 10:
                        HarmonyPatch_ScreenSpaceReflection.MySSRPresets.debugSettings.debugMode = ScreenSpaceReflection.SSRDebugMode.ReflectionProbeMinusSSR;
                        return false;
                    case 11:
                        HarmonyPatch_ScreenSpaceReflection.MySSRPresets.debugSettings.debugMode = ScreenSpaceReflection.SSRDebugMode.SSRMinusReflectionProbe;
                        return false;
                    case 12:
                        HarmonyPatch_ScreenSpaceReflection.MySSRPresets.debugSettings.debugMode = ScreenSpaceReflection.SSRDebugMode.NoGlossy;
                        return false;
                    case 13:
                        HarmonyPatch_ScreenSpaceReflection.MySSRPresets.debugSettings.debugMode = ScreenSpaceReflection.SSRDebugMode.NegativeNoGlossy;
                        return false;
                    case 14:
                        HarmonyPatch_ScreenSpaceReflection.MySSRPresets.debugSettings.debugMode = ScreenSpaceReflection.SSRDebugMode.MipLevel;
                        return false;
                    default:
                        return false;
                }
            }
        }
    }

    public static class HarmonyPatch_CharBody
    {
        [HarmonyPatch(typeof(CharBody))]
        [HarmonyPatch("InitBaseCustomTextureBody")]
        public static class HarmonyPatch_CharBody_InitBaseCustomTextureBody
        {
            static bool Prefix(ref byte _sex, CharBody __instance, ref bool __result)
            {
                if(__instance.customTexCtrlBody != null)
                {
                    __instance.customTexCtrlBody.Release();
                    Traverse.Create(__instance).Property("customTexCtrlBody").SetValue(null);
                }

                string text = (_sex != 0) ? "chara/cf_m_base.unity3d" : "chara/cm_m_base.unity3d";
                string drawMatName = (_sex != 0) ? "cf_m_body" : "cm_m_body";
                string createMatName = (_sex != 0) ? "cf_m_body_create" : "cm_m_body_create";
                Traverse.Create(__instance).Property("customTexCtrlBody").SetValue(new CustomTextureControl());
                int num = HarmonyPatch_Config.BasicSettings.enable4KSkinDiffuse ? 4096 : 1024;
                __instance.customTexCtrlBody.Initialize(text, drawMatName, text, createMatName, num, num);

                __result = true;
                return false;
            }
        }

        [HarmonyPatch(typeof(CharBody))]
        [HarmonyPatch("InitBaseCustomTextureFace")]
        public static class HarmonyPatch_CharBody_InitBaseCustomTextureFace
        {
            static bool Prefix(ref byte _sex, ref string drawAssetBundleName, ref string drawAssetName, CharBody __instance, ref bool __result)
            {
                if(__instance.customTexCtrlFace != null)
                {
                    __instance.customTexCtrlFace.Release();
                    Traverse.Create(__instance).Property("customTexCtrlFace").SetValue(null);
                }

                string createMatABName = (_sex != 0) ? "chara/cf_m_base.unity3d" : "chara/cm_m_base.unity3d";
                string createMatName = (_sex != 0) ? "cf_m_face_create" : "cm_m_face_create";
                Traverse.Create(__instance).Property("customTexCtrlFace").SetValue(new CustomTextureControl());
                int num = HarmonyPatch_Config.BasicSettings.enable4KSkinDiffuse ? 4096 : 1024;
                __instance.customTexCtrlFace.Initialize(drawAssetBundleName, drawAssetName, createMatABName, createMatName, num, num);
                __result = true;

                return false;
            }
        }
    }

    public static class HarmonyPatch_CustomTextureControl
    {
        [HarmonyPatch(typeof(CustomTextureControl))]
        [HarmonyPatch("Initialize")]
        public static class HarmonyPatch_CustomTextureControl_Initialize
        {
            static void Postfix(CustomTextureControl __instance)
            {
                if(HarmonyPatch_Config.BasicSettings.enableSkinDiffuseMipMap)
                {
                    var createTex = Traverse.Create(__instance).Field("createTex");
                    createTex.Property("filterMode").SetValue(FilterMode.Trilinear);
                    createTex.Property("useMipMap").SetValue(true);
                }
            }
        }

        [HarmonyPatch(typeof(CustomTextureControl))]
        [HarmonyPatch("SetOffsetAndTiling")]
        [HarmonyPatch(new Type[]{ typeof(string), typeof(int), typeof(int), typeof(int), typeof(int), typeof(float), typeof(float) })]
        public static class HarmonyPatch_CustomTextureControl_SetOffsetAndTiling
        {
            static bool Prefix(ref string propertyName, ref int baseW, ref int baseH, ref int addW, ref int addH, ref float addPx, ref float addPy, CustomTextureControl __instance)
            {
                if(!__instance.InitEnd) return false;

                if(addPx >= 0f & addPy >= 0f & (float)addW + addPx <= 1024f & (float)addH + addPy <= 1024f)
                {
                    baseW = 1024;
                    baseH = 1024;
                }
                else
                {
                    addPx = Mathf.Abs(addPx);
                    addPy = Mathf.Abs(addPy);
                    baseH = 4096;
                    baseW = 4096;
                }

                float num = (float)baseW / (float)addW;
                float num2 = (float)baseH / (float)addH;
                float ox = -(addPx / (float)baseW) * num;
                float oy = -(((float)baseH - addPy - (float)addH) / (float)baseH) * num2;
                __instance.SetOffsetAndTiling(propertyName, num, num2, ox, oy);

                return false;
            }
        }
    }

    //[HarmonyPatch(typeof(HMapInfoCtrl))]
    //[HarmonyPatch("LoadMapInfo")]
    public static class HarmonyPatch_HMapInfoCtrl_LoadMapInfo
    {
        static IEnumerable<CodeInstruction> Transpiler(ILGenerator ilGenerator, IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            var configEffector = AccessTools.Field(typeof(HMapInfoCtrl), "configEffector");

            for(int i = 0; i < codes.Count; i++)
            {
                //Console.WriteLine(codes[i]);

                if(codes[i].opcode == OpCodes.Ldfld && codes[i].operand == configEffector)
                {
                    List<CodeInstruction> newCodes = new List<CodeInstruction>()
                    {
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(HMapInfoCtrl), "lightBack")),
                        new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HarmonyPatch_Config), "get_BasicSettings")),
                        new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(BasicSetting), "DirectionalBackLightIntensity")),
                        new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Light), "set_intensity"))
                    };

                    //Console.WriteLine("HMapInfoCtrl code inserted at {0} - {1}", i, 1);
                    codes.InsertRange(i - 1, newCodes);
                    break;
                }
            }

            return codes.AsEnumerable();
        }
    }

    [HarmonyPatch(typeof(LightMapDataObject))]
    [HarmonyPatch("Change")]
    public static class HarmonyPatch_LightMapDataObject_Change
    {
        static bool Prefix(LightMapDataObject __instance)
        {
            LightmapData[] array = new LightmapData[__instance.light.Length];
            for(int i = 0; i < array.Length; i++)
            {
                array[i] = new LightmapData
                {
                    lightmapNear = __instance.dir[i],
                    lightmapFar = __instance.light[i]
                };
            }

            LightmapSettings.lightmaps = array;
            LightmapSettings.lightProbes = __instance.lightProbes;
            LightmapSettings.lightmapsMode = __instance.lightmapsMode;
            RenderSettings.reflectionBounces = HarmonyPatch_Config.BasicSettings.ReflectionBounces;
            RenderSettings.reflectionIntensity = HarmonyPatch_Config.BasicSettings.ReflectionIntensity;

            if(__instance.cubemap != null)
            {
                RenderSettings.customReflection = __instance.cubemap;
                RenderSettings.defaultReflectionMode = UnityEngine.Rendering.DefaultReflectionMode.Custom;
                RenderSettings.customReflection.mipMapBias = HarmonyPatch_Config.BasicSettings.ReflectionCubemapMipmapBias;
            }
            else
            {
                RenderSettings.defaultReflectionMode = UnityEngine.Rendering.DefaultReflectionMode.Skybox;
            }
            
            __instance.fog?.Change();

            return false;
        }
    }

    public static class HarmonyPatch_SSAOPro
    {
        [HarmonyPatch(typeof(SSAOPro))]
        [HarmonyPatch("OnRenderImage")]
        public static class HarmonyPatch_SSAOPro_OnRenderImage
        {
            static bool Prefix(ref RenderTexture source, ref RenderTexture destination, SSAOPro __instance)
            {
                if(__instance.ShaderSSAO == null)
                {
                    Graphics.Blit(source, destination);
                    return false;
                }

                var m_Camera = Traverse.Create(__instance).Field("m_Camera");
                __instance.Material.SetMatrix("_InverseViewProject", (m_Camera.Property("projectionMatrix").GetValue<Matrix4x4>() * m_Camera.Property("worldToCameraMatrix").GetValue<Matrix4x4>()).inverse);
                __instance.Material.SetMatrix("_CameraModelView", m_Camera.Property("cameraToWorldMatrix").GetValue<Matrix4x4>());
                __instance.Material.SetTexture("_NoiseTex", __instance.NoiseTexture);
                __instance.Material.SetVector("_Params1", new Vector4((!(__instance.NoiseTexture == null)) ? ((float)__instance.NoiseTexture.width) : 0f, __instance.Radius * HarmonyPatch_Config.SSAOSettings.SSAORadius, __instance.Intensity * HarmonyPatch_Config.SSAOSettings.SSAOIntensity, __instance.Distance * HarmonyPatch_Config.SSAOSettings.SSAODistance));
                __instance.Material.SetVector("_Params2", new Vector4(__instance.Bias * HarmonyPatch_Config.SSAOSettings.SSAOBias, __instance.LumContribution * HarmonyPatch_Config.SSAOSettings.SSAOLumContribution, HarmonyPatch_Config.SSAOSettings.SSAOCutoffDistance, HarmonyPatch_Config.SSAOSettings.SSAOfalloffDistance));
                __instance.Material.SetColor("_OcclusionColor", __instance.OcclusionColor);

                int num = Traverse.Create(__instance).Method("SetShaderStates").GetValue<int>();
                if(__instance.Blur != SSAOPro.BlurMode.None)
                {
                    int num2 = 7;
                    RenderTexture temporary = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGBHalf);
                    RenderTexture temporary2 = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGBHalf);
                    Graphics.Blit(temporary, temporary, __instance.Material, 0);
                    Graphics.Blit(source, temporary, __instance.Material, num);
                    __instance.Material.SetFloat("_BilateralThreshold", __instance.BlurBilateralThreshold * HarmonyPatch_Config.SSAOSettings.SSAOBlurBilateralThreshold);

                    for(int i = 0; i < HarmonyPatch_Config.SSAOSettings.SSAOBlurPasses; i++)
                    {
                        __instance.Material.SetVector("_Direction", new Vector2(1f / (float)source.width, 0f));
                        Graphics.Blit(temporary, temporary2, __instance.Material, num2);
                        __instance.Material.SetVector("_Direction", new Vector2(0f, 1f / (float)source.height));
                        Graphics.Blit(temporary2, temporary, __instance.Material, num2);
                    }

                    if(!__instance.DebugAO)
                    {
                        __instance.Material.SetTexture("_SSAOTex", temporary);
                        Graphics.Blit(source, destination, __instance.Material, 8);
                    }
                    else
                    {
                        Graphics.Blit(temporary, destination);
                    }

                    RenderTexture.ReleaseTemporary(temporary);
                    RenderTexture.ReleaseTemporary(temporary2);

                    return false;
                }

                RenderTexture temporary3 = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGBHalf);
                Graphics.Blit(temporary3, temporary3, __instance.Material, 0);

                if(__instance.DebugAO)
                {
                    Graphics.Blit(source, temporary3, __instance.Material, num);
                    Graphics.Blit(temporary3, destination);
                    RenderTexture.ReleaseTemporary(temporary3);
                    return false;
                }

                Graphics.Blit(source, temporary3, __instance.Material, num);
                __instance.Material.SetTexture("_SSAOTex", temporary3);
                Graphics.Blit(source, destination, __instance.Material, 8);
                RenderTexture.ReleaseTemporary(temporary3);

                return false;
            }
        }

        [HarmonyPatch(typeof(SSAOPro))]
        [HarmonyPatch("SetShaderStates")]
        public static class HarmonyPatch_SSAOPro_SetShaderStates
        {
            static bool Prefix(SSAOPro __instance, ref int __result)
            {
                var depthTextureMode = Traverse.Create(__instance).Field("m_Camera").Property("depthTextureMode");
                depthTextureMode.SetValue(depthTextureMode.GetValue<DepthTextureMode>() | DepthTextureMode.Depth | DepthTextureMode.DepthNormals);

                var keywords = new string[]{ HarmonyPatch_Config.SSAOSettings.SSAOSamples, "HIGH_PRECISION_DEPTHMAP_OFF" };
                Traverse.Create(__instance).Field("keywords").SetValue(keywords);
                __instance.Material.shaderKeywords = keywords;

                int num = 0;
                if(__instance.NoiseTexture != null) num = 1;
                if(__instance.LumContribution >= 0.001f) num += 2;
                __result = 1 + num;

                return false;
            }
        }
    }
}
