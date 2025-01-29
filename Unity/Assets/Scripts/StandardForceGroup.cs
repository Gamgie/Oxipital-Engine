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

        [Range(0, 1)]
        [InBuffer(4)]
        public float radialInOut;


        [Header("Axial")]
        [Range(0, 1)]
        [InBuffer(5)]
        public float axialIntensity;

        [Range(1, 3)]
        [InBuffer(6)]
        public float axialFactor;

        [InBuffer(7)]
        public Vector3 axisMultiplier = Vector3.up;
        [InBuffer(10)]
        public Vector3 axialFrequency;

        [Header("Linear")]
        [Range(0, 1)]
        [InBuffer(13)]
        public float linearIntensity;


        [Header("Orthoradial")]
        [Range(0, 1)]
        [InBuffer(14)]
        public float orthoIntensity;

        [Range(0, 1)]
        [InBuffer(15)]
        public float orthoInnerRadius = 0.5f;

        [Range(1, 3)]
        [InBuffer(16)]
        public float orthoFactor = 2;

        [Range(-1, 1)]
        [InBuffer(17)]
        public float orthoClockwise;


        [Header("Turbulence Curl")]
        [Range(0, 1)]
        [InBuffer(18)]
        public float curlIntensity;

        [Range(0, 5)]
        [InBuffer(19)]
        public float curlFrequency;

        [InBuffer(20)]
        public float curlOctaves;

        [Range(0, 1)]
        [InBuffer(21)]
        public float curlRoughness;

        [Range(0, 1)]
        [InBuffer(22)]
        public float curlLacunarity;

        [Range(0, 1)]
        [InBuffer(23)]
        public float curlScale;

        [Range(0, 1)]
        [InBuffer(24)]
        public float curlTranslation;


        [Header("Perlin")]
        [Range(0, 1)]
        [InBuffer(25)]
        public float perlinIntensity;

        [Range(0, 5)]
        [InBuffer(26)]
        public float perlinFrequency;

        [Range(1, 8)]
        [InBuffer(27)]
        public float perlinOctaves;

        [Range(0, 1)]
        [InBuffer(28)]
        public float perlinRoughness;

        [Range(0, 1)]
        [InBuffer(29)]
        public float perlinLacunarity;

        [Range(0, 1)]
        [InBuffer(30)]
        public float perlinTranslationSpeed;


        [Header("Orthoaxial")]
        [Range(0, 1)]
        [InBuffer(31)]
        public float orthoaxialIntensity;

        [Range(0, 1)]
        [InBuffer(32)]
        public float orthoaxialInnerRadius;

        [Range(1, 3)]
        [InBuffer(33)]
        public float orthoaxialFactor;

        [Range(-1, 1)]
        [InBuffer(34)]
        public float orthoaxialClockwise = 1;


        protected override Type getGroupType()
        {
            return GetType();
        }
    }
}