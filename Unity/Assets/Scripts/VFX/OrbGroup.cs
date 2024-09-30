using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using System;

[ExecuteInEditMode]
public class OrbGroup : MonoBehaviour
{
    public enum EmitterShape { Sphere, Plane, Torus, Cube, Pipe, Egg, Line, Circle, Merkaba, Pyramid, Landscape }
    public enum EmitterPlacementMode
    {
        Surface,
        Edge
    }

    public int orbGroupId;
    public VisualEffect orbPrefab;
    public int patternID = -1;

    [Header("PS Parameters")]
    [Range(0, 4000)]
    public float rate;
    [Range(0, 200)]
    public float life;
    [ColorUsage(true, true)]
    public Color color;
    public float colorNoiseAmp;
    public float colorNoiseFreq;
    public bool useColorTexture;
    public Texture colorTexture;
    public int colorIntensity;
    [Range(0, 1)]
    public float alpha;
    public float alphaNoiseAmplitude = 0;
    public float alphaNoiseFrequency = 1;
    public int alphaNoiseOctaves = 1;
    public float alphaNoiseLacunarity = 2;
    public float alphaNoiseRoughness = 0.5f;
    [Range(0, 100)]
    public float size;
    public float drag;
    [Range(0, 1)]
    public float velocityDrag;
    [Range(0, 20)]
    public float noisyDrag;
    [Range(0, 5)]
    public float noisyDragFrequency;
    public bool staticParticle;
    public bool stationaryTransparent;
    public float stationaryMaxSpeed; // When in stationary, we interpolat alpha according to speed. This the max speed for alpha to reach value 1.

    [Header("Emitter Parameters")]
    public EmitterShape emitterShape;
    public EmitterPlacementMode emitterPlacementMode;
    public Vector3 emitterPosition;
    public Vector3 emitterRotation;
    public float emitterSize;
    [Range(0, 1)]
    public float emitterSizeOffset;
    public Mesh[] meshArray;
    public Texture[] sdfCollisionArray;
    public bool emitFromInside;
    public bool activateCollision;
    public bool showMesh = false;

    public OrbGroupData data = new OrbGroupData();

    private List<VisualEffect> _visualEffects;
    private int _emitterShapeIndex;
    private OrbsManager _orbsMngr;
    private BalletPattern _pattern;
    private int _orbCount = 0;
    private MeshRenderer[] _meshRenderer;
    private MeshFilter[] _meshFilter;

    public void Initialize(OrbsManager orbsMngr)
    {
        this._orbsMngr = orbsMngr;
        _visualEffects = new List<VisualEffect>();

        // Add a pattern linked to this orbGroup.
        _pattern = orbsMngr.balletMngr.AddPattern(BalletManager.PatternGroup.Orb);
        patternID = _pattern.id;

        // Initialize with the first orb
        if (data != null)
        {
            SetOrbCount(data.orbCount);
        }
        else
        {
            SetOrbCount(1);
            Debug.LogError("No data found while creating " + gameObject.name + ". Initialize orbcount to 1 by default because no loaded data found at startup.");
        }


        // Update shape according to index
        SetEmitterShape();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateEmitter();

        UpdateVisualEffect();

        // Update pattern link
        if (patternID != -1) // Update only if we have selected a pattern id
        {
            if (_pattern != null)
            {
                if (_pattern.id != patternID)
                {
                    _pattern = _orbsMngr.GetPattern(patternID); // Id are diferent so we should update
                }
            }
            else // If pattern is null, it means we have to update also
            {
                _pattern = _orbsMngr.GetPattern(patternID);
            }

            if (_pattern == null)
                patternID = -1;
        }

        UpdatePositions();
    }

