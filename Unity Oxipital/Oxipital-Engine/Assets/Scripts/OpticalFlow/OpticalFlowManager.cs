using UnityEngine;
using UnityEngine.VFX;

namespace Oxipital
{
    // Optical flow pipeline (Kinect -> TouchDesigner -> Spout -> Unity) :
    //   OpticalFlowSource -> OpticalFlowCompute -> OpticalFlowProcessor -> OpticalFlowMetrics
    // Runs the stages in order in LateUpdate (after KlakSpout received the frame in Update).
    // Ballet pushes the result into every OrbGroup through ApplyTo, like the force buffers.
    [DefaultExecutionOrder(-50)]
    [RequireComponent(typeof(OpticalFlowSource), typeof(OpticalFlowCompute), typeof(OpticalFlowProcessor))]
    public class OpticalFlowManager : MonoBehaviour
    {
        [Header("Force")]
        public bool forceEnabled = true;
        [Range(0, 1)]
        public float intensity = 1;
        // Add mode : velocity += flow * strength * dt
        [Range(0, 50)]
        public float strength = 10;
        // Distance from the plane (plane local Z) where particles are still pushed
        [Range(0, 10)]
        public float depthBand = 1;

        [Header("Wind")]
        // 0 = Add (push), 1 = Wind (particles dragged towards the flow speed)
        [Range(0, 1)]
        public float windBlend = 0;
        [Range(0, 20)]
        public float windSpeed = 3;
        [Range(0, 20)]
        public float windDrag = 4;

        [Header("Plane")]
        // Height of the flow plane in meters. The width follows the aspect ratio of the Spout texture (OxipitalFlow),
        // crop included, so the image is never stretched.
        [Range(.1f, 20)]
        public float planeScale = 4;

        [OSCQuery.DoNotExpose]
        public FlowPlane plane;

        OpticalFlowSource source;
        OpticalFlowCompute compute;
        OpticalFlowProcessor processor;
        OpticalFlowMetrics metrics;
        OpticalFlowDebugView debugView;

        bool allocated;
        bool warnedMissingShaders;

        public OpticalFlowSource Source => source;
        public OpticalFlowCompute Compute => compute;
        public OpticalFlowProcessor Processor => processor;
        public RenderTexture FlowMap => allocated ? processor.Output : null;

        void OnEnable()
        {
            source = GetComponent<OpticalFlowSource>();
            compute = GetComponent<OpticalFlowCompute>();
            processor = GetComponent<OpticalFlowProcessor>();
            metrics = GetComponent<OpticalFlowMetrics>();
            debugView = GetComponent<OpticalFlowDebugView>();
            if (plane == null) plane = GetComponentInChildren<FlowPlane>();
        }

        void OnDisable()
        {
            ReleaseAll();
        }

        void Update()
        {
            ApplyPlaneScale();
            if (!HasShaders()) return;

            // Working resolution follows the Spout sender texture (textures rebuilt when it changes)
            source.UpdateReceivedSize();
            int w = Mathf.Max(source.DesiredSize.x, 16);
            int h = Mathf.Max(source.DesiredSize.y, 16);
            if (allocated && w == source.Width && h == source.Height) return;

            ReleaseAll();
            source.Allocate(w, h);
            compute.Allocate(w, h);
            processor.Allocate(w, h);
            allocated = true;
        }

        void LateUpdate()
        {
            if (!allocated) return;

            source.Execute();
            compute.Execute(source);
            source.Commit();
            processor.Execute(source, compute.RawFlow);

            if (metrics != null && metrics.enabled) metrics.Execute(processor.Output, plane);
            if (debugView != null && debugView.enabled) debugView.Render(this, processor.shader);
        }

        // Plane size from planeScale and the Spout texture aspect ratio. Also called by FlowPlane in edit mode,
        // where OnEnable has not run (until a sender is received, the source default size is used).
        internal void ApplyPlaneScale()
        {
            FlowPlane target = plane != null ? plane : GetComponentInChildren<FlowPlane>();
            OpticalFlowSource input = source != null ? source : GetComponent<OpticalFlowSource>();
            if (target == null || input == null) return;

            Vector2Int size = input.DesiredSize;
            float width = size.x * (1 - input.cropLeft - input.cropRight);
            float height = size.y * (1 - input.cropBottom - input.cropTop);
            float aspect = width > 0 && height > 0 ? width / height : 1;

            Vector3 scale = new Vector3(planeScale * aspect, planeScale, 1);
            if (target.transform.localScale != scale) target.transform.localScale = scale;
        }

        void ReleaseAll()
        {
            if (!allocated) return;
            source.Release();
            compute.Release();
            processor.Release();
            allocated = false;
        }

        bool HasShaders()
        {
            if (source.shader != null && compute.shader != null && processor.shader != null) return true;

            if (!warnedMissingShaders) Debug.LogWarning("OpticalFlowManager : compute shaders are not assigned", this);
            warnedMissingShaders = true;
            return false;
        }

        // Pushes the flow into a VFX Graph using the "Optical Flow Force" block. Graphs without it are skipped.
        // weight : per emitter multiplier (OrbGroup.opticalFlowWeight)
        public void ApplyTo(VisualEffect vfx, float weight)
        {
            if (!vfx.HasVector4(FlowIDs.VfxParams)) return;

            float gain = forceEnabled && FlowMap != null && plane != null ? intensity * weight : 0;

            // x = add strength, y = depth band, z = wind speed, w = wind rate
            vfx.SetVector4(FlowIDs.VfxParams, new Vector4(
                strength * (1 - windBlend) * gain,
                depthBand,
                windSpeed,
                windDrag * windBlend * gain));

            if (gain <= 0) return;

            vfx.SetTexture(FlowIDs.VfxFlowMap, FlowMap);
            vfx.SetMatrix4x4(FlowIDs.VfxWorldToPlane, plane.WorldToPlane);
            vfx.SetVector3(FlowIDs.VfxPlaneRight, plane.Right);
            vfx.SetVector3(FlowIDs.VfxPlaneUp, plane.Up);
        }
    }
}
