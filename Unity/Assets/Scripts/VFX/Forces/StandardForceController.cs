using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.VFX;


public class InBuffer : Attribute
{
    public InBuffer(int index, BufferType type = BufferType.Auto)
    {
        this.index = index;
        this.type = type;
    }
    public enum BufferType { Auto, Float, Vector3 };
    public BufferType type = BufferType.Auto;
    public int index;
}
public class StandardForceController : MonoBehaviour
{
    public int forceID;
    public int forceCount = 1;

    [Space(20)]
    [Header("General Parameters")]
    [InBuffer(0)]
    public float intensity;
    [Range(0, 20)]
    [InBuffer(1)]
    public float radius;
    [InBuffer(0)]
    public Vector3 axis;

    [Header("Radial")]
    [Range(0, 1)]
    [InBuffer(2)]
    public float radialIntensity;
    [Range(0, 1)]
    [InBuffer(3)]
    public float radialFrequency;

    [Header("Axial")]
    [Range(0, 1)]
    [InBuffer(4)]
    public float axialIntensity;
    [InBuffer(1)]
    public Vector3 axialFrequency;
    [Range(0, 3)]
    [InBuffer(5)]
    public float axialFactor;

    [Header("Linear")]
    [Range(0, 1)]
    [InBuffer(6)]
    public float linearIntensity;

    [Header("Orthoradial")]
    [Range(0, 1)]
    [InBuffer(7)]
    public float orthoIntensity;
    [Range(0, 1)]
    [InBuffer(8)]
    public float orthoInnerRadius = 0.5f;
    [Range(0, 3)]
    [InBuffer(9)]
    public float orthoFactor = 2;
    [Range(0, 1)]
    [InBuffer(10)]
    public float orthoClockwise;

    [Header("Turbulence Curl")]
    [Range(0, 1)]
    [InBuffer(11)]
    public float curlIntensity;
    [Range(0, 5)]
    [InBuffer(12)]
    public float curlFrequency;
    [Range(0, 1)]
    [InBuffer(13)]
    public float curlDrag;
    [Range(1, 8)]
    [InBuffer(14)]
    public float curlOctaves;
    [Range(0, 1)]
    [InBuffer(15)]
    public float curlRoughness;
    [Range(0, 1)]
    [InBuffer(16)]
    public float curlLacunarity;
    [Range(0, 1)]
    [InBuffer(17)]
    public float curlScale;
    [Range(0, 1)]
    [InBuffer(18)]
    public float curlTranslation;

    [Header("Perlin")]
    [Range(0, 1)]
    [InBuffer(19)]
    public float perlinIntensity;
    [Range(0, 5)]
    [InBuffer(20)]
    public float perlinFrequency;
    [Range(1, 8)]
    [InBuffer(21)]
    public float perlinOctaves;
    [Range(0, 1)]
    [InBuffer(22)]
    public float perlinRoughness;
    [Range(0, 1)]
    [InBuffer(23)]
    public float perlinLacunarity;
    [Range(0, 1)]
    [InBuffer(24)]
    public float perlinTranslationSpeed;

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
            _positionsBufferID = Shader.PropertyToID("Force " + forceID + " Positions");
        }

        // Parse force's field and prepare list
        Type type = typeof(StandardForceController);
        _floatFields = new List<FieldInfo>();
        _vector3Fields = new List<FieldInfo>();

        foreach (FieldInfo f in type.GetFields())
        {
            InBuffer inBufferAttribute = f.GetCustomAttribute<InBuffer>();
            if (inBufferAttribute == null) continue;

            InBuffer.BufferType bufferType = inBufferAttribute.type;
            if (bufferType == InBuffer.BufferType.Auto)
            {
                if (f.FieldType == typeof(float)) bufferType = InBuffer.BufferType.Float;
                else if (f.FieldType == typeof(Vector3)) bufferType = InBuffer.BufferType.Vector3;
            }
            else
            {
                //check that type is compliant
                if (f.FieldType == typeof(float) && bufferType != InBuffer.BufferType.Float)
                    throw new Exception("Field " + f.Name + " is declared as a float but is not marked as such in the InBuffer attribute.");
            }

            switch (bufferType)
            {
                case InBuffer.BufferType.Float:
                    while (_floatFields.Count <= inBufferAttribute.index) _floatFields.Add(null);
                    _floatFields[inBufferAttribute.index] = f;
                    break;

                case InBuffer.BufferType.Vector3:
                    while (_vector3Fields.Count <= inBufferAttribute.index) _vector3Fields.Add(null);
                    _vector3Fields[inBufferAttribute.index] = f;
                    break;

                default:
                    throw new Exception("Field " + f.Name + " type <> buffer type mismatch.");
            }
        }

        if (_floatBuffer == null && _floatFields.Count != 0)
        {
            _floatBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, _floatFields.Count, Marshal.SizeOf(typeof(float)));
            _floatBufferID = Shader.PropertyToID("Force "+ forceID + " Floats");
        }
        else
		{
            Debug.LogError("[Force"+forceID+"] Cannot create float buffer at init.");
		}
        if (_vector3Buffer == null && _vector3Fields.Count != 0)
        {
            _vector3Buffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, _vector3Fields.Count, Marshal.SizeOf(typeof(Vector3)));
            _vector3BufferID = Shader.PropertyToID("Force " + forceID + " Vector3");
        }
        else
		{
            Debug.LogError("[Force" + forceID + "] Cannot create vector3 buffer at init.");
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
        if (_floatBuffer != null)
        {
            List<float> floats = GetList<float>(_floatFields, -1);
            if (floats != null || floats.Count != 0)
                _floatBuffer.SetData(floats);
        }

        // Update vector3
        if (_vector3Buffer != null)
        {
            List<Vector3> vec3 = GetList<Vector3>(_vector3Fields, Vector3.zero);
            if (vec3 != null || vec3.Count != 0)
                _vector3Buffer.SetData(vec3);
        }

        foreach (VisualEffect vfx in _vfxs)
        {
            if (vfx == null)
                UpdateVfxArray();

            if (vfx == null)
                return;

            // Float Buffer
            if (vfx.HasGraphicsBuffer(_floatBufferID))
                vfx.SetGraphicsBuffer(_floatBufferID, _floatBuffer);

            // Vector3 Buffer
            if (vfx.HasGraphicsBuffer(_vector3BufferID))
                vfx.SetGraphicsBuffer(_vector3BufferID, _vector3Buffer);

            // Positions Buffer
            if (vfx.HasGraphicsBuffer(_positionsBufferID))
                vfx.SetGraphicsBuffer(_positionsBufferID, _positionsBuffer);
        }
    }

    private List<T> GetList<T>(List<FieldInfo> fields,T defaultValue)
    {
        List<T> list = new List<T>();
        int index = 0;
        foreach (FieldInfo field in fields)
        {
            if (field == null)
            {
                Debug.LogWarning("Field " + index + " is null in " + gameObject.name + " for " + field.Name + " " + index + ", setting to default value");
                list.Add(defaultValue);
            }
            else
            {
                T value = (T)field.GetValue(this);
                list.Add(value);
            }
        }

        return list;
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
