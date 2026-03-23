using UnityEngine;
using Klak.Syphon;
using Klak.Spout;

[ExecuteInEditMode]
public class CameraSpoutManager : MonoBehaviour
{

    #region Public members
    public Klak.Spout.SpoutSender spoutSender;
    public Klak.Syphon.SyphonServer syphonServer;
    public Camera mainCamera;
    public int width;
    public int height;
    public RenderTextureFormat rtFormat;
    public bool loadFromPlayerPrefs = true;
	#endregion

	#region Private members
	RenderTexture m_renderTexture;
    #endregion

    private void OnEnable()
    {
        if(loadFromPlayerPrefs)
        {
			int tempWidth = PlayerPrefs.GetInt("SpoutWidth", 0);
			int tempHeight = PlayerPrefs.GetInt("SpoutHeight", 0);

            if (tempHeight != 0 && tempWidth != 0)
            {
                width = tempWidth;
                height = tempHeight;
            }
		}
        

        // Handle multi platform
        if (Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor)
        {
			// Enable syphon
			if (syphonServer != null)
                syphonServer.enabled = true;

			// Disable spout
			if (spoutSender != null)
				spoutSender.enabled = false;
        }
        else if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
        {
			// Disable syphon
			if (syphonServer != null)
				syphonServer.enabled = false;

			// Enable spout
			if (spoutSender != null)
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
        if (loadFromPlayerPrefs)
        {
            PlayerPrefs.SetInt("SpoutWidth", width);
            PlayerPrefs.SetInt("SpoutHeight", height);
        }
    }
}
