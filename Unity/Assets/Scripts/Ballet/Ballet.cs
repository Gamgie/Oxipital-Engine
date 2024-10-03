using Oxipital;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ballet : MonoBehaviour
{
    BaseManager<OrbGroup> orbManager;
    BaseManager<StandardForceGroup> standardForceManager;

    void OnEnable()
    {
        Transform orbTransform = transform.Find("Orbs");
        if (orbTransform == null) orbTransform = new GameObject("Orbs").transform;
        orbTransform.parent = transform;
        if (orbManager == null) orbManager = orbTransform.GetComponent<BaseManager<OrbGroup>>();
        if (orbManager == null) orbManager = orbTransform.gameObject.AddComponent<BaseManager<OrbGroup>>();

        Transform standardForceTransform = transform.Find("Forces");
        if (standardForceTransform == null) standardForceTransform = new GameObject("Forces").transform;
        standardForceTransform.parent = transform;
        if (standardForceManager == null) standardForceManager = standardForceTransform.GetComponent<BaseManager<StandardForceGroup>>();
        if (standardForceManager == null) standardForceManager = standardForceTransform.gameObject.AddComponent<BaseManager<StandardForceGroup>>();
    }

    // Update is called once per frame
    void Update()
    {
        Dictionary<int, GraphicsBuffer> forceBuffers = new Dictionary<int, GraphicsBuffer>(standardForceManager.items.Count);
        foreach (StandardForceGroup group in standardForceManager.items) forceBuffers.Add(group.bufferID, group.buffer);

        foreach(OrbGroup group in orbManager.items)
        {
            group.setForceBuffers(forceBuffers);
        }
    }
}
