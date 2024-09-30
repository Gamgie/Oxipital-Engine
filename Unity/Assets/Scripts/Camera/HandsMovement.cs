using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using DG.Tweening;
using UnityEngine;

public class HandsMovement : CameraMovement
{
	public Vector3 handPosition;
	public Vector3 centerLookAt;
	public bool zBlocked;

	private CinemachineTransposer _transposer;

	public override void Init()
	{
		base.Init();
		_transposer = virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
	}

	public override void Reset(float duration)
	{
		transform.DOMove(Vector3.zero, duration);
		transform.DORotate(Vector3.zero, duration);
	}

	public override void SetCameraTransform(Vector3 position, Quaternion rotation)
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
			//_transposer.m_FollowOffset = new Vector3(0, 0, -offset);
		}
	}

	public override bool UpdateMovement()
	{
		// our camera is not active so leave now.
		if (!base.UpdateMovement())
			return false;

		Vector3 position = handPosition - centerLookAt;
		transform.position = position;

		return true;
	}

	public void DefineCenter()
	{
		centerLookAt = handPosition;
	}
}
