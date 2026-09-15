using UnityEngine;

public class SolarSystemController : MonoBehaviour
{
    public GameObject sun;
    public GameObject[] moons;
    public GameObject mainMoon;
    public GameObject earth;

    [Range(0, 1)] public float sunOpacity = 1.0f;
    [Range(0, 1)] public float moonOpacity = 1.0f;
    [Range(0, 1)] public float mainMoonOpacity = 1.0f;
    [Range(0, 1)] public float earthOpacity = 1.0f;

    Renderer sunRenderer;
    Renderer[] moonRenderers;
    Renderer mainMoonRenderer;
    Renderer earthRenderer;

    MaterialPropertyBlock mpb;

    // valeurs pr�c�dentes pour �viter les �critures inutiles
    float lastSunOpacity = -1f;
    float lastMoonOpacity = -1f;
    float lastMainMoonOpacity = -1f;
    float lastEarthOpacity = -1f;

    void Start()
    {

        mpb = new MaterialPropertyBlock();

        if (sun != null)
            sunRenderer = sun.GetComponent<Renderer>();

        if (moons != null && moons.Length > 0)
        {
            moonRenderers = new Renderer[moons.Length];
            for (int i = 0; i < moons.Length; i++)
                moonRenderers[i] = moons[i].GetComponent<Renderer>();
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

        if (moonOpacity != lastMoonOpacity)
        {
            SetMoonsOpacity(moonOpacity);
            lastMoonOpacity = moonOpacity;
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

    // Valeurs de blend HDRP (UnityEngine.Rendering.BlendMode) : One = 1, Zero = 0, OneMinusSrcAlpha = 10.
    // Ce sont les m�mes valeurs que celles d�j� pr�sentes dans les .mat (SrcBlend/DstBlend en mode Transparent).
    const int BlendOne = 1;
    const int BlendZero = 0;
    const int BlendOneMinusSrcAlpha = 10;

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
        if (opaque)
        {
            mat.SetFloat("_SurfaceType", 0f); // 0 = Opaque (HDRP Lit)
            mat.SetOverrideTag("RenderType", "Opaque");
            mat.SetInt("_SrcBlend", BlendOne);
            mat.SetInt("_DstBlend", BlendZero);
            mat.SetInt("_ZWrite", 1);
            mat.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry;
        }
        else
        {
            mat.SetFloat("_SurfaceType", 1f); // 1 = Transparent (HDRP Lit)
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.SetInt("_SrcBlend", BlendOne);
            mat.SetInt("_DstBlend", BlendOneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }
    }

    void SetMoonsOpacity(float opacity)
    {
        if (moons == null || moonRenderers == null) return;

        for (int i = 0; i < moons.Length; i++)
            SetOpacity(moons[i], moonRenderers[i], opacity);
    }
}