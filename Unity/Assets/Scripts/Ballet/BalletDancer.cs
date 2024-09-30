using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalletDancer : MonoBehaviour
{
    public int id;
    public bool isVisible;
    public bool isLinked;
    
    private  Color _color;
    private MeshRenderer _meshRenderer;

    // Start is called before the first frame update
    void Start()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if(_meshRenderer)
		{
            _meshRenderer.enabled = isVisible;
            _meshRenderer.material.color = _color;
        }
		else
		{
            _meshRenderer = GetComponent<MeshRenderer>();

            if (_meshRenderer == null)
                Debug.LogError("Can't find a mesh renderer on " + gameObject.name);
        } 
    }

    public void SetColor(Color c)
    {
        _color = c;
    }
}
