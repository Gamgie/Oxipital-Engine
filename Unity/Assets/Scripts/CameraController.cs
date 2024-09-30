using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Cinemachine;

public class CameraController : MonoBehaviour
{
    public enum CameraMovementType { Orbital, Spaceship, Hands }

    public bool renderMainWindow = true;

    [Header("General Parameters")]
    public Transform lookAtTarget;
    public new Camera camera;
    public float fov;
    public float followZOffset;
    public float resetDuration;

    [Header("Camera type")]
    public CameraMovementType cameraType;
    [Range(0,20)]
    public float cameraTransitionDuration;

    public OrbitalMovement orbitalCamera;
    public SpaceshipMovement spaceshipCamera;
    public HandsMovement handsCamera;
    [Range(0, 5)]
    public float cameraNoiseGain;
    [Range(0, 5)]
    public float cameraNoiseFrequency;

    [Header("Xwing")]
    public bool showXwing;
    public GameObject xWing;
    
    private Camera _cameraFeedback;
    private List<CameraMovement> _cameraList;
    private CameraMovement _activeCamera;
    private CinemachineBrain _cinemachineBrain;

    private void OnEnable()
    {
        _cameraFeedback = camera.transform.GetChild(0).GetComponent<Camera>();

        _cameraList = new List<CameraMovement>();
        _cameraList.Add(orbitalCamera);
        _cameraList.Add(spaceshipCamera);
        _cameraList.Add(handsCamera);

        foreach (CameraMovement c in _cameraList)
        {
            c.Init();
        }

        SwitchCamera(cameraType);

        LoadData();

        _cinemachineBrain = camera.GetComponent<CinemachineBrain>();
    }

    // Update is called once per frame
    void Update()
    {
        // Switch camera type here : orbital or spaceship basically.
        if(_activeCamera != null && cameraType != _activeCamera.type)
		{
            SwitchCamera(cameraType);
		}

        // update feedback camera FOV
        if (_cameraFeedback != null)
        {
            _cameraFeedback.fieldOfView = fov;

            if(renderMainWindow != _cameraFeedback.enabled)
			{
                _cameraFeedback.enabled = renderMainWindow;
                GameObject o = GameObject.FindGameObjectWithTag("Tools Manager");
                if(o != null)
                {
                    ToolsManager toolsMngr = o.GetComponent<ToolsManager>();
                    if (toolsMngr != null)
                        toolsMngr.activateGraphy = renderMainWindow;
                }     
            }
        }

        xWing.SetActive(showXwing);

        // Update camera transition duration
        if(_cinemachineBrain != null)
            _cinemachineBrain.m_DefaultBlend.m_Time = cameraTransitionDuration;

        // Update noise parameters in all camera
        foreach(CameraMovement c in _cameraList)
		{
            c.UpdateNoiseParameter(cameraNoiseGain, cameraNoiseFrequency);
		}
    }

    // Movement need physics so we need to update 
	private void FixedUpdate()
	{
        // Update our active camera
        foreach (CameraMovement c in _cameraList)
        {
            if (c != null && c.IsActive)
            {
                c.UpdateMovement();
                c.UpdateZOffset(followZOffset);
                c.UpdateFOV(fov);
            }
        }
    }

	void SwitchCamera(CameraMovementType type)
	{
        Vector3 cameraPosition = Vector3.zero;
        Quaternion cameraRotation = Quaternion.identity;

        if (_activeCamera)
		{
            cameraPosition = _activeCamera.virtualCamera.transform.position;
            cameraRotation = _activeCamera.virtualCamera.transform.rotation;
            _activeCamera.SetActive(false);
        }

        // Activate the selected camera
        switch (type)
		{
            case CameraMovementType.Hands:
                _activeCamera = handsCamera;
                break;
            case CameraMovementType.Orbital:
                _activeCamera = orbitalCamera;

                break;
            case CameraMovementType.Spaceship:
                _activeCamera = spaceshipCamera;
                _activeCamera.SetCameraTransform(cameraPosition, cameraRotation);
                break;
        }

        _activeCamera.SetActive(true, cameraTransitionDuration);
    }

    public void ResetCameraPosition()
    {
        foreach (CameraMovement c in _cameraList)
        {
            if (c.IsActive)
                c.Reset(resetDuration);
        }
    }

    public CameraControllerData StoreData()
	{
        CameraControllerData result = new CameraControllerData();

        result.renderMainWindow = renderMainWindow;
        result.fov = fov;
        result.followZOffset = followZOffset;
        result.resetDuration = resetDuration;
        result.cameraType = (int)cameraType;
        result.showXWing = showXwing;
        result.cameraTransitionDuration = cameraTransitionDuration;
        result.cameraNoiseGain = cameraNoiseGain;
        result.cameraNoiseFrequency = cameraNoiseFrequency;

        return result;
    }

    public void LoadData()
	{
        // Find Data manager and load its data.
        DataManager dataMngr = GameObject.FindGameObjectWithTag("Data Manager").GetComponent<DataManager>();
        OxipitalData data = dataMngr.LoadData();

        if(data.cameraControllerData == null)
		{
            return;
		}

        // Update cameraController parameters with this data
        renderMainWindow = data.cameraControllerData.renderMainWindow;
        fov = data.cameraControllerData.fov;
        followZOffset = data.cameraControllerData.followZOffset;
        resetDuration = data.cameraControllerData.resetDuration;
        cameraType = (CameraMovementType) data.cameraControllerData.cameraType;
        showXwing = data.cameraControllerData.showXWing;
        cameraTransitionDuration = data.cameraControllerData.cameraTransitionDuration;
        cameraNoiseGain = data.cameraControllerData.cameraNoiseGain;
        cameraNoiseFrequency = data.cameraControllerData.cameraNoiseFrequency;

        // Then update each camera parameters
        spaceshipCamera.LoadData(data.spaceshipMovementData);
        orbitalCamera.LoadData(data.orbitalMovementData);
    }

    public List<CameraMovement> GetCameraList()
	{
        return _cameraList;
	}
}

[System.Serializable]
public class CameraControllerData
{
    public bool renderMainWindow;
    public float fov;
    public float followZOffset;
    public float resetDuration;
    public int cameraType;
    public bool showXWing;
    public float cameraTransitionDuration;
    public float cameraNoiseGain;
    public float cameraNoiseFrequency;
}