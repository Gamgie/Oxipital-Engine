using UnityEngine;
using DG.Tweening;
using Unity.Cinemachine;

// Orbit camera driven by a body-worn IMU (SOMI-1 via Chataigne) : this transform is the pivot,
// sitting on centerPosition and rotated by the sensor; the virtual camera follows it at
// FollowOffset (CameraController.followZOffset) and always looks at the center.
// The sensor's Euler triple is turned into a quaternion right away and all the
// math is done on relative quaternions (never back to Euler), so there is no
// gimbal lock nor +-180 wrap spin in this path.
public class SensorMovement : CameraMovement
{
	// Order in which the Euler triple is composed: ZYX = rotate about Z, then Y, then X (intrinsic).
	public enum EulerOrder { XYZ, XZY, YXZ, YZX, ZXY, ZYX }
	// Which sensor axis feeds Unity's X, Y, Z: XZY = (sensor X, sensor Z, sensor Y).
	public enum AxisSwizzle { XYZ, XZY, YXZ, YZX, ZXY, ZYX }

	[Header("Sensor Input")]
	// Map the sensor orientation here as a whole Point3D (degrees), not component by component.
	public Vector3 sensorOrientation;
	// SOMI-1 : X pitch (-90..90), Y heading (-180..180), Z roll (-180..180) = YXZ.
	public EulerOrder eulerOrder = EulerOrder.YXZ;

	[Header("Sensor to Unity Frame")]
	// SOMI-1 already uses Y as up, so no swizzle.
	public AxisSwizzle axisSwizzle = AxisSwizzle.XYZ;
	// Flips the rotation direction around each Unity axis : -1 flips, anything else (0 included) keeps.
	public Vector3 axisSigns = Vector3.one;
	// Use if the sensor reports earth->sensor instead of sensor->earth (body turn tilts the view when it should not).
	public bool localFrameDelta = false;

	[Header("Response")]
	// true : the world turns with the body (what is in front of you stays in front). false : camera acts as your head.
	public bool invertDirection = true;
	[Range(0, 2)]
	public float yawAmount = 1;
	[Range(0, 2)]
	public float tiltAmount = 1;
	[Range(0, 1)]
	public float smoothing = 0.05f;
	// Sensor rotation faster than this (degrees per second, between two sensor updates) is a glitch : held without moving. 0 = off.
	// A body hardly turns over 2 turns/s; the SOMI's heading glitches near X = +-90 sweep at 900-3000 deg/s.
	[Range(0, 2000)]
	public float maxTurnSpeed = 720;

	[Header("Output")]
	// Continuous body turn since the neutral pose (degrees, goes past +-180 : 720 = two turns).
	public float turnAngle;

	[Header("Neutral Pose")]
	// Raw sensor orientation of the performer's neutral pose (the body's 0). Set by CalibrateNeutral(), saved between sessions.
	public Vector3 neutralSensorOrientation;
	// Where the camera looks when the body is in the neutral pose (Unity Euler degrees, 0,0,0 = looking forward).
	public Vector3 neutralView;
	// Time to ease the view onto the new neutral after CalibrateNeutral() or Reset, instead of snapping.
	[Range(0, 10)]
	public float calibrationBlendTime = 1;

	[Header("Orbit")]
	// Center the camera orbits around and looks at. The distance is CameraController.followZOffset.
	public Vector3 centerPosition;

	private const string NeutralPrefsKey = "SensorMovement.neutralSensorOrientation";

	private CinemachineFollow _transposer;
	private bool _hasNeutral;
	private Quaternion _blendOffset = Quaternion.identity;
	private Tween _blendTween;

	// Filtered body rotation state, updated once per frame by UpdateSensorState.
	private bool _hasSensorState;
	private Quaternion _glitchCorrection = Quaternion.identity;
	private Quaternion _lastDelta = Quaternion.identity;
	private Quaternion _swing = Quaternion.identity;
	private float _lastTwistAngle;
	private float _glitchHoldTime;
	private Vector3 _lastSensorOrientation;
	private float _lastSampleTime;
	// How long a jump is held as a glitch before it is accepted as the new reference.
	private const float MaxGlitchHoldTime = 0.2f;
	// Floor for the time between two sensor updates, so a burst of updates in consecutive frames isn't seen as too fast.
	private const float MinSampleInterval = 1f / 60f;

	public override void Init()
	{
		base.Init();
		type = CameraController.CameraMovementType.Sensor;
		_isActive = false;

		_transposer = virtualCamera.GetComponentInChildren<CinemachineFollow>();

		_hasNeutral = PlayerPrefs.HasKey(NeutralPrefsKey + ".x");
		if (_hasNeutral)
		{
			neutralSensorOrientation = new Vector3(
				PlayerPrefs.GetFloat(NeutralPrefsKey + ".x"),
				PlayerPrefs.GetFloat(NeutralPrefsKey + ".y"),
				PlayerPrefs.GetFloat(NeutralPrefsKey + ".z"));
		}
	}

