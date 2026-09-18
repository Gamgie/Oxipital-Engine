using UnityEngine;

// Baked, GPU-ready Gaussian splat data produced by OxiSplatPlyImporter from a .ply file.
// Positions/covariance/color/SH are stored as raw binary blobs (TextAsset-style byte[] fields
// serialized straight into this asset) so loading at runtime is a single upload per buffer,
// no per-splat parsing.
[CreateAssetMenu(menuName = "Oxipital/Oxi Splat Asset", fileName = "OxiSplatAsset")]
public class OxiSplatAsset : ScriptableObject
{
    // Struct layouts below must match OxiSplatCompute.compute exactly (field order, size).

    public int splatCount;
    public Bounds bounds;

    // float3 pos, per splat (12 bytes/splat)
    public byte[] positionData;

    // float3 cov0, float3 cov1 (symmetric 3x3: cov0=(m00,m01,m02), cov1=(m11,m12,m22)) - 24 bytes/splat
    public byte[] covarianceData;

    // float3 dc color (already SH0-decoded to 0..1 range) + float opacity (already sigmoid-applied) - 16 bytes/splat
    public byte[] colorOpacityData;

    // float3 sh[15] per splat, coefficient-major (degree 1-3 rest terms) - 180 bytes/splat
    public byte[] shData;
}
