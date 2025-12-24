using Oxipital;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public class RecordScene : MonoBehaviour
{
    public enum RecordState { record, replay, none};

    public RecordState recorderState;
    public bool record; // If we are in record state, then record a frame when isRecording is on
    public string clipName;
    public string clipPath;
    public GameObject balletParent;

    private GameObjectRecorder m_Recorder;
    private AnimationClip m_Clip;
    private bool m_isRecording;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(!Application.isEditor || recorderState == RecordState.none)
        {
            GetComponent<Animation>().enabled = false;
            return;
        }

        if(recorderState == RecordState.record)
        {
            // Prepare recording once all scene is set up
            Invoke("LateStart", 10);

            // Disable animation as we don't want to play it now
            GetComponent<Animation>().enabled = false;

            return;
        }
        else if (recorderState == RecordState.replay)
        {
            GetComponent<Animation>().Play();
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (recorderState != RecordState.record || m_Recorder == null)
            return;

        // we start a record so let's turn on recording
        if (record && !m_isRecording)
        {
            m_isRecording = true;

            // Create a new AnimationClip object
            m_Clip = new AnimationClip();
        }
            

        // Record this frame
        if (m_isRecording)
        {
            // Take a snapshot and record all the bindings values for this frame.
            m_Recorder.TakeSnapshot(Time.deltaTime);
        }

        // We stop record so let's save this recorder to a clip.
        if(!record && m_isRecording)
        {
            m_isRecording = false;

            m_Clip.legacy = true;
            m_Recorder.SaveToClip(m_Clip);

            SaveAnimationClip();
        }

    }

    void LateStart()
    {
        // Create recorder and record the script GameObject.
        m_Recorder = new GameObjectRecorder(gameObject);

        // Bind all necessary component
        m_Recorder.BindComponentsOfType<CameraController>(gameObject, true);
        m_Recorder.BindComponentsOfType<Transform>(balletParent, true);  
        m_Recorder.BindComponentsOfType<SpaceshipMovement>(gameObject, true);
        m_Recorder.BindComponentsOfType<OrbManager>(gameObject, true);
        m_Recorder.BindComponentsOfType<OrbGroup>(gameObject, true);
        m_Recorder.BindComponentsOfType<StandardForceManager>(gameObject, true);
        m_Recorder.BindComponentsOfType<StandardForceGroup>(gameObject, true);
        m_Recorder.BindComponentsOfType<LineDancePattern>(gameObject, true);
        m_Recorder.BindComponentsOfType<CircleDancePattern>(gameObject, true);
        m_Recorder.BindComponentsOfType<ManualDancePattern>(gameObject, true);
    }

    void SaveAnimationClip()
    {
        // Ensure the folder exists
        if (!AssetDatabase.IsValidFolder(clipPath.TrimEnd('/')))
        {
            Directory.CreateDirectory(clipPath);
            AssetDatabase.Refresh();
        }

        // Create the full path
        string fileName = clipName + ".anim";
        string fullPath = Path.Combine(clipPath, fileName);
        fullPath = fullPath.Replace("\\", "/"); // Convert all backslashes to forward slashes

        // Check if file already exists
        if (File.Exists(fullPath))
        {
            //Add a number suffix to make it unique.
            int counter = 1;
            string baseFileName = clipName;
            while (File.Exists(fullPath))
            {
                fileName = $"{baseFileName}_{counter}.anim";
                fullPath = Path.Combine(clipPath, fileName);
                fullPath = fullPath.Replace("\\", "/"); // Convert all backslashes to forward slashes
                counter++;
            }

            Debug.Log($"File already exists. Saving as: {fileName}");
        }

        // Save the clip as an asset in the project
        AssetDatabase.CreateAsset(m_Clip, fullPath);
        AssetDatabase.SaveAssets(); // Ensure assets are saved to disk
        AssetDatabase.Refresh(); // Refresh the asset database to make the new file visible

        Debug.Log($"Animation clip saved at: {fullPath}");
    }
}
