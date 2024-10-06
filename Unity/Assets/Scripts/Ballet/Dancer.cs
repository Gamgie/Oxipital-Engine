using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OSCQuery;

namespace Oxipital
{
    public class Dancer : BaseItem
    {
        [Range(0, 1)]
        public float weight = 0;

        [Range(0, 1)]
        public float intensity = 1;

        [Range(0, 20)]
        public float size = 1;

        //for randomized speed in patterns
        [DoNotExpose]
        public float localPatternTime = 0;
        [DoNotExpose]
        public float randomFactor = 0;

        //for physics based patterns
        Vector3 lastPosition;
        [DoNotExpose]
        public Vector3 velocity;

        [DoNotExpose]
        [HideInInspector]
        internal Color debugColor = Color.red;

        internal Quaternion targetRotation;

        virtual public void Start()
        {
            randomFactor = Random.value;
        }

        override protected void Update()
        {
            base.Update();
            lastPosition = transform.localPosition;
        }

        private void OnDrawGizmos()
        {
            float w = weight * (1 - killProgress);

            Color c = Color.Lerp(Color.gray, debugColor, intensity);
            Color color = Color.Lerp(new Color(1.0f, 0, 0, 0), c, w);
            Gizmos.color = color;
            Gizmos.DrawWireSphere(transform.position, size);
        }


    }
}