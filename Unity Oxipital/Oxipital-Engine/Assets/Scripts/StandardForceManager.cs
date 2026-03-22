using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Oxipital
{
    public class StandardForceManager : BaseManager<StandardForceGroup>
    {
        StandardForceManager() : base("Force Group")
        {
        }

		protected override void OnEnable()
		{
			base.OnEnable();

			for (int i = 0; i < items.Count; i++)
			{
				StandardForceGroup o = items[i];
				o.setID(i+1);
			}
		}

		protected override StandardForceGroup addItem()
        {
            StandardForceGroup g = base.addItem();
            g.setID(items.Count);
            return g;
		}

		internal StandardForceGroup GetForceById(int id)
		{
			foreach (StandardForceGroup forceGroup in items)
			{
				int orbID = forceGroup.getID();
				if (orbID == id) return forceGroup;
			}
			return null;
		}
	}
}