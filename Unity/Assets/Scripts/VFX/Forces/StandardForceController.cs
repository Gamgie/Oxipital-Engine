using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.VFX;

public class StandardForceController : MonoBehaviour
{
    public int ID
    {
        get
        {
            return forceID;
        }
    }

    public int forceID;
    public int forceCount = 1;

    [Space(10)]
    public float intensity;
    [Range(0, 20)]
    public float radius;
    public Vector3 axis;

    BalletPattern _pattern; // Handle positions of the force
    VisualEffect[] _vfxs;
    GraphicsBuffer _positionsBuffer;
    int _positionsBufferID;
    GraphicsBuffer _floatBuffer;
    int _floatBufferID;
    GraphicsBuffer _vector3Buffer;
    int _vector3BufferID;

    BalletManager _balletMngr;
    OrbsManager _orbsMngr;
    BalletPatternController _patternController;
    List<FieldInfo> _floatFields;
    List<FieldInfo> _vector3Fields;

    public void Initiliaze(OrbsManager orbsMngr, BalletManager balletMngr)
    {
        _orbsMngr = orbsMngr;
        _balletMngr = balletMngr;

        // Listen to orbManager to know when we created a new orb.
        // Update our list when it is the case
        if (_orbsMngr != null)
        {
            _orbsMngr.GetOnOrbCreated().AddListener(UpdateVfxArray);
        }
        else
        {
            Debug.LogError("Can not find Orbs Manager in " + gameObject.name);
        }

        if (_balletMngr != null)
        {
            // Add a pattern to ballet manager
            _pattern = _balletMngr.AddPattern(BalletManager.PatternGroup.Force);
            if (_pattern != null)
            {
                _patternController = this.gameObject.AddComponent<BalletPatternController>();
                _patternController.SetPattern(_pattern);
            }
        }
        else
        {
            Debug.LogError("Can't find Ballet Manager in " + gameObject.name);
        }

        if (_positionsBuffer == null)
        {
            _positionsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, forceCount, Marshal.SizeOf(typeof(Vector3)));
            _positionsBufferID = Shader.PropertyToID(forceID + " Positions Graphics Buffer");
        }

        // Parse force's field and prepare list
        Type type = typeof(StandardForceController);
        _floatFields = new List<FieldInfo>();
        _vector3Fields = new List<FieldInfo>();

        foreach (FieldInfo f in type.GetFields())
		{
            if(f.FieldType == typeof(float))
			{
                _floatFields.Add(f);
            }
            else if (f.FieldType == typeof(Vector3))
            {
                _vector3Fields.Add(f);
            }
        }

        if (_floatBuffer == null)
		{
            _floatBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, _floatFields.Count, Marshal.SizeOf(typeof(float)));
            _floatBufferID = Shader.PropertyToID(forceID + " Float Graphics Buffer");
        }
        if(_vector3Buffer == null)
		{
            _vector3Buffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, _vector3Fields.Count, Marshal.SizeOf(typeof(float)));
            _vector3BufferID = Shader.PropertyToID(forceID + " Vector3 Graphics Buffer");
        }
    }

    void Update()
    {
        if (_vfxs == null)
        {
            UpdateVfxArray();
        }

        // Update pattern dancer count
        if (_pattern != null && forceCount != _pattern.dancerCount)
        {
            _pattern.UpdateDancerCount(forceCount);

            if (_positionsBuffer != null)
                _positionsBuffer.Release();

            _positionsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, forceCount, Marshal.SizeOf(typeof(Vector3)));
        }

        // Update positions
        if (_positionsBuffer != null)
        {
            List<Vector3> target = GetPositions();
            if (target != null || target.Count != 0)
                _positionsBuffer.SetData(target);
        }

        // Update floats
        if(_floatBuffer != null)
		{
            List<float> floats = GetFloatList();
            if (floats != null || floats.Count != 0)
                _floatBuffer.SetData(floats);
		}

        // Update vector3
        if (_vector3Buffer != null)
        {
            List<Vector3> vec3 = GetVector3List();
            if (vec3 != null || vec3.Count != 0)
                _floatBuffer.SetData(vec3);
        }

        foreach (VisualEffect vfx in _vfxs)
        {
            if (vfx == null)
                UpdateVfxArray();

            if (vfx == null)
                return;

            // float Buffer
            if (vfx.HasGraphicsBuffer(_floatBufferID))
                vfx.SetGraphicsBuffer(_floatBufferID, _floatBuffer);

            // vector3 Buffer
            if (vfx.HasGraphicsBuffer(_vector3BufferID))
                vfx.SetGraphicsBuffer(_vector3BufferID, _vector3Buffer);

            // positions Buffer
            if (vfx.HasGraphicsBuffer(_positionsBufferID))
                vfx.SetGraphicsBuffer(_positionsBufferID, _positionsBuffer);
        }
    }

	private List<float> GetFloatList()
	{
		throw new NotImplementedException();
	}

	void UpdateVfxArray()
    {
        if (_orbsMngr == null)
        {
            _vfxs = GetComponentsInChildren<VisualEffect>();
        }
        else
        {
            _vfxs = _orbsMngr.GetComponentsInChildren<VisualEffect>();
        }
    }

    protected List<Vector3> GetPositions()
    {
        List<Vector3> targetPositions = new List<Vector3>();

        if (_pattern == null)
            return null;

        for (int i = 0; i < _pattern.dancerCount; i++)
        {
            targetPositions.Add(_pattern.GetPosition(i));
        }

        return targetPositions;
    }

    private void OnDestroy()
    {
        if (_positionsBuffer != null)
            _positionsBuffer.Release();
    }
}
