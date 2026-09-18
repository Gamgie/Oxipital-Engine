using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using UnityEditor;
using UnityEngine;

// Converts a standard 3D Gaussian Splatting .ply (x,y,z,nx,ny,nz,f_dc_0-2,f_rest_0-44,
// opacity,scale_0-2,rot_0-3, binary_little_endian, one "vertex" element) into an
// OxiSplatAsset with GPU-ready buffers. Decoding formulas (sigmoid opacity, log-scale,
// wxyz-swizzled quaternion, SH0-to-color) and the channel-major -> coefficient-major
// f_rest reorder match the standard reference format used by INRIA's original trainer
// and most capture/training tools.
public static class OxiSplatPlyImporter
{
    const int ShRestCount = 15; // degree 1-3, 15 coefficients per channel

    [MenuItem("Oxipital/Import Gaussian Splat PLY...")]
    public static void ImportMenuItem()
    {
        string plyPath = EditorUtility.OpenFilePanel("Select Gaussian Splat .ply", "", "ply");
        if (string.IsNullOrEmpty(plyPath))
            return;

        string defaultName = Path.GetFileNameWithoutExtension(plyPath);
        string savePath = EditorUtility.SaveFilePanelInProject(
            "Save Oxi Splat Asset", defaultName, "asset", "Choose where to save the imported asset");
        if (string.IsNullOrEmpty(savePath))
            return;

        // Standard/original capture PLYs (INRIA trainer, Postshot, etc.) store f_rest
        // channel-major (all 15 R coefficients, then all 15 G, then all 15 B) and need the
        // transpose in ImportInternal. A PLY re-exported by the stock GaussianSplatting
        // package's own "Export PLY" button is already coefficient-major instead (it dumps
        // its internal struct directly) - transposing that a second time scrambles the
        // higher-order SH terms. No prompt here; assumes a standard/original capture. Call
        // Import(path, savePath, sourceIsCoefficientMajor: true) directly for a re-export.
        Import(plyPath, savePath);
    }

