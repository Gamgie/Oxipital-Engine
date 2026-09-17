using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class PostProcessingController : MonoBehaviour
{

    public Volume postProcessVolume;
	[Range(0, 1)]
	public float postProcessWeight;
    public Volume outdoorScene;
	[Range(0, 1)]
	public float outdoorWeight;
    [Range(0, 20)]
    public float skyboxExposure;

    private Bloom bloom;
    private HDRISky hdrSky;
    private ScreenSpaceLensFlare lensFlare;

    [Range(0,1)]
    public float bloomIntensity;

    [Range(0, 20)]
    public float lensFlareIntensity;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        postProcessVolume.profile.TryGet<Bloom>(out bloom);
        postProcessVolume.profile.TryGet<HDRISky>(out hdrSky);
        postProcessVolume.profile.TryGet<ScreenSpaceLensFlare>(out lensFlare);
    }

    // Update is called once per frame
    void Update()
    {
		bloom.intensity.value = bloomIntensity;
        outdoorScene.weight = 1-postProcessWeight;
        postProcessVolume.weight = postProcessWeight;
        hdrSky.exposure.value = skyboxExposure;
        lensFlare.intensity.value = lensFlareIntensity;

    }
}
