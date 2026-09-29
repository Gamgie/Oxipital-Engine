using UnityEngine;

namespace Oxipital
{
    // Flow stage : turns the source into a raw flow field (RG, length <= 1)
    //   SpoutFlow mode               : the flow comes from TouchDesigner, only normalized here
    //   SpoutMaskComputeInUnity mode : computed from the body mask, with the Gradient or Lucas-Kanade method
    public class OpticalFlowCompute : MonoBehaviour
    {
        public enum FlowAlgorithm { Gradient, LucasKanade }

        public FlowAlgorithm algorithm = FlowAlgorithm.Gradient;
        public bool preBlur = true;

        [Header("Gradient")]
        [Range(1, 6)]
        public float offset = 3;
        [Range(0, .1f)]
        public float lambda = .01f;
        [Range(0, 20)]
        public float gradientGain = 3;

        [Header("Lucas-Kanade")]
        // Half window size, keep the working resolution low (160x120) : no pyramid, small displacements only
        [Range(1, 8)]
        public int window = 5;
        [Range(0, 2)]
        public float lucasKanadeGain = .25f;

        [Header("TouchDesigner Flow")]
        [Range(0, 10)]
        public float spoutFlowGain = 1;

        [Header("Response")]
        [Range(0, .2f)]
        public float threshold = .05f;
        [Range(.2f, 4)]
        public float power = 1;

        [OSCQuery.DoNotExpose]
        public ComputeShader shader;

        RenderTexture lumA;
        RenderTexture lumB;
        RenderTexture lkGradients;
        RenderTexture rawFlow;

        int kFromInput, kLuminance, kLumBlur, kFlowGradient, kLKGradients, kLKFlow;

        public RenderTexture RawFlow => rawFlow;

        internal void Allocate(int w, int h)
        {
            lumA = OpticalFlowUtil.CreateRT("OpticalFlow Luminance A", w, h, OpticalFlowUtil.RGHalf);
            lumB = OpticalFlowUtil.CreateRT("OpticalFlow Luminance B", w, h, OpticalFlowUtil.RGHalf);
            lkGradients = OpticalFlowUtil.CreateRT("OpticalFlow LK Gradients", w, h, OpticalFlowUtil.RGBAHalf);
            rawFlow = OpticalFlowUtil.CreateRT("OpticalFlow Raw", w, h, OpticalFlowUtil.RGHalf);

            kFromInput = shader.FindKernel("FromInput");
            kLuminance = shader.FindKernel("Luminance");
            kLumBlur = shader.FindKernel("LumBlur");
            kFlowGradient = shader.FindKernel("FlowGradient");
            kLKGradients = shader.FindKernel("LKGradients");
            kLKFlow = shader.FindKernel("LKFlow");
        }

        internal void Release()
        {
            OpticalFlowUtil.Release(ref lumA);
            OpticalFlowUtil.Release(ref lumB);
            OpticalFlowUtil.Release(ref lkGradients);
            OpticalFlowUtil.Release(ref rawFlow);
        }

        internal void Execute(OpticalFlowSource source)
        {
            int w = source.Width;
            int h = source.Height;

            shader.SetInts(FlowIDs.Size, w, h);
            shader.SetVector(FlowIDs.TexelSize, new Vector2(1f / w, 1f / h));
            shader.SetVector(FlowIDs.FlowSign, source.FlowSign);
            shader.SetFloat(FlowIDs.Threshold, threshold);
            shader.SetFloat(FlowIDs.Power, power);

            if (source.IsFlowMode)
            {
                shader.SetFloat(FlowIDs.Gain, spoutFlowGain);
                shader.SetTexture(kFromInput, FlowIDs.Current, source.Current);
                shader.SetTexture(kFromInput, FlowIDs.FlowOut, rawFlow);
                OpticalFlowUtil.Dispatch(shader, kFromInput, w, h);
                return;
            }

            // Current and previous masks packed in one texture, then blurred together
            shader.SetTexture(kLuminance, FlowIDs.Current, source.Current);
            shader.SetTexture(kLuminance, FlowIDs.Previous, source.Previous);
            shader.SetTexture(kLuminance, FlowIDs.LumOut, lumA);
            OpticalFlowUtil.Dispatch(shader, kLuminance, w, h);

            if (preBlur)
            {
                BlurLuminance(lumA, lumB, new Vector2Int(1, 0), w, h);
                BlurLuminance(lumB, lumA, new Vector2Int(0, 1), w, h);
            }

            if (algorithm == FlowAlgorithm.Gradient)
            {
                shader.SetFloat(FlowIDs.Offset, offset);
                shader.SetFloat(FlowIDs.Lambda, lambda);
                shader.SetFloat(FlowIDs.Gain, gradientGain);
                shader.SetTexture(kFlowGradient, FlowIDs.Lum, lumA);
                shader.SetBuffer(kFlowGradient, FlowIDs.State, source.State);
                shader.SetTexture(kFlowGradient, FlowIDs.FlowOut, rawFlow);
                OpticalFlowUtil.Dispatch(shader, kFlowGradient, w, h);
            }
            else
            {
                shader.SetTexture(kLKGradients, FlowIDs.Lum, lumA);
                shader.SetTexture(kLKGradients, FlowIDs.GradOut, lkGradients);
                OpticalFlowUtil.Dispatch(shader, kLKGradients, w, h);

                shader.SetInt(FlowIDs.Window, window);
                shader.SetFloat(FlowIDs.DetEpsilon, 1e-7f);
                shader.SetFloat(FlowIDs.Gain, lucasKanadeGain);
                shader.SetTexture(kLKFlow, FlowIDs.Grad, lkGradients);
                shader.SetBuffer(kLKFlow, FlowIDs.State, source.State);
                shader.SetTexture(kLKFlow, FlowIDs.FlowOut, rawFlow);
                OpticalFlowUtil.Dispatch(shader, kLKFlow, w, h);
            }
        }

        void BlurLuminance(RenderTexture from, RenderTexture to, Vector2Int dir, int w, int h)
        {
            shader.SetInts(FlowIDs.Dir, dir.x, dir.y);
            shader.SetTexture(kLumBlur, FlowIDs.Lum, from);
            shader.SetTexture(kLumBlur, FlowIDs.LumOut, to);
            OpticalFlowUtil.Dispatch(shader, kLumBlur, w, h);
        }
    }
}
