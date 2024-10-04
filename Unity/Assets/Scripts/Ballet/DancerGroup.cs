using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using OSCQuery;
using System.Reflection;
using System.Runtime.InteropServices;
using System;

namespace Oxipital
{

    //public interface IDancerGroup<out T> where T : Dancer
    //{
    //    BaseManager<Dancer> getManager();
    //}

    [DoNotExposeChildren]
    public class DancerGroup<T> : BaseManager<T> where T : Dancer
    {
        const int MAX_DANCERS = 16;
        public const int DANCER_DATA_SIZE = 8;

        [Header("Patterns")]
        [Range(0, 20)]
        public float patternSize = 1; // Size of this pattern

        [Range(0, 1)]
        public float patternSizeSpread = 0;
        [Range(0, 1)]
        public float patternAxisSpread = 0;


        [Header("Animation")]
        [Range(-1, 1)]
        public float patternSpeed = 1f; // speed of the choreography
        [Range(0, 1)]
        public float patternSpeedRandom = 0;

        //[DoNotExpose]
        [HideInInspector]
        [DoNotExpose]
        public float patternTime = 0;

        [Range(0, 1)]
        public float patternTimeOffset = 0; // offset phase


        [Range(0, 10)]
        public float patternSizeLFOFrequency;
        [Range(0, 10)]
        public float patternSizeLFOAmplitude;

        [Header("Dancer")]
        [Range(0, 20)]
        public float dancerSize = 1;
        [Range(0, 1)]
        public float dancerHyperSize = 0;
        [Range(0, 1)]
        public float dancerSizeSpread = 0;

        [Range(0, 1)]
        public float dancerWeightSizeFactor = 0;
        [Range(0, 1)]
        public float dancerIntensity = 1;
        [Range(0, 1)]
        public float dancerWeightIntensityFactor = 0;

        public Vector3 dancerLookAt = Vector3.up;
        [Range(0, 2)]
        public float dancerLookAtMode = 0; // 0 = local, 1 = group, 2 = global

        List<DancePattern> patterns;

        //Graphics buffer

        public GraphicsBuffer buffer;

        Dictionary<int, FieldInfo> fieldInfos;
        int groupFixedDataSize;

        [Header("Debug")]
        [DoNotExpose]
        public Color debugColor = Color.red;
        public DancerGroup(string itemName = "Dancer") : base(itemName)
        {

        }

        override protected void OnEnable()
        {
            init();
        }

        virtual protected void OnDisable()
        {
            clear();
        }

        void init()
        {
            items = GetComponentsInChildren<T>().ToList();
            patterns = GetComponents<DancePattern>().ToList();
            initBufferAndFieldInfoList();
        }

        void clear()
        {
            buffer.Release();
        }

        public void initBufferAndFieldInfoList()
        {

            fieldInfos = new Dictionary<int, FieldInfo>();

            Type type = getGroupType();

            int lastGroupFloatIndex = 0;
            foreach (FieldInfo f in type.GetFields())
            {
                InBuffer inBufferAttribute = f.GetCustomAttribute<InBuffer>();
                if (inBufferAttribute == null) continue;

                fieldInfos[inBufferAttribute.index] = f;

                int fieldNumFloats = 1;
                if (f.FieldType == typeof(Vector3)) fieldNumFloats = 3;
                if (f.FieldType == typeof(Vector4)) fieldNumFloats = 4;
                if (f.FieldType == typeof(Color)) fieldNumFloats = 3;
                lastGroupFloatIndex = inBufferAttribute.index + fieldNumFloats;
            }

            // Instantiate buffers
            if (buffer == null)
            {
                groupFixedDataSize = lastGroupFloatIndex + 1;
                int maxBufferSize = 1 //dancer count
                    + 1 //items start index
                    + groupFixedDataSize
                    + MAX_DANCERS * DANCER_DATA_SIZE;

                buffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, maxBufferSize, Marshal.SizeOf(typeof(float)));
            }
        }


