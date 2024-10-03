using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Oxipital
{
    public class CircleDancePattern : DancePattern
    {
        override protected List<Vector3> getPatternPositions(DancerGroup<Dancer> group)
        {
            List<Vector3> positions = new List<Vector3>();

            int fullDancerCount = group.getFullDancersCount(); //count of full dancers
            int dancerCount = group.items.Count; //count of all dancers, potential one more than full if count is not a whole number
            float relativeProgression = group.getCountRelativeProgression(); //progression between full dancers

            Random.InitState(1);
            for (int i = 0; i < dancerCount; i++)
            {
                float tAtFull = (float)i / fullDancerCount;
                float tAtNextFull = (float)(i) / dancerCount;

                float rel = i / group.count;
                float initT = Mathf.Lerp(tAtFull, tAtNextFull, relativeProgression) + group.patternTimeOffset * rel;
                float normalT = initT + group.patternTime;
                float randomT = initT + group.items[i].localPatternTime;

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
}