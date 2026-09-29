using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace Oxipital
{
    // Where the 2D optical flow lives in the 3D scene : a unit quad in the local XY plane.
    // Its scale is driven by OpticalFlowManager.planeScale (height in meters, width from the Spout texture aspect ratio),
    // scale Z stays 1 so the depth band is in meters.
    //
    // The plane is drawn as a real wireframe (geometry, not a gizmo) so it shows in every camera, including the
    // dome / Spout output : the plane outline, plus the depth band box where particles are pushed.
    [ExecuteAlways]
    public class FlowPlane : MonoBehaviour
    {
        [Header("Wireframe")]
        public bool showWireframe = true;
        public bool showDepthBand = true;
        public Color wireframeColor = new Color(0, 1, 1);
        // Display brightness : 1 = wireframeColor as is, whatever the camera exposure
        [Range(0, 10)]
        public float wireframeIntensity = 1;
        // Line thickness in meters
        [Range(.001f, .2f)]
        public float lineWidth = .02f;

        public Matrix4x4 WorldToPlane => transform.worldToLocalMatrix;
        public Vector3 Right => transform.right;
        public Vector3 Up => transform.up;

        static readonly int UnlitColorID = Shader.PropertyToID("_UnlitColor");
        static readonly int EmissiveExposureWeightID = Shader.PropertyToID("_EmissiveExposureWeight");

        OpticalFlowManager manager;

        GameObject wireObject;
        MeshRenderer wireRenderer;
        Mesh wireMesh;
        Material wireMaterial;
        bool warnedShader;

        // What the current mesh was built for
        Vector3 builtSize;
        float builtDepth = -1;
        float builtWidth;
        bool builtDepthBand;

        void OnEnable()
        {
            manager = GetComponentInParent<OpticalFlowManager>();
            if (manager == null) manager = FindFirstObjectByType<OpticalFlowManager>();
        }

        void OnDisable()
        {
            DestroyWireframe();
        }

        void Update()
        {
            // In play mode the manager applies the scale itself (before Ballet reads the plane)
            if (!Application.isPlaying && manager != null) manager.ApplyPlaneScale();
        }

        void LateUpdate()
        {
            UpdateWireframe();
        }

        void UpdateWireframe()
        {
            if (!showWireframe)
            {
                if (wireObject != null) wireObject.SetActive(false);
                return;
            }

            if (wireObject == null && !CreateWireframe()) return;
            wireObject.SetActive(true);

            // The mesh is built in meters under a child that cancels the plane scale, so lines keep their thickness
            Vector3 size = transform.localScale;
            wireObject.transform.localPosition = Vector3.zero;
            wireObject.transform.localRotation = Quaternion.identity;
            wireObject.transform.localScale = new Vector3(Inverse(size.x), Inverse(size.y), Inverse(size.z));

            float depth = manager != null ? manager.depthBand * size.z : 0;
            bool depthBand = showDepthBand && depth > 0;
            if (size != builtSize || depth != builtDepth || lineWidth != builtWidth || depthBand != builtDepthBand)
            {
                BuildMesh(size, depthBand ? depth : 0, lineWidth);
                builtSize = size;
                builtDepth = depth;
                builtWidth = lineWidth;
                builtDepthBand = depthBand;
            }

            // Emissive with exposure weight 0 : the color is displayed as is, independent of the scene exposure
            HDMaterial.SetEmissiveColor(wireMaterial, wireframeColor * wireframeIntensity);
        }

        bool CreateWireframe()
        {
            Shader shader = Shader.Find("HDRP/Unlit");
            if (shader == null)
            {
                if (!warnedShader) Debug.LogWarning("FlowPlane : HDRP/Unlit shader not found, wireframe disabled", this);
                warnedShader = true;
                return false;
            }

            wireMaterial = new Material(shader) { name = "Flow Plane Wireframe", hideFlags = HideFlags.HideAndDontSave };
            HDMaterial.SetSurfaceType(wireMaterial, false);
            wireMaterial.SetColor(UnlitColorID, Color.black);
            wireMaterial.SetFloat(EmissiveExposureWeightID, 0);
            HDMaterial.ValidateMaterial(wireMaterial);

            wireMesh = new Mesh { name = "Flow Plane Wireframe", hideFlags = HideFlags.HideAndDontSave };

            wireObject = new GameObject("Flow Plane Wireframe", typeof(MeshFilter), typeof(MeshRenderer));
            wireObject.hideFlags = HideFlags.HideAndDontSave;
            wireObject.transform.SetParent(transform, false);
            wireObject.GetComponent<MeshFilter>().sharedMesh = wireMesh;

            wireRenderer = wireObject.GetComponent<MeshRenderer>();
            wireRenderer.sharedMaterial = wireMaterial;
            wireRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            wireRenderer.receiveShadows = false;
            wireRenderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            wireRenderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;

            builtDepth = -1; // force a rebuild
            return true;
        }

        void DestroyWireframe()
        {
            DestroySafe(wireObject);
            DestroySafe(wireMesh);
            DestroySafe(wireMaterial);
            wireObject = null;
            wireMesh = null;
            wireMaterial = null;
        }

        static void DestroySafe(Object o)
        {
            if (o == null) return;
            if (Application.isPlaying) Destroy(o);
            else DestroyImmediate(o);
        }

        static float Inverse(float v) => Mathf.Abs(v) > 1e-5f ? 1 / v : 0;

        // Plane outline (z = 0) and, when depth > 0, the depth band box (z = -depth .. +depth).
        // Every edge is a square bar of width w, extended by w / 2 at both ends so the corners are closed.
        void BuildMesh(Vector3 size, float depth, float w)
        {
            float hx = size.x * .5f;
            float hy = size.y * .5f;

            var vertices = new System.Collections.Generic.List<Vector3>();
            var indices = new System.Collections.Generic.List<int>();

            AddRectangle(vertices, indices, hx, hy, 0, w);
            if (depth > 0)
            {
                AddRectangle(vertices, indices, hx, hy, -depth, w);
                AddRectangle(vertices, indices, hx, hy, depth, w);
                for (int i = 0; i < 4; i++)
                {
                    float x = (i & 1) == 0 ? -hx : hx;
                    float y = (i & 2) == 0 ? -hy : hy;
                    AddBar(vertices, indices, new Vector3(x, y, -depth), new Vector3(x, y, depth), w);
                }
            }

            wireMesh.Clear();
            wireMesh.SetVertices(vertices);
            wireMesh.SetTriangles(indices, 0);
            wireMesh.RecalculateNormals();
            wireMesh.RecalculateBounds();
        }

        static void AddRectangle(System.Collections.Generic.List<Vector3> vertices, System.Collections.Generic.List<int> indices, float hx, float hy, float z, float w)
        {
            AddBar(vertices, indices, new Vector3(-hx, -hy, z), new Vector3(hx, -hy, z), w);
            AddBar(vertices, indices, new Vector3(-hx, hy, z), new Vector3(hx, hy, z), w);
            AddBar(vertices, indices, new Vector3(-hx, -hy, z), new Vector3(-hx, hy, z), w);
            AddBar(vertices, indices, new Vector3(hx, -hy, z), new Vector3(hx, hy, z), w);
        }

        // Axis aligned box around the segment a -> b
        static void AddBar(System.Collections.Generic.List<Vector3> vertices, System.Collections.Generic.List<int> indices, Vector3 a, Vector3 b, float w)
        {
            Vector3 min = Vector3.Min(a, b) - Vector3.one * (w * .5f);
            Vector3 max = Vector3.Max(a, b) + Vector3.one * (w * .5f);

            int start = vertices.Count;
            for (int i = 0; i < 8; i++)
                vertices.Add(new Vector3((i & 1) == 0 ? min.x : max.x, (i & 2) == 0 ? min.y : max.y, (i & 4) == 0 ? min.z : max.z));

            // Corner index = x + 2y + 4z, faces wound clockwise seen from outside
            int[] faces =
            {
                0, 2, 3, 0, 3, 1, // -Z
                4, 5, 7, 4, 7, 6, // +Z
                0, 4, 6, 0, 6, 2, // -X
                1, 3, 7, 1, 7, 5, // +X
                0, 1, 5, 0, 5, 4, // -Y
                2, 6, 7, 2, 7, 3, // +Y
            };
            foreach (int f in faces) indices.Add(start + f);
        }

        // Editor only : flow direction (the plane itself is drawn by the wireframe)
        void OnDrawGizmos()
        {
            if (manager == null || !Application.isPlaying) return;

            OpticalFlowMetrics metrics = manager.GetComponent<OpticalFlowMetrics>();
            if (metrics == null) return;

            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.yellow;
            Vector3 direction = new Vector3(metrics.averageDirection.x, metrics.averageDirection.y, 0) * .5f;
            Gizmos.DrawLine(Vector3.zero, direction);
            Gizmos.DrawWireSphere(direction, .02f);
        }
    }
}
