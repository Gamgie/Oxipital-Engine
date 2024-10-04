using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.VFX;

namespace Oxipital
{
    public class StandardForceGroup : DancerGroup<Dancer>
    {
        [Space(20)]
        [Header("General Parameters")]
        [Range(0, 1)]
        [InBuffer(0)]
        public float forceFactorInside = 1;
        [Range(0, 1)]
        [InBuffer(1)]
        public float forceFactorOutside = 0;

        [Header("Radial")]
        [Range(0, 1)]
        [InBuffer(2)]
        public float radialIntensity;

        [Range(0, 1)]
        [InBuffer(3)]
        public float radialFrequency;


        [Header("Axial")]
        [Range(0, 1)]
        [InBuffer(4)]
        public float axialIntensity;


        [Range(1, 3)]
        [InBuffer(7)]
        public float axialFactor;

        [InBuffer(8)]
        public Vector3 axialFrequency;


        [Header("Linear")]
        [Range(0, 1)]
        [InBuffer(11)]
        public float linearIntensity;


        [Header("Orthoradial")]
        [Range(0, 1)]
        [InBuffer(12)]
        public float orthoIntensity;

        [Range(0, 1)]
        [InBuffer(13)]
        public float orthoInnerRadius = 0.5f;

        [Range(1, 3)]
        [InBuffer(14)]
        public float orthoFactor = 2;

        [Range(0, 1)]
        [InBuffer(15)]
        public float orthoClockwise;


        [Header("Turbulence Curl")]
        [Range(0, 1)]
        [InBuffer(16)]
        public float curlIntensity;

        [Range(0, 5)]
        [InBuffer(17)]
        public float curlFrequency;

        [Range(0, 1)]
        [InBuffer(18)]
        public float curlDrag;

        [Range(1, 8)]
        [InBuffer(19)]
        public float curlOctaves;

        [Range(0, 1)]
        [InBuffer(20)]
        public float curlRoughness;

        [Range(0, 1)]
        [InBuffer(21)]
        public float curlLacunarity;

        [Range(0, 1)]
        [InBuffer(22)]
        public float curlScale;

        [Range(0, 1)]
        [InBuffer(23)]
        public float curlTranslation;


        [Header("Perlin")]
        [Range(0, 1)]
        [InBuffer(24)]
        public float perlinIntensity;

        [Range(0, 5)]
        [InBuffer(25)]
        public float perlinFrequency;

        [Range(1, 8)]
        [InBuffer(26)]
        public float perlinOctaves;

        [Range(0, 1)]
        [InBuffer(27)]
        public float perlinRoughness;

        [Range(0, 1)]
        [InBuffer(28)]
        public float perlinLacunarity;

        [Range(0, 1)]
        [InBuffer(29)]
        public float perlinTranslationSpeed;


        [Header("Orthoaxial")]
        [Range(0, 1)]
        [InBuffer(30)]
        public float orthoaxialIntensity;

        [Range(0, 1)]
        [InBuffer(31)]
        public float orthoaxialInnerRadius;

        [Range(1, 3)]
        [InBuffer(32)]
        public float orthoaxialFactor;

        [Range(0, 1)]
        [InBuffer(33)]
        public float orthoaxialClockwise;


        protected override Type getGroupType()
        {
            return GetType();
        }
    }
}