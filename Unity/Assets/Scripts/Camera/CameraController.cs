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
    public bool orthographic = false;
    public float fov;
    public float orthoSize = 2;
    public float followZOffset;
    public float resetDuration;

    [Header("Camera type")]
    public CameraMovementType cameraType;
    [Range(0, 20)]
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

    [Header("Full Dome Controller")]
    public bool isFullDome = false;
    public GameObject domeCameraRig;

    private List<CameraMovement> _cameraList;
    private CameraMovement _activeCamera;
    private CinemachineBrain _cinemachineBrain;

    private void OnEnable()
    {

        _cameraList = new List<CameraMovement>();
        _cameraList.Add(orbitalCamera);
        _cameraList.Add(spaceshipCamera);
        _cameraList.Add(handsCamera);

        foreach (CameraMovement c in _cameraList)
        {
            c.Init();
        }

        SwitchCamera(cameraType);

        _cinemachineBrain = camera.GetComponent<CinemachineBrain>();
    }

    // Update is called once per frame
    void Update()
    {
        // Switch camera type here
        if (_activeCamera != null && cameraType != _activeCamera.type)
        {
            SwitchCamera(cameraType);
        }



        GameObject o = GameObject.FindGameObjectWithTag("Tools Manager");
        if (o != null)
        {
            ToolsManager toolsMngr = o.GetComponent<ToolsManager>();
            if (toolsMngr != null)
                toolsMngr.activateGraphy = renderMainWindow;
        }


        xWing.SetActive(showXwing);

        // Update camera transition duration
        if (_cinemachineBrain != null)
            _cinemachineBrain.m_DefaultBlend.m_Time = cameraTransitionDuration;

        // Update noise parameters in all camera
        foreach (CameraMovement c in _cameraList)
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

                if(!isFullDome)
                {
                    if (orthographic)
                    {
                        camera.orthographic = true;
                        c.UpdateOrthoSize(orthoSize);

                    }
                    else
                    {
                        camera.orthographic = false;
                        c.UpdateFOV(fov);
                    }
                }
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
}