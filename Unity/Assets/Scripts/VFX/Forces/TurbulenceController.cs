using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;


public class TurbulenceController : ForceController
{
    [Header("Turbulence")]
    public float turbFrequency = 1;
    [Range(1, 8)]
    public int turbOctave = 1;
    [Range(0, 1f)]
    public float turbroughness = 0.5f;
    [Range(0, 5f)]
    public float turbLacunarity = 2;
    [Range(0, 5f)]
    public float turbScale = 1;
    [Range(0, 1f)]
    public float turbEvolutionSpeed = 1;

    private void OnEnable()
    {
        key = "Turb";
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        foreach (VisualEffect visualEffect in _vfxs)
        {
            // Turb parameter update
            if (visualEffect.HasFloat(key + " Frequency") == true)
                visualEffect.SetFloat(key + " Frequency", turbFrequency);

            if (visualEffect.HasInt("Octave") == true)
                visualEffect.SetInt("Octave", turbOctave);

            if (visualEffect.HasFloat("Roughness") == true)
                visualEffect.SetFloat("Roughness", turbroughness);

            if (visualEffect.HasFloat("Lacunarity") == true)
                visualEffect.SetFloat("Lacunarity", turbLacunarity);

            if (visualEffect.HasFloat(key + " Scale") == true)
                visualEffect.SetFloat(key + " Scale", turbScale);

            if (visualEffect.HasFloat(key + " Evolution Speed") == true)
                visualEffect.SetFloat(key + " Evolution Speed", turbEvolutionSpeed);
        }
    }

    public override ForceControllerData StoreData()
    {
        ForceControllerData data = new ForceControllerData();
        base.StoreBaseData(data);

        // Turbulence Parameters
        data.additionalParameters = new List<AdditionalParameters>();
        data.additionalParameters.Add(new AdditionalParameters("turbFrequency", turbFrequency));
        data.additionalParameters.Add(new AdditionalParameters("turbOctave", turbOctave));
        data.additionalParameters.Add(new AdditionalParameters("turbroughness", turbroughness));
        data.additionalParameters.Add(new AdditionalParameters("turbLacunarity", turbLacunarity));
        data.additionalParameters.Add(new AdditionalParameters("turbScale", turbScale));
        data.additionalParameters.Add(new AdditionalParameters("turbEvolutionSpeed", turbEvolutionSpeed));

        return data;
    }

    public override void LoadData(ForceControllerData data)
    {
        base.LoadBaseData(data);

        // Turbulence Parameters
        foreach (var pair in data.additionalParameters)
        {
            switch (pair.key)
            {
                case "turbFrequency":
                    turbFrequency = pair.floatParameter;
                    break;
                case "turbOctave":
                    turbOctave = (int) pair.floatParameter;
                    break;
                case "turbroughness":
                    turbroughness = pair.floatParameter;
                    break;
                case "turbLacunarity":
                    turbLacunarity = pair.floatParameter;
                    break;
                case "turbScale":
                    turbScale = pair.floatParameter;
                    break;
                case "turbEvolutionSpeed":
                    turbEvolutionSpeed = pair.floatParameter;
                    break;
                default:
                    break;
            }
        }
    }
}
