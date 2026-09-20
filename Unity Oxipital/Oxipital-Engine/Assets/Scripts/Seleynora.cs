using System.Drawing;
using UnityEngine;

public class Seleynora : MonoBehaviour
{
    public Light pointLight;
    [Range(0,1)] public float pointLightIntensity;
    public float maxIntensity;

    [Header("Grogu")]
    public float groguHeight;
    public GameObject groguObject;

    [Header("Emergency Sphere")]
    public Renderer emergencySphere;
    [Range(0,1)] public float emergencySphereVisibility;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        pointLight.intensity = maxIntensity * pointLightIntensity;
        groguObject.transform.localPosition = new Vector3(groguObject.transform.localPosition.x, groguObject.transform.localPosition.y, groguHeight);
        UnityEngine.Color color = emergencySphere.material.color;
        color.a = emergencySphereVisibility;
        emergencySphere.material.color = color;
    }
}
