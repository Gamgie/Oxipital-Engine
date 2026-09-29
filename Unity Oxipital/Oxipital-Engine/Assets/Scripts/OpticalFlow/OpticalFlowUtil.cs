using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Oxipital
{
    // Shared helpers for the optical flow pipeline (render textures, dispatch, shader property ids)
    static class OpticalFlowUtil
    {
        public const GraphicsFormat RGBAHalf = GraphicsFormat.R16G16B16A16_SFloat;
        public const GraphicsFormat RGHalf = GraphicsFormat.R16G16_SFloat;

        // Linear (never sRGB: we store vectors), random-writable, bilinear, clamped
        public static RenderTexture CreateRT(string name, int width, int height, GraphicsFormat format, bool mips = false)
        {
            var desc = new RenderTextureDescriptor(width, height, format, GraphicsFormat.None)
            {
                enableRandomWrite = true,
                useMipMap = mips,
                autoGenerateMips = false,
                msaaSamples = 1,
            };

            var rt = new RenderTexture(desc)
            {
                name = name,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontSave,
            };
            rt.Create();
            Clear(rt);
            return rt;
        }

        public static void Clear(RenderTexture rt)
        {
            var previous = RenderTexture.active;
            RenderTexture.active = rt;
            GL.Clear(false, true, Color.clear);
            RenderTexture.active = previous;
        }

        public static void Release(ref RenderTexture rt)
        {
            if (rt == null) return;
            rt.Release();
            if (Application.isPlaying) Object.Destroy(rt);
            else Object.DestroyImmediate(rt);
            rt = null;
        }

        public static void Release(ref GraphicsBuffer buffer)
        {
            buffer?.Release();
            buffer = null;
        }

        // All image kernels use [numthreads(8,8,1)]
        public static void Dispatch(ComputeShader cs, int kernel, int width, int height)
        {
            cs.Dispatch(kernel, (width + 7) / 8, (height + 7) / 8, 1);
        }
    }

    static class FlowIDs
    {
        // Compute shaders
        public static readonly int Size = Shader.PropertyToID("_Size");
        public static readonly int TexelSize = Shader.PropertyToID("_TexelSize");
        public static readonly int State = Shader.PropertyToID("_State");
        public static readonly int DeltaTime = Shader.PropertyToID("_DeltaTime");

        public static readonly int Input = Shader.PropertyToID("_Input");
        public static readonly int Current = Shader.PropertyToID("_Current");
        public static readonly int CurrentRead = Shader.PropertyToID("_CurrentRead");
        public static readonly int Previous = Shader.PropertyToID("_Previous");
        public static readonly int PreviousOut = Shader.PropertyToID("_PreviousOut");
        public static readonly int Crop = Shader.PropertyToID("_Crop");
        public static readonly int FlipSign = Shader.PropertyToID("_FlipSign");
        public static readonly int FlowInRG = Shader.PropertyToID("_FlowInRG");
        public static readonly int ChangeThreshold = Shader.PropertyToID("_ChangeThreshold");
        public static readonly int ForceChange = Shader.PropertyToID("_ForceChange");

        public static readonly int TestOut = Shader.PropertyToID("_TestOut");
        public static readonly int DiscCenter = Shader.PropertyToID("_DiscCenter");
        public static readonly int DiscVelocity = Shader.PropertyToID("_DiscVelocity");
        public static readonly int DiscRadius = Shader.PropertyToID("_DiscRadius");
        public static readonly int DiscSoftness = Shader.PropertyToID("_DiscSoftness");
        public static readonly int TestFlowMode = Shader.PropertyToID("_TestFlowMode");

        public static readonly int Lum = Shader.PropertyToID("_Lum");
        public static readonly int LumOut = Shader.PropertyToID("_LumOut");
        public static readonly int Grad = Shader.PropertyToID("_Grad");
        public static readonly int GradOut = Shader.PropertyToID("_GradOut");
        public static readonly int FlowOut = Shader.PropertyToID("_FlowOut");
        public static readonly int Dir = Shader.PropertyToID("_Dir");
        public static readonly int Offset = Shader.PropertyToID("_Offset");
        public static readonly int Lambda = Shader.PropertyToID("_Lambda");
        public static readonly int Threshold = Shader.PropertyToID("_Threshold");
        public static readonly int Power = Shader.PropertyToID("_Power");
        public static readonly int Gain = Shader.PropertyToID("_Gain");
        public static readonly int FlowSign = Shader.PropertyToID("_FlowSign");
        public static readonly int Window = Shader.PropertyToID("_Window");
        public static readonly int DetEpsilon = Shader.PropertyToID("_DetEpsilon");

        public static readonly int RawFlow = Shader.PropertyToID("_RawFlow");
        public static readonly int Trail = Shader.PropertyToID("_Trail");
        public static readonly int Decay = Shader.PropertyToID("_Decay");
        public static readonly int InjectGain = Shader.PropertyToID("_InjectGain");
        public static readonly int StaleTimeout = Shader.PropertyToID("_StaleTimeout");
        public static readonly int BlurIn = Shader.PropertyToID("_BlurIn");
        public static readonly int BlurOut = Shader.PropertyToID("_BlurOut");
        public static readonly int Radius = Shader.PropertyToID("_Radius");
        public static readonly int Blurred = Shader.PropertyToID("_Blurred");
        public static readonly int MaskChannel = Shader.PropertyToID("_MaskChannel");
        public static readonly int Output = Shader.PropertyToID("_Output");
        public static readonly int OutputGain = Shader.PropertyToID("_OutputGain");
        public static readonly int MaxLength = Shader.PropertyToID("_MaxLength");
        public static readonly int Feedback = Shader.PropertyToID("_Feedback");

        public static readonly int DebugIn = Shader.PropertyToID("_DebugIn");
        public static readonly int DebugOut = Shader.PropertyToID("_DebugOut");
        public static readonly int DebugMode = Shader.PropertyToID("_DebugMode");
        public static readonly int DebugChannel = Shader.PropertyToID("_DebugChannel");
        public static readonly int DebugGain = Shader.PropertyToID("_DebugGain");

        // VFX Graph exposed properties (must match the blackboard names in the graphs)
        public static readonly int VfxFlowMap = Shader.PropertyToID("Optical Flow Map");
        public static readonly int VfxWorldToPlane = Shader.PropertyToID("Optical Flow World To Plane");
        public static readonly int VfxPlaneRight = Shader.PropertyToID("Optical Flow Plane Right");
        public static readonly int VfxPlaneUp = Shader.PropertyToID("Optical Flow Plane Up");
        public static readonly int VfxParams = Shader.PropertyToID("Optical Flow Params");
    }
}
