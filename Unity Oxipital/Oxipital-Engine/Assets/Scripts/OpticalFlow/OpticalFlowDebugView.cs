using UnityEngine;
using UnityEngine.UI;

namespace Oxipital
{
    // Debug views of the pipeline (hue = direction, brightness = magnitude) :
    //   view      : screen overlay in the bottom left corner (or into the given RawImage)
    //   planeView : image drawn in the 3D scene on the FlowPlane, to align the camera feed with reality
    public class OpticalFlowDebugView : MonoBehaviour
    {
        public enum View { Off, Source, RawFlow, Trail, Output, Mask }
        // Input = Spout image as received, Source = after crop / mirror / flip (what the flow really uses)
        public enum PlaneView { Off, Input, Source, Output }

        [Header("Screen Overlay")]
        public View view = View.Off;
        [Range(.1f, 10)]
        public float gain = 1;
        [Range(64, 1024)]
        public int displayWidth = 320;

        [Header("Flow Plane View")]
        public PlaneView planeView = PlaneView.Off;
        [Range(0, 1)]
        public float planeOpacity = .8f;
        // Unlit HDRP colour, raise it if the scene exposure makes the image too dark
        [Range(0, 50)]
        public float planeBrightness = 1;

        [OSCQuery.DoNotExpose]
        public RawImage target;
        [OSCQuery.DoNotExpose]
        public Material planeMaterial;

        RenderTexture debugTexture;
        GameObject overlay;
        RawImage overlayImage;

        RenderTexture planeTexture;
        GameObject planeQuad;
        Material planeMaterialInstance;

        int kDebug = -1;

        static readonly int UnlitColorMap = Shader.PropertyToID("_UnlitColorMap");
        static readonly int UnlitColor = Shader.PropertyToID("_UnlitColor");

        internal void Render(OpticalFlowManager manager, ComputeShader cs)
        {
            if (kDebug < 0) kDebug = cs.FindKernel("DebugView");

            RenderOverlay(manager, cs);
            RenderPlane(manager, cs);
        }

        void RenderOverlay(OpticalFlowManager manager, ComputeShader cs)
        {
            RawImage image = target != null ? target : overlayImage;

            if (view == View.Off)
            {
                if (overlay != null) overlay.SetActive(false);
                if (target != null) target.enabled = false;
                return;
            }

            OpticalFlowSource source = manager.Source;
            EnsureTexture(ref debugTexture, "OpticalFlow Debug", source.Width, source.Height);

            Texture input;
            int mode = 1;
            Vector4 channel = Vector4.zero;
            switch (view)
            {
                case View.Source:
                    input = source.Current;
                    if (!source.IsFlowMode) { mode = 2; channel = new Vector4(1, 0, 0, 0); }
                    break;
                case View.RawFlow: input = manager.Compute.RawFlow; break;
                case View.Trail: input = manager.Processor.Trail; break;
                case View.Mask: input = manager.Processor.Output; mode = 2; channel = new Vector4(0, 0, 0, 1); break;
                default: input = manager.Processor.Output; break;
            }

            Draw(cs, input, debugTexture, mode, channel, source.Width, source.Height);

            if (image == null) image = CreateOverlay();
            if (target == null) overlay.SetActive(true);
            image.enabled = true;
            image.texture = debugTexture;
            image.rectTransform.sizeDelta = new Vector2(displayWidth, displayWidth * source.Height / (float)source.Width);
        }

        void RenderPlane(OpticalFlowManager manager, ComputeShader cs)
        {
            bool visible = planeView != PlaneView.Off && manager.plane != null && planeMaterial != null;
            if (!visible)
            {
                if (planeQuad != null) planeQuad.SetActive(false);
                return;
            }

            OpticalFlowSource source = manager.Source;
            EnsureTexture(ref planeTexture, "OpticalFlow Plane View", source.Width, source.Height);

            // Input and Source show the image carrying the body (grey), Output shows the flow colours
            Texture input;
            int mode = 2;
            Vector4 channel = source.MaskChannel;
            switch (planeView)
            {
                case PlaneView.Input: input = source.Input; break;
                case PlaneView.Source: input = source.Current; break;
                default: input = manager.Processor.Output; mode = 1; channel = Vector4.zero; break;
            }

            Draw(cs, input, planeTexture, mode, channel, source.Width, source.Height);

            if (planeQuad == null) CreatePlaneQuad();
            if (planeQuad.transform.parent != manager.plane.transform)
                planeQuad.transform.SetParent(manager.plane.transform, false);
            planeQuad.SetActive(true);

            planeMaterialInstance.SetTexture(UnlitColorMap, planeTexture);
            planeMaterialInstance.SetColor(UnlitColor, new Color(planeBrightness, planeBrightness, planeBrightness, planeOpacity));
        }

        void Draw(ComputeShader cs, Texture input, RenderTexture output, int mode, Vector4 channel, int w, int h)
        {
            cs.SetInts(FlowIDs.Size, w, h);
            cs.SetInt(FlowIDs.DebugMode, mode);
            cs.SetVector(FlowIDs.DebugChannel, channel);
            cs.SetFloat(FlowIDs.DebugGain, gain);
            cs.SetTexture(kDebug, FlowIDs.DebugIn, input);
            cs.SetTexture(kDebug, FlowIDs.DebugOut, output);
            OpticalFlowUtil.Dispatch(cs, kDebug, w, h);
        }

        static void EnsureTexture(ref RenderTexture rt, string name, int w, int h)
        {
            if (rt != null && rt.width == w && rt.height == h) return;
            OpticalFlowUtil.Release(ref rt);
            rt = OpticalFlowUtil.CreateRT(name, w, h, OpticalFlowUtil.RGBAHalf);
        }

        RawImage CreateOverlay()
        {
            overlay = new GameObject("OpticalFlow Debug Overlay", typeof(Canvas));
            overlay.transform.SetParent(transform, false);
            Canvas canvas = overlay.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;

            GameObject imageObject = new GameObject("Image", typeof(RawImage));
            imageObject.transform.SetParent(overlay.transform, false);
            overlayImage = imageObject.GetComponent<RawImage>();
            RectTransform rect = overlayImage.rectTransform;
            rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.zero;
            rect.anchoredPosition = new Vector2(10, 10);
            return overlayImage;
        }

        // Unit quad in the plane local XY, same mapping as the flow force (uv = local.xy + 0.5)
        void CreatePlaneQuad()
        {
            planeQuad = new GameObject("OpticalFlow Plane View", typeof(MeshFilter), typeof(MeshRenderer));
            planeQuad.hideFlags = HideFlags.DontSave;
            planeQuad.GetComponent<MeshFilter>().sharedMesh = Resources.GetBuiltinResource<Mesh>("Quad.fbx");

            planeMaterialInstance = new Material(planeMaterial) { name = "OpticalFlow Plane View (Instance)" };
            MeshRenderer meshRenderer = planeQuad.GetComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = planeMaterialInstance;
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;
        }

        internal void Release()
        {
            OpticalFlowUtil.Release(ref debugTexture);
            OpticalFlowUtil.Release(ref planeTexture);
            if (overlay != null) Destroy(overlay);
            if (planeQuad != null) Destroy(planeQuad);
            if (planeMaterialInstance != null) Destroy(planeMaterialInstance);
            overlay = null;
            overlayImage = null;
            planeQuad = null;
            planeMaterialInstance = null;
        }

        void OnDisable()
        {
            Release();
        }
    }
}
