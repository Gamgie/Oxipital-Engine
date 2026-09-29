using UnityEngine;

namespace Oxipital
{
    // Processing stage (the "look") : trail + separable blur, output RGBAHalf with mips
    //   RG = processed flow (length <= maxLength), B = magnitude, A = body mask
    // The last mip (1x1) is the global average, read back by OpticalFlowMetrics.
    public class OpticalFlowProcessor : MonoBehaviour
    {
        [Range(0, .99f)]
        public float trailWeight = .9f;
        [Range(0, 10)]
        public float blurRadius = 3;
        // Blur the trail itself (TouchDesigner "Feedback + Blur + x0.98" look) instead of only the output
        public bool blurInFeedback = false;
        [Range(0, 5)]
        public float outputGain = 1;
        [Range(0, 2)]
        public float maxLength = 1;

        [OSCQuery.DoNotExpose]
        public ComputeShader shader;

        RenderTexture trail;
        RenderTexture blurTemp;
        RenderTexture blurred;
        RenderTexture output;

        int kTrail, kBlur, kCompose;

        public RenderTexture Trail => trail;
        public RenderTexture Output => output;

        internal void Allocate(int w, int h)
        {
            trail = OpticalFlowUtil.CreateRT("OpticalFlow Trail", w, h, OpticalFlowUtil.RGHalf);
            blurTemp = OpticalFlowUtil.CreateRT("OpticalFlow Blur Temp", w, h, OpticalFlowUtil.RGHalf);
            blurred = OpticalFlowUtil.CreateRT("OpticalFlow Blurred", w, h, OpticalFlowUtil.RGHalf);
            output = OpticalFlowUtil.CreateRT("OpticalFlow Output", w, h, OpticalFlowUtil.RGBAHalf, true);

            kTrail = shader.FindKernel("Trail");
            kBlur = shader.FindKernel("Blur");
            kCompose = shader.FindKernel("Compose");
        }

        internal void Release()
        {
            OpticalFlowUtil.Release(ref trail);
            OpticalFlowUtil.Release(ref blurTemp);
            OpticalFlowUtil.Release(ref blurred);
            OpticalFlowUtil.Release(ref output);
        }

        internal void Execute(OpticalFlowSource source, RenderTexture rawFlow)
        {
            int w = source.Width;
            int h = source.Height;

            // Framerate independent trail. Per camera frame (30 fps) the reference formula is
            //   trail = trail * w + flow
            // Here the flow is held between camera frames and injected continuously :
            //   decay = w ^ (dt * 30), inject = (1 - decay) / (1 - w)
            // which gives exactly the reference at 30 fps, the same steady state at any framerate,
            // and no flickering when trailWeight = 0.
            float weight = Mathf.Clamp(trailWeight, 0, .99f);
            float dt = Mathf.Min(Time.unscaledDeltaTime, .1f);
            float decay = weight > 0 ? Mathf.Pow(weight, dt * 30) : 0;
            float inject = (1 - decay) / (1 - weight);

            shader.SetInts(FlowIDs.Size, w, h);
            shader.SetFloat(FlowIDs.Decay, decay);
            shader.SetFloat(FlowIDs.InjectGain, inject);
            shader.SetFloat(FlowIDs.StaleTimeout, source.staleTimeout);
            shader.SetTexture(kTrail, FlowIDs.RawFlow, rawFlow);
            shader.SetTexture(kTrail, FlowIDs.Trail, trail);
            shader.SetBuffer(kTrail, FlowIDs.State, source.State);
            OpticalFlowUtil.Dispatch(shader, kTrail, w, h);

            // Radius 0 is a plain copy, so Compose never reads and writes the trail at the same time
            shader.SetInt(FlowIDs.Radius, Mathf.RoundToInt(blurRadius));
            Blur(trail, blurTemp, new Vector2Int(1, 0), w, h);
            Blur(blurTemp, blurred, new Vector2Int(0, 1), w, h);

            shader.SetFloat(FlowIDs.OutputGain, outputGain);
            shader.SetFloat(FlowIDs.MaxLength, maxLength);
            shader.SetFloat(FlowIDs.Feedback, blurInFeedback ? 1 : 0);
            shader.SetVector(FlowIDs.MaskChannel, source.MaskChannel);
            shader.SetTexture(kCompose, FlowIDs.Blurred, blurred);
            shader.SetTexture(kCompose, FlowIDs.Current, source.Current);
            shader.SetTexture(kCompose, FlowIDs.Trail, trail);
            shader.SetTexture(kCompose, FlowIDs.Output, output);
            OpticalFlowUtil.Dispatch(shader, kCompose, w, h);

            output.GenerateMips();
        }

        void Blur(RenderTexture from, RenderTexture to, Vector2Int dir, int w, int h)
        {
            shader.SetInts(FlowIDs.Dir, dir.x, dir.y);
            shader.SetTexture(kBlur, FlowIDs.BlurIn, from);
            shader.SetTexture(kBlur, FlowIDs.BlurOut, to);
            OpticalFlowUtil.Dispatch(shader, kBlur, w, h);
        }
    }
}
