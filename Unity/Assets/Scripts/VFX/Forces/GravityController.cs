using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;


public class GravityController : ForceController
{
    private void OnEnable()
    {
        key = "Gravity";
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
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
