using UnityEngine;

public class CameraTransform : MonoBehaviour
{
    public Transform targetObject;
    [Range(0f, 5f)]
    public float offset;
    public Vector3 position;
    public Vector3 rotation;

    // Update is called once per frame
    void Update()
    {
        if(targetObject != null)
        {
            targetObject.localPosition = new Vector3(0, 0, offset);
            position = targetObject.transform.position;
            rotation = targetObject.transform.rotation.eulerAngles;
        }
    }
}
