using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class SpaceshipMovement : CameraMovement
{
	[Header("Camera Position")]
	public bool manualControl;
	public Vector3 cameraPosition;
	public Vector3 cameraRotation;

	[Header("Translation")]
	[Range(0, 100)]
	public float thrust;
	[Range(-1,1)]
	public float thrust1D;

	[Range(0, 100)]
	public float lateralThrust;
	[Range(-1, 1)]
	public float lateralThrust1D;

	[Range(0, 100)]
	public float verticalThrust;
	[Range(-1, 1)]
	public float verticalThrust1D;

	[Header("Rotation")]
	[Range(0, 100)]
	public float yawTorque;
	[Range(-1, 1)]
	public float yaw1D;

	[Range(0, 100)]
	public float pitchTorque;
	[Range(-1, 1)]
	public float pitch1D;

	[Range(0, 100)]
	public float rollTorque;
	[Range(-1, 1)]
	public float roll1D;

	[Header("Physics Parameters")]
	[Range(0, 5)]
	public float directionnalDrag;
	[Range(0, 5)]
	public float angularDrag;

	private CinemachineTransposer _transposer;
	private Quaternion _initialRotation;

	public override void Init()
	{
		base.Init();
		type = CameraController.CameraMovementType.Spaceship;

		_transposer = virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
		_initialRotation = _rigidbody.rotation;
	}

	public override bool UpdateMovement()
	{
		if (!base.UpdateMovement())
			return false;

		// We want to control manually the camera
		if (manualControl)
		{
			_rigidbody.isKinematic = true;
			_rigidbody.position = cameraPosition;
			transform.localEulerAngles = cameraRotation;
			
			return true;
		}
				
		if(_rigidbody != null)
		{
			_rigidbody.isKinematic = false;

			// Thrust
			_rigidbody.AddRelativeForce(Vector3.forward * thrust * thrust1D * Time.deltaTime, ForceMode.Force);

			// Lateral Thrust
			_rigidbody.AddRelativeForce(Vector3.right* lateralThrust * lateralThrust1D * Time.deltaTime, ForceMode.Force);

			// Vertical Thrust
			_rigidbody.AddRelativeForce(Vector3.up * verticalThrust * verticalThrust1D * Time.deltaTime, ForceMode.Force);

			// Roll
			_rigidbody.AddRelativeTorque(Vector3.back * roll1D * rollTorque * Time.deltaTime);

			// Pitch
			_rigidbody.AddRelativeTorque(Vector3.left * pitch1D * pitchTorque * Time.deltaTime);

			// Yaw
			_rigidbody.AddRelativeTorque(Vector3.up * yaw1D * yawTorque * Time.deltaTime);


			_rigidbody.drag = directionnalDrag;
			_rigidbody.angularDrag= angularDrag;

		}

		// Save transform in parameter to use it in chataigne.
		cameraPosition = transform.position;
		cameraRotation = transform.rotation.eulerAngles;

		return true;
	}

	public override void Reset(float duration)
	{

	}

	public override void UpdateFOV(float fov)
	{
		virtualCamera.m_Lens.FieldOfView = fov;
	}

	public override void UpdateZOffset(float offset)
	{
		// Control distance between target and camera
		if (_transposer != null)
		{
			_transposer.m_FollowOffset = new Vector3(0, 0, -offset);
		}
	}

	public override void SetActive(bool activate, float duration = 0)
	{
		base.SetActive(activate);
		_rigidbody.isKinematic = !activate;
	}

	public SpaceshipMovementData StoreData()
	{
		SpaceshipMovementData data = new SpaceshipMovementData();

		data.thrust = thrust;
		data.yawTorque = yawTorque;
		data.pitchTorque = pitchTorque;
		data.rollTorque = rollTorque;
		data.directionnalDrag = directionnalDrag;
		data.angularDrag = angularDrag;
		data.manualControl = manualControl;
		data.cameraPosition = cameraPosition;
		data.cameraRotation = cameraRotation;

		return data;
	}

	public void LoadData(SpaceshipMovementData data)
	{
		thrust = data.thrust;
		yawTorque = data.yawTorque;
		pitchTorque = data.pitchTorque;
		rollTorque = data.rollTorque;
		directionnalDrag = data.directionnalDrag;
		angularDrag = data.angularDrag;
		manualControl = data.manualControl;
		cameraPosition = data.cameraPosition;
		cameraRotation = data.cameraRotation;
	}


	public override void SetCameraTransform(Vector3 position, Quaternion rotation)
	{
		_rigidbody.isKinematic = true;
		_rigidbody.position = position;
		_rigidbody.rotation = rotation;
	}
}

[System.Serializable]
public class SpaceshipMovementData
{
	public float thrust;
	public float yawTorque;
	public float pitchTorque;
	public float rollTorque;
	public float directionnalDrag;
	public float angularDrag;
	public bool manualControl;
	public Vector3 cameraPosition;
	public Vector3 cameraRotation;
}