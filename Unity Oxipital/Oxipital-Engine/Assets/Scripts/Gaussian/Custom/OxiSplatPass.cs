using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

// Drives every active OxiSplatRenderer once per camera per frame. Injection point is set
// on the CustomPassVolume component in the scene (not here) - use AfterPostProcess.
//
// This is because our splat colors (EvalSH's output) are already final, display-ready
// values, not physically-lit HDR radiance - injecting any earlier (e.g. BeforeTransparent)
// means they flow through the scene's exposure and tonemapping along with everything else,
// which for a high fixed exposure tuned for normal lit geometry blows them out to washed-out
// white/low-contrast. The stock GaussianSplatting package's own setup notes recommend
// AfterPostProcess for exactly this reason ("stop auto-exposure from going wild").
//
// All CustomPass injection points for a camera complete before that camera's own
// endCameraRendering fires (this is guaranteed by HDRP's pipeline), so AfterPostProcess
// still finishes before DomeWarp's RenderPipelineManager.endCameraRendering blit runs -
// no race there regardless of which injection point is used.
//
// Renders straight into the camera's own color+depth buffers (no intermediate render
// target / composite blit needed) since our shader alpha-blends directly.
[System.Serializable]
class OxiSplatPass : CustomPass
{
    // Cameras to skip entirely, e.g. the dome rig's wide-FOV camera once it's being handled
    // instead by OxiSplatCubeCapture (narrow-FOV faces composited separately) - rendering
    // splats here too for that camera would double them up with the wrong-at-wide-FOV shape
    // on top of the correct cube-capture overlay.
    public static readonly System.Collections.Generic.List<Camera> ExcludedCameras = new System.Collections.Generic.List<Camera>();

    [Tooltip("Off by default: the Scene view is its own full HDRP camera render, so with this off, having both Scene and Game view open doesn't double the splat compute+sort+draw cost every frame - the usual cause of Editor lag while orbiting/looking around. Doesn't affect the actual Game view / dome / NDI output. Turn on if you specifically need to see splats while placing/editing objects in the Scene view.")]
    public bool renderInSceneView = false;

    protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
    {
    }

    protected override void Execute(CustomPassContext ctx)
    {
        if (OxiSplatRenderer.Active.Count == 0)
            return;

        Camera cam = ctx.hdCamera.camera;
        if (ExcludedCameras.Contains(cam))
            return;
        if (!renderInSceneView && cam.cameraType == CameraType.SceneView)
            return;

        CoreUtils.SetRenderTarget(ctx.cmd, ctx.cameraColorBuffer, ctx.cameraDepthBuffer);

        for (int i = 0; i < OxiSplatRenderer.Active.Count; i++)
        {
            OxiSplatRenderer r = OxiSplatRenderer.Active[i];
            if (r != null && r.isActiveAndEnabled)
                r.RenderForCamera(ctx.cmd, cam);
        }
    }

    protected override void Cleanup()
    {
    }
}
