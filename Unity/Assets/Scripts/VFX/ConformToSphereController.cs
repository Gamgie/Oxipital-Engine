using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class ConformToSphereController : ForceController
{
    public Transform sphereCenter;
    public float attractionSpeed = 5;
    public float attractionForce = 20;
    public float stickDistance = 0.1f;
    public float stickForce = 50;

    // Update is called once per frame
    protected override void Update()
    {
        if (_vfxs == null)
            return;

        foreach (VisualEffect visualEffect in _vfxs)
        {
            if (visualEffect.HasVector3("CTS Center" + _suffix))
                visualEffect.SetVector3("CTS Center" + _suffix, sphereCenter.position);

            if (visualEffect.HasFloat("CTS Radius" + _suffix))
                visualEffect.SetFloat("CTS Radius" + _suffix, radius);

            if (visualEffect.HasFloat("CTS Attraction Speed" + _suffix))
                visualEffect.SetFloat("CTS Attraction Speed" + _suffix, attractionSpeed);

            if (visualEffect.HasFloat("CTS Attraction Force" + _suffix))
                visualEffect.SetFloat("CTS Attraction Force" + _suffix, attractionForce);

            if (visualEffect.HasFloat("CTS Stick Distance" + _suffix))
                visualEffect.SetFloat("CTS Stick Distance" + _suffix, stickDistance);

            if (visualEffect.HasFloat("CTS Stick Force" + _suffix))
                visualEffect.SetFloat("CTS Stick Force" + _suffix, stickForce);
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