    void UpdateVisualEffect()
    {
        foreach (VisualEffect vfx in _visualEffects)
        {
            if (vfx == null)
            {
                Debug.LogError(vfx.gameObject.name + " should have a visual effect attached to it.");
                return;
            }

            // Ps parameters update
            if (vfx.HasInt("Rate") == true)
                vfx.SetInt("Rate", Convert.ToInt32(rate));

            if (vfx.HasFloat("LifeTime") == true)
                vfx.SetFloat("LifeTime", life);

            if (vfx.HasFloat("Alpha") == true)
                vfx.SetFloat("Alpha", alpha);

            if (vfx.HasFloat("Alpha Noise Amplitude") == true)
                vfx.SetFloat("Alpha Noise Amplitude", alphaNoiseAmplitude);

            Vector4 alphaParameters = new Vector4();
            alphaParameters.x = alphaNoiseFrequency;
            alphaParameters.y = alphaNoiseOctaves;
            alphaParameters.z = alphaNoiseRoughness;
            alphaParameters.w = alphaNoiseLacunarity;

            if (vfx.HasVector4("Alpha Noise Parameters") == true)
                vfx.SetVector4("Alpha Noise Parameters", alphaParameters);

            if (vfx.HasFloat("Size") == true)
                vfx.SetFloat("Size", size);

            if (vfx.HasFloat("Linear Drag") == true)
                vfx.SetFloat("Linear Drag", drag);

            if (vfx.HasFloat("Velocity Drag") == true)
                vfx.SetFloat("Velocity Drag", velocityDrag);

            float factor = Mathf.Pow(2, colorIntensity);
            if (vfx.HasVector4("Color") == true)
                vfx.SetVector4("Color", new Vector3(color.r * factor, color.g * factor, color.b * factor));

            if (vfx.HasFloat("Color_NoiseAmp") == true)
                vfx.SetFloat("Color_NoiseAmp", colorNoiseAmp);

            if (vfx.HasFloat("Color_NoiseFreq") == true)
                vfx.SetFloat("Color_NoiseFreq", colorNoiseFreq);

            if (vfx.HasBool("UseColorTexture") == true)
                vfx.SetBool("UseColorTexture", useColorTexture);

            if (vfx.HasBool("Static Particle") == true)
                vfx.SetBool("Static Particle", staticParticle);

            if (vfx.HasBool("Stationary Transparent") == true)
                vfx.SetBool("Stationary Transparent", stationaryTransparent);

            if (vfx.HasFloat("Stationary Max Speed") == true)
                vfx.SetFloat("Stationary Max Speed", stationaryMaxSpeed);

            if (vfx.HasFloat("Noisy Linear Drag") == true)
                vfx.SetFloat("Noisy Linear Drag", noisyDrag);

            if (vfx.HasFloat("Noisy Linear Drag Frequency") == true)
                vfx.SetFloat("Noisy Linear Drag Frequency", noisyDragFrequency);
        }
    }

    void UpdateEmitter()
    {
        int i = 0;
        foreach (VisualEffect vfx in _visualEffects)
        {
            // If no emitter mesh in graph then no need to go further
            if (vfx.HasMesh("Emitter Mesh") == false)
                return;

            // Compute EmitterSize linked to size offset
            float actualEmitterSize = Math.Max(emitterSize * (1 - i * emitterSizeOffset), 0);

            // Update Emitter transform
            if (vfx.HasVector3("Emitter Angles") == true)
                vfx.SetVector3("Emitter Angles", emitterRotation);

            if (vfx.HasFloat("Emitter Scale") == true)
                vfx.SetFloat("Emitter Scale", actualEmitterSize);

            if (_meshRenderer != null)
            {
                foreach (MeshRenderer mr in _meshRenderer)
                {
                    if (mr == null)
                    {
                        _meshRenderer = GetComponentsInChildren<MeshRenderer>();
                        break;
                    }

                    if (showMesh)
                    {
                        mr.transform.localScale = new Vector3(actualEmitterSize * 0.95f, actualEmitterSize * 0.95f, actualEmitterSize * 0.95f);
                        mr.transform.rotation = Quaternion.Euler(emitterRotation);
                        mr.enabled = true;

                    }
                    else
                    {
                        mr.enabled = false;
                    }
                }
            }

            // Check if we need to update mesh in graph
            _emitterShapeIndex = GetEmitterShapeIndex();
            if (emitterShape == EmitterShape.Line)
            {
                if (vfx.HasBool("isCircle") == true)
                    vfx.SetBool("isCircle", false);

                if (vfx.HasBool("isLine") == true)
                    vfx.SetBool("isLine", true);
            }
            else if (emitterShape == EmitterShape.Circle)
            {
                if (vfx.HasBool("isLine") == true)
                    vfx.SetBool("isLine", false);

                if (vfx.HasBool("isCircle") == true)
                    vfx.SetBool("isCircle", true);
            }
            else
            {
                if (vfx.HasBool("isLine") == true)
                    vfx.SetBool("isLine", false);

                if (vfx.HasBool("isCircle") == true)
                    vfx.SetBool("isCircle", false);

                Mesh actualMesh = vfx.GetMesh("Emitter Mesh");
                if (actualMesh != meshArray[_emitterShapeIndex])
                {
                    vfx.SetMesh("Emitter Mesh", meshArray[_emitterShapeIndex]);
                    vfx.SetTexture("Collision SDF", sdfCollisionArray[_emitterShapeIndex]);
                    foreach (MeshFilter mF in _meshFilter)
                    {
                        if (meshArray[_emitterShapeIndex] != null)
                            mF.mesh = meshArray[_emitterShapeIndex];
                    }
                }
            }

            if (vfx.HasBool("Emit From Inside") == true)
                vfx.SetBool("Emit From Inside", emitFromInside);

            if (vfx.HasBool("Activate Collision") == true)
                vfx.SetBool("Activate Collision", activateCollision);

            if (vfx.HasInt("Emitter Placement Mode") == true)
            {
                int switchPlacementMode = -1;

                switch (emitterPlacementMode)
                {
                    case EmitterPlacementMode.Surface:
                        switchPlacementMode = 0;
                        break;
                    case EmitterPlacementMode.Edge:
                        switchPlacementMode = 1;
                        break;
                    default:
                        break;
                }

                vfx.SetInt("Emitter Placement Mode", switchPlacementMode);
            }
            i++;
        }
    }

