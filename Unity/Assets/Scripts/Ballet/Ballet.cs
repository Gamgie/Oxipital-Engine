using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Oxipital
{
    public class Ballet : MonoBehaviour
    {
        OrbManager orbManager;
        StandardForceManager standardForceManager;

        public float totalParticles;
        void OnEnable()
        {
            orbManager = GetComponentInChildren<OrbManager>();
            standardForceManager = GetComponentInChildren<StandardForceManager>();
        }

        // Update is called once per frame
        void Update()
        {
            // Gather all force buffer
			Dictionary<string, GraphicsBuffer> forceBuffers = new Dictionary<string, GraphicsBuffer>(standardForceManager.items.Count);
			foreach (StandardForceGroup group in standardForceManager.items)
			{
				forceBuffers.Add(group.gameObject.name, group.buffer);
			}

            // Send it to orbs and compute total particle count
            totalParticles = 0;
            foreach (OrbGroup group in orbManager.items)
            {
                group.setForceBuffers(forceBuffers);
                totalParticles += group.vfx.aliveParticleCount;
            }
        }
    }
}
