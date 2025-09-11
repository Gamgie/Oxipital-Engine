using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Oxipital
{
	public class LineDancePattern : DancePattern
	{
		override protected List<Vector3> getPatternPositions<T>(DancerGroup<T> group)
		{
			List<Vector3> positions = new List<Vector3>();

			if (group.items.Count < 1)
			{
				positions.Add(Vector3.zero);
				return positions;
			}

			int fullDancerCount = group.getFullDancersCount(); //count of full items
			int dancerCount = group.items.Count; //count of all items, potential one more than full if count is not a whole number
			float relativeProgression = group.getCountRelativeProgression(); //progression between full items
			float groupCount = group.getCount();

			for (int i = 0; i < dancerCount; i++)
			{
				float tAtFull = fullDancerCount == 1 ? 0 : (float)i / (fullDancerCount - 1);
				float tAtNextFull = fullDancerCount == 1 ? 0 : (float)(i) / (dancerCount - 1);

				float rel = i / Mathf.Max(1,(groupCount-1)) * Mathf.PI;
				float initT = Mathf.Lerp(tAtFull, tAtNextFull, relativeProgression) + group.patternTimeOffset * rel;
				float normalT = group.patternTime;
				float randomT = group.items[i].localPatternTime;

				//Vector3 zeroFullPos = fullDancerCount == 1 ? Vector3.zero : Vector3.Lerp(Vector3.down, Vector3.up, Mathf.Cos(Mathf.PI*tAtFull));
				//Vector3 zeroNextPos = Vector3.Lerp(Vector3.down, Vector3.up, tAtNextFull);
				//Vector3 zeroPos = Vector3.Lerp(zeroFullPos, zeroNextPos, relativeProgression);

				//Vector3 normalPos = Vector3.Lerp(Vector3.down, Vector3.up, Mathf.Cos(normalT * Mathf.PI * 2) * .5f + .5f);
				//Vector3 randomPos = Vector3.Lerp(Vector3.down, Vector3.up, Mathf.Cos(randomT * Mathf.PI * 2) * .5f + .5f);
				//Vector3 oscPos = Vector3.Lerp(normalPos, randomPos, group.patternSpeedRandom);
				//Vector3 targetPos = Vector3.Lerp(zeroPos, oscPos, Mathf.Abs(group.patternSpeed * speedWeight));
				Vector3 oscPos = Vector3.Lerp(Vector3.down, Vector3.up, Mathf.Cos(2*Mathf.PI*normalT + rel) * .5f + .5f);
                Vector3 randomOscPos = Vector3.Lerp(Vector3.down, Vector3.up, Mathf.Cos(2 * Mathf.PI * randomT + rel) * .5f + .5f);
                Vector3 targetPos = Vector3.Lerp(oscPos, randomOscPos, group.patternSpeedRandom); ;

                positions.Add(targetPos); 
			}

			return positions;
		}
	}
}