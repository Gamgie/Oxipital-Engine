using UnityEngine;
using Klak.Spout;
using UnityEngine.UI;

public class SpoutFeedback : MonoBehaviour
{
    public SpoutSender spout;

    CameraController cameraController;

    public void OnEnable()
    {
         if (!cameraController)
            cameraController = FindFirstObjectByType<CameraController>();
        if (cameraController != null && cameraController.isFullDome)
        {
            transform.parent.gameObject.SetActive(false);
        }

        return;
    }

    void Update()
    {
       if (cameraController != null && cameraController.isFullDome)
            return;

        GetComponent<RawImage>().texture = spout.sourceTexture;
    }

}
