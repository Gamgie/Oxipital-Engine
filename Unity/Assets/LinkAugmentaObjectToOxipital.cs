using Augmenta;
using NUnit.Framework;
using Oxipital;
using System.Collections.Generic;
using UnityEngine;

public class LinkAugmentaObjectToOxipital : MonoBehaviour
{
    List<int> orbIdList = new List<int>();
	List<int> forceIdList = new List<int>();

    private OrbManager orbManager;
    private StandardForceManager forceManager;
    private AugmentaObject augmentaObject;

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
    {
        augmentaObject = GetComponent<AugmentaObject>();
	}

    // Update is called once per frame
    void Update()
    {
        
    }
}