	public override void SetActive(bool activate, float duration = 0)
	{
		base.SetActive(activate);

		if (!activate)
			return;

		// Never calibrated : the current body pose becomes the neutral one.
		if (!_hasNeutral)
			SaveNeutral();

		// Start on the sensor pose right away, the Cinemachine blend smooths the switch.
		ResetSensorState();
		_blendTween?.Kill();
		_blendOffset = Quaternion.identity;
		transform.SetPositionAndRotation(centerPosition, GetTargetRotation());
	}

	// Rotation is applied at frame rate here instead of in UpdateMovement (FixedUpdate)
	// so the camera follows the body smoothly instead of stepping at the physics rate.
	private void Update()
	{
		if (!_isActive)
			return;

		UpdateSensorState();
		Quaternion target = _blendOffset * GetTargetRotation();

		if (smoothing > 0)
			target = Quaternion.Slerp(transform.rotation, target, 1 - Mathf.Exp(-Time.deltaTime / smoothing));

		transform.SetPositionAndRotation(centerPosition, target);
	}

	// Stand in your neutral pose, facing your center, and trigger this : that pose now shows neutralView.
	public void CalibrateNeutral()
	{
		BlendChange(SaveNeutral, calibrationBlendTime);
	}

	// Ease the neutral view back to looking forward.
	public override void Reset(float duration)
	{
		BlendChange(() => neutralView = Vector3.zero, duration);
	}

	public override void UpdateZOffset(float offset)
	{
		// Distance between the center and the camera
		if (_transposer != null)
		{
			_transposer.FollowOffset = new Vector3(0, 0, -offset);
		}
	}

	public override void SetCameraTransform(Vector3 position, Quaternion rotation)
	{
		// The pivot stays on the center and its rotation comes from the sensor; Cinemachine's blend smooths the switch.
	}

	private void SaveNeutral()
	{
		neutralSensorOrientation = sensorOrientation;
		_hasNeutral = true;
		ResetSensorState();

		PlayerPrefs.SetFloat(NeutralPrefsKey + ".x", neutralSensorOrientation.x);
		PlayerPrefs.SetFloat(NeutralPrefsKey + ".y", neutralSensorOrientation.y);
		PlayerPrefs.SetFloat(NeutralPrefsKey + ".z", neutralSensorOrientation.z);
		PlayerPrefs.Save();
	}

	// Applies a change of neutral pose/view, then eases the view from where it was onto the new target.
	private void BlendChange(System.Action change, float duration)
	{
		Quaternion before = transform.rotation;
		change();

		_blendTween?.Kill();
		_blendOffset = Quaternion.identity;
		if (!_isActive || duration <= 0)
			return;

		Quaternion from = before * Quaternion.Inverse(GetTargetRotation());
		_blendOffset = from;
		float t = 0;
		_blendTween = DOTween.To(() => t, x =>
		{
			t = x;
			_blendOffset = Quaternion.Slerp(from, Quaternion.identity, t);
		}, 1, duration);
	}

	// Camera rotation for the current body pose : body rotation away from the neutral pose, applied on the neutral view.
	// The turn uses the continuous turnAngle, so several full turns (and yawAmount != 1) never jump at +-180.
	private Quaternion GetTargetRotation()
	{
		Quaternion scaledTwist = Quaternion.AngleAxis(turnAngle * yawAmount, Vector3.up);
		Quaternion scaledSwing = Quaternion.SlerpUnclamped(Quaternion.identity, _swing, tiltAmount);

		Quaternion body = scaledSwing * scaledTwist;
		if (invertDirection)
			body = Quaternion.Inverse(body);

		return body * Quaternion.Euler(neutralView);
	}

	// Restart the filtered state on the current sensor value (after calibration or when activated).
	private void ResetSensorState()
	{
		_hasSensorState = false;
		UpdateSensorState();
	}

