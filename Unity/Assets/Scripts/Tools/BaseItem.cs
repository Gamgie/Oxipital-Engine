using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseItem : MonoBehaviour
{
    //Kill routine
    internal float killProgress = 0;
    float timeAtKill = 0;
    float timeToKill = 0;

    
    virtual protected void Update()
    {
        if (timeToKill > 0) killProgress = (Time.time - timeAtKill) / timeToKill;
    }

    virtual public void kill(float time = 0)
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
}
