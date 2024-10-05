using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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

        override protected void Update()
        {
            base.Update();
            debugColor = GetComponentInParent<OrbGroup>().color;
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

            //Update intensity here as we need to pass it outside the GraphicsBuffer
            //Discussion : https://discussions.unity.com/t/spawn-a-variable-amount-of-particles-from-graphics-buffer/899049/2
            bool isDying = killProgress > 0;
            vfx.SetFloat("Emitter Intensity", isDying ? 0 : intensity);
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

        internal void setMesh(Mesh m)
        {
            if (vfx == null) return;
            if (m == null) return;

            if (!vfx.HasMesh("Emitter Mesh"))
            {
                Debug.LogWarning("Emitter Mesh not found in VFX");
                return;
            }

            vfx.SetMesh("Emitter Mesh", m);
            GetComponent<MeshToSDF>().mesh = m;
        }
        public override void kill(float time)
        {
            base.kill(time);
            if(vfx == null) return;
            vfx.SetFloat("Emitter Intensity", 0);
        }
    }
}