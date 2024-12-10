using UnityEngine;
using Klak.Spout;
using UnityEngine.UI;

public class SpoutFeedback : MonoBehaviour
{
    public SpoutSender spout;

    void Update()
    {
        GetComponent<RawImage>().texture = spout.sourceTexture;
    }

}
