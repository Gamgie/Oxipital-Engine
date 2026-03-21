using Augmenta;
using NUnit.Framework;
using Oxipital;
using System.Collections.Generic;
using UnityEngine;

public class LinkAugmentaObjectToOxipital : MonoBehaviour
{
    public enum AugmentaPositions { worldPos3D, worldPos2D, centroid}

    public int[] orbIdList;
	public int[] forceIdList;
    public bool connectToManualPattern = true;
    public AugmentaPositions positionsType = AugmentaPositions.worldPos3D;
    

    private OrbManager orbManager;
    private StandardForceManager forceManager;
    private AugmentaObject augmentaObject;
    private AugmentaScene augmentaScene;

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
    {
        augmentaObject = GetComponent<AugmentaObject>();
        orbManager = GetComponentInParent<LinkAugmentaManagerToOxipital>().orbManager;
        forceManager = GetComponentInParent<LinkAugmentaManagerToOxipital>().forceManager;
		augmentaScene = GetComponentInParent<AugmentaScene>();
	}

    // Update is called once per frame
    void Update()
    {
        if(connectToManualPattern)
        {
            for (int i = 0; i < orbIdList.Length; i++)
            {
                int orbId = orbIdList[i];
                OrbGroup orb = orbManager.GetOrbById(orbId);
				ApplyDataOnManualPattern(orbId, orb);
			}

			for (int i = 0; i < forceIdList.Length; i++)
			{
				int forceId = forceIdList[i];
				StandardForceGroup force = forceManager.GetForceById(forceId);
				ApplyDataOnManualPattern(forceId, force);
			}
		}
	}

    void ApplyDataOnManualPattern(int id, DancerGroup<Dancer> dancerGroup)
    {
		if (dancerGroup != null)
		{
			if (augmentaScene != null)
			{
				dancerGroup.count = augmentaScene.augmentaObjectCount;

			}

			ManualDancePattern manualPattern = dancerGroup.GetComponent<ManualDancePattern>();
			if (manualPattern != null)
			{
				switch (positionsType)
				{
					case AugmentaPositions.centroid:
						manualPattern.positions[augmentaObject.oid] = augmentaObject.centroid;
						break;
					case AugmentaPositions.worldPos3D:
						manualPattern.positions[augmentaObject.oid] = augmentaObject.worldPosition3D;
						break;
					case AugmentaPositions.worldPos2D:
						manualPattern.positions[augmentaObject.oid] = augmentaObject.worldPosition2D;
						break;
					default:
						break;
				}



			}
		}
	}
}
