using System;
using System.IO;
using System.Reflection;
using Harmony;
using UnityEngine;
using UnityStandardAssets.CinematicEffects;
using UnityStandardAssets.ImageEffects;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace Harmony4KPatch
{
    [HarmonyPatch(typeof(ScreenSpaceReflection))]
    [HarmonyPatch("OnEnable")]
    public static class HarmonyPatch_ScreenSpaceReflection
    {
        public static int SSRPresets;
        public static ScreenSpaceReflection.SSRSettings MySSRPresets;

        static void Postfix(ScreenSpaceReflection __instance)
        {
            if(SSRPresets == 0)
            {
                __instance.settings = MySSRPresets;
                return;
            }
            if(1 == SSRPresets)
            {
                __instance.settings = ScreenSpaceReflection.SSRSettings.performanceSettings;
                return;
            }
            if(2 == SSRPresets)
            {
                __instance.settings = ScreenSpaceReflection.SSRSettings.defaultSettings;
                return;
            }
            if(3 == SSRPresets)
            {
                __instance.settings = ScreenSpaceReflection.SSRSettings.highQualitySettings;
                return;
            }
        }
    }

    [HarmonyPatch(typeof(Antialiasing))]
    [HarmonyPatch("OnRenderImage")]
    public static class HarmonyPatch_Antialiasing
    {
        static bool Prefix(ref RenderTexture source, ref RenderTexture destination, Antialiasing __instance)
        {
            if(!__instance.CheckResources())
            {
                Graphics.Blit(source, destination);
                return false;
            }
            
            var materialFXAAPreset3 = Traverse.Create(__instance).Field("materialFXAAPreset3").GetValue<Material>();
            if(materialFXAAPreset3 != null)
            {
                Graphics.Blit(source, destination, materialFXAAPreset3);
                return false;
            }

            Graphics.Blit(source, destination);
            return false;
        }
    }

    //[HarmonyPatch(typeof(BloomAndFlares))]
    //[HarmonyPatch("OnRenderImage")]
    public static class HarmonyPatch_BloomAndFlares
    {
        public static float BlmThrshd;
        public static float BlmIntst;
        public static float BlmSprd;

        // bloom lol
    }

    public static class HarmonyPatch_ColorCorrectionCurves
    {
        public static float CCCSaturation;
        public static string CCCName;
        public static int CCCCustom;

        [HarmonyPatch(typeof(ColorCorrectionCurves))]
        [HarmonyPatch("Start")]
        public static class HarmonyPatch_ColorCorrectionCurves_Start
        {
            static void Postfix(ColorCorrectionCurves __instance)
            {
                __instance.saturation = CCCSaturation;
            }
        }

        [HarmonyPatch(typeof(ColorCorrectionCurves))]
        [HarmonyPatch("CheckResources")]
        public static class HarmonyPatch_ColorCorrectionCurves_CheckResources
        {
            static bool Prefix(ColorCorrectionCurves __instance, ref bool __result)
            {
                var traverse = Traverse.Create(__instance);

                try
                {
                    traverse.Method("CheckSupport", __instance.mode == ColorCorrectionCurves.ColorCorrectionMode.Advanced).GetValue();
                    var CheckShaderAndCreateMaterial = traverse.Method("CheckShaderAndCreateMaterial", new Type[] { typeof(Shader), typeof(Material) });
                    traverse.Field("ccMaterial").SetValue(CheckShaderAndCreateMaterial.GetValue(__instance.simpleColorCorrectionCurvesShader, traverse.Field("ccMaterial").GetValue()));
                    traverse.Field("ccDepthMaterial").SetValue(CheckShaderAndCreateMaterial.GetValue(__instance.colorCorrectionCurvesShader, traverse.Field("ccDepthMaterial").GetValue()));
                    traverse.Field("selectiveCcMaterial").SetValue(CheckShaderAndCreateMaterial.GetValue(__instance.colorCorrectionSelectiveShader, traverse.Field("selectiveCcMaterial").GetValue()));

                    if(traverse.Field("rgbChannelTex").GetValue() == null)
                    {
                        traverse.Field("rgbChannelTex").SetValue(new Texture2D(1024, 4, TextureFormat.RGBAFloat, false, true));
                    }
                    traverse.Field("rgbChannelTex").Property("hideFlags").SetValue(52);
                    traverse.Field("rgbChannelTex").Property("wrapMode").SetValue(1);

                    var isSupported = traverse.Field("isSupported").GetValue<bool>();
                    if(!isSupported)
                    {
                        traverse.Method("ReportAutoDisable").GetValue();
                    }
                    __result = isSupported;
                }
                catch(Exception x)
                {
                    Console.WriteLine(x);
                }

                return false;
            }
        }

        [HarmonyPatch(typeof(ColorCorrectionCurves))]
        [HarmonyPatch("UpdateParameters")]
        public static class HarmonyPatch_ColorCorrectionCurves_UpdateParameters
        {
            static bool Prefix(ColorCorrectionCurves __instance)
            {
                __instance.CheckResources();

                var rgbChannelTex = Traverse.Create(__instance).Field("rgbChannelTex");
                if(CCCCustom == 0 && __instance.redChannel != null && __instance.greenChannel != null && __instance.blueChannel != null)
                {
                    for(float num = 0f; num <= 1f; num += 0.0009775171f)
                    {
                        float num2 = Mathf.Clamp(__instance.redChannel.Evaluate(num), 0f, 1f);
                        float num3 = Mathf.Clamp(__instance.greenChannel.Evaluate(num), 0f, 1f);
                        float num4 = Mathf.Clamp(__instance.blueChannel.Evaluate(num), 0f, 1f);
                        rgbChannelTex.Method("SetPixel", new Type[]{ typeof(int), typeof(int), typeof(Color) }).GetValue((int)Mathf.Ceil(num * 1023f), 0, new Color(num2, num2, num2));
                        rgbChannelTex.Method("SetPixel", new Type[]{ typeof(int), typeof(int), typeof(Color) }).GetValue((int)Mathf.Ceil(num * 1023f), 1, new Color(num3, num3, num3));
                        rgbChannelTex.Method("SetPixel", new Type[]{ typeof(int), typeof(int), typeof(Color) }).GetValue((int)Mathf.Ceil(num * 1023f), 2, new Color(num4, num4, num4));
                    }

                    rgbChannelTex.Method("Apply", new Type[]{ typeof(bool) }).GetValue(false);
                    return false;
                }

                string text = Path.Combine(Application.dataPath, "../UserData/curve/" + CCCName + ".dds");
                Debug.Log("Using custom curve " + text);
                byte[] array = File.ReadAllBytes(text);
                int num5 = 128;
                byte[] array2 = new byte[array.Length - num5];
                Buffer.BlockCopy(array, num5, array2, 0, array2.Length - num5);
                rgbChannelTex.Method("LoadRawTextureData", new Type[]{ typeof(byte[]) }).GetValue(array2);
                rgbChannelTex.Method("Apply", new Type[]{ typeof(bool) }).GetValue(false);

                return false;
            }
        }

        [HarmonyPatch(typeof(ColorCorrectionCurves))]
        [HarmonyPatch("OnRenderImage")]
        public static class HarmonyPatch_ColorCorrectionCurves_OnRenderImage
        {
            static bool Prefix(ref RenderTexture source, ref RenderTexture destination, ColorCorrectionCurves __instance)
            {
                if(!__instance.CheckResources())
                {
                    Graphics.Blit(source, destination);
                    return false;
                }

                var traverse = Traverse.Create(__instance);
                var updateTexturesOnStartup = traverse.Field("updateTexturesOnStartup");
                if(traverse.Field("updateTexturesOnStartup").GetValue<bool>())
                {
                    __instance.UpdateParameters();
                    traverse.Field("updateTexturesOnStartup").SetValue(false);
                }

                RenderTexture renderTexture = destination;
                if(__instance.selectiveCc)
                {
                    renderTexture = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.DefaultHDR);
                }
                
                traverse.Field("ccMaterial").Method("SetTexture", new Type[]{ typeof(string), typeof(Texture2D) }).GetValue("_RgbTex", traverse.Field("rgbChannelTex").GetValue());
                traverse.Field("ccMaterial").Method("SetFloat", new Type[]{ typeof(string), typeof(float) }).GetValue("_Saturation", __instance.saturation);
                Graphics.Blit(source, renderTexture, traverse.Field("ccMaterial").GetValue<Material>());

                if(__instance.selectiveCc)
                {
                    var SetColor = traverse.Field("selectiveCcMaterial").Method("SetColor", new Type[]{ typeof(string), typeof(Color) });
                    SetColor.GetValue("selColor", __instance.selectiveFromColor);
                    SetColor.GetValue("targetColor", __instance.selectiveToColor);
                    Graphics.Blit(renderTexture, destination, traverse.Field("selectiveCcMaterial").GetValue<Material>());
                    RenderTexture.ReleaseTemporary(renderTexture);
                }

                return false;
            }
        }
    }

    [HarmonyPatch(typeof(UnityStandardAssets.ImageEffects.DepthOfField))]
    [HarmonyPatch("OnRenderImage")]
    public static class HarmonyPatch_DepthOfField
    {
        // add this after CheckResources check as a transpiler
        static void Prefix(UnityStandardAssets.ImageEffects.DepthOfField __instance)
        {
            __instance.highResolution = true;
        }

        //static IEnumerable<CodeInstruction> Transpiler(ILGenerator ilGenerator, IEnumerable<CodeInstruction> instructions)
        //{
        //    List<CodeInstruction> codes = new List<CodeInstruction>(instructions);

        //    for(int i = 0; i < codes.Count; i++)
        //    {
        //        if(codes[i].opcode == OpCodes.Ret)
        //        {
        //            List<CodeInstruction> newCodes = new List<CodeInstruction>()
        //            {
        //                new CodeInstruction(OpCodes.Ldarg_0),
        //                new CodeInstruction(OpCodes.Ldc_I4_1),
        //                new CodeInstruction(OpCodes.Stfld, AccessTools.Field(typeof(UnityStandardAssets.ImageEffects.DepthOfField), "highResolution"))
        //            };

        //            //Console.WriteLine("HMapInfoCtrl code inserted at {0} + {1}", i, 1);
        //            codes.InsertRange(i + 1, newCodes);
        //            break;
        //        }
        //    }

        //    return codes.AsEnumerable();
        //}
    }

    //[HarmonyPatch(typeof(SunShafts))]
    //[HarmonyPatch("OnRenderImage")]
    public static class HarmonyPatch_SunShafts
    {
        // sunshafts lol
    }

    [HarmonyPatch(typeof(VignetteAndChromaticAberration))]
    [HarmonyPatch("OnRenderImage")]
    public static class HarmonyPatch_VignetteAndChromaticAberration
    {
        // add this after CheckResources check as a transpiler
        static void Prefix(VignetteAndChromaticAberration __instance)
        {
            __instance.blur = 0f;
            __instance.blurDistance = 0f;
        }
    }
}
