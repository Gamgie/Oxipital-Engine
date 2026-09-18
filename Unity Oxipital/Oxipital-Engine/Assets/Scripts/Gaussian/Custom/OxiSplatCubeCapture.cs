using UnityEngine;
using UnityEngine.Rendering;

// Renders every active OxiSplatRenderer into a small cubemap (5 faces, ~90 deg each,
// skipping the bottom face by default) instead of through the dome rig's single ~180 deg
// DomeWarp camera. OxiSplatCompute.compute's covariance projection is a *local linear*
// approximation of the true perspective projection (standard EWA splatting) - accurate
// near the optical axis, but increasingly wrong toward the edges of a wide FOV, which is
// what was showing as splats rendering "on their thin side" near the periphery of the dome
// view. Keeping each capture face at a normal ~90 deg FOV keeps every splat well inside the
// approximation's accurate range, the same way a normal single camera already did.
//
// The cubemap is then warped into a fisheye image with the SAME cube-to-dome technique
// com.pfc.dome-tools' own Cubemap Rendering mode uses (reusing its existing shader/material,
// assigned here rather than depending on that package's RealtimeCubemap component), and
// alpha-composited on top of the dome's already-rendered output. This deliberately does not
// use Camera.RenderToCubemap or any real Camera component for the capture - each face is a
// self-contained virtual view (see OxiSplatRenderer.RenderForVirtualView), sidestepping the
// HDCamera-reuse uncertainty investigated earlier with that API.
[ExecuteAlways]
public class OxiSplatCubeCapture : MonoBehaviour
{
    [Header("Capture")]
    [Tooltip("World position the cube faces are captured from - should match where the dome rig's own camera sits.")]
    public Transform captureOrigin;
    [Range(256, 2048)] public int faceSize = 1024;
    public bool skipBottomFace = true;
    [Range(70f, 110f)] public float faceFovDegrees = 92f; // a little over 90 so adjacent faces overlap slightly, avoiding seams
    public float nearClip = 0.05f;
    public float farClip = 1000f;

    [Header("Cube to dome warp")]
    [Tooltip("Assign Hidden/OxiSplat/CubeToDome (OxiSplatCubeToDome.shader) - a self-contained equidistant fisheye warp, not dependent on com.pfc.dome-tools' own shader.")]
    public Shader cubeToDomeShader;
    [Tooltip("Assign Hidden/OxiSplat/AlphaComposite (OxiSplatAlphaComposite.shader). Exposed explicitly rather than found by name so it isn't stripped from builds.")]
    public Shader alphaCompositeShader;
    [Tooltip("Rotation reference the warp orients the dome image by - use the same transform your DomeWarp/RealtimeCubemap component uses.")]
    public Transform domeRotationReference;
    [Range(120f, 360f)] public float domeViewAngle = 210f;

    [Header("Composite target")]
    [Tooltip("The dome's already-rendered output texture (e.g. Dome Content Texture) to alpha-composite the splats on top of.")]
    public RenderTexture domeOutputTexture;
    [Tooltip("Camera whose end-of-render triggers the composite, so this runs after the dome's own content is already in domeOutputTexture. Should be the same camera DomeWarp is attached to.")]
    public Camera triggerCamera;

    RenderTexture _cubemap;
    RenderTexture _fisheyeOverlay;
    Material _warpMaterial;
    Material _compositeMaterial;

    static readonly Vector3[] FaceForward =
    {
        Vector3.right, Vector3.left, Vector3.up, Vector3.down, Vector3.forward, Vector3.back
    };
    static readonly Vector3[] FaceUp =
    {
        Vector3.down, Vector3.down, Vector3.forward, Vector3.back, Vector3.down, Vector3.down
    };
    const int NegativeYFaceIndex = 3;

    static readonly int PropCubeTex = Shader.PropertyToID("_CubeTex");
    static readonly int PropAngle = Shader.PropertyToID("_AngleDegrees");
    static readonly int PropCubeToDomeWorldTransform = Shader.PropertyToID("_DomeWorldTransform");

    void OnEnable()
    {
        // endContextRendering fires once per frame, strictly after every camera's own
        // endCameraRendering callback (including DomeWarp's) has already run - unlike
        // endCameraRendering, whose subscriber order between components isn't guaranteed,
        // this reliably composites on top of DomeWarp's output instead of racing it.
        RenderPipelineManager.endContextRendering += OnEndContextRendering;

        // Stop OxiSplatPass (BeforeTransparent, wide-FOV-unsafe) from also rendering splats
        // for this camera now that cube-capture is handling it.
        if (triggerCamera != null)
            OxiSplatPass.ExcludedCameras.Add(triggerCamera);
    }

    void OnDisable()
    {
        RenderPipelineManager.endContextRendering -= OnEndContextRendering;
        if (triggerCamera != null)
            OxiSplatPass.ExcludedCameras.Remove(triggerCamera);
        ReleaseTargets();
    }

