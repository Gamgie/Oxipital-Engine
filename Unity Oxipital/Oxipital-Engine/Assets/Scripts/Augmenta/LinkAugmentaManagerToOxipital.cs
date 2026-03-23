using Augmenta;
using Oxipital;
using UnityEngine;

public class LinkAugmentaManagerToOxipital : MonoBehaviour
{
	public enum AugmentaPositions { worldPos3D, worldPos2D, centroid }

	[Header("Oxipital")]
	public StandardForceManager forceManager;
	public OrbManager orbManager;
	public int[] orbIdList;
	public int[] forceIdList;
	public bool connectToManualPattern = true;
	
	[Header("Augmenta")]
	public AugmentaPositions positionsType = AugmentaPositions.worldPos3D;

	private AugmentaScene augmentaScene;
	private AugmentaManager augmentaManager;

	private void Start()
	{
		augmentaScene = GetComponentInChildren<AugmentaScene>();
		augmentaManager = GetComponent<AugmentaManager>();
	}

	private void Update()
	{
		if (connectToManualPattern)
		{
			// Go through all the augmenta objects and apply their position to the manual pattern of the forces and orbs
			foreach (int key in augmentaManager.augmentaObjects.Keys)
			{
				int oid = augmentaManager.augmentaObjects[key].oid;

				// Get the position of the augmenta object according to the selected type
				Vector3 augmentaObjectPosition = Vector3.zero;
				switch (positionsType)
				{
					case AugmentaPositions.worldPos3D:
						augmentaObjectPosition = augmentaManager.augmentaObjects[key].worldPosition3D;
						break;
					case AugmentaPositions.worldPos2D:
						augmentaObjectPosition = augmentaManager.augmentaObjects[key].worldPosition2D;
						break;
					case AugmentaPositions.centroid:
						augmentaObjectPosition = augmentaManager.augmentaObjects[key].centroid;
						break;
				}

				// We have target position, let's apply it to the manual pattern of the forces and orbs
				for (int i = 0; i < orbIdList.Length; i++)
				{
					int orbId = orbIdList[i];
					OrbGroup orbGroup = orbManager.GetOrbById(orbId);
					orbGroup.count = augmentaManager.augmentaObjects.Count-1; // Set the count of the orb group to the number of augmenta objectsS
					ManualDancePattern orbManualDancePattern = orbGroup.GetComponent<ManualDancePattern>();
					ApplyDataOnManualPattern(oid, orbManualDancePattern, augmentaObjectPosition);
				}

				for (int i = 0; i < forceIdList.Length; i++)
				{
					int forceId = forceIdList[i];
					StandardForceGroup forceGroup = forceManager.GetForceById(forceId);
					forceGroup.count = augmentaManager.augmentaObjects.Count-1; // Set the count of the force group to the number of augmenta objects
					ManualDancePattern forceManualDancePattern = forceGroup.GetComponent<ManualDancePattern>();
					ApplyDataOnManualPattern(oid, forceManualDancePattern, augmentaObjectPosition);
				}
			}
		}
	}
	void ApplyDataOnManualPattern(int id, ManualDancePattern manualDancePattern, Vector3 position)
	{
		if (manualDancePattern != null)
		{
			manualDancePattern.positions[id] = position;
		}
	}
}
