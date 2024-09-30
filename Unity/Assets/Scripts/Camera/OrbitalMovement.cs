using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using DG.Tweening;
using System;

public class OrbitalMovement : CameraMovement
{
	public float rotateYSpeed;
	public float rotateXSpeed;
	public float rotateZSpeed;
	[Space()]
	public bool controlRotateWithAngle = false;
	public float rotateYAngle;
	public float rotateXAngle;
	public float rotateZAngle;
	[Space()]
	public Vector3 positionTarget;
	public float moveToDuration;

	private CinemachineTransposer _transposer;
	private Tween _moveTween;

	public override void Init()
	{
		base.Init();
		_transposer = virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
		type = CameraController.CameraMovementType.Orbital;
		_isActive = false;
	}

	public override bool UpdateMovement()
	{
		// our camera is not active so leave now.
		if (!base.UpdateMovement())
			return false;

		// Rotate target transform according to speed
		if (controlRotateWithAngle == false)
		{
			transform.Rotate(rotateXSpeed * Time.deltaTime, rotateYSpeed * Time.deltaTime, rotateZSpeed * Time.deltaTime);
			rotateXAngle = transform.eulerAngles.x;
			rotateYAngle = transform.eulerAngles.y;
			rotateZAngle = transform.eulerAngles.z;
		}
		else // We want to control rotation with angle directly
		{
			transform.eulerAngles = new Vector3(rotateXAngle, rotateYAngle, rotateZAngle);
		}

		if(!_moveTween.active)
			transform.position = positionTarget;
		
		return true;
	}

	public override void Reset(float duration)
	{
		rotateYSpeed = 0;
		rotateXSpeed = 0;
		rotateZSpeed = 0;
		transform.DOMove(Vector3.zero, duration);
		transform.DORotate(Vector3.zero, duration);
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

	public void TopView()
	{
		rotateYSpeed = 0;
		rotateXSpeed = 0;
		transform.DORotate(new Vector3(90, 0, 0), moveToDuration);
	}

	public void DownView()
	{
		rotateYSpeed = 0;
		rotateXSpeed = 0;
		transform.DORotate(new Vector3(-90, 0, 0), moveToDuration);
	}

	public void LeftView()
	{
		rotateYSpeed = 0;
		rotateXSpeed = 0;
		transform.DORotate(new Vector3(0, 90, 0), moveToDuration);
	}

	public void RightView()
	{
		rotateYSpeed = 0;
		rotateXSpeed = 0;
		transform.DORotate(new Vector3(0, -90, 0), moveToDuration);
	}

	public void FrontView()
	{
		rotateYSpeed = 0;
		rotateXSpeed = 0;
		transform.transform.DORotate(new Vector3(0, 0, 0), moveToDuration);
	}

	public void RandomView()
	{
		transform.DORotate(new Vector3(UnityEngine.Random.Range(-180, 180), UnityEngine.Random.Range(-180, 180), UnityEngine.Random.Range(-180, 180)), moveToDuration);
	}

	public OrbitalMovementData StoreData()
	{
		OrbitalMovementData data = new OrbitalMovementData();
		data.positionTarget = positionTarget;
		data.moveToDuration = moveToDuration;

		return data;
	}

	public void LoadData(OrbitalMovementData data)
	{
		positionTarget = data.positionTarget;
		moveToDuration = data.moveToDuration;
	}

	public override void SetActive(bool activate, float duration = 0)
	{
		base.SetActive(activate);
		//_rigidbody.isKinematic = activate;

		if(activate)
			_moveTween = transform.DOMove(positionTarget, duration);
	}

	public override void SetCameraTransform(Vector3 position, Quaternion rotation)
	{
		virtualCamera.transform.position = position;
		virtualCamera.transform.rotation = rotation;
	}
}

[System.Serializable]
public class OrbitalMovementData
{
	public Vector3 positionTarget;
	public float moveToDuration;
}