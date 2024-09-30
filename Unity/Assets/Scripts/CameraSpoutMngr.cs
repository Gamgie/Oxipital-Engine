using UnityEngine;
using Klak.Syphon;
using Klak.Spout;

[ExecuteInEditMode]
public class CameraSpoutMngr : MonoBehaviour
{

    #region Public members
    public Klak.Spout.SpoutSender spoutSender;
    public Klak.Syphon.SyphonServer syphonServer;
    public Camera mainCamera;
    public int width;
    public int height;
    public RenderTextureFormat rtFormat;
    #endregion

    #region Private members
    RenderTexture m_renderTexture;
    #endregion

    private void OnEnable()
    {
        width = PlayerPrefs.GetInt("SpoutWidth", 1920);
        height = PlayerPrefs.GetInt("SpoutHeight", 1080);

        // Handle multi platform
        if (Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor)
        {
            // Enable syphon
            syphonServer.enabled = true;
            // Disable spout
            spoutSender.enabled = false;
        }
        else if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
        {
            // Disable syphon
            syphonServer.enabled = false;
            // Enable spout
            spoutSender.enabled = true;
        }
    }

	// Update is called once per frame
	void Update()
    {
        UpdateTexture();
    }

    void UpdateTexture()
    {
        if(width == 0 || height == 0)
		{
            Debug.LogError("Can't instantiate a render texture with size of 0");
            return;
		}

        // No texture or not a valid one
        if(m_renderTexture == null || width != m_renderTexture.width || height != m_renderTexture.height)
		{
            m_renderTexture = new RenderTexture(width, height, 16, rtFormat);
            m_renderTexture.Create();


            if (Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor)
            {
                // Update syphon
                syphonServer.SourceTexture = m_renderTexture;
                mainCamera.targetTexture = m_renderTexture;
            }
            else if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
            {
                // Udate spout and main camera
                spoutSender.sourceTexture = m_renderTexture;
                mainCamera.targetTexture = m_renderTexture;
            }


            // Disable and enable again to re-init spout plugin
            spoutSender.enabled = false;
            spoutSender.enabled = true;
        }
    }

	private void OnDestroy()
	{
        PlayerPrefs.SetInt("SpoutWidth", width);
        PlayerPrefs.SetInt("SpoutHeight", height);
    }
}
