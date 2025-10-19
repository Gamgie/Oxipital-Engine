using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Oxipital
{
	public class LineDancePattern : DancePattern
	{
		[Range(0,1)]
		public float oscilatingDirection = 0;
		[Range(0, 1)]
		public float oscilatingSize = 1;

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

				float rel = Mathf.Acos( Mathf.Clamp(((2*i / Mathf.Max(1,(groupCount-1))) - 1.0f),-1.0f,1.0f));
				float initT = Mathf.Lerp(tAtFull, tAtNextFull, relativeProgression) + group.patternTimeOffset * rel;
				float normalT = group.patternTime;
				float randomT = group.items[i].localPatternTime;

				// We compute the line and each element position
				Vector3 oscPos = Vector3.Lerp(Vector3.left, Vector3.right, Mathf.Cos(2*Mathf.PI*normalT + rel) * .5f + .5f);
				Vector3 randomOscPos = Vector3.Lerp(Vector3.down, Vector3.up, Mathf.Cos(2 * Mathf.PI * randomT + rel) * .5f + .5f);

				Vector3 linePos = Vector3.Lerp(Vector3.left, Vector3.right, Mathf.Cos(2 * Mathf.PI + rel) * .5f + .5f);
				Vector3 orthoOscPos = Vector3.Lerp(Vector3.down, Vector3.up, Mathf.Cos(2 * Mathf.PI * normalT + rel) * .5f + .5f) * oscilatingSize;
                Vector3 targetPos = Vector3.Lerp(oscPos, randomOscPos, group.patternSpeedRandom);
				targetPos = Vector3.Lerp(targetPos, linePos + orthoOscPos, oscilatingDirection);

                positions.Add(targetPos); 
			}

			return positions;
		}
	}
}