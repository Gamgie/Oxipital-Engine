using UnityEngine;

public class Targetframerate : MonoBehaviour
{

    public int targetFramerate = 60;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Update()
    {
        Application.targetFrameRate = targetFramerate;
    }
}
