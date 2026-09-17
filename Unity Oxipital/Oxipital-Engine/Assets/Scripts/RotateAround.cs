using UnityEngine;

public class RotateAround : MonoBehaviour
{
    public Vector3 target;
    [Tooltip("Rotation speed in degrees per second")]
    public float speed;
    public Vector3 axis = Vector3.up;
    [Tooltip("Pick a random orbit axis on Start instead of using Axis above")]
    public bool randomAxis;

    void Start()
    {
        if (randomAxis)
            axis = Random.onUnitSphere;
    }

    void Update()
    {
        transform.RotateAround(target, axis, speed * Time.deltaTime);
    }
}
