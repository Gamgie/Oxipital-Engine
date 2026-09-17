using UnityEngine;
using GaussianSplatting.Runtime;

[ExecuteAlways]
[RequireComponent(typeof(GaussianSplatRenderer))]
public class GaussianSplatVisibility : MonoBehaviour
{
    [Tooltip("Splat scale to use when visibility is 1")]
    public float maxScale = 1f;
    [Range(0f, 1f)] public float visibility = 1f;

    GaussianSplatRenderer splatRenderer;

    void OnEnable()
    {
        splatRenderer = GetComponent<GaussianSplatRenderer>();
    }

    void Update()
    {
        if (splatRenderer == null)
            splatRenderer = GetComponent<GaussianSplatRenderer>();

        Apply();
    }

    void Apply()
    {
        bool shouldBeEnabled = visibility > 0f;
        if (splatRenderer.enabled != shouldBeEnabled)
            splatRenderer.enabled = shouldBeEnabled;

        if (shouldBeEnabled)
        {
            splatRenderer.m_OpacityScale = visibility;
            splatRenderer.m_SplatScale = visibility * maxScale;
        }
    }

    public void FadeIn() => visibility = 1f;
    public void FadeOut() => visibility = 0f;
}
