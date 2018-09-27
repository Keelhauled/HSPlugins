using System;
using Harmony;

namespace NeckSettings
{
    static class PHLook
    {
        [HarmonyPatch(typeof(NeckLookCalcVer2))]
        [HarmonyPatch("Init")]
        static class HarmonyPatch_NeckLookCalcVer2_Init
        {
            static void Postfix(NeckLookCalcVer2 __instance)
            {
                __instance.calcLerp = 0.8f;

                foreach(var item in __instance.neckTypeStates)
                {
                    if(item.lookType == NECK_LOOK_TYPE_VER2.TARGET)
                    {
                        item.limitBreakCorrectionValue = 89f;
                        item.leapSpeed = 3f;
                    }
                }
            }
        }
    }
}
