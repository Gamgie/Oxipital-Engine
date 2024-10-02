using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DancePattern : MonoBehaviour
{
    public enum BlendMode { Add, Multiply, Replace }
    public BlendMode blendMode;

    [Range(0, 1)]
    public float weight = 1;


    public virtual void updatePattern(DancerGroup group)
    {
        if (group.dancers.Count == 0) return;

        List<Vector3> positions = getPatternPositions(group);

        for (int i = 0; i < group.dancers.Count; i++)
        {
            Dancer dancer = group.dancers[i];
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

            float rel = i / group.count;

            float pRelSize = Mathf.Lerp(1, rel, group.patternSizeSpread);
            float pSize = Mathf.Lerp(0, group.patternSize, pRelSize);

            float targetSize = pSize + Mathf.Sin(Time.time * group.patternSizeLFOFrequency) * group.patternSizeLFOAmplitude;
            dancer.transform.localPosition = Vector3.Lerp(dancer.transform.position, targetPos, smoothWeight) * targetSize;
        }
    }

    protected virtual List<Vector3> getPatternPositions(DancerGroup group)
    {
        List<Vector3> positions = new List<Vector3>();
        for (int i = 0; i < group.dancers.Count; i++) positions.Add(Vector3.zero);
        return positions;
    }
}

public class LinePattern : DancePattern
{
    override protected List<Vector3> getPatternPositions(DancerGroup group)
    {
        List<Vector3> positions = new List<Vector3>();

        if (group.dancers.Count <= 1)
        {
            positions.Add(Vector3.zero);
            return positions;
        }

        int fullDancerCount = group.getFullDancersCount(); //count of full dancers
        int dancerCount = group.dancers.Count; //count of all dancers, potential one more than full if count is not a whole number
        float relativeProgression = group.getCountRelativeProgression(); //progression between full dancers

        for (int i = 0; i < dancerCount; i++)
        {

            Vector3 posAtFull = fullDancerCount == 1 ? Vector3.zero : Vector3.Lerp(Vector3.down, Vector3.up, (float)i / (fullDancerCount - 1));
            Vector3 posAtNextFull = Vector3.Lerp(Vector3.down, Vector3.up, (float)(i) / (dancerCount - 1));
            positions.Add(Vector3.Lerp(posAtFull, posAtNextFull, relativeProgression));
        }

        return positions;
    }
}

public class CirclePattern : DancePattern
{
    override protected List<Vector3> getPatternPositions(DancerGroup group)
    {
        List<Vector3> positions = new List<Vector3>();

        int fullDancerCount = group.getFullDancersCount(); //count of full dancers
        int dancerCount = group.dancers.Count; //count of all dancers, potential one more than full if count is not a whole number
        float relativeProgression = group.getCountRelativeProgression(); //progression between full dancers

        Random.InitState(1);
        for (int i = 0; i < dancerCount; i++)
        {
            float tAtFull = (float)i / fullDancerCount;
            float tAtNextFull = (float)(i) / dancerCount;

            float rel = i / group.count;
            float initT = Mathf.Lerp(tAtFull, tAtNextFull, relativeProgression) + group.patternTimeOffset * rel;
            float normalT = initT + group.patternTime;
            float randomT = initT + group.dancers[i].localPatternTime;

            Vector3 normalPos = new Vector3(Mathf.Cos(normalT * Mathf.PI * 2), 0, Mathf.Sin(normalT * Mathf.PI * 2));
            Vector3 randomPos = new Vector3(Mathf.Cos(randomT * Mathf.PI * 2), 0, Mathf.Sin(randomT * Mathf.PI * 2));

            Vector3 targetPos = Vector3.Lerp(normalPos, randomPos, group.patternSpeedRandom);

            Quaternion rotation = Random.rotation;
            targetPos = Vector3.Lerp(targetPos, rotation * targetPos, group.patternAxisSpread);


            positions.Add(targetPos);
        }

        return positions;
    }
}