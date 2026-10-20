using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolsManager : MonoBehaviour
{
    public bool activateGraphy;
    public bool activateReporter;

    [SerializeField]
    private GameObject graphy;

    [SerializeField]
    private GameObject reporter;

    // Update is called once per frame
    void Update()
    {
        if(activateGraphy != graphy.activeSelf)
		{
            graphy.SetActive(activateGraphy);

        }

        if (activateReporter != reporter.activeSelf)
        {
            reporter.SetActive(activateReporter);

        }
    }
}
