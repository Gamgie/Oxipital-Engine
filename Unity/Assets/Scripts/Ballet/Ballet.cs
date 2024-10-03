using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Oxipital
{
    public class Ballet : MonoBehaviour
    {
        OrbManager orbManager;
        StandardForceManager standardForceManager;

        void OnEnable()
        {
            orbManager = GetComponentInChildren<OrbManager>();
            standardForceManager = GetComponentInChildren<StandardForceManager>();
        }

        // Update is called once per frame
        void Update()
        {
            //Dictionary<int, GraphicsBuffer> forceBuffers = new Dictionary<int, GraphicsBuffer>(standardForceManager.items.Count);
            //foreach (StandardForceGroup group in standardForceManager.items)
            //{
            //    forceBuffers.Add(group.bufferID, group.buffer);
            //}

            //foreach (OrbGroup group in orbManager.items)
            //{
            //    group.setForceBuffers(forceBuffers);
            //}
        }
    }
}
