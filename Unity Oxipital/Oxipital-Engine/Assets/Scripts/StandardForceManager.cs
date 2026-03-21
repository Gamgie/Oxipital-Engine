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

        protected override StandardForceGroup addItem()
        {
            StandardForceGroup g = base.addItem();
            g.SetGroupId(items.Count);
            return g;
		}

		internal StandardForceGroup GetForceById(int id)
		{
			foreach (StandardForceGroup forceGroup in items)
			{
				int orbID = forceGroup.GetGroupId();
				if (orbID == id) return forceGroup;
			}
			return null;
		}
	}
}