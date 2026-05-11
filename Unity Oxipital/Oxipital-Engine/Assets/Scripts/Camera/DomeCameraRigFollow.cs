using UnityEngine;

public class DomeCameraRigFollow : MonoBehaviour
{
    public GameObject cameraTarget;

	// PFC Dome creator is offset from center. We need another target for position.
	public GameObject offsetTarget;
    [Range(-2f,-0.01f)]
    public float offset = -0.2f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        offsetTarget.transform.localPosition = new Vector3(0, 0, -offset*20);

        transform.position = offsetTarget.transform.position;
        transform.rotation = cameraTarget.transform.rotation;
        transform.Rotate(90,0,0);
        transform.Rotate(0,180,0);
	}
}
