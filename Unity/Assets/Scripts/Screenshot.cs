using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class Screenshot : MonoBehaviour
{
    public string filepath;
    public string filename;
    public int supersize;
    public KeyCode takeScreenshotKeyCode;
    public List<GameObject> objectToHide;

    private int fileIndex = 0;

    private void OnEnable()
    {
        FileIndexSetup();
    }

    private void Update()
    {
        if (Input.GetKeyUp(takeScreenshotKeyCode))
        {
            TakeScreenshot();
        }
    }

    public void TakeScreenshot()
    {
        string newScreenCaptureName = filepath + "\\" + filename + "." + fileIndex + ".png";

        while (File.Exists(newScreenCaptureName))
        {
            fileIndex++;
            newScreenCaptureName = filepath + "\\" + filename + "." + fileIndex + ".png";
        }

        List<GameObject> objectToActivate = new List<GameObject>();
        foreach(GameObject go in objectToHide)
		{
            if(go.activeSelf)
			{
                objectToActivate.Add(go);
                go.SetActive(false);
			}
		}

        ScreenCapture.CaptureScreenshot(newScreenCaptureName, supersize);
        fileIndex++;

        StartCoroutine(ActivateObjects(objectToActivate));
    }

    void FileIndexSetup()
    {
        fileIndex = 0;

        string screenCaptureFileName = filepath + "\\" + filename + ".";

        foreach (string file in System.IO.Directory.GetFiles(filepath))
        {
            string actualFileNumber = file.Substring(screenCaptureFileName.Length);
            actualFileNumber = actualFileNumber.Remove(actualFileNumber.Length - 4);
            try
            {
                int actualFileIndex = Convert.ToInt32(actualFileNumber);
                if (actualFileIndex > fileIndex)
                    fileIndex = actualFileIndex + 1;
            }
            catch(OverflowException)
            {
                Debug.LogError(" \" " + actualFileNumber + " \" is outside the range of the Int32 type.");
            }
            catch(FormatException)
            {
                Debug.LogError(" \" " + actualFileNumber + " \" is not in a recognizable format.");
            }
        }
    }

    IEnumerator ActivateObjects(List<GameObject> goList)
	{
        yield return new WaitForSeconds(2);

        foreach (GameObject go in goList)
        {
            go.SetActive(true);
        }
    }
}
