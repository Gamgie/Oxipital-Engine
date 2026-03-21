using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Oxipital
{
    public class DancePattern : MonoBehaviour
    {
        public enum BlendMode { Add, Multiply, Replace }
        public BlendMode blendMode;

        [Range(0, 1)]
        public float weight = 1;
        [Range(0, 1)]
        public float speedWeight = 1;
        public virtual void updatePattern<T>(DancerGroup<T> group) where T : Dancer
        {
            if (group.items.Count == 0) return;
            if (weight == 0) return;

            List<Vector3> positions = getPatternPositions(group);

            for (int i = 0; i < group.items.Count; i++)
            {
                Dancer dancer = group.items[i];
                if (dancer == null) continue;

                Vector3 targetPos = Vector3.zero;
                switch (blendMode)
                {
                    case BlendMode.Add:
                        targetPos = dancer.transform.localPosition + positions[i];
                        break;
                    case BlendMode.Multiply:
                        targetPos = Vector3.Scale(dancer.transform.localPosition, positions[i]);
                        break;
                    case BlendMode.Replace:
                        targetPos = positions[i];
                        break;

                }

                float smoothWeight = Mathf.Sin(weight * Mathf.PI - Mathf.PI / 2) * 0.5f + 0.5f;

                float rel = i*1f / group.count;

                float pRelSize = Mathf.Lerp(1, rel, group.patternSizeSpread);
                float pSize = Mathf.Lerp(0, group.patternSize, pRelSize);

                dancer.transform.localPosition = Vector3.Lerp(dancer.transform.localPosition, targetPos, smoothWeight) * pSize;
            }
        }

        protected virtual List<Vector3> getPatternPositions<T>(DancerGroup<T> group) where T : Dancer
        {
            List<Vector3> positions = new List<Vector3>();
            for (int i = 0; i < group.items.Count; i++) positions.Add(Vector3.zero);
            return positions;
        }
    }

}