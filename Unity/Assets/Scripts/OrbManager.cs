using Augmenta;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

namespace Oxipital
{
    public class OrbManager : BaseManager<OrbGroup>
    {
        
        public float firstItemIntensity = .5f;

        AugmentaObject augmentaObject;

        OrbManager() : base("Orb Group")
        {
        }

        protected override void addItem()
        {
            base.addItem();
            if (items.Count == 1) items[0].dancerIntensity = firstItemIntensity;

            SetAugmentaObject();
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

        public void OnObjectCreated(AugmentaObject o)
		{
            augmentaObject = o;

            SetAugmentaObject();
        }

        internal void SetAugmentaObject()
		{
            if (augmentaObject == null)
                return;

            foreach (OrbGroup orbGroup in items) orbGroup.SetAugmentaObject(augmentaObject);
        }
    }
}