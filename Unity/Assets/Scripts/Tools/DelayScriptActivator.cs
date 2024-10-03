using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelayScriptActivator : MonoBehaviour
{
    public MonoBehaviour scriptToDelay;
    public float delay;

    private void OnEnable()
    {
        StartCoroutine(EnableScript(delay));
    }

    IEnumerator EnableScript(float Delay)
    {
        yield return new WaitForSeconds(delay);
        scriptToDelay.enabled = true;
    }
}
