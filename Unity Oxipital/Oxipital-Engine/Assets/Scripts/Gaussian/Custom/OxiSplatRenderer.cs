using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// Holds one OxiSplatAsset's GPU buffers and knows how to compute view data + sort + draw
// itself for a given camera. OxiSplatPass (an HDRP CustomPass) drives every active instance
// each frame. See OxiSplatCompute.compute for the off-axis projection fix and the GPU
// counting-sort this uses instead of a comparison sort.
[ExecuteAlways]
public class OxiSplatRenderer : MonoBehaviour
{
    public static readonly List<OxiSplatRenderer> Active = new List<OxiSplatRenderer>();

    const uint NumBuckets = 65536;
    const int NumScanSteps = 16; // log2(NumBuckets)

    public OxiSplatAsset asset;
    public ComputeShader computeShader;
    public Shader renderShader;
    [Range(0.1f, 3f)] public float splatScale = 1f;
    [Tooltip("Caps a splat's raw on-screen radius in pixels, before Splat Scale is applied. Without this, splats very close to the camera (or inside the splat volume) balloon to cover the whole screen - this is what was causing the huge white blob / near-zero FPS when the camera entered the cloud. Lower this to also cut fill-rate cost (fewer/smaller overlapping quads) at high output resolutions like the dome's.")]
    [Range(8f, 512f)] public float maxRadiusPx = 64f;
    [Tooltip("1 = unchanged. Multiplies splat color in linear space (after the sRGB->linear conversion), equivalent to scaling light intensity.")]
    [Range(0.1f, 5f)] public float brightness = 1f;
    [Tooltip("0 = unchanged. Lifts the black floor without touching the white point (0 -> this value, 1 stays 1) - for dome viewing where true black in a night scene reads as \"nothing there.\"")]
    [Range(0f, 0.5f)] public float blackLevel = 0f;
    [Tooltip("Global opacity multiplier for fading the whole splat cloud in/out - 0 = invisible, 1 = normal. A plain public field, so exposed over OSCQuery like the rest of this project's live show-control parameters.")]
    [Range(0f, 1f)] public float fade = 1f;
    [Tooltip("1 = process every splat (full quality). Raise to process only every Nth splat instead - a direct, linear cut to compute cost (full SH evaluation across millions of splats), for when Max Radius Px / Splat Scale alone aren't enough because the bottleneck is compute, not fill-rate/overdraw.")]
    [Range(1, 8)] public int decimationStride = 1;

    Material _material;
    GraphicsBuffer _posBuffer, _covBuffer, _colorOpacityBuffer, _shBuffer;
    GraphicsBuffer _indexBuffer;
    GraphicsBuffer _viewDataBuffer, _splatBucketBuffer, _sortedIndicesBuffer;
    GraphicsBuffer _histogramBuffer, _scanABuffer, _scanBBuffer, _bucketOffsetsBuffer;
    int _splatCount;

    int _kClearHistogram, _kCalcViewData, _kScanStep, _kComputeOffsets, _kFillDefault, _kScatter;

    static readonly int PropInPositions = Shader.PropertyToID("_InPositions");
    static readonly int PropInCov = Shader.PropertyToID("_InCov");
    static readonly int PropInColorOpacity = Shader.PropertyToID("_InColorOpacity");
    static readonly int PropInSH = Shader.PropertyToID("_InSH");
    static readonly int PropOutViewData = Shader.PropertyToID("_OutViewData");
    static readonly int PropSplatBucket = Shader.PropertyToID("_SplatBucket");
    static readonly int PropHistogram = Shader.PropertyToID("_Histogram");
    static readonly int PropScanSrc = Shader.PropertyToID("_ScanSrc");
    static readonly int PropScanDst = Shader.PropertyToID("_ScanDst");
    static readonly int PropBucketOffsets = Shader.PropertyToID("_BucketOffsets");
    static readonly int PropSortedIndices = Shader.PropertyToID("_SortedIndices");
    static readonly int PropSplatCount = Shader.PropertyToID("_SplatCount");
    static readonly int PropDecimationStride = Shader.PropertyToID("_DecimationStride");
    static readonly int PropMatrixObjectToWorld = Shader.PropertyToID("_MatrixObjectToWorld");
    static readonly int PropMatrixV = Shader.PropertyToID("_MatrixV");
    static readonly int PropMatrixP = Shader.PropertyToID("_MatrixP");
    static readonly int PropMatrixVP = Shader.PropertyToID("_MatrixVP");
    static readonly int PropCamPos = Shader.PropertyToID("_CamPos");
    static readonly int PropScreenSize = Shader.PropertyToID("_ScreenSize");
    static readonly int PropSplatScale = Shader.PropertyToID("_SplatScale");
    static readonly int PropMaxRadiusPx = Shader.PropertyToID("_MaxRadiusPx");
    static readonly int PropBrightness = Shader.PropertyToID("_Brightness");
    static readonly int PropBlackLevel = Shader.PropertyToID("_BlackLevel");
    static readonly int PropFade = Shader.PropertyToID("_Fade");
    static readonly int PropNearClip = Shader.PropertyToID("_NearClip");
    static readonly int PropMinDepth = Shader.PropertyToID("_MinDepth");
    static readonly int PropDepthScale = Shader.PropertyToID("_DepthScale");
    static readonly int PropScanOffset = Shader.PropertyToID("_ScanOffset");
    static readonly int PropViewData = Shader.PropertyToID("_ViewData");

