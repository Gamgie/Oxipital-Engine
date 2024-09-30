using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;


public class AxialController : ForceController
{
    [Header("Axial")]
    [Range(0, 20)]
    public float frequencyX = 0;
    [Range(0, 20)]
    public float frequencyY = 0;
    [Range(0, 20)]
    public float frequencyZ = 0;

    private void OnEnable()
    {
        key = "Axial";
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        foreach (VisualEffect visualEffect in _vfxs)
        {
            // FrequencyX
            if (visualEffect.HasFloat(key + " FrequencyX") == true)
                visualEffect.SetFloat(key + " FrequencyX", frequencyX);

            // FrequencyY
            if (visualEffect.HasFloat(key + " FrequencyY") == true)
                visualEffect.SetFloat(key + " FrequencyY", frequencyY);

            // FrequencyZ
            if (visualEffect.HasFloat(key + " FrequencyZ") == true)
                visualEffect.SetFloat(key + " FrequencyZ", frequencyZ);
        }
    }

    public override ForceControllerData StoreData()
    {
        ForceControllerData data = new ForceControllerData();
        base.StoreBaseData(data);

        data.additionalParameters = new List<AdditionalParameters>();
        data.additionalParameters.Add(new AdditionalParameters("FrequencyX",frequencyX));
        data.additionalParameters.Add(new AdditionalParameters("FrequencyY", frequencyY));
        data.additionalParameters.Add(new AdditionalParameters("FrequencyZ", frequencyZ));

        return data;
    }

    public override void LoadData(ForceControllerData data)
    {
        base.LoadBaseData(data);

        foreach (var pair in data.additionalParameters)
        {
            if(pair.key == "FrequencyX")
			{
                frequencyX = pair.floatParameter;
			}
            else if (pair.key == "FrequencyY")
            {
                frequencyY = pair.floatParameter;
            }
            else if (pair.key == "FrequencyZ")
            {
                frequencyZ = pair.floatParameter;
            }
        }
     }
}
