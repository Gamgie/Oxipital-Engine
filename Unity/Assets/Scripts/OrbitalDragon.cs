using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class OrbitalDragon : MonoBehaviour
{
    
    public float radius;
    public float velocity;
    public float pitch;
    public float dampSmoothTime = 0.3f;
    public float stopDuration = 5.0f;
    public bool showDebugSphere;
    public GameObject debugSphere;

    private bool _isActive = false;
    private Vector3 _oldPosition;
    private Vector3 _velocityDirection;
    private Vector3 _dampVelocity = Vector3.zero;
    private float _dampedRadiusVelocity;
    private float _smoothRadius;
    private Vector3 _rotationCenter;

    public bool IsActive { get => _isActive; }


    // Start is called before the first frame update
    void Start()
    {
        _oldPosition = transform.position;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        debugSphere.SetActive(showDebugSphere);

        if (!IsActive)
        {
            return;
        }

        _smoothRadius = radius;
        FollowOrbital();
    }

    void OrientAlongVelocity()
    {
        // velocity direction
        Vector3 actualDirection = transform.position - _oldPosition;

        // damped the actual velocity direction to the old one 
        _velocityDirection = Vector3.SmoothDamp(_velocityDirection, actualDirection, ref _dampVelocity, dampSmoothTime);

        // rotate accordingly
        if(_velocityDirection != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(_velocityDirection, Vector3.up);
    }

    public void Init()
    {
        if(!IsActive)
        {
            transform.position = new Vector3(0.1f, transform.position.y, 0);
            _rotationCenter = transform.position;
            DOTween.Kill(this);
            _isActive = true;

            Debug.Log("Start Dragon");
        }
    }

    public void Stop()
    {
        if(_isActive)
        {
            transform.DOMove(new Vector3(0, transform.position.y, 0), stopDuration).OnComplete(StopComplete);
            Debug.Log("Stop Dragon");
        }
    }

    void FollowOrbital()
    {
        // Compute position according to radius    
        Vector3 positionTarget = Vector3.Lerp(transform.position, _smoothRadius * Vector3.Normalize(this.transform.position - new Vector3(0,transform.position.y,0)),0.01f);
        transform.position = new Vector3(positionTarget.x, transform.position.y, positionTarget.z);

        //Update height if needed 

        // Rotate around center 
        transform.RotateAround(_rotationCenter, Vector3.down, velocity * Time.deltaTime);

        OrientAlongVelocity();

        //pitch rotation
        transform.eulerAngles = new Vector3(pitch, transform.eulerAngles.y, transform.eulerAngles.z);

        //update old position
        _oldPosition = transform.position;
    }

    IEnumerator InitSequence()
    {
        _smoothRadius = 0;
        while(true)
        {
            _smoothRadius = Mathf.SmoothDamp(_smoothRadius, radius, ref _dampedRadiusVelocity, 0.1f,0.1f);
            FollowOrbital();

            if (radius - _smoothRadius < 0.1f)
                break;

            yield return new WaitForEndOfFrame();
        }
        
        _isActive = true;
    }

    private void StopComplete()
    {
        _isActive = false;
    }
}
