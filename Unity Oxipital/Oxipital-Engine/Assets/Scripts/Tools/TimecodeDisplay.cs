using UnityEngine;
using System;

public class TimecodeDisplay : MonoBehaviour
{
	public float size = 20;
	public float framerate = 50;
	public float startTime = 0.0f;
	public int framecount = 0;
	public bool showTimeCode = true;
	public bool showFrameNumber = true;

	private GUIStyle style = new GUIStyle();

    public void Start()
    {
        

    }

    void OnGUI()
	{

		style.fontSize = (int)size;
		style.normal.textColor = Color.white;

		if (showTimeCode)
		{
			GUI.Label(new Rect(0, 0, size, size), "Time : " + FormatTime(), style);
		}

		if (showFrameNumber)
		{
			if (showTimeCode)
			{
				GUI.Label(new Rect(0, size, size, size), "Frame : " + (((startTime * framerate) + Time.frameCount) - 1), style);
			}
			else
			{
				GUI.Label(new Rect(0, 0, size, size), "Frame : " + (((startTime * framerate) + Time.frameCount) - 1), style);
			}
		}

		framecount = (int)(startTime * framerate) + Time.frameCount - 1;
	}


	string FormatTime() {

		string timeString = "";
		int remainingFrames = ((int)(startTime * 50) + Time.frameCount) - 1;

		timeString += Math.DivRem(remainingFrames, (int)framerate * 60, out remainingFrames).ToString("D2");
		timeString += ":";
		timeString += Math.DivRem(remainingFrames, (int)framerate, out remainingFrames).ToString("D2");
		timeString += ":";
		timeString += remainingFrames.ToString("D2");

		return timeString;

	}
}