	// Once per frame : body rotation away from the neutral pose, glitch filtered, split into
	// a tilt (swing) and a continuous turn around world up (turnAngle, unwrapped across +-180).
	private void UpdateSensorState()
	{
		// Only new sensor values change the state (the sensor sends ~60 values/s, Unity may run faster).
		if (_hasSensorState && sensorOrientation == _lastSensorOrientation)
			return;

		float sampleInterval = Mathf.Max(Time.time - _lastSampleTime, MinSampleInterval);
		_lastSampleTime = Time.time;
		_lastSensorOrientation = sensorOrientation;

		Quaternion rawDelta = GetSensorDelta();

		if (!_hasSensorState)
		{
			_glitchCorrection = Quaternion.identity;
			_glitchHoldTime = 0;
		}

		Quaternion delta = _glitchCorrection * rawDelta;

		// Faster than a body can turn : hold the last value. A spike is simply skipped (no motion lost);
		// if the jump lasts, the data really moved (e.g. sensor heading reset) and it becomes the new reference without moving.
		if (_hasSensorState && maxTurnSpeed > 0 && Quaternion.Angle(_lastDelta, delta) > maxTurnSpeed * sampleInterval)
		{
			_glitchHoldTime += sampleInterval;
			if (_glitchHoldTime >= MaxGlitchHoldTime)
			{
				_glitchCorrection = _lastDelta * Quaternion.Inverse(rawDelta);
				_glitchHoldTime = 0;
			}
			delta = _lastDelta;
		}
		else
		{
			_glitchHoldTime = 0;
		}

		// Swing-twist decomposition around world up : twist is the body turn, swing the tilt/roll.
		Quaternion twist = new Quaternion(0, delta.y, 0, delta.w);
		float twistLength = Mathf.Sqrt(twist.y * twist.y + twist.w * twist.w);
		if (twistLength < 1e-6f)
			twist = Quaternion.identity;
		else
		{
			// Keep w positive so the twist angle is the shortest one.
			float sign = twist.w < 0 ? -1 : 1;
			twist = new Quaternion(0, sign * twist.y / twistLength, 0, sign * twist.w / twistLength);
		}
		_swing = delta * Quaternion.Inverse(twist);

		// Unwrap : add the shortest step since last frame, so 179 -> -179 is +2 and not -358.
		float twistAngle = 2 * Mathf.Atan2(twist.y, twist.w) * Mathf.Rad2Deg;
		if (!_hasSensorState)
			turnAngle = twistAngle;
		else
			turnAngle += Mathf.DeltaAngle(_lastTwistAngle, twistAngle);

		_lastTwistAngle = twistAngle;
		_lastDelta = delta;
		_hasSensorState = true;
	}

	// Raw body rotation away from the neutral pose, in Unity's world frame.
	private Quaternion GetSensorDelta()
	{
		Quaternion current = GetSensorRotation(sensorOrientation);
		Quaternion neutral = GetSensorRotation(neutralSensorOrientation);
		return localFrameDelta
			? Quaternion.Inverse(neutral) * current
			: current * Quaternion.Inverse(neutral);
	}

	// Raw sensor orientation converted to a quaternion in Unity's frame.
	private Quaternion GetSensorRotation(Vector3 orientation)
	{
		Quaternion q = ComposeEuler(orientation, eulerOrder);

		Vector3 v = new Vector3(q.x, q.y, q.z);
		v = Swizzle(v, axisSwizzle);
		v = new Vector3(SignOf(axisSigns.x) * v.x, SignOf(axisSigns.y) * v.y, SignOf(axisSigns.z) * v.z);

		return Normalize(new Quaternion(v.x, v.y, v.z, q.w));
	}

	private static Quaternion ComposeEuler(Vector3 angles, EulerOrder order)
	{
		Quaternion x = Quaternion.AngleAxis(angles.x, Vector3.right);
		Quaternion y = Quaternion.AngleAxis(angles.y, Vector3.up);
		Quaternion z = Quaternion.AngleAxis(angles.z, Vector3.forward);

		switch (order)
		{
			case EulerOrder.XYZ: return x * y * z;
			case EulerOrder.XZY: return x * z * y;
			case EulerOrder.YXZ: return y * x * z;
			case EulerOrder.YZX: return y * z * x;
			case EulerOrder.ZXY: return z * x * y;
			default: return z * y * x;
		}
	}

	private static Vector3 Swizzle(Vector3 v, AxisSwizzle swizzle)
	{
		switch (swizzle)
		{
			case AxisSwizzle.XZY: return new Vector3(v.x, v.z, v.y);
			case AxisSwizzle.YXZ: return new Vector3(v.y, v.x, v.z);
			case AxisSwizzle.YZX: return new Vector3(v.y, v.z, v.x);
			case AxisSwizzle.ZXY: return new Vector3(v.z, v.x, v.y);
			case AxisSwizzle.ZYX: return new Vector3(v.z, v.y, v.x);
			default: return v;
		}
	}

	// A sign must stay +-1 : scaling a quaternion component by 0 breaks the rotation and makes it jump.
	private static float SignOf(float value)
	{
		return value < 0 ? -1 : 1;
	}

	private static Quaternion Normalize(Quaternion q)
	{
		float length = Mathf.Sqrt(q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w);
		if (length < 1e-6f)
			return Quaternion.identity;

		return new Quaternion(q.x / length, q.y / length, q.z / length, q.w / length);
	}
}
