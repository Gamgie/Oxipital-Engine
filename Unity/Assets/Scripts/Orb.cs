using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;


namespace Oxipital
{
    [RequireComponent(typeof(VisualEffect))]
    public class Orb : Dancer
    {

        VisualEffect vfx;

        protected void OnEnable()
        {
            if (vfx == null) vfx = GetComponent<VisualEffect>();
        }

        internal void setForceBuffers(Dictionary<string, GraphicsBuffer> forceBuffers)
        {
            if (vfx == null) return;

            foreach (var b in forceBuffers)
            {
                if (!vfx.HasGraphicsBuffer(b.Key))
                {
                    Debug.LogWarning(b.Key + " not found in VFX");
                    continue;
                }

                vfx.SetGraphicsBuffer(b.Key, b.Value);
            }
        }

        internal void setOrbBuffer(GraphicsBuffer buffer, int index)
        {
            if (vfx == null) return;

            if (!vfx.HasGraphicsBuffer("Orb Buffer"))
            {
                Debug.LogWarning("Orb Buffer not found in VFX");
                return;
            }

            vfx.SetInt("Orb Index", index);
            vfx.SetGraphicsBuffer("Orb Buffer", buffer);
        }
    }
}