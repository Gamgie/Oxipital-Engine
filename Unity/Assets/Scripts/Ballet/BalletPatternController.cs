using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalletPatternController : MonoBehaviour
{
    [Header("Ballet Pattern Parameters")]
    public BalletPattern.BalletPatternType patternType = BalletPattern.BalletPatternType.Point;
    public Vector3 position; // Position of this pattern
    public Vector3 patternRotation = Vector3.zero; // Rotation in euler angle of this pattern
    [Range(0, 10)]
    public float patternSize = 1; // Size of this pattern
    [Range(0, 5)]
    public float speed = 1f; // speed of the choreography
    [Range(0, 20)]
    public float lerpDuration = 3f; // Time for moving from a pattern to another
    [Range(0, 1)]
    public float phase; // Rotation phase

    [Header("Size LFO")]
    [Range(0, 2)]
    public float sizeLFOFrequency;
    [Range(0, 3)]
    public float sizeLFOAmplitude;

    private BalletPattern m_pattern;

    // Update is called once per frame
    void Update()
    {
        if(m_pattern != null)
		{
            m_pattern.patternType = patternType;
            m_pattern.position = position;
            m_pattern.rotation = patternRotation;
            m_pattern.size = patternSize;
            m_pattern.speed = speed;
            m_pattern.lerpDuration = lerpDuration;
            m_pattern.phase = phase;
            m_pattern.sizeLFOFrequency = sizeLFOFrequency;
            m_pattern.sizeLFOAmplitude = sizeLFOAmplitude;
        }

    }

    public void SetPattern(BalletPattern p)
    {
        m_pattern = p;

        // Update parameter of the controller to pattern data
        // Get ballet pattern parameters
        patternType = p.patternType;
        position = p.position;
        patternRotation = p.rotation;
        patternSize = p.size;
        speed = p.speed;
        lerpDuration = p.lerpDuration;
        phase = p.phase;
        sizeLFOFrequency = p.sizeLFOFrequency;
        sizeLFOAmplitude = p.sizeLFOAmplitude;
    }

    public void ResetPattern()
	{
        m_pattern.ResetSpeed();
	}

}
