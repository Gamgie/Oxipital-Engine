using UnityEngine;

public class ObjectPosition : MonoBehaviour
{
    public Transform targetObject;
    [Range(0f, 5f)]
    public float offset;
    public Vector3 position;

    // Update is called once per frame
    void Update()
    {
        if(targetObject != null)
        {
            targetObject.localPosition = new Vector3(0, 0, offset);
            position = targetObject.transform.position;
        }
    }
}
