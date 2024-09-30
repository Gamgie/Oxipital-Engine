using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class PerlinController : ForceController
{
    public enum PerlinNoiseType { Perlin, Cellular }

    [Header("Perlin")]
    public PerlinNoiseType noiseType = PerlinNoiseType.Perlin;
    [Range(0, 3f)]
    public float evolutionSpeed = 0;
    [Range(0,3)]
    public float frequency = 1;
    [Range(1,8)]
    public int octaves = 1;
    [Range(0,1)]
    public float roughness = 0.5f;
    [Range(0,5)]
    public float lacunarity = 2;
    [Range(-1,1)]
    public float minRange = -1;
    [Range(-1, 1)]
    public float maxRange = 1;

    private void OnEnable()
    {
        key = "Perlin";
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        foreach (VisualEffect visualEffect in _vfxs)
        {
            // Evolution Speed
            if (visualEffect.HasFloat(key + " Evolution Speed" + _suffix))
                visualEffect.SetFloat(key + " Evolution Speed" + _suffix, evolutionSpeed);

            // Frequency
            if (visualEffect.HasFloat(key + " Frequency") == true)
                visualEffect.SetFloat(key + " Frequency", frequency);

            // Octaves
            if (visualEffect.HasInt(key + " Octaves") == true)
                visualEffect.SetInt(key + " Octaves", octaves);

            // Roughness
            if (visualEffect.HasFloat(key + " Roughness") == true)
                visualEffect.SetFloat(key + " Roughness", roughness);

            // Lacunarity
            if (visualEffect.HasFloat(key + " Lacunarity") == true)
                visualEffect.SetFloat(key + " Lacunarity", lacunarity);

            // Min Range
            if (visualEffect.HasFloat(key + " Min Range") == true)
                visualEffect.SetFloat(key + " Min Range", minRange);

            // Max Range
            if (visualEffect.HasFloat(key + " Max Range") == true)
                visualEffect.SetFloat(key + " Max Range", maxRange);

            // Noise Type
            if (visualEffect.HasInt(key + " Noise Type") == true)
                visualEffect.SetInt(key + " Noise Type", (int) noiseType);
        }
    }

    public override ForceControllerData StoreData()
    {
        ForceControllerData data = new ForceControllerData();
        base.StoreBaseData(data);

        data.additionalParameters = new List<AdditionalParameters>();
        data.additionalParameters.Add(new AdditionalParameters("noiseType", Convert.ToInt32(noiseType)));
        data.additionalParameters.Add(new AdditionalParameters("evolutionSpeed", evolutionSpeed));
        data.additionalParameters.Add(new AdditionalParameters("frequency", frequency));
        data.additionalParameters.Add(new AdditionalParameters("roughness", roughness));
        data.additionalParameters.Add(new AdditionalParameters("lacunarity", lacunarity));
        data.additionalParameters.Add(new AdditionalParameters("minRange", minRange));
        data.additionalParameters.Add(new AdditionalParameters("maxRange", maxRange));
        data.additionalParameters.Add(new AdditionalParameters("octaves", octaves));

        return data;
    }

    public override void LoadData(ForceControllerData data)
    {
        base.LoadBaseData(data);

        // Perlin Parameters
        foreach (var pair in data.additionalParameters)
        {
            switch(pair.key)
			{
                case "noiseType":
                    noiseType = (PerlinNoiseType) pair.floatParameter;
                    break;
                case "evolutionSpeed":
                    evolutionSpeed = pair.floatParameter;
                    break;
                case "frequency":
                    frequency = pair.floatParameter;
                    break;
                case "roughness":
                    roughness = pair.floatParameter;
                    break;
                case "lacunarity":
                    lacunarity = pair.floatParameter;
                    break;
                case "minRange":
                    minRange = pair.floatParameter;
                    break;
                case "maxRange":
                    maxRange = pair.floatParameter;
                    break;
                case "octaves":
                    octaves = (int) pair.floatParameter;
                    break;
                default:
                    break;
			}
        }
    }
}
