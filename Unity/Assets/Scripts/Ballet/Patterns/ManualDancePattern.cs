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
		public Vector3 pos7;
		public Vector3 pos8;
		public Vector3 pos9;
		public Vector3 pos10;
		public Vector3 pos11;
		public Vector3 pos12;
		public Vector3 pos13;
		public Vector3 pos14;
		public Vector3 pos15;
		public Vector3 pos16;

		override protected List<Vector3> getPatternPositions<T>(DancerGroup<T> group)
		{
			List<Vector3> positions = new List<Vector3>();

			positions.Add(pos1);
			positions.Add(pos2);
			positions.Add(pos3);
			positions.Add(pos4);
			positions.Add(pos5);
			positions.Add(pos6);
			positions.Add(pos7);
			positions.Add(pos8);
			positions.Add(pos9);
			positions.Add(pos10);
			positions.Add(pos11);
			positions.Add(pos12);
			positions.Add(pos13);
			positions.Add(pos14);
			positions.Add(pos15);
			positions.Add(pos16);

			return positions;
		}
	}
}