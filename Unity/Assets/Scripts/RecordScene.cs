using UnityEditor.Animations;
using UnityEngine;

public class RecordScene : MonoBehaviour
{
    public bool isRecording;
    public AnimationClip clip;

    private GameObjectRecorder m_Recorder;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Create recorder and record the script GameObject.
        m_Recorder = new GameObjectRecorder(gameObject);

        // Bind all the Transforms on the GameObject and all its children.
        m_Recorder.BindAll(this.gameObject, true);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (clip == null)
            return;

        // Take a snapshot and record all the bindings values for this frame.
        m_Recorder.TakeSnapshot(Time.deltaTime);
    }

    void OnDisable()
    {
        if (clip == null)
            return;

        if (m_Recorder.isRecording)
        {
            // Save the recorded session to the clip.
            m_Recorder.SaveToClip(clip);
        }
    }
}
