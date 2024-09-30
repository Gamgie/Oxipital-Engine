using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class SpiralController : ForceController
{
    [Header("Spiral")]
    public float frequency;
    public float sinwaveIntensity = 0.6f;
    public float verticalForce = 1f;

	private void OnEnable()
	{
        key = "Spiral";
	}

	// Update is called once per frame
	protected override void Update()
    {
        base.Update();

        foreach (VisualEffect visualEffect in _vfxs)
        {
            // Frequency
            if (visualEffect.HasFloat(key + " Frequency" + _suffix))
                visualEffect.SetFloat(key + " Frequency" + _suffix, frequency);

            // Alpha
            if (visualEffect.HasFloat(key + " Alpha" + _suffix))
                visualEffect.SetFloat(key + " Alpha" + _suffix, sinwaveIntensity);

            // Vertical Force
            if (visualEffect.HasFloat(key + " Vertical Force" + _suffix))
                visualEffect.SetFloat(key + " Vertical Force" + _suffix, verticalForce);
        }
    }

    public override ForceControllerData StoreData()
    {
        ForceControllerData data = new ForceControllerData();
        base.StoreBaseData(data);

        //Spiral Parameters
        data.additionalParameters = new List<AdditionalParameters>();
        data.additionalParameters.Add(new AdditionalParameters("sinwaveIntensity", sinwaveIntensity));
        data.additionalParameters.Add(new AdditionalParameters("frequency", frequency));
        data.additionalParameters.Add(new AdditionalParameters("verticalForce", verticalForce));

        return data;
    }

    public override void LoadData(ForceControllerData data)
    {
        base.LoadBaseData(data);

        //Spiral Parameters
        foreach (var pair in data.additionalParameters)
        {
            switch (pair.key)
            {
                case "frequency":
                    frequency = pair.floatParameter;
                    break;
                case "sinwaveIntensity":
                    sinwaveIntensity = pair.floatParameter;
                    break;
                case "verticalForce":
                    verticalForce = pair.floatParameter;
                    break;
                default:
                    break;
            }
        }
    }

}
