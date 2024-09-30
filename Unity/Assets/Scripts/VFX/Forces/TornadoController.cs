using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;


public class TornadoController : ForceController
{
    [Header("Tornado")]
    public bool clockwise;
    public bool bothHemisphere;
    [Header("Vertical Force")]
    [Range(0,1)]
    public float verticalIntensity;
    [Header("Orthogonal Force")]
    [Range(0, 1)]
    public float orthoIntensity;
    public bool orthoSquaredLength;
    [Range(0, 1)]
    public float orthoInnerRadius;
    [Header("Axial Force")]
    [Range(0, 1)]
    public float axialIntensity;
    [Header("Turbulence Force")]
    [Range(0, 1)]
    public float turbulenceIntensity;
    [Range(0, 10)]
    public float turbulenceFrequency;
    [Range(0, 1)]
    public float turbulenceEvolution;

    private void OnEnable()
	{
        key = "Tornado";
    }

	// Update is called once per frame
	protected override void Update()
    {
        base.Update();

        foreach (VisualEffect visualEffect in _vfxs)
        {
            // Clockwise
            if (visualEffect.HasBool(key + " Clockwise" + _suffix))
                visualEffect.SetBool(key + " Clockwise" + _suffix, clockwise);

            // Both Hemisphere
            if (visualEffect.HasBool(key + " Both Hemisphere" + _suffix))
                visualEffect.SetBool(key + " Both Hemisphere" + _suffix, bothHemisphere);

            // Vertical Intensity
            if (visualEffect.HasFloat(key + " Vertical Intensity" + _suffix))
                visualEffect.SetFloat(key + " Vertical Intensity" + _suffix, verticalIntensity);

            // Ortho Intensity
            if (visualEffect.HasFloat(key + " Ortho Intensity" + _suffix))
                visualEffect.SetFloat(key + " Ortho Intensity" + _suffix, orthoIntensity);

            // Ortho Squared Length
            if (visualEffect.HasBool(key + " Ortho Squared Length" + _suffix))
                visualEffect.SetBool(key + " Ortho Squared Length" + _suffix, orthoSquaredLength);

            // Ortho Inner Radius
            if (visualEffect.HasFloat(key + " Ortho Inner Radius" + _suffix))
                visualEffect.SetFloat(key + " Ortho Inner Radius" + _suffix, orthoInnerRadius);

            // Axial Intensity
            if (visualEffect.HasFloat(key + " Axial Intensity" + _suffix))
                visualEffect.SetFloat(key + " Axial Intensity" + _suffix, axialIntensity);

            // Turbulence Intensity
            if (visualEffect.HasFloat(key + " Turbulence Intensity" + _suffix))
                visualEffect.SetFloat(key + " Turbulence Intensity" + _suffix, turbulenceIntensity);

            // Turbulence Frequency
            if (visualEffect.HasFloat(key + " Turbulence Frequency" + _suffix))
                visualEffect.SetFloat(key + " Turbulence Frequency" + _suffix, turbulenceFrequency);

            // Turbulence Evolution
            if (visualEffect.HasFloat(key + " Turbulence Evolution" + _suffix))
                visualEffect.SetFloat(key + " Turbulence Evolution" + _suffix, turbulenceEvolution);
        }
    }

    public override ForceControllerData StoreData()
	{
        ForceControllerData data = new ForceControllerData();
        base.StoreBaseData(data);

        data.additionalParameters = new List<AdditionalParameters>();
        data.additionalParameters.Add(new AdditionalParameters("clockwise", Convert.ToSingle(clockwise)));
        data.additionalParameters.Add(new AdditionalParameters("bothHemisphere", Convert.ToSingle(bothHemisphere)));
        data.additionalParameters.Add(new AdditionalParameters("verticalIntensity", verticalIntensity));
        data.additionalParameters.Add(new AdditionalParameters("orthoIntensity", orthoIntensity));
        data.additionalParameters.Add(new AdditionalParameters("orthoSquaredLength", Convert.ToSingle(orthoSquaredLength)));
        data.additionalParameters.Add(new AdditionalParameters("orthoInnerRadius", orthoInnerRadius));
        data.additionalParameters.Add(new AdditionalParameters("axialIntensity", axialIntensity));
        data.additionalParameters.Add(new AdditionalParameters("turbulenceIntensity", turbulenceIntensity));
        data.additionalParameters.Add(new AdditionalParameters("turbulenceFrequency", turbulenceFrequency));
        data.additionalParameters.Add(new AdditionalParameters("turbulenceEvolution", turbulenceEvolution));

        return data;
    }

    public override void LoadData(ForceControllerData data)
	{
        base.LoadBaseData(data);

        foreach (var pair in data.additionalParameters)
        {
            switch (pair.key)
            {
                case "clockwise":
                    clockwise = Convert.ToBoolean(pair.floatParameter);
                    break;
                case "bothHemisphere":
                    bothHemisphere = Convert.ToBoolean(pair.floatParameter);
                    break;
                case "verticalIntensity":
                    verticalIntensity = pair.floatParameter;
                    break;
                case "orthoIntensity":
                    orthoIntensity = pair.floatParameter;
                    break;
                case "orthoSquaredLength":
                    orthoSquaredLength = Convert.ToBoolean(pair.floatParameter);
                    break;
                case "orthoInnerRadius":
                    orthoInnerRadius = pair.floatParameter;
                    break;
                case "axialIntensity":
                    axialIntensity = pair.floatParameter;
                    break;
                case "turbulenceIntensity":
                    turbulenceIntensity = pair.floatParameter;
                    break;
                case "turbulenceFrequency":
                    turbulenceFrequency = pair.floatParameter;
                    break;
                case "turbulenceEvolution":
                    turbulenceEvolution = pair.floatParameter;
                    break;
                default:
                    break;
            }
        }
    }
}