    void UpdatePositions()
    {
        if (_pattern == null)
            return;

        if (_meshRenderer == null)
        {
            _meshRenderer = GetComponentsInChildren<MeshRenderer>();
        }

        for (int i = 0; i < _visualEffects.Count; i++)
        {
            if (_visualEffects[i].HasVector3("Emitter Position") == true)
            {
                _visualEffects[i].SetVector3("Emitter Position", _pattern.GetPosition(i));
                if (_meshRenderer[i] != null)
                {
                    _meshRenderer[i].transform.position = _pattern.GetPosition(i);
                }
            }
        }
    }

    private int GetEmitterShapeIndex()
    {
        int result = -1;

        switch (emitterShape)
        {
            case EmitterShape.Sphere:
                result = 0;
                break;
            case EmitterShape.Plane:
                result = 1;
                break;
            case EmitterShape.Cube:
                result = 2;
                break;
            case EmitterShape.Torus:
                result = 3;
                break;
            case EmitterShape.Pipe:
                result = 4;
                break;
            case EmitterShape.Egg:
                result = 5;
                break;
            case EmitterShape.Line:
                result = 6;
                break;
            case EmitterShape.Circle:
                result = 7;
                break;
            case EmitterShape.Merkaba:
                result = 8;
                break;
            case EmitterShape.Pyramid:
                result = 9;
                break;
            case EmitterShape.Landscape:
                result = 10;
                break;
        }

        return result;
    }

    private void SetEmitterShape()
    {
        switch (_emitterShapeIndex)
        {
            case 0:
                emitterShape = EmitterShape.Sphere;
                break;
            case 1:
                emitterShape = EmitterShape.Plane;
                break;
            case 2:
                emitterShape = EmitterShape.Cube;
                break;
            case 3:
                emitterShape = EmitterShape.Torus;
                break;
            case 4:
                emitterShape = EmitterShape.Pipe;
                break;
            case 5:
                emitterShape = EmitterShape.Egg;
                break;
            case 6:
                emitterShape = EmitterShape.Line;
                break;
            case 7:
                emitterShape = EmitterShape.Circle;
                break;
            case 8:
                emitterShape = EmitterShape.Merkaba;
                break;
            case 9:
                emitterShape = EmitterShape.Pyramid;
                break;
            case 10:
                emitterShape = EmitterShape.Landscape;
                break;
        }
    }

    // Reset all particle system in this orbgroup
    public void Reinit()
    {
        foreach (VisualEffect vfx in _visualEffects)
        {
            if (vfx == null)
            {
                Debug.LogError("Trying to reinit an invalid object");
                return;
            }

            vfx.Reinit();
        }
    }

