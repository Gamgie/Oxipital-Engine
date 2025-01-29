using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Oxipital
{
	public class ManualDancePattern : DancePattern
	{
		public Vector3 pos1;
		public Vector3 pos2;
		public Vector3 pos3;
		public Vector3 pos4;
		public Vector3 pos5;
		public Vector3 pos6;

		override protected List<Vector3> getPatternPositions<T>(DancerGroup<T> group)
		{
			List<Vector3> positions = new List<Vector3>();

			positions.Add(pos1);
			positions.Add(pos2);
			positions.Add(pos3);
			positions.Add(pos4);
			positions.Add(pos5);
			positions.Add(pos6);

			return positions;
		}
	}
}