using UnityEngine;

namespace Oxipital
{
    // Synthetic source to tune the pipeline without a Kinect : a soft disc following the mouse
    // (or moving on its own), refreshed at a camera-like frame rate so the new frame detection is exercised too.
    // Writes the same contract as TouchDesigner for the current OpticalFlowSource mode.
    public class OpticalFlowTestSource : MonoBehaviour
    {
        public bool followMouse = true;
        [Range(0, 2)]
        public float autoSpeed = .5f;
        [Range(.01f, .5f)]
        public float radius = .15f;
        [Range(0, 1)]
        public float softness = .5f;
        [Range(1, 120)]
        public float frameRate = 30;

        // Disc velocity of the last generated frame, in pixels per camera frame (to calibrate axes and gains)
        public Vector2 velocity;

        RenderTexture output;
        Vector2 lastCenter = new Vector2(.5f, .5f);
        float timer;
        float autoTime;
        bool hasFrame;
        int kTestDisc = -1;

        internal Texture Render(OpticalFlowSource source)
        {
            ComputeShader cs = source.shader;
            int w = source.Width;
            int h = source.Height;

            if (output == null || output.width != w || output.height != h)
            {
                OpticalFlowUtil.Release(ref output);
                output = OpticalFlowUtil.CreateRT("OpticalFlow Test Source", w, h, OpticalFlowUtil.RGBAHalf);
                kTestDisc = cs.FindKernel("TestDisc");
                hasFrame = false;
            }

            // Only produce a new image at the camera frame rate, like the real sensor
            float dt = Time.unscaledDeltaTime;
            timer += dt;
            autoTime += dt * autoSpeed;
            float period = 1f / Mathf.Max(frameRate, 1);
            if (hasFrame && timer < period) return output;
            timer = Mathf.Repeat(timer, period);

            Vector2 center = GetCenter();
            velocity = hasFrame ? Vector2.Scale(center - lastCenter, new Vector2(w, h)) : Vector2.zero;
            lastCenter = center;
            hasFrame = true;

            cs.SetInts(FlowIDs.Size, w, h);
            cs.SetVector(FlowIDs.DiscCenter, center);
            cs.SetVector(FlowIDs.DiscVelocity, velocity);
            cs.SetFloat(FlowIDs.DiscRadius, radius);
            cs.SetFloat(FlowIDs.DiscSoftness, softness);
            cs.SetFloat(FlowIDs.TestFlowMode, source.IsFlowMode ? 1 : 0);
            cs.SetTexture(kTestDisc, FlowIDs.TestOut, output);
            OpticalFlowUtil.Dispatch(cs, kTestDisc, w, h);

            return output;
        }

        Vector2 GetCenter()
        {
            Vector3 mouse = Input.mousePosition;
            bool mouseInside = mouse.x >= 0 && mouse.y >= 0 && mouse.x <= Screen.width && mouse.y <= Screen.height;

            if (followMouse && mouseInside)
                return new Vector2(mouse.x / Screen.width, mouse.y / Screen.height);

            return new Vector2(.5f + .3f * Mathf.Sin(autoTime * 2.1f), .5f + .3f * Mathf.Sin(autoTime * 1.3f));
        }

        void OnDisable()
        {
            OpticalFlowUtil.Release(ref output);
            hasFrame = false;
        }
    }
}
