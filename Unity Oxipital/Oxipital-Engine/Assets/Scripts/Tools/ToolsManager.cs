using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oxipital;

public class ToolsManager : MonoBehaviour
{
    public bool activateGraphy;
    public bool activateReporter;

    [SerializeField]
    private GameObject graphy;

    [SerializeField]
    private GameObject reporter;

    // The camera's renderMainWindow shows or hides Graphy (the camera package no longer knows about this tool)
    private CameraController cameraController;

    void Start()
    {
        cameraController = FindFirstObjectByType<CameraController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (cameraController != null)
            activateGraphy = cameraController.renderMainWindow;

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
