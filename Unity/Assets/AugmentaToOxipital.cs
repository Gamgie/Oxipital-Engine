using Augmenta;
using Oxipital;
using UnityEngine;

public class AugmentaToOxipital : MonoBehaviour
{
    OrbManager orbManager;
    AugmentaObject obj;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        orbManager = FindAnyObjectByType<OrbManager>();
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var group in orbManager.items) group.augmentaObject = obj;
    }

    public void OnObjectCreated(AugmentaObject obj)
    {
        Debug.Log("Object created");
        this.obj = obj;
    }

    public void OnObjectRemoved(AugmentaObject obj)
    {
        Debug.Log("Object removed");
        if(this.obj == obj) this.obj = null;
    }
}
