using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class RadialController : ForceController
{
    [Header("Radial")]
    [Range(0, 3)]
    public float radialFrequency;
    [Range(0,1)]
    public float radialSmoothness = 1;
    [Range(0, 3)]
    public float sphericalFrequency;
    [Range(0, 1)]
    public float sphericalSmoothness = 1;

    private void OnEnable()
    {
        key = "Attractor";
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        foreach (VisualEffect visualEffect in _vfxs)
        {

            // Radial Frequency
            if (visualEffect.HasFloat(key + " Radial Frequency" + _suffix))
                visualEffect.SetFloat(key + " Radial Frequency" + _suffix, radialFrequency);

            // Radial Smoothness
            if (visualEffect.HasFloat(key + " Radial Smoothness" + _suffix))
                visualEffect.SetFloat(key + " Radial Smoothness" + _suffix, radialSmoothness);

            // Spherical Frequency
            if (visualEffect.HasFloat(key + " Spherical Frequency" + _suffix))
                visualEffect.SetFloat(key + " Spherical Frequency" + _suffix, sphericalFrequency);

            // Spherical Smoothness
            if (visualEffect.HasFloat(key + " Spherical Smoothness" + _suffix))
                visualEffect.SetFloat(key + " Spherical Smoothness" + _suffix, sphericalSmoothness);
        }
    }

    public override ForceControllerData StoreData()
    {
        ForceControllerData data = new ForceControllerData();
        base.StoreBaseData(data);

        data.additionalParameters = new List<AdditionalParameters>();
        data.additionalParameters.Add(new AdditionalParameters("radialFrequency", radialFrequency));
        data.additionalParameters.Add(new AdditionalParameters("radialSmoothness", radialSmoothness));
        data.additionalParameters.Add(new AdditionalParameters("sphericalFrequency", sphericalFrequency));
        data.additionalParameters.Add(new AdditionalParameters("sphericalSmoothness", sphericalSmoothness));

        return data;
    }

    public override void LoadData(ForceControllerData data)
    {
        base.LoadBaseData(data);

        foreach (var pair in data.additionalParameters)
        {
            switch (pair.key)
            {
                case "radialFrequency":
                    radialFrequency = pair.floatParameter;
                    break;
                case "radialSmoothness":
                    radialSmoothness = pair.floatParameter;
                    break;
                case "sphericalFrequency":
                    sphericalFrequency = pair.floatParameter;
                    break;
                case "sphericalSmoothness":
                    sphericalSmoothness = pair.floatParameter;
                    break;
                default:
                    break;
            }
        }
    }
}
