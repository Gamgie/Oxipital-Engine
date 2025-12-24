using Oxipital;
using UnityEditor.Animations;
using UnityEngine;

public class RecordScene : MonoBehaviour
{
    public enum RecordState { record, replay, none};

    public RecordState recordState;
    public bool isRecording; // If we are in record state, then record a frame when isRecording is on
    public AnimationClip clip;
    public Transform orbitalTransform;

    private GameObjectRecorder m_Recorder;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(!Application.isEditor || recordState == RecordState.none)
        {
            GetComponent<Animation>().enabled = false;
            return;
        }

        if(recordState == RecordState.record)
        {
            Invoke("LateStart", 10);
            GetComponent<Animation>().enabled = false;
            return;
        }
        else if (recordState == RecordState.replay)
        {
            GetComponent<Animation>().Play();
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (clip == null || recordState != RecordState.record || m_Recorder == null)
            return;

        if(isRecording)
        {
            // Take a snapshot and record all the bindings values for this frame.
            m_Recorder.TakeSnapshot(Time.deltaTime);
        }
    }

    void OnDisable()
    {
        if (clip == null || recordState != RecordState.record)
            return;

        if (m_Recorder.isRecording)
        {
            // Save the recorded session to the clip.
            clip.legacy = true;
            m_Recorder.SaveToClip(clip);
        }
    }

    void LateStart()
    {
        // Create recorder and record the script GameObject.
        m_Recorder = new GameObjectRecorder(gameObject);

        // Bind all the Transforms on the GameObject and all its children.
        //m_Recorder.BindAll(this.gameObject, true);
        m_Recorder.BindComponentsOfType<CameraController>(gameObject, true);
        //m_Recorder.BindComponentsOfType<OrbitalMovement>(gameObject, true);
        m_Recorder.BindComponentsOfType<Transform>(orbitalTransform.gameObject, false);  
        m_Recorder.BindComponentsOfType<SpaceshipMovement>(gameObject, true);
        m_Recorder.BindComponentsOfType<OrbManager>(gameObject, true);
        m_Recorder.BindComponentsOfType<OrbGroup>(gameObject, true);
        m_Recorder.BindComponentsOfType<StandardForceManager>(gameObject, true);
        m_Recorder.BindComponentsOfType<StandardForceGroup>(gameObject, true);
        m_Recorder.BindComponentsOfType<LineDancePattern>(gameObject, true);
        m_Recorder.BindComponentsOfType<CircleDancePattern>(gameObject, true);
        m_Recorder.BindComponentsOfType<ManualDancePattern>(gameObject, true);

        clip.ClearCurves();
    }
}
