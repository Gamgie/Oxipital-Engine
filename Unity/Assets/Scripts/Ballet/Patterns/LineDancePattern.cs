using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Oxipital
{
    public class LineDancePattern : DancePattern
    {
        override protected List<Vector3> getPatternPositions(DancerGroup<Dancer> group)
        {
            List<Vector3> positions = new List<Vector3>();

            if (group.items.Count <= 1)
            {
                positions.Add(Vector3.zero);
                return positions;
            }

            int fullDancerCount = group.getFullDancersCount(); //count of full items
            int dancerCount = group.items.Count; //count of all items, potential one more than full if count is not a whole number
            float relativeProgression = group.getCountRelativeProgression(); //progression between full items

            for (int i = 0; i < dancerCount; i++)
            {

                Vector3 posAtFull = fullDancerCount == 1 ? Vector3.zero : Vector3.Lerp(Vector3.down, Vector3.up, (float)i / (fullDancerCount - 1));
                Vector3 posAtNextFull = Vector3.Lerp(Vector3.down, Vector3.up, (float)(i) / (dancerCount - 1));
                positions.Add(Vector3.Lerp(posAtFull, posAtNextFull, relativeProgression));
            }

            return positions;
        }
    }
}