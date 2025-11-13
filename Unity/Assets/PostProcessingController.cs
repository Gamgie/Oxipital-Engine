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
    
    private Bloom bloom;

    [Range(0,1)]
    public float bloomIntensity;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        postProcessVolume.profile.TryGet<Bloom>(out bloom);
    }

    // Update is called once per frame
    void Update()
    {
		bloom.intensity.value = bloomIntensity;
        outdoorScene.weight = outdoorWeight;
        postProcessVolume.weight = postProcessWeight;
	}
}
