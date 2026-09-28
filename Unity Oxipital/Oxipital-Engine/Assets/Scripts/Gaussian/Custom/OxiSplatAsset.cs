using UnityEngine;

// Baked, GPU-ready Gaussian splat data produced by OxiSplatPlyImporter from a .ply file.
// Positions/covariance/color/SH are stored as separate binary TextAsset sub-files (written by
// the importer as .bytes next to this asset) rather than byte[] fields on this ScriptableObject:
// a byte[] here would be YAML-serialized as one giant hex string inline in the .asset file
// (2x the raw size, no line/size cap), which both blows past Unity/.NET's ~2GB single-string
// limit for large splat clouds and makes the default Inspector try to reflect a
// hundred-million-element array the moment the asset is selected, freezing the editor. A
// TextAsset reference is cheap to inspect and its bytes are stored as real binary.
[CreateAssetMenu(menuName = "Oxipital/Oxi Splat Asset", fileName = "OxiSplatAsset")]
public class OxiSplatAsset : ScriptableObject
{
    // Struct layouts below must match OxiSplatCompute.compute exactly (field order, size).

    public int splatCount;
    public Bounds bounds;

    // float3 pos, per splat (12 bytes/splat)
    public TextAsset positionData;

    // float3 cov0, float3 cov1 (symmetric 3x3: cov0=(m00,m01,m02), cov1=(m11,m12,m22)) - 24 bytes/splat
    public TextAsset covarianceData;

    // float3 dc color (already SH0-decoded to 0..1 range) + float opacity (already sigmoid-applied) - 16 bytes/splat
    public TextAsset colorOpacityData;

    // float3 sh[15] per splat, coefficient-major (degree 1-3 rest terms) - 180 bytes/splat
    public TextAsset shData;
}