    public static void Import(string plyPath, string assetSavePath, bool sourceIsCoefficientMajor = false)
    {
        try
        {
            ImportInternal(plyPath, assetSavePath, sourceIsCoefficientMajor);
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    static void ImportInternal(string plyPath, string assetSavePath, bool sourceIsCoefficientMajor)
    {
        EditorUtility.DisplayProgressBar("Importing Gaussian Splat", "Reading header...", 0f);

        using FileStream fs = new FileStream(plyPath, FileMode.Open, FileAccess.Read, FileShare.Read, 1 << 20);
        var (vertexCount, properties, headerLength) = ReadHeader(fs);

        int stride = properties.Count * 4;
        Dictionary<string, int> offsets = new Dictionary<string, int>();
        for (int i = 0; i < properties.Count; i++)
            offsets[properties[i]] = i * 4;

        string[] required = { "x", "y", "z", "f_dc_0", "f_dc_1", "f_dc_2", "opacity", "scale_0", "scale_1", "scale_2", "rot_0", "rot_1", "rot_2", "rot_3" };
        foreach (var r in required)
        {
            if (!offsets.ContainsKey(r))
                throw new IOException($"PLY file is missing required property '{r}'. This doesn't look like a Gaussian Splat file.");
        }

        bool hasFullSH = true;
        int[] restOffsets = new int[45];
        for (int i = 0; i < 45; i++)
        {
            string key = $"f_rest_{i}";
            if (!offsets.TryGetValue(key, out restOffsets[i]))
            {
                hasFullSH = false;
                break;
            }
        }

        int posX = offsets["x"], posY = offsets["y"], posZ = offsets["z"];
        int dc0 = offsets["f_dc_0"], dc1 = offsets["f_dc_1"], dc2 = offsets["f_dc_2"];
        int opacityOff = offsets["opacity"];
        int s0 = offsets["scale_0"], s1 = offsets["scale_1"], s2 = offsets["scale_2"];
        int r0 = offsets["rot_0"], r1 = offsets["rot_1"], r2 = offsets["rot_2"], r3 = offsets["rot_3"];

        byte[] posData = new byte[vertexCount * 12];
        byte[] covData = new byte[vertexCount * 24];
        byte[] colorOpacityData = new byte[vertexCount * 16];
        byte[] shData = hasFullSH ? new byte[vertexCount * ShRestCount * 3 * 4] : new byte[0];

        Vector3 boundsMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        Vector3 boundsMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);

        int chunkVerts = 100000;
        byte[] readBuf = new byte[(long)chunkVerts * stride];
        float[] restBuf = hasFullSH ? new float[45] : null;
        float[] shOut = hasFullSH ? new float[45] : null;

        fs.Seek(headerLength, SeekOrigin.Begin);

        int processed = 0;
        while (processed < vertexCount)
        {
            int vertsThisChunk = Math.Min(chunkVerts, vertexCount - processed);
            int bytesToRead = vertsThisChunk * stride;
            int readTotal = 0;
            while (readTotal < bytesToRead)
            {
                int n = fs.Read(readBuf, readTotal, bytesToRead - readTotal);
                if (n <= 0) throw new IOException("Unexpected end of PLY file.");
                readTotal += n;
            }

            Span<byte> chunk = readBuf.AsSpan(0, bytesToRead);

            for (int v = 0; v < vertsThisChunk; v++)
            {
                int b = v * stride;
                int gi = processed + v;

                float px = ReadF(chunk, b + posX);
                float py = ReadF(chunk, b + posY);
                float pz = ReadF(chunk, b + posZ);

                boundsMin.x = Mathf.Min(boundsMin.x, px); boundsMax.x = Mathf.Max(boundsMax.x, px);
                boundsMin.y = Mathf.Min(boundsMin.y, py); boundsMax.y = Mathf.Max(boundsMax.y, py);
                boundsMin.z = Mathf.Min(boundsMin.z, pz); boundsMax.z = Mathf.Max(boundsMax.z, pz);

                int po = gi * 12;
                WriteF(posData, po, px);
                WriteF(posData, po + 4, py);
                WriteF(posData, po + 8, pz);

                // rotation: file stores (w,x,y,z); normalize then reorder to Unity's (x,y,z,w).
                float qw = ReadF(chunk, b + r0);
                float qx = ReadF(chunk, b + r1);
                float qy = ReadF(chunk, b + r2);
                float qz = ReadF(chunk, b + r3);
                float qlen = Mathf.Sqrt(qw * qw + qx * qx + qy * qy + qz * qz);
                if (qlen > 1e-8f) { qw /= qlen; qx /= qlen; qy /= qlen; qz /= qlen; }
                else { qw = 1; qx = qy = qz = 0; }

                float sx = Mathf.Abs(Mathf.Exp(ReadF(chunk, b + s0)));
                float sy = Mathf.Abs(Mathf.Exp(ReadF(chunk, b + s1)));
                float sz = Mathf.Abs(Mathf.Exp(ReadF(chunk, b + s2)));

                WriteCovariance(covData, gi * 24, qx, qy, qz, qw, sx, sy, sz);

                // Store the raw DC coefficient, unconverted - OxiSplatCompute.compute's
                // EvalSH applies the SH_C0 * dc + 0.5 conversion itself, combined with the
                // higher-order SH bands. Converting here too would double-apply it.
                float cr = ReadF(chunk, b + dc0);
                float cg = ReadF(chunk, b + dc1);
                float cb = ReadF(chunk, b + dc2);
                float rawOpacity = ReadF(chunk, b + opacityOff);
                float opacity = 1f / (1f + Mathf.Exp(-rawOpacity));

                int co = gi * 16;
                WriteF(colorOpacityData, co, cr);
                WriteF(colorOpacityData, co + 4, cg);
                WriteF(colorOpacityData, co + 8, cb);
                WriteF(colorOpacityData, co + 12, opacity);

                if (hasFullSH)
                {
                    for (int k = 0; k < 45; k++)
                        restBuf[k] = ReadF(chunk, b + restOffsets[k]);

                    if (sourceIsCoefficientMajor)
                    {
                        // Already R,G,B-per-coefficient (a re-export from the stock
                        // package's own exporter) - use as-is, no transpose.
                        for (int k = 0; k < 45; k++)
                            shOut[k] = restBuf[k];
                    }
                    else
                    {
                        // Standard PLY stores f_rest channel-major (15 R, then 15 G, then
                        // 15 B); we store coefficient-major (R,G,B per coefficient) for EvalSH.
                        for (int k = 0; k < 15; k++)
                        {
                            shOut[k * 3 + 0] = restBuf[k];
                            shOut[k * 3 + 1] = restBuf[k + 15];
                            shOut[k * 3 + 2] = restBuf[k + 30];
                        }
                    }

                    int so = gi * ShRestCount * 3 * 4;
                    for (int k = 0; k < 45; k++)
                        WriteF(shData, so + k * 4, shOut[k]);
                }
            }

            processed += vertsThisChunk;
            if (EditorUtility.DisplayCancelableProgressBar("Importing Gaussian Splat",
                    $"{processed:N0} / {vertexCount:N0} splats", (float)processed / vertexCount))
            {
                throw new OperationCanceledException("Import cancelled.");
            }
        }

        if (!hasFullSH)
            Debug.LogWarning("OxiSplatPlyImporter: PLY has no f_rest_0..44 properties - splats will render with flat (DC-only) color, no view-dependent shading.");

        OxiSplatAsset asset = AssetDatabase.LoadAssetAtPath<OxiSplatAsset>(assetSavePath);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<OxiSplatAsset>();
            AssetDatabase.CreateAsset(asset, assetSavePath);
        }

        asset.splatCount = vertexCount;
        asset.bounds = new Bounds((boundsMin + boundsMax) * 0.5f, boundsMax - boundsMin);
        asset.positionData = posData;
        asset.covarianceData = covData;
        asset.colorOpacityData = colorOpacityData;
        asset.shData = shData;

        EditorUtility.SetDirty(asset);
        AssetDatabase.SaveAssets();

        Debug.Log($"OxiSplatPlyImporter: imported {vertexCount:N0} splats from {plyPath} -> {assetSavePath}");
    }

