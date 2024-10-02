using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Dancer : MonoBehaviour
{
   
    public float weight = 0;
    public float radius = 1;

    //Kill routine
    public float killProgress = 0;
    float timeAtKill = 0;
    float timeToKill = 0;

    public float localPatternTime = 0;

    public float randomFactor = 0;

    Vector3 lastPosition;
    public Vector3 velocity;
    virtual public void Start()
    {
        randomFactor = Random.value;
    }
    virtual public void Update()
    {
        if (timeToKill > 0) killProgress = (Time.time - timeAtKill) / timeToKill;

        //velocity = Vector3.Distance(transform.localPosition, lastPosition) / Time.deltaTime;
        lastPosition = transform.localPosition;
    }

    public void kill(float time = 0)
    {
        if (time == 0)
        {
            Destroy(gameObject);
            return;
        }

        timeAtKill = Time.time;
        timeToKill = time;
        killProgress = 0;
        Destroy(gameObject, timeToKill);
    }

    private void OnDrawGizmos()
    {
        float w = weight * (1 - killProgress);
        Color color = Color.Lerp(new Color(1.0f, 0, 0, 0), Color.red, w);
        Gizmos.color = color;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}