    void AddOrb()
    {
        if (_visualEffects == null)
        {
            Debug.LogError("Try to add an Orb in a null list from " + gameObject.name);
            return;
        }

        VisualEffect o = Instantiate(orbPrefab).GetComponent<VisualEffect>();
        o.name = "Orb" + orbGroupId + "_" + _visualEffects.Count;
        o.transform.parent = transform;
        _visualEffects.Add(o);

        // Send a message to the manager
        _orbsMngr.GetOnOrbCreated().Invoke();
        Debug.Log(name + " / " + o.name + " created.");

        // Update OrbCount accordingly
        _orbCount = _visualEffects.Count;

        // Add a dancer in pattern
        if (_pattern != null)
        {
            if (_pattern.dancerCount < GetOrbCount())
                _pattern.AddDancer();
        }
        else
        {
            Debug.LogError("No pattern found in " + this.gameObject.name);
        }

        // Get Mesh references
        _meshFilter = GetComponentsInChildren<MeshFilter>();
        _meshRenderer = GetComponentsInChildren<MeshRenderer>();
    }

    void DestroyOrb(int index = -1)
    {
        if (_visualEffects == null)
        {
            Debug.LogError("Try to destroy an Orb in a null list from " + gameObject.name);
            return;
        }

        VisualEffect orbToBeDestroyed = null;

        if (index == -1) // Remove last one
        {
            index = _visualEffects.Count - 1;
            orbToBeDestroyed = _visualEffects[index];
            _pattern.RemoveDancer();
        }
        else // Remove at index
        {
            orbToBeDestroyed = _visualEffects[index];
            _pattern.RemoveDancer(index);
        }

        // Set the particle rate to 0 for the one is about to be destroyed
        if (orbToBeDestroyed.HasInt("Rate") == true)
            orbToBeDestroyed.SetInt("Rate", Convert.ToInt32(0));

        // Destroy orb after all particles are dead
        Destroy(orbToBeDestroyed.gameObject, life);
        _visualEffects.RemoveAt(index);

        // Update OrbCount accordingly
        _orbCount = _visualEffects.Count;
        _orbsMngr.GetOnOrbCreated().Invoke();
    }

    public int GetOrbCount()
    {
        return _orbCount;
    }

    public void SetOrbCount(int count)
    {
        if (count == _orbCount)
        {
            // Update OrbCount to ensure there are diferent.
            _orbCount = _visualEffects.Count;
            return;
        }


        //_pattern.UpdateDancerCount(count);

        // Update list of vfx.
        if (count > _visualEffects.Count)
        {
            int instanceCount = count - _visualEffects.Count;
            for (int i = 0; i < instanceCount; i++)
            {
                AddOrb();
            }
        }
        else
        {
            int removeCount = _visualEffects.Count - count;
            for (int i = 0; i < removeCount; i++)
            {
                DestroyOrb();
            }
        }
    }

    private void OnDestroy()
    {
        _orbsMngr.balletMngr.RemovePattern(BalletManager.PatternGroup.Orb, orbGroupId);
    }

    private void RemoveElement<T>(ref T[] arr, int index)
    {
        for (int i = index; i < arr.Length - 1; i++)
        {
            arr[i] = arr[i + 1];
            Array.Resize(ref arr, arr.Length - 1);
        }
    }

