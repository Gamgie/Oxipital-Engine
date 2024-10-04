using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Oxipital
{
    public class OrbManager : BaseManager<OrbGroup>
    {
        OrbManager() : base("Orb Group")
        {
        }


        override protected void killLastItem() { 
            (items[items.Count - 1] as OrbGroup).kill(getKillTime()); 
        }

        public override void kill(float time)
        {
            base.kill(time);
            count = 0;
        }
        protected override float getKillTime()
        {
            float result = 0;
            foreach (OrbGroup orbGroup in items) result = Mathf.Max(orbGroup.life);
            return result;
        }
    }
}