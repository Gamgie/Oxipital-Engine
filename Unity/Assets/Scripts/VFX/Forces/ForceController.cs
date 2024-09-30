using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.VFX;

public abstract class ForceController : MonoBehaviour
{
    public string Key
	{
        get
		{
            return key;
		}
	}

    public int forceID = 0;
    public int forceCount = 1;
    
    [Space(10)]
    public float intensity;
    [Range(0,15)]
    public float radius;
    public Vector3 axis;

    protected string key; // The name of the force
    protected BalletPattern pattern; // Handle positions of the force
    protected VisualEffect[] _vfxs;
    protected string _suffix = "";
    protected GraphicsBuffer _buffer;
    protected int _bufferID;
    
    private BalletManager _balletMngr;
    private OrbsManager _orbsMngr;
    private BalletPatternController _patternController;


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

        // Set suffix to handle same multiple force on the same object.
        if (forceID != 0)
        {
            _suffix = " " + forceID.ToString();
        }

        if(_balletMngr != null)
		{
            // Add a pattern to ballet manager
            pattern = _balletMngr.AddPattern(BalletManager.PatternGroup.Force);
            if(pattern != null)
			{
                _patternController = this.gameObject.AddComponent<BalletPatternController>();
                _patternController.SetPattern(pattern);
			}
        }
        else
		{
            Debug.LogError("Can't find Ballet Manager in " + gameObject.name);
		}

        if (_buffer == null)
		{
            _buffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, forceCount, Marshal.SizeOf(typeof(Vector3)));
            _bufferID = Shader.PropertyToID(key + " Graphics Buffer"); ;
        }
    }

    protected virtual void Update()
    {
        if(_vfxs == null)
		{
            UpdateVfxArray();
		}

        // Update pattern dancer count
        if(pattern != null && forceCount != pattern.dancerCount)
		{
            pattern.UpdateDancerCount(forceCount);

            if (_buffer != null)
                _buffer.Release();

            _buffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, forceCount, Marshal.SizeOf(typeof(Vector3)));
        }     

        // Update positions
        if (_buffer != null)
		{
            List<Vector3> target = GetPositions();
            if(target != null || target.Count != 0)
                _buffer.SetData(target);
        }

        foreach (VisualEffect vfx in _vfxs)
        {
            if (vfx == null)
                UpdateVfxArray();

            if (vfx == null)
                return;

            // Intensity
            if (intensity < 0.001 && intensity > -0.001)
                intensity = 0;

            if (vfx.HasFloat(key + " Intensity" + _suffix))
                vfx.SetFloat(key + " Intensity" + _suffix, intensity);

            // Radius
            if (vfx.HasFloat(key + " Radius" + _suffix))
                vfx.SetFloat(key + " Radius" + _suffix, radius);

            // Axis
            if (vfx.HasVector3(key + " Axis" + _suffix))
                vfx.SetVector3(key + " Axis" + _suffix, axis);

            // Buffer
            if (vfx.HasGraphicsBuffer(_bufferID))
                vfx.SetGraphicsBuffer(_bufferID, _buffer);
        }
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

        if (pattern == null)
            return null;

        for(int i = 0; i < pattern.dancerCount; i++)
		{
            targetPositions.Add(pattern.GetPosition(i));
		}

        return targetPositions;
	}

	private void OnDestroy()
	{
        if(_buffer != null)
            _buffer.Release();
	}

    public abstract ForceControllerData StoreData();

    public abstract void LoadData(ForceControllerData data);

    protected ForceControllerData StoreBaseData(ForceControllerData data)
    {
        data.forceID = forceID;
        data.key = key;
        data.forceCount = forceCount;
        data.intensity = intensity;
        data.radius = radius;
        data.axis = axis;

        return data;
	}

    protected void LoadBaseData(ForceControllerData data)
    {
        forceID = data.forceID;
        forceCount = data.forceCount;
        intensity = data.intensity;
        radius = data.radius;
        axis = data.axis;
    }
}

[System.Serializable] 
public class ForceControllerData
{
    public int forceID = 0;
    public string key = "";
    public int forceCount = 1;
    public float intensity;
    public float radius;
    public Vector3 axis;
    public List<AdditionalParameters> additionalParameters;
}

[System.Serializable] 
public class AdditionalParameters
{
    public string key;
    public float floatParameter;

    public AdditionalParameters(string key, float value)
    {
        this.key = key;
        this.floatParameter = value;
    }
}