        protected override void Update()
        {
            base.Update();
            if (buffer == null) init();

            patternTime += Time.deltaTime * patternSpeed;

            for (int i = 0; i < items.Count; i++)
            {
                Dancer d = items[i];
                d.weight = i <= count - 1 ? 1 : getCountRelativeProgression();
                d.intensity = dancerIntensity * Mathf.Lerp(1, d.weight, dancerWeightIntensityFactor);

                float dancerSizeSpreadFactor = Mathf.Lerp(1, i / count, dancerSizeSpread);
                d.size = dancerSize * dancerSizeSpreadFactor * Mathf.Lerp(1, d.weight, dancerWeightSizeFactor);
                d.size = Mathf.Lerp(d.size, 100, dancerHyperSize);

                d.transform.localPosition = Vector3.zero;

                d.localPatternTime += Time.deltaTime * (patternSpeed * Mathf.Lerp(1, .5f + d.randomFactor, patternSpeedRandom));
                
                d.debugColor = debugColor;
            }


            foreach (var p in patterns)
            {
                p.updatePattern(this);
            }


            foreach (var d in items)
            {
                Vector3 localLookAtTarget = transform.TransformPoint(d.transform.localPosition + dancerLookAt);
                Vector3 groupLookAtTarget = transform.TransformPoint(dancerLookAt);
                Vector3 absoluteLookAtTarget = dancerLookAt;
                Vector3 target = dancerLookAtMode < 1 ? Vector3.Lerp(localLookAtTarget, groupLookAtTarget, dancerLookAtMode) :
                                Vector3.Lerp(groupLookAtTarget, absoluteLookAtTarget, dancerLookAtMode - 1);
                d.transform.LookAt(target);
            }

            //Update buffer with new data
            buffer.SetData(getList());
        }

        //Helpers
        protected virtual Type getGroupType() { return GetType(); }

        float[] getList()
        {
            int indexOffset = 2; // first 2 are dancer count and group start index
            float[] list = new float[indexOffset + groupFixedDataSize + items.Count * DANCER_DATA_SIZE];

            int itemsStartIndex = indexOffset + groupFixedDataSize - 1;

            list[0] = items.Count;
            list[1] = itemsStartIndex;

            //fill fixed data
            foreach (var f in fieldInfos)
            {
                if (f.Value == null) continue;
                int index = f.Key + indexOffset;

                if (f.Value.FieldType == typeof(Vector3))
                {
                    Vector3 v = (Vector3)f.Value.GetValue(this);
                    list[index] = v.x;
                    list[index + 1] = v.y;
                    list[index + 2] = v.z;
                }
                else if (f.Value.FieldType == typeof(Vector4))
                {
                    Vector4 v = (Vector4)f.Value.GetValue(this);
                    list[index] = v.x;
                    list[index + 1] = v.y;
                    list[index + 2] = v.z;
                    list[index + 3] = v.w;
                }
                else if (f.Value.FieldType == typeof(Color))
                {
                    Color v = (Color)f.Value.GetValue(this);
                    list[index] = v.r;
                    list[index + 1] = v.g;
                    list[index + 2] = v.b;

                }
                else if (f.Value.FieldType == typeof(Single))
                {
                    var val = f.Value.GetValue(this);
                    list[index] = (float)val;
                }
                else if (f.Value.FieldType == typeof(Boolean))
                {
                    list[index] = (bool)f.Value.GetValue(this) ? 1 : 0;
                }
                else if (f.Value.FieldType == typeof(float))
                {
                    list[index] = (float)f.Value.GetValue(this);
                }
                else if (f.Value.FieldType == typeof(int))
                {
                    list[index] = (int)f.Value.GetValue(this);
                }
                else if (f.Value.FieldType == typeof(double))
                {
                    list[index] = (float)(double)f.Value.GetValue(this);
                }
                else if (f.Value.FieldType.BaseType == typeof(Enum))
                {
                    list[index] = (float)(int)f.Value.GetValue(this);

                }
            }

            //fill dancer data
            for (int i = 0; i < items.Count; i++)
            {
                Dancer d = items[i];
                list[itemsStartIndex + i * DANCER_DATA_SIZE] = d.transform.position.x;
                list[itemsStartIndex + i * DANCER_DATA_SIZE + 1] = d.transform.position.y;
                list[itemsStartIndex + i * DANCER_DATA_SIZE + 2] = d.transform.position.z;
                list[itemsStartIndex + i * DANCER_DATA_SIZE + 3] = d.transform.eulerAngles.x;
                list[itemsStartIndex + i * DANCER_DATA_SIZE + 4] = d.transform.eulerAngles.y;
                list[itemsStartIndex + i * DANCER_DATA_SIZE + 5] = d.transform.eulerAngles.z;
                list[itemsStartIndex + i * DANCER_DATA_SIZE + 6] = d.intensity;
                list[itemsStartIndex + i * DANCER_DATA_SIZE + 7] = d.size;
            }

            return list;
        }
        public int getFullDancersCount() { return Mathf.FloorToInt(count); }
        public float getCountRelativeProgression() { return count - getFullDancersCount(); }
    }
}