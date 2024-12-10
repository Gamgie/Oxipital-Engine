using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

namespace Oxipital
{
    public class OrbManager : BaseManager<OrbGroup>
    {
        
        public float firstItemIntensity = .5f;

        OrbManager() : base("Orb Group")
        {
        }

        protected override OrbGroup addItem()
        {
            OrbGroup g = base.addItem();
            if (items.Count == 1) items[0].dancerIntensity = firstItemIntensity;
            return g;
        }

        override protected void killLastItem() { 
            (items[items.Count - 1] as OrbGroup).kill(getKillTime()); 
        }

        protected override float getKillTime()
        {
            float result = 0;
            foreach (OrbGroup orbGroup in items) result = Mathf.Max(orbGroup.life);
            return result;
        }
    }
}