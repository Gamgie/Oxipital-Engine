using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Oxipital
{
	public class ManualDancePattern : DancePattern
	{
		public Vector3[] positions;

		override protected List<Vector3> getPatternPositions<T>(DancerGroup<T> group)
		{
			List<Vector3> positionsList = new List<Vector3>();

			if (positions.Length == 0)
			{
				positions = new Vector3[1];
				positions[0] = Vector3.zero; 
			}

			foreach(Vector3 pos in positions)
			{
				positionsList.Add(pos);
			}

			return positionsList;
		}
	}
}