    void ReleaseTargets()
    {
        if (_cubemap != null) { _cubemap.Release(); _cubemap = null; }
        if (_fisheyeOverlay != null) { _fisheyeOverlay.Release(); _fisheyeOverlay = null; }
        if (_warpMaterial != null) { DestroyImmediate(_warpMaterial); _warpMaterial = null; }
        if (_compositeMaterial != null) { DestroyImmediate(_compositeMaterial); _compositeMaterial = null; }
    }

    bool EnsureTargets()
    {
        if (captureOrigin == null || cubeToDomeShader == null || domeRotationReference == null || domeOutputTexture == null)
            return false;

        if (_cubemap == null || _cubemap.width != faceSize)
        {
            if (_cubemap != null) _cubemap.Release();
            _cubemap = new RenderTexture(faceSize, faceSize, 16, RenderTextureFormat.ARGB32)
            {
                dimension = TextureDimension.Cube,
                name = "OxiSplatCubemap"
            };
            _cubemap.Create();
        }

        if (_fisheyeOverlay == null || _fisheyeOverlay.width != domeOutputTexture.width || _fisheyeOverlay.height != domeOutputTexture.height)
        {
            if (_fisheyeOverlay != null) _fisheyeOverlay.Release();
            _fisheyeOverlay = new RenderTexture(domeOutputTexture.width, domeOutputTexture.height, 0, RenderTextureFormat.ARGB32)
            {
                name = "OxiSplatFisheyeOverlay"
            };
            _fisheyeOverlay.Create();
        }

        if (_warpMaterial == null)
            _warpMaterial = new Material(cubeToDomeShader) { hideFlags = HideFlags.HideAndDontSave };

        if (_compositeMaterial == null && alphaCompositeShader != null)
            _compositeMaterial = new Material(alphaCompositeShader) { hideFlags = HideFlags.HideAndDontSave };

        return true;
    }

    void OnEndContextRendering(ScriptableRenderContext ctx, System.Collections.Generic.List<Camera> cameras)
    {
        if (triggerCamera != null && !cameras.Contains(triggerCamera))
            return;
        if (OxiSplatRenderer.Active.Count == 0)
            return;
        if (!EnsureTargets())
            return;

        CommandBuffer cmd = CommandBufferPool.Get("OxiSplatCubeCapture");

        int faceCount = 6;
        for (int face = 0; face < faceCount; face++)
        {
            if (skipBottomFace && face == NegativeYFaceIndex)
                continue;

            CoreUtils.SetRenderTarget(cmd, _cubemap, ClearFlag.Color, Color.clear, 0, (CubemapFace)face);

            Vector3 pos = captureOrigin.position;
            Quaternion rot = Quaternion.LookRotation(FaceForward[face], FaceUp[face]);
            Matrix4x4 matrixV = Matrix4x4.TRS(pos, rot, Vector3.one).inverse;
            // Unity's view matrices are right-handed (camera looks down -Z); flip Z to match.
            matrixV = Matrix4x4.Scale(new Vector3(1, 1, -1)) * matrixV;

            Matrix4x4 matrixP = Matrix4x4.Perspective(faceFovDegrees, 1f, nearClip, farClip);
            matrixP = GL.GetGPUProjectionMatrix(matrixP, true);

            for (int i = 0; i < OxiSplatRenderer.Active.Count; i++)
            {
                OxiSplatRenderer r = OxiSplatRenderer.Active[i];
                if (r != null && r.isActiveAndEnabled)
                    r.RenderForVirtualView(cmd, matrixV, matrixP, pos, new Vector2(faceSize, faceSize), nearClip);
            }
        }

        _warpMaterial.SetTexture(PropCubeTex, _cubemap);
        _warpMaterial.SetFloat(PropAngle, domeViewAngle);
        Matrix4x4 domeRotMat = Matrix4x4.TRS(Vector3.zero, domeRotationReference.rotation, Vector3.one);
        _warpMaterial.SetMatrix(PropCubeToDomeWorldTransform, domeRotMat);
        Shader.SetGlobalMatrix(PropCubeToDomeWorldTransform, domeRotMat);

        cmd.SetRenderTarget(_fisheyeOverlay);
        cmd.ClearRenderTarget(false, true, Color.clear);
        cmd.Blit(null, _fisheyeOverlay, _warpMaterial, 0);

        if (_compositeMaterial != null)
        {
            // Blit binds _fisheyeOverlay to _MainTex automatically; the shader alpha-blends
            // it against whatever's already in domeOutputTexture (DomeWarp's output).
            cmd.Blit(_fisheyeOverlay, domeOutputTexture, _compositeMaterial);
        }
        else
        {
            Debug.LogWarning("OxiSplatCubeCapture: alphaCompositeShader not assigned - falling back to a direct copy, which will erase the dome's existing content instead of blending over it.", this);
            cmd.Blit(_fisheyeOverlay, domeOutputTexture);
        }

        Graphics.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }
}
