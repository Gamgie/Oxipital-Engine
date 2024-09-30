using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public abstract class CameraMovement : MonoBehaviour
{
	protected bool _isActive;
	public CinemachineVirtualCamera virtualCamera;
	public CameraController.CameraMovementType type;
	protected CinemachineBasicMultiChannelPerlin noise;

	protected Rigidbody _rigidbody;

	public bool IsActive { get => _isActive; }

	public virtual void Init()
	{
		_rigidbody = GetComponent<Rigidbody>();
		noise = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
	}

	public abstract void Reset(float duration);

	public virtual bool UpdateMovement()
	{
		// Do not move our object if we are not the active camera
		if (!_isActive)
			return false;

		return true;
	}

	public abstract void UpdateFOV(float fov);

	public abstract void UpdateZOffset(float offset);

	public virtual void SetActive(bool activate, float duration = 0)
	{
		_isActive = activate;

		if (_isActive)
		{
			virtualCamera.Priority = 10;
		}
		else
		{
			virtualCamera.Priority = 0;
		}
	}

	public abstract void SetCameraTransform(Vector3 position, Quaternion rotation);

	public void UpdateNoiseParameter(float gain, float frequency)
	{
		if(noise != null)
		{
			noise.m_AmplitudeGain = gain;
			noise.m_FrequencyGain = frequency;
		}
		else
		{
			Debug.LogError("Can not find noise component in virtual camera parameter");
		}
	}
}