    #region ManageData
    public OrbGroupData StoreData()
    {
        data.orbGroupId = orbGroupId;
        data.name = name;
        data.orbCount = GetOrbCount();
        data.rate = rate;
        data.life = life;
        data.colorR = color.r;
        data.colorG = color.g;
        data.colorB = color.b;
        data.colorNoiseAmp = colorNoiseAmp;
        data.colorNoiseFreq = colorNoiseFreq;
        data.useColorTexture = useColorTexture;
        data.colorIntensity = colorIntensity;
        data.alpha = alpha;
        data.alphaNoiseAmplitude = alphaNoiseAmplitude;
        data.alphaNoiseFrequency = alphaNoiseFrequency;
        data.alphaNoiseOctaves = alphaNoiseOctaves;
        data.alphaNoiseLacunarity = alphaNoiseLacunarity;
        data.alphaNoiseRoughness = alphaNoiseRoughness;
        data.size = size;
        data.drag = drag;
        data.velocityDrag = velocityDrag;
        data.noisyDrag = noisyDrag;
        data.noisyDragFrequency = noisyDragFrequency;
        data.staticParticle = staticParticle;
        data.stationaryTransparent = stationaryTransparent;
        data.stationaryMaxSpeed = stationaryMaxSpeed;
        data.emitterShapeIndex = GetEmitterShapeIndex();
        data.emitterPlacementMode = (int)emitterPlacementMode;
        data.emitterPositionX = emitterPosition.x;
        data.emitterPositionY = emitterPosition.y;
        data.emitterPositionZ = emitterPosition.z;
        data.emitterOrientationX = emitterRotation.x;
        data.emitterOrientationY = emitterRotation.y;
        data.emitterOrientationZ = emitterRotation.z;
        data.emitterSize = emitterSize;
        data.emitterSizeOffset = emitterSizeOffset;
        data.emitFromInside = emitFromInside;
        data.activateCollision = activateCollision;
        data.showMesh = showMesh;

        return data;
    }

    public void LoadData()
    {
        orbGroupId = data.orbGroupId;
        name = data.name;
        rate = data.rate;
        life = data.life;
        color = new Color(data.colorR, data.colorG, data.colorB);
        colorNoiseAmp = data.colorNoiseAmp;
        colorNoiseFreq = data.colorNoiseFreq;
        useColorTexture = data.useColorTexture;
        colorIntensity = data.colorIntensity;
        alpha = data.alpha;
        alphaNoiseAmplitude = data.alphaNoiseAmplitude;
        alphaNoiseFrequency = data.alphaNoiseFrequency;
        alphaNoiseOctaves = data.alphaNoiseOctaves;
        alphaNoiseLacunarity = data.alphaNoiseLacunarity;
        alphaNoiseRoughness = data.alphaNoiseRoughness;
        size = data.size;
        drag = data.drag;
        noisyDrag = data.noisyDrag;
        noisyDragFrequency = data.noisyDragFrequency;
        velocityDrag = data.velocityDrag;
        staticParticle = data.staticParticle;
        stationaryTransparent = data.stationaryTransparent;
        stationaryMaxSpeed = data.stationaryMaxSpeed;
        _emitterShapeIndex = data.emitterShapeIndex;
        emitterPlacementMode = (EmitterPlacementMode)data.emitterPlacementMode;
        emitterPosition = new Vector3(data.emitterPositionX, data.emitterPositionY, data.emitterPositionZ);
        emitterRotation = new Vector3(data.emitterOrientationX, data.emitterOrientationY, data.emitterOrientationZ);
        emitterSize = data.emitterSize;
        emitterSizeOffset = data.emitterSizeOffset;
        emitFromInside = data.emitFromInside;
        activateCollision = data.activateCollision;
        showMesh = data.showMesh;
    }
    #endregion
}

[System.Serializable]
public class OrbGroupData
{
    public int orbGroupId;
    public string name;
    public int orbCount;
    public float rate;
    public float life;
    public float colorR;
    public float colorG;
    public float colorB;
    public float colorNoiseAmp;
    public float colorNoiseFreq;
    public float colorNoiseOffset;
    public bool useColorTexture;
    public int colorIntensity;
    public float alpha;
    public float alphaNoiseAmplitude;
    public float alphaNoiseFrequency;
    public int alphaNoiseOctaves;
    public float alphaNoiseLacunarity;
    public float alphaNoiseRoughness;
    public float size;
    public float drag;
    public float velocityDrag;
    public float noisyDrag;
    public float noisyDragFrequency;
    public bool staticParticle;
    public bool stationaryTransparent;
    public float stationaryMaxSpeed;
    public int emitterShapeIndex;
    public int emitterPlacementMode;
    public float emitterPositionX;
    public float emitterPositionY;
    public float emitterPositionZ;
    public float emitterOrientationX;
    public float emitterOrientationY;
    public float emitterOrientationZ;
    public float emitterSize;
    public float emitterSizeOffset;
    public bool emitFromInside;
    public bool activateCollision;
    public bool showMesh;
}
