using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class SolarSystemController : MonoBehaviour
{
    public GameObject sun;
    public GameObject[] moonsSpace;
    public GameObject[] moonsGaussian;
    public GameObject mainMoon;
    public GameObject earth;

    [Range(0, 1)] public float sunOpacity = 1.0f;
    [Range(0, 1)] public float moonSpaceOpacity = 1.0f;
    [Range(0, 1)] public float moonGaussianOpacity = 1.0f;
    [Range(0, 1)] public float mainMoonOpacity = 1.0f;
    [Range(0, 1)] public float earthOpacity = 1.0f;

    Renderer sunRenderer;
    Renderer[] moonSpaceRenderers;
    Renderer[] moonGaussianRenderers;
    Renderer mainMoonRenderer;
    Renderer earthRenderer;

    MaterialPropertyBlock mpb;

    // valeurs pr�c�dentes pour �viter les �critures inutiles
    float lastSunOpacity = -1f;
    float lastMoonSpaceOpacity = -1f;
    float lastMoonGaussianOpacity = -1f;
    float lastMainMoonOpacity = -1f;
    float lastEarthOpacity = -1f;

    void Start()
    {

        mpb = new MaterialPropertyBlock();

        if (sun != null)
            sunRenderer = sun.GetComponent<Renderer>();

        if (moonsSpace != null && moonsSpace.Length > 0)
        {
            moonSpaceRenderers = new Renderer[moonsSpace.Length];
            for (int i = 0; i < moonsSpace.Length; i++)
                moonSpaceRenderers[i] = moonsSpace[i].GetComponent<Renderer>();
        }

        if (moonsGaussian != null && moonsGaussian.Length > 0)
        {
            moonGaussianRenderers = new Renderer[moonsGaussian.Length];
            for (int i = 0; i < moonsGaussian.Length; i++)
                moonGaussianRenderers[i] = moonsGaussian[i].GetComponent<Renderer>();
        }

        if (mainMoon != null)
            mainMoonRenderer = mainMoon.GetComponent<Renderer>();

        if (earth != null)
            earthRenderer = earth.GetComponent<Renderer>();
    }

    void Update()
    {
        if (sunOpacity != lastSunOpacity)
        {
            SetOpacity(sun, sunRenderer, sunOpacity);
            lastSunOpacity = sunOpacity;
        }

        if (moonSpaceOpacity != lastMoonSpaceOpacity)
        {
            SetMoonsSpaceOpacity(moonSpaceOpacity);
            lastMoonSpaceOpacity = moonSpaceOpacity;
        }

        if (moonGaussianOpacity != lastMoonGaussianOpacity)
        {
            SetMoonsGaussianOpacity(moonGaussianOpacity);
            lastMoonGaussianOpacity = moonGaussianOpacity;
        }

        if (mainMoonOpacity != lastMainMoonOpacity)
        {
            SetOpacity(mainMoon, mainMoonRenderer, mainMoonOpacity);
            lastMainMoonOpacity = mainMoonOpacity;
        }

        if (earthOpacity != lastEarthOpacity)
        {
            SetOpacity(earth, earthRenderer, earthOpacity);
            lastEarthOpacity = earthOpacity;
        }
    }

    void SetOpacity(GameObject go, Renderer renderer, float opacity)
    {
        // Objet totalement transparent : on le d�sactive pour �viter que la
        // lumi�re touche encore ses bords (silhouette visible malgr� alpha = 0).
        bool shouldBeActive = opacity != 0f;
        if (go != null && go.activeSelf != shouldBeActive)
            go.SetActive(shouldBeActive);

        if (renderer == null) return;

        Material mat = renderer.material;

        // R�cup�re la couleur actuelle pour ne modifier que l'alpha
        Color baseColor = mat.color;
        baseColor.a = opacity;
        mat.color = baseColor;

        // Alpha = 1 => objet totalement opaque (plus de blending), ce qui r��crit
        // correctement le depth buffer et �vite le fight en Z avec les particules.
        // Alpha < 1 => on repasse en mode Transparent pour permettre le fondu.
        SetSurfaceOpaque(mat, opacity >= 1f);
    }

    void SetSurfaceOpaque(Material mat, bool opaque)
    {
        // Flipping _SurfaceType alone isn't enough for HDRP/Lit - it also needs its
        // blend keywords, render pass and stencil bits (deferred lighting routing)
        // rebuilt consistently, which is what HDMaterial.SetSurfaceType does. Doing
        // this by hand (as before) left the stencil state stuck in "opaque" mode, so
        // the transparent pass just never rendered the object instead of fading it.
        HDMaterial.SetSurfaceType(mat, !opaque);
    }

    void SetMoonsSpaceOpacity(float opacity)
    {
        if (moonsSpace == null || moonSpaceRenderers == null) return;

        for (int i = 0; i < moonsSpace.Length; i++)
            SetOpacity(moonsSpace[i], moonSpaceRenderers[i], opacity);
    }

    void SetMoonsGaussianOpacity(float opacity)
    {
        if (moonsGaussian == null || moonGaussianRenderers == null) return;

        for (int i = 0; i < moonsGaussian.Length; i++)
            SetOpacity(moonsGaussian[i], moonGaussianRenderers[i], opacity);
    }
}