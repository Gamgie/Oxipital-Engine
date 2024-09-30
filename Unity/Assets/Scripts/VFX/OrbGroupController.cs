using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbGroupController : MonoBehaviour
{
    public DataManager dataMngr;
    public BalletManager balletMngr;
    public int idControlled;
    public int orbCount;
    public OrbGroupController[] orbGroupControllers; // They need to know each other to not control the same orbgroup at the same time

    [Header("PS Parameters")]
    [Range(0, 200000)]
    public float rate;
    [Range(0, 200)]
    public float life;
    [ColorUsage(true, true)]
    public Color color;
    [Range(0, 1)]
    public float colorNoiseAmp;
    [Range(0, 10)]
    public float colorNoiseFreq;
    public bool useColorTexture;
    public int colorIntensity;
    [Range(0, 10)]
    public float colorSmoothSpeed;
    [Space(10)]
    [Range(0, 1)]
    public float alpha;
    [Range(0, 2)]
    public float alphaNoiseAmplitude = 0;
    [Range(0, 10)]
    public float alphaNoiseFrequency = 1;
    [Range(0, 8)]
    public int alphaNoiseOctaves = 1;
    [Range(0, 3)]
    public float alphaNoiseLacunarity = 2;
    [Range(0, 1)]
    public float alphaNoiseRoughness = 0.5f;
    [Space(10)]
    [Range(0, 100)]
    public float size;
    [Range(0, 10)]
    public float drag;
    [Range(0, 1)]
    public float velocityDrag;
    [Range(0, 20)]
    public float noisyDrag;
    [Range(0, 5)]
    public float noisyDragFrequency;
    public bool staticParticle;
    public bool stationaryTransparent;
    [Range(0,30)]
    public float stationaryMaxSpeed; // When in stationary, we interpolate alpha according to speed. This the max speed for alpha to reach value 1.

    [Header("Emitter Parameters")]
    public OrbGroup.EmitterShape emitterShape;
    public OrbGroup.EmitterPlacementMode emitterPlacementMode;
    public float emitterSize;
    [Range(0,1)]
    public float emitterSizeOffset; // Percent amount to decrease size for each element of this orb group
    public bool emitFromInside;
    public bool activateCollision;
    public bool showMesh;
    public Vector3 emitterRotation; // Rotation in euler angle of the emitter


    [Header("Ballet Pattern Parameters")]
    public BalletPattern.BalletPatternType patternType = BalletPattern.BalletPatternType.Circle;
    public Vector3 position; // Position of this pattern
    public Vector3 patternRotation = Vector3.zero; // Rotation in euler angle of this pattern
    [Range(0, 10)]
    public float patternSize = 1; // Size of this pattern
    [Range(0, 5)]
    public float speed = 1f; // Speed of the choreography
    [Range(0, 20)]
    public float lerpDuration = 3f; // Time for moving from a pattern to another
    [Range(0, 1)]
    public float phase; // Rotation phase

    [Header("Size LFO")]
    [Range(0, 2)]
    public float sizeLFOFrequency;
    [Range(0, 3)]
    public float sizeLFOAmplitude;

    public List<Vector3> manualPositions; // each Position is controlled Manually

    private OrbsManager _orbsManager;
    private int _idControlled = -1;

    // Start is called before the first frame update
    void Start()
    {
        // Initialize manual positions list
        manualPositions = new List<Vector3>();

        _orbsManager = transform.parent.GetComponent<OrbsManager>();

        OxipitalData loadedData = dataMngr.LoadData();
        if(loadedData.orbGroupControllersData == null)
		{
            Debug.LogError("No OrbGroupControllerdata found");
		}
        else
		{
            foreach (OrbGroupControllerData ogcData in loadedData.orbGroupControllersData)
            {
                if (ogcData.name == this.name)
                {
                    idControlled = ogcData.idControlled;
                    colorSmoothSpeed = ogcData.colorSmooth;
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Our id was updated then we need to update controller according to values
        if(idControlled != _idControlled)
		{
            // If it is already controlled by another OrbGroupController, don't update parameters
            if(!CheckAvailability())
            {
                rate = 0;
                life = 0;
                orbCount = 0;
                color = Color.black;
                colorNoiseAmp = 0;
                colorNoiseFreq = 0;
                useColorTexture = false;
                colorIntensity = 0;
                alpha = 0;
                alphaNoiseAmplitude = 0;
                alphaNoiseFrequency = 0;
                alphaNoiseOctaves = 0;
                alphaNoiseLacunarity = 0;
                alphaNoiseRoughness = 0f;
                size = 0;
                drag = 0;
                velocityDrag = 0;
                noisyDrag = 0;
                noisyDragFrequency = 0;
                staticParticle = false;
                stationaryTransparent = false;
                stationaryMaxSpeed = 0;
                emitterSize = 0;
                emitterSizeOffset = 0;
                emitFromInside = false;
                activateCollision = false;
                showMesh = false;

                position = Vector3.zero;
                patternRotation = Vector3.zero;
                emitterRotation = Vector3.zero;
                size = 0;
                speed = 0;
                lerpDuration = 0;
                phase = 0;
                sizeLFOFrequency = 0;
                sizeLFOAmplitude = 0;

                Debug.Log("Cannot control an OrbGroup already controlled");

                _idControlled = -1;

                return;
            }

            // Update data from the newly connected orbGroup
            foreach (OrbGroup oG in _orbsManager.orbs)
            {
                if (oG.orbGroupId == idControlled)
                {
                    // Get Orb parameters
                    orbCount = oG.GetOrbCount();
                    rate = oG.rate;
                    life = oG.life;
                    color = oG.color;
                    colorNoiseAmp = oG.colorNoiseAmp;
                    colorNoiseFreq = oG.colorNoiseFreq;
                    useColorTexture = oG.useColorTexture;
                    colorIntensity = oG.colorIntensity;
                    alpha = oG.alpha;
                    size = oG.size;
                    alphaNoiseAmplitude = oG.alphaNoiseAmplitude;
                    alphaNoiseFrequency = oG.alphaNoiseFrequency;
                    alphaNoiseOctaves = oG.alphaNoiseOctaves;
                    alphaNoiseLacunarity =oG. alphaNoiseLacunarity;
                    alphaNoiseRoughness = oG.alphaNoiseRoughness;
                    drag = oG.drag;
                    velocityDrag = oG.velocityDrag;
                    noisyDrag = oG.noisyDrag;
                    noisyDragFrequency = oG.noisyDragFrequency;
                    staticParticle = oG.staticParticle;
                    stationaryTransparent = oG.stationaryTransparent;
                    stationaryMaxSpeed = oG.stationaryMaxSpeed;
                    emitterShape = oG.emitterShape;
                    emitterPlacementMode = oG.emitterPlacementMode;
                    emitterSize = oG.emitterSize;
                    emitterSizeOffset = oG.emitterSizeOffset;
                    emitFromInside = oG.emitFromInside;
                    activateCollision = oG.activateCollision;
                    showMesh = oG.showMesh;
                    emitterRotation = oG.emitterRotation;

                    // Get ballet pattern parameters
                    BalletPattern balletPattern = balletMngr.GetPattern(BalletManager.PatternGroup.Orb,idControlled);
                    patternType = balletPattern.patternType;
                    position = balletPattern.position;
                    patternRotation = balletPattern.rotation;
                    patternSize = balletPattern.size;
                    speed = balletPattern.speed;
                    lerpDuration = balletPattern.lerpDuration;
                    phase = balletPattern.phase;
                    sizeLFOFrequency = balletPattern.sizeLFOFrequency;
                    sizeLFOAmplitude = balletPattern.sizeLFOAmplitude;
                    manualPositions = balletPattern.manualPositions;

                    _idControlled = idControlled;
                }
            }
        }
        else
		{
            foreach (OrbGroup oG in _orbsManager.orbs)
            {
                if (oG.orbGroupId == idControlled)
                { 
                    // Update orb parameters
                    oG.SetOrbCount(orbCount);
                    oG.rate = rate;
                    oG.life = life;
                    oG.color = Color.Lerp(oG.color, color, colorSmoothSpeed * Time.deltaTime * 0.1f);
                    oG.colorNoiseAmp = colorNoiseAmp;
                    oG.colorNoiseFreq = colorNoiseFreq;
                    oG.useColorTexture = useColorTexture;
                    oG.colorIntensity = colorIntensity;
                    oG.alpha = alpha;
                    oG.alphaNoiseAmplitude = alphaNoiseAmplitude;
                    oG.alphaNoiseFrequency = alphaNoiseFrequency;
                    oG.alphaNoiseOctaves = alphaNoiseOctaves;
                    oG.alphaNoiseLacunarity = alphaNoiseLacunarity;
                    oG.alphaNoiseRoughness = alphaNoiseRoughness;
                    oG.size = size;
                    oG.drag = drag;
                    oG.velocityDrag = velocityDrag;
                    oG.noisyDrag = noisyDrag;
                    oG.noisyDragFrequency = noisyDragFrequency;
                    oG.staticParticle = staticParticle;
                    oG.stationaryTransparent = stationaryTransparent;
                    oG.stationaryMaxSpeed = stationaryMaxSpeed;
                    oG.emitterShape = emitterShape;
                    oG.emitterPlacementMode = emitterPlacementMode;
                    oG.emitterSize = emitterSize;
                    oG.emitterSizeOffset = emitterSizeOffset;
                    oG.emitFromInside = emitFromInside;
                    oG.activateCollision = activateCollision;
                    oG.showMesh = showMesh;
                    oG.emitterRotation = emitterRotation;

                    // Update ballet pattern
                    BalletPattern balletPattern = balletMngr.GetPattern(BalletManager.PatternGroup.Orb,idControlled);
                    balletPattern.patternType = patternType;
                    balletPattern.position = position;
                    balletPattern.rotation = patternRotation;
                    balletPattern.size = patternSize;
                    balletPattern.speed = speed;
                    balletPattern.lerpDuration = lerpDuration;
                    balletPattern.phase = phase;
                    balletPattern.sizeLFOFrequency = sizeLFOFrequency;
                    balletPattern.sizeLFOAmplitude = sizeLFOAmplitude;
                    balletPattern.manualPositions = manualPositions;
                }
            }
        }
    }

    public bool CheckAvailability()
	{
        bool result = true;

        foreach(OrbGroupController ogc in orbGroupControllers)
		{
            if(ogc.idControlled == idControlled)
            {
                result = false;
            }
		}

        return result;
	}

    public void ResetPattern()
	{
        BalletPattern balletPattern = balletMngr.GetPattern(BalletManager.PatternGroup.Orb, idControlled);
        balletPattern.ResetSpeed();
    }
}

[System.Serializable]
public class OrbGroupControllerData
{
    public int idControlled;
    public string name;
    public float colorSmooth;
}