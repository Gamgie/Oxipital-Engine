using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
	//public string fileName;
 //   public OrbsManager orbsMngr;
	//public OrbGroupController[] orbsControllers;
	//public BalletManager balletMngr;
	////public ForceManager forceMngr;

	//private string path;
	//private OxipitalData loadedData;

	//[Header("Camera Data")]
	//public CameraController cameraController;

	//public OxipitalData LoadData()
	//{
	//	if(path == null)
	//	{
	//		path = Application.dataPath;
	//	}

	//	if(loadedData == null)
	//	{
	//		string filepath = path + "/" + fileName + ".json";
	//		try
	//		{
	//			string orbsData = System.IO.File.ReadAllText(filepath);

	//			loadedData = JsonUtility.FromJson<OxipitalData>(orbsData);

	//			Debug.Log("file loaded at " + path + "/" + fileName);
	//		}
	//		catch(System.Exception e)
	//		{
	//			// If no json is found, we initialize system
	//			// with OrbGroup with one orb in each.
	//			loadedData = new OxipitalData();

	//			orbsMngr.AddOrbGroup().SetOrbCount(1);
	//			orbsMngr.AddOrbGroup().SetOrbCount(1);

	//			SaveData();
	//		}
	//	}

	//	return loadedData;
	//}

	//public void SaveData()
	//{
	//	if(orbsMngr == null)
	//	{
	//		Debug.LogError("Can not save file because there is no OrbManager found");
	//		return;
	//	}

	//	// Save data of orb mngr.
	//	OxipitalData data = new OxipitalData();
	//	data.orbCount = orbsMngr.orbGroupCount;

	//	// Save data for each OrbGroup.
	//	data.orbGroupData = new List<OrbGroupData>();
	//	foreach(OrbGroup o in orbsMngr.orbs)
	//	{
	//		data.orbGroupData.Add(o.StoreData());

	//	}

	//	// Save data for each OrbGroupController.
	//	data.orbGroupControllersData = new List<OrbGroupControllerData>();
	//	foreach(OrbGroupController oc in orbsControllers)
	//	{
	//		OrbGroupControllerData ogcData = new OrbGroupControllerData();
	//		ogcData.name = oc.name;
	//		ogcData.idControlled = oc.idControlled;
	//		ogcData.colorSmooth = oc.colorSmoothSpeed;
	//		data.orbGroupControllersData.Add(ogcData);
	//	}

	//	// Save patterns data of the Ballet.
	//	data.balletMngrData = new BalletManagerData();
	//	// Save orbs pattern data.
	//	foreach(BalletPattern pattern in balletMngr.orbsPatterns)
	//	{
	//		data.balletMngrData.orbsPatternData.Add(pattern.StoreData());
	//	}
	//	// Save force pattern data.
	//	foreach (BalletPattern pattern in balletMngr.forcePatterns)
	//	{
	//		data.balletMngrData.forcePatternData.Add(pattern.StoreData());
	//	}

	//	// Save Force
	//	/*data.forceControllerData = new List<ForceControllerData>();
	//	foreach (ForceController f in forceMngr.Forces)
	//	{
	//		data.forceControllerData.Add(f.StoreData());
	//	}*/

	//	// Save CameraController data
	//	data.cameraControllerData = cameraController.StoreData();

	//	// Save CameraMovement data
	//	foreach (CameraMovement camMovement in cameraController.GetCameraList())
	//	{
	//		switch (camMovement.type)
	//		{
	//			case CameraController.CameraMovementType.Hands:
	//				break;
	//			case CameraController.CameraMovementType.Orbital:
	//				OrbitalMovement orbitalMovement = (OrbitalMovement) camMovement;
	//				data.orbitalMovementData = orbitalMovement.StoreData();
	//				break;
	//			case CameraController.CameraMovementType.Spaceship:
	//				SpaceshipMovement spaceshipMovement = (SpaceshipMovement) camMovement;
	//				data.spaceshipMovementData = spaceshipMovement.StoreData();
	//				break;
	//		}
	//	}
		

	//	// Save to file
	//	string orbsData = JsonUtility.ToJson(data);
	//	string filepath = path + "/" + fileName + ".json";
	//	System.IO.File.WriteAllText(filepath, orbsData);
	//	Debug.Log("file saved at " + filepath);
	//}
}

//[System.Serializable]
//public class OxipitalData
//{
//	public int orbCount;
//	public List<OrbGroupData> orbGroupData;
//	public List<OrbGroupControllerData> orbGroupControllersData;
//	public BalletManagerData balletMngrData;
//	public List<ForceControllerData> forceControllerData;
//	public CameraControllerData cameraControllerData;
//	public OrbitalMovementData orbitalMovementData;
//	public SpaceshipMovementData spaceshipMovementData;
//}