    void OnEnable()
    {
        if (!SetUp())
        {
            enabled = false;
            return;
        }
        Active.Add(this);
    }

    void OnDisable()
    {
        Active.Remove(this);
        TearDown();
    }

    bool SetUp()
    {
        if (asset == null || asset.splatCount <= 0 || computeShader == null || renderShader == null)
            return false;

        _splatCount = asset.splatCount;

        _posBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, _splatCount, 12);
        _posBuffer.SetData(asset.positionData);

        _covBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, _splatCount, 24);
        _covBuffer.SetData(asset.covarianceData);

        _colorOpacityBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, _splatCount, 16);
        _colorOpacityBuffer.SetData(asset.colorOpacityData);

        _shBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, _splatCount * 15, 12);
        _shBuffer.SetData(asset.shData);

        // +1: a permanently-invisible dummy entry that unwritten scatter slots point to.
        _viewDataBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, _splatCount + 1, 60);
        _splatBucketBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, _splatCount, 4);
        _sortedIndicesBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, _splatCount, 4);

        _histogramBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, (int)NumBuckets, 4);
        _scanABuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, (int)NumBuckets, 4);
        _scanBBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, (int)NumBuckets, 4);
        _bucketOffsetsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, (int)NumBuckets, 4);

        _indexBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Index, 6, 4);
        _indexBuffer.SetData(new uint[] { 0, 1, 2, 1, 3, 2 });

        _kClearHistogram = computeShader.FindKernel("CSClearHistogram");
        _kCalcViewData = computeShader.FindKernel("CSCalcViewData");
        _kScanStep = computeShader.FindKernel("CSScanStep");
        _kComputeOffsets = computeShader.FindKernel("CSComputeOffsets");
        _kFillDefault = computeShader.FindKernel("CSFillDefault");
        _kScatter = computeShader.FindKernel("CSScatter");

        _material = new Material(renderShader) { hideFlags = HideFlags.HideAndDontSave };
        return true;
    }

    void TearDown()
    {
        _posBuffer?.Dispose();
        _covBuffer?.Dispose();
        _colorOpacityBuffer?.Dispose();
        _shBuffer?.Dispose();
        _viewDataBuffer?.Dispose();
        _splatBucketBuffer?.Dispose();
        _sortedIndicesBuffer?.Dispose();
        _histogramBuffer?.Dispose();
        _scanABuffer?.Dispose();
        _scanBBuffer?.Dispose();
        _bucketOffsetsBuffer?.Dispose();
        _indexBuffer?.Dispose();
        if (_material != null)
            DestroyImmediate(_material);
    }

    // View-space near/far bounds of the splat's world-space AABB against this camera, used
    // to scale depth into the [0, NumBuckets) range the counting sort buckets on.
    void ComputeDepthRange(Matrix4x4 matrixV, Matrix4x4 matrixObjectToWorld, out float minDepth, out float maxDepth)
    {
        Bounds b = asset.bounds;
        Vector3 c = b.center, e = b.extents;
        minDepth = float.MaxValue;
        maxDepth = float.MinValue;
        for (int i = 0; i < 8; i++)
        {
            Vector3 local = c + new Vector3(
                (i & 1) == 0 ? -e.x : e.x,
                (i & 2) == 0 ? -e.y : e.y,
                (i & 4) == 0 ? -e.z : e.z);
            Vector3 world = matrixObjectToWorld.MultiplyPoint3x4(local);
            Vector3 view = matrixV.MultiplyPoint3x4(world);
            minDepth = Mathf.Min(minDepth, view.z);
            maxDepth = Mathf.Max(maxDepth, view.z);
        }
        // Guard against a degenerate (zero-volume or dead-on-axis) range.
        if (maxDepth - minDepth < 0.001f)
            maxDepth = minDepth + 0.001f;
    }

    // Called by OxiSplatPass once per camera per frame, with the camera's real HDRP color+
    // depth already bound as the render target.
    public void RenderForCamera(CommandBuffer cmd, Camera cam)
    {
        Matrix4x4 matrixV = cam.worldToCameraMatrix;
        Matrix4x4 matrixP = GL.GetGPUProjectionMatrix(cam.projectionMatrix, true);
        RenderCore(cmd, matrixV, matrixP, cam.transform.position, new Vector2(cam.pixelWidth, cam.pixelHeight), cam.nearClipPlane);
    }

    // Used by OxiSplatCubeCapture to render into a virtual (non-Camera) narrow-FOV view -
    // e.g. one face of a cube capture rig - with explicit view/projection matrices instead
    // of pulling them from a real Camera.
    public void RenderForVirtualView(CommandBuffer cmd, Matrix4x4 matrixV, Matrix4x4 matrixP, Vector3 camPos, Vector2 screenSize, float nearClip)
    {
        RenderCore(cmd, matrixV, matrixP, camPos, screenSize, nearClip);
    }

    void RenderCore(CommandBuffer cmd, Matrix4x4 matrixV, Matrix4x4 matrixP, Vector3 camPos, Vector2 screenSize, float nearClip)
    {
        if (_splatCount <= 0)
            return;
        // Fully skip the compute+sort+draw pipeline when faded out, rather than run all of
        // it just to produce zero-alpha output - this is the whole point of Fade existing
        // as a cheap way to hide the splats during a transition.
        if (fade <= 0f)
            return;

        Matrix4x4 matrixObjectToWorld = transform.localToWorldMatrix;
        Matrix4x4 matrixVP = matrixP * matrixV;

        ComputeDepthRange(matrixV, matrixObjectToWorld, out float minDepth, out float maxDepth);
        float depthScale = (NumBuckets - 1) / (maxDepth - minDepth);

        int effectiveCount = Mathf.Max(1, _splatCount / Mathf.Max(1, decimationStride));

        cmd.SetComputeIntParam(computeShader, PropSplatCount, effectiveCount);
        cmd.SetComputeIntParam(computeShader, PropDecimationStride, Mathf.Max(1, decimationStride));
        cmd.SetComputeMatrixParam(computeShader, PropMatrixObjectToWorld, matrixObjectToWorld);
        cmd.SetComputeMatrixParam(computeShader, PropMatrixV, matrixV);
        cmd.SetComputeMatrixParam(computeShader, PropMatrixP, matrixP);
        cmd.SetComputeMatrixParam(computeShader, PropMatrixVP, matrixVP);
        cmd.SetComputeVectorParam(computeShader, PropCamPos, camPos);
        cmd.SetComputeVectorParam(computeShader, PropScreenSize, new Vector4(screenSize.x, screenSize.y, 0, 0));
        cmd.SetComputeFloatParam(computeShader, PropSplatScale, splatScale);
        cmd.SetComputeFloatParam(computeShader, PropMaxRadiusPx, maxRadiusPx);
        cmd.SetComputeFloatParam(computeShader, PropNearClip, nearClip);
        cmd.SetComputeFloatParam(computeShader, PropBrightness, brightness);
        cmd.SetComputeFloatParam(computeShader, PropBlackLevel, blackLevel);
        cmd.SetComputeFloatParam(computeShader, PropFade, fade);
        cmd.SetComputeFloatParam(computeShader, PropMinDepth, minDepth);
        cmd.SetComputeFloatParam(computeShader, PropDepthScale, depthScale);

        cmd.SetComputeBufferParam(computeShader, _kClearHistogram, PropHistogram, _histogramBuffer);
        cmd.DispatchCompute(computeShader, _kClearHistogram, Mathf.CeilToInt(NumBuckets / 256f), 1, 1);

        cmd.SetComputeBufferParam(computeShader, _kCalcViewData, PropInPositions, _posBuffer);
        cmd.SetComputeBufferParam(computeShader, _kCalcViewData, PropInCov, _covBuffer);
        cmd.SetComputeBufferParam(computeShader, _kCalcViewData, PropInColorOpacity, _colorOpacityBuffer);
        cmd.SetComputeBufferParam(computeShader, _kCalcViewData, PropInSH, _shBuffer);
        cmd.SetComputeBufferParam(computeShader, _kCalcViewData, PropOutViewData, _viewDataBuffer);
        cmd.SetComputeBufferParam(computeShader, _kCalcViewData, PropSplatBucket, _splatBucketBuffer);
        cmd.SetComputeBufferParam(computeShader, _kCalcViewData, PropHistogram, _histogramBuffer);
        cmd.DispatchCompute(computeShader, _kCalcViewData, Mathf.CeilToInt((effectiveCount + 1) / 256f), 1, 1);

        // Inclusive prefix sum over the histogram via Hillis-Steele scan: step 0 reads the
        // untouched _Histogram, later steps ping-pong between _ScanA/_ScanB. _Histogram
        // itself is never written after CSCalcViewData, so CSComputeOffsets can still read
        // each bucket's original count from it afterwards.
        GraphicsBuffer scanSrc = _histogramBuffer;
        GraphicsBuffer scanDst = _scanABuffer;
        for (int step = 0; step < NumScanSteps; step++)
        {
            uint offset = 1u << step;
            cmd.SetComputeIntParam(computeShader, PropScanOffset, (int)offset);
            cmd.SetComputeBufferParam(computeShader, _kScanStep, PropScanSrc, scanSrc);
            cmd.SetComputeBufferParam(computeShader, _kScanStep, PropScanDst, scanDst);
            cmd.DispatchCompute(computeShader, _kScanStep, Mathf.CeilToInt(NumBuckets / 256f), 1, 1);

            GraphicsBuffer nextSrc = scanDst;
            GraphicsBuffer nextDst = (scanSrc == _histogramBuffer) ? _scanBBuffer : scanSrc;
            scanSrc = nextSrc;
            scanDst = nextDst;
        }

        cmd.SetComputeBufferParam(computeShader, _kComputeOffsets, PropScanSrc, scanSrc);
        cmd.SetComputeBufferParam(computeShader, _kComputeOffsets, PropHistogram, _histogramBuffer);
        cmd.SetComputeBufferParam(computeShader, _kComputeOffsets, PropBucketOffsets, _bucketOffsetsBuffer);
        cmd.DispatchCompute(computeShader, _kComputeOffsets, Mathf.CeilToInt(NumBuckets / 256f), 1, 1);

        cmd.SetComputeBufferParam(computeShader, _kFillDefault, PropSortedIndices, _sortedIndicesBuffer);
        cmd.DispatchCompute(computeShader, _kFillDefault, Mathf.CeilToInt(effectiveCount / 256f), 1, 1);

        cmd.SetComputeBufferParam(computeShader, _kScatter, PropSplatBucket, _splatBucketBuffer);
        cmd.SetComputeBufferParam(computeShader, _kScatter, PropBucketOffsets, _bucketOffsetsBuffer);
        cmd.SetComputeBufferParam(computeShader, _kScatter, PropSortedIndices, _sortedIndicesBuffer);
        cmd.DispatchCompute(computeShader, _kScatter, Mathf.CeilToInt(effectiveCount / 256f), 1, 1);

        _material.SetBuffer(PropSortedIndices, _sortedIndicesBuffer);
        _material.SetBuffer(PropViewData, _viewDataBuffer);
        _material.SetVector(PropScreenSize, new Vector4(screenSize.x, screenSize.y, 0, 0));

        cmd.DrawProcedural(_indexBuffer, Matrix4x4.identity, _material, 0, MeshTopology.Triangles, 6, effectiveCount);
    }
}