    static (int vertexCount, List<string> properties, int headerLength) ReadHeader(FileStream fs)
    {
        byte[] probe = new byte[1 << 16];
        int n = fs.Read(probe, 0, probe.Length);
        string text = Encoding.ASCII.GetString(probe, 0, n);
        int endIdx = text.IndexOf("end_header\n", StringComparison.Ordinal);
        if (endIdx < 0)
            throw new IOException("Could not find end_header in PLY (header longer than 64KB or not binary_little_endian?).");
        int headerLength = endIdx + "end_header\n".Length;
        string header = text.Substring(0, headerLength);

        if (!header.Contains("format binary_little_endian"))
            throw new IOException("Only binary_little_endian PLY files are supported.");

        int vertexCount = 0;
        List<string> properties = new List<string>();
        foreach (string rawLine in header.Split('\n'))
        {
            string line = rawLine.Trim();
            if (line.StartsWith("element vertex"))
                vertexCount = int.Parse(line.Substring("element vertex".Length).Trim());
            else if (line.StartsWith("property float"))
                properties.Add(line.Substring("property float".Length).Trim());
            else if (line.StartsWith("property ") && !line.StartsWith("property list"))
                throw new IOException($"Unsupported PLY property type: '{line}' - only 'float' properties are supported.");
        }

        if (vertexCount <= 0)
            throw new IOException("PLY file has no 'element vertex' declaration.");

        return (vertexCount, properties, headerLength);
    }

    static float ReadF(Span<byte> data, int offset) => MemoryMarshal.Read<float>(data.Slice(offset, 4));

    static void WriteF(byte[] dst, int offset, float value)
    {
        MemoryMarshal.Write(dst.AsSpan(offset, 4), ref value);
    }

    // Precomputes the 3D covariance (rotation * scale, symmetric 3x3 stored as two float3)
    // once at import time since it's static per splat - saves per-frame GPU work.
    static void WriteCovariance(byte[] dst, int offset, float qx, float qy, float qz, float qw, float sx, float sy, float sz)
    {
        float r00 = 1 - 2 * (qy * qy + qz * qz);
        float r01 = 2 * (qx * qy - qw * qz);
        float r02 = 2 * (qx * qz + qw * qy);
        float r10 = 2 * (qx * qy + qw * qz);
        float r11 = 1 - 2 * (qx * qx + qz * qz);
        float r12 = 2 * (qy * qz - qw * qx);
        float r20 = 2 * (qx * qz - qw * qy);
        float r21 = 2 * (qy * qz + qw * qx);
        float r22 = 1 - 2 * (qx * qx + qy * qy);

        // M = R * S (S diagonal), cov = M * M^T
        float m00 = r00 * sx, m01 = r01 * sy, m02 = r02 * sz;
        float m10 = r10 * sx, m11 = r11 * sy, m12 = r12 * sz;
        float m20 = r20 * sx, m21 = r21 * sy, m22 = r22 * sz;

        float c00 = m00 * m00 + m01 * m01 + m02 * m02;
        float c01 = m00 * m10 + m01 * m11 + m02 * m12;
        float c02 = m00 * m20 + m01 * m21 + m02 * m22;
        float c11 = m10 * m10 + m11 * m11 + m12 * m12;
        float c12 = m10 * m20 + m11 * m21 + m12 * m22;
        float c22 = m20 * m20 + m21 * m21 + m22 * m22;

        WriteF(dst, offset, c00);
        WriteF(dst, offset + 4, c01);
        WriteF(dst, offset + 8, c02);
        WriteF(dst, offset + 12, c11);
        WriteF(dst, offset + 16, c12);
        WriteF(dst, offset + 20, c22);
    }
}
