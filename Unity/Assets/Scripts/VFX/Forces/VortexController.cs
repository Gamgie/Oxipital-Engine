using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;


public class VortexController : ForceController
{
    [Header("Vortex")]
    [Range(0, 1)]
    public float innerRadius = 0.5f;
    public bool clockwise = true;
    [Range(0, 1)]
    public float orthoradialIntensity = 1f;
    [Range(0, 1)]
    public float cylindricIntensity = 1f;
    public bool squaredOrthoradial = true;

	private void OnEnable()
	{
        key = "Vortex";
    }

	// Update is called once per frame
	protected override void Update()
    {
        base.Update();

        foreach (VisualEffect visualEffect in _vfxs)
        {
            // Orthoradial Intensity
            if (visualEffect.HasFloat(key + " Orthoradial Intensity" + _suffix))
                visualEffect.SetFloat(key + " Orthoradial Intensity" + _suffix, orthoradialIntensity);

            // Cylindric Intensity
            if (visualEffect.HasFloat(key + " Cylindric Intensity" + _suffix))
                visualEffect.SetFloat(key + " Cylindric Intensity" + _suffix, cylindricIntensity);

            // Clockwise
            if (visualEffect.HasBool(key + " Clockwise" + _suffix))
                visualEffect.SetBool(key + " Clockwise" + _suffix, clockwise);

            // Vortex Squared Orthoradial
            if (visualEffect.HasBool(key + " Squared Orthoradial" + _suffix))
                visualEffect.SetBool(key + " Squared Orthoradial" + _suffix, clockwise);

            // Inner Radius
            if (visualEffect.HasFloat(key + " Inner Radius" + _suffix))
                visualEffect.SetFloat(key + " Inner Radius" + _suffix, innerRadius);
        }
    }

    public override ForceControllerData StoreData()
	{
        ForceControllerData data = new ForceControllerData();
        base.StoreBaseData(data);

        // Vortex Param
        data.additionalParameters = new List<AdditionalParameters>();
        data.additionalParameters.Add(new AdditionalParameters("innerRadius", innerRadius));
        data.additionalParameters.Add(new AdditionalParameters("clockwise", Convert.ToSingle(clockwise)));
        data.additionalParameters.Add(new AdditionalParameters("Squared Orthoradial", Convert.ToSingle(squaredOrthoradial)));
        data.additionalParameters.Add(new AdditionalParameters("orthoradialIntensity", orthoradialIntensity));
        data.additionalParameters.Add(new AdditionalParameters("cylindricIntensity", cylindricIntensity));

        return data;
    }

    public override void LoadData(ForceControllerData data)
	{
        base.LoadBaseData(data);

        //Vortex Parameters
        foreach (var pair in data.additionalParameters)
        {
            switch (pair.key)
            {
                case "innerRadius":
                    innerRadius = pair.floatParameter;
                    break;
                case "clockwise":
                    clockwise = Convert.ToBoolean(pair.floatParameter);
                    break;
                case "squaredOrthoradial":
                    squaredOrthoradial = Convert.ToBoolean(pair.floatParameter);
                    break;
                case "orthoradialIntensity":
                    orthoradialIntensity = pair.floatParameter;
                    break;
                case "cylindricIntensity":
                    cylindricIntensity = pair.floatParameter;
                    break;
                default:
                    break;
            }
        }
    }
}