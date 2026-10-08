using UnityEngine;

public class ActivateGameObject : MonoBehaviour
{
    public bool isActive = false;
    public GameObject targetGameObject;

    // Update is called once per frame
    void Update()
    {
        targetGameObject.SetActive(isActive);
    }
}
