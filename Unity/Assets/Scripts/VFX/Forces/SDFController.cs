using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class SDFController : ForceController
{
    public Texture3D texture;
    public float attractionSpeed = 5;
    public float attractionForce = 20;
    public float stickDistance = 0.1f;
    public float stickForce = 50;
    public Texture3D[] textureAvailable;


    // Update is called once per frame
    protected override void Update()
    {
        string sdf = "SDF";

        foreach(VisualEffect vfx in _vfxs)
        {
            // Texture
            if (vfx.HasTexture(sdf + " Texture" + _suffix))
                vfx.SetTexture(sdf + " Texture" + _suffix, texture);

            // Attraction Speed
            if (vfx.HasFloat(sdf + " Attraction Speed" + _suffix))
                vfx.SetFloat(sdf + " Attraction Speed" + _suffix, attractionSpeed);

            // Attraction Force
            if (vfx.HasFloat(sdf + " Attraction Force" + _suffix))
                vfx.SetFloat(sdf + " Attraction Force" + _suffix, attractionForce);

            // Stick Force
            if (vfx.HasFloat(sdf + " Stick Force" + _suffix))
                vfx.SetFloat(sdf + " Stick Force" + _suffix, stickForce);

            // Stick Distance
            if (vfx.HasFloat(sdf + " Stick Distance" + _suffix))
                vfx.SetFloat(sdf + " Stick Distance" + _suffix, stickDistance);
        }
    }

    public override ForceControllerData StoreData()
    {
        ForceControllerData data = new ForceControllerData();
        base.StoreBaseData(data);

        return data;
    }

    public override void LoadData(ForceControllerData data)
    {
        base.LoadBaseData(data);
    }
}
