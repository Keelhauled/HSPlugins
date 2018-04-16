namespace Config
{
    public class BasicSetting : BaseSystem
    {
        public int MSAA;
        public float DirectionalBackLightIntensity;
        public float CameraFOV;
        public float ReflectionCubemapMipmapBias;
        public float ReflectionIntensity;
        public int ReflectionBounces;
        public bool enable4KSkinDiffuse;
        public bool enableSkinDiffuseMipMap;
        public string StylePreset;
        public string SSAOPreset;
        public string SSRPreset;
        public string ShadowPreset;

        public BasicSetting(string elementName) : base(elementName)
        {
        }

        public override void Init()
        {
            enable4KSkinDiffuse = true;
            enableSkinDiffuseMipMap = true;
            MSAA = 8;
            ReflectionCubemapMipmapBias = 0f;
            ReflectionIntensity = 0.45f;
            ReflectionBounces = 1;
            DirectionalBackLightIntensity = 1f;
            CameraFOV = 35f;
            StylePreset = "Preset_2";
            ShadowPreset = "Default";
            SSAOPreset = "High";
            SSRPreset = "Performance";
        }
    }

    public class BloomSetting : BaseSystem
    {
        public float Bloomintensity;
        public float BloomThreshold;
        public float BloomBlurSpread;

        public BloomSetting(string elementName) : base(elementName)
        {
        }

        public override void Init()
        {
            Bloomintensity = 0.42f;
            BloomThreshold = 0.33f;
            BloomBlurSpread = 0.8f;
        }
    }

    public class CurveSetting : BaseSystem
    {
        public int Curve;
        public string CurveName;
        public float CurveSaturation;

        public CurveSetting(string elementName) : base(elementName)
        {
        }

        public override void Init()
        {
            Curve = 1;
            CurveName = "SampleCurve2";
            CurveSaturation = 1f;
        }
    }

    public class SSAOSetting : BaseSystem
    {
        public string SSAOSamples;
        public float SSAOBias;
        public float SSAOIntensity;
        public float SSAORadius;
        public float SSAOLumContribution;
        public float SSAODistance;
        public float SSAOCutoffDistance;
        public float SSAOfalloffDistance;
        public int SSAOBlurPasses;
        public float SSAOBlurBilateralThreshold;

        public SSAOSetting(string elementName) : base(elementName)
        {
        }

        public override void Init()
        {
            SSAOBias = 1.2f;
            SSAOBlurBilateralThreshold = 6E-05f;
            SSAOBlurPasses = 3;
            SSAOCutoffDistance = 70f;
            SSAOfalloffDistance = 20f;
            SSAOLumContribution = 2f;
            SSAODistance = 1.6f;
            SSAORadius = 0.7f;
            SSAOSamples = "SAMPLES_ULTRA";
            SSAOIntensity = 1f;
        }
    }

    public class SSRSetting : BaseSystem
    {
        public int SSRpresets;
        public float SSRscreenEdgeFading;
        public float SSRmaxDistance;
        public float SSRfadeDistance;
        public float SSRreflectionMultiplier;
        public bool SSRenableHDR;
        public bool SSRadditiveReflection;
        public int SSRmaxSteps;
        public int SSRrayStepSize;
        public float SSRwidthModifier;
        public float SSRsmoothFallbackThreshold;
        public float SSRdistanceBlur;
        public float SSRfresnelFade;
        public float SSRfresnelFadePower;
        public float SSRsmoothFallbackDistance;
        public bool SSRuseTemporalConfidence;
        public float SSRtemporalFilterStrength;
        public bool SSRtreatBackfaceHitAsMiss;
        public bool SSRallowBackwardsRays;
        public bool SSRtraceBehindObjects;
        public bool SSRhighQualitySharpReflections;
        public bool SSRtraceEverywhere;
        public int SSRresolution;
        public bool SSRbilateralUpsample;
        public bool SSRimproveCorners;
        public bool SSRreduceBanding;
        public bool SSRhighlightSuppression;
        public int SSRdebugMode;

        public SSRSetting(string elementName) : base(elementName)
        {
        }

        public override void Init()
        {
            SSRpresets = 0;
            SSRscreenEdgeFading = 0.02f;
            SSRmaxDistance = 50f;
            SSRfadeDistance = 50f;
            SSRreflectionMultiplier = 1f;
            SSRenableHDR = true;
            SSRadditiveReflection = false;
            SSRmaxSteps = 144;
            SSRrayStepSize = 3;
            SSRwidthModifier = 0.02f;
            SSRsmoothFallbackThreshold = 0.4f;
            SSRdistanceBlur = 1f;
            SSRfresnelFade = 0.1f;
            SSRfresnelFadePower = 0.5f;
            SSRsmoothFallbackDistance = 0.2f;
            SSRuseTemporalConfidence = true;
            SSRtemporalFilterStrength = 0.05f;
            SSRtreatBackfaceHitAsMiss = false;
            SSRallowBackwardsRays = false;
            SSRtraceBehindObjects = true;
            SSRhighQualitySharpReflections = true;
            SSRtraceEverywhere = false;
            SSRresolution = 2;
            SSRbilateralUpsample = true;
            SSRimproveCorners = false;
            SSRreduceBanding = true;
            SSRhighlightSuppression = true;
            SSRdebugMode = 0;
        }
    }

    public class ShadowSetting : BaseSystem
    {
        public float ShadowDistance;
        public int ShadowProjection;
        public int ShadowCascades;
        public float ShadowCascade2Split;
        public float ShadowCascade4Split_x;
        public float ShadowCascade4Split_y;
        public float ShadowCascade4Split_z;
        public float ShadowNearPlaneOffset;

        public ShadowSetting(string elementName) : base(elementName)
        {
        }

        public override void Init()
        {
            ShadowDistance = 20f;
            ShadowProjection = 0;
            ShadowCascades = 4;
            ShadowCascade2Split = 0.3333333f;
            ShadowCascade4Split_x = 0.06666667f;
            ShadowCascade4Split_y = 0.2f;
            ShadowCascade4Split_z = 0.4666667f;
            ShadowNearPlaneOffset = 2f;
        }
    }
}
