using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using System;


namespace Oxipital
{
    public class OrbGroup : DancerGroup<Orb>
    {
        [Header("Emission")]
        [InBuffer(0)]
        [Range(0, 40)]
        public float life = 20;

        public enum EmitterShape { Sphere, Plane, Torus, Cube, Pipe, Egg, Line, Circle, Merkaba, Pyramid, Landscape }
        [InBuffer(1)]
        public EmitterShape emitterShape;

        [InBuffer(2)]
        [Range(0, 1)]
        public float emitterSurfaceFactor = 0;

        [InBuffer(3)]
        [Range(0, 1)]
        public float emitterVolumeFactor = 0;

        [Header("Appearance")]
        [InBuffer(4)]
        public Color color = Color.white;

        [InBuffer(7)]
        [Range(0, 1)]
        public float alpha = .5f;

        [InBuffer(8)]
        [Range(0, 1)]
        public float hdrMultiplier = 1;

        [InBuffer(9)]
        [Range(0, 1)]
        public float alphaSpeedThreshold = 0;

        [InBuffer(10)]
        [Range(0, 1)]
        public float textureOpacity = 0;

        [Header("Physics")]
        [InBuffer(11)]
        [Range(0, 1)]
        public float forceWeight = 1;

        [InBuffer(12)]
        [Range(0, 1)]
        public float drag = .5f;

        [InBuffer(13)]
        [Range(0, 1)]
        public float velocityDrag = 0;

        [InBuffer(14)]
        [Range(0, 1)]
        public float noisyDrag = 0;

        [InBuffer(15)]
        [Range(0, 1)]
        public float noisyDragFrequency = 0;

        [InBuffer(16)]
        public bool activateCollision = false;

        [Header("Debug")]
        public bool showMesh = false;


        //OrbGroup() : base("Orb") { }

        protected override void Update()
        {
            base.Update();
            for (int i = 0; i < items.Count; i++)
            {
                items[i].setOrbBuffer(buffer, i);
            }
        }

        public void setForceBuffers(Dictionary<string, GraphicsBuffer> forceBuffers)
        {
            foreach (Orb orb in items)
            {
                orb.setForceBuffers(forceBuffers);
            }
        }
        override protected Type getGroupType()
        {
            return GetType();
        }
    }
}


