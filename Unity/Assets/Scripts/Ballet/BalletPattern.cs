using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalletPattern : MonoBehaviour
{
    public enum BalletPatternType { Point, Line, Circle, Orbit, Atom, Manual }

    public BalletDancer dancerPrefab;

    public int id;
    public int dancerCount;
    public BalletPatternType patternType = BalletPatternType.Point;
    public Vector3 position; // Position of this pattern
    public Vector3 rotation = Vector3.zero; // Rotation in euler angle of this pattern
    [Range(0,10)]
    public float size = 1; // Size of this pattern
    [Range(0, 5)]
    public float speed = 1f; // speed of the choreography
    [Range(0, 10)]
    public float lerpDuration = 3f; // Time for moving from a pattern to another
    public AnimationCurve lerpCurve; // Interpolation curve for lerping
    [Range(0, 1)]
    public float phase; // Rotation phase

    [Header("Size LFO")]
    [Range(0, 10)]
    public float sizeLFOFrequency;
    [Range(0, 10)]
    public float sizeLFOAmplitude;

    [Space(20)]
    public List<Vector3> manualPositions; // each Position is controlled Manually

    [Header("Private members")]
    private List<BalletDancer> dancers; // a list of objects to choreograph
    private List<Vector3> cirlePositions; // Computed position of the circle
    private List<Vector3> linePositions; // Computed position of the line
    private List<Vector3> orbitPositions; // Computed position of the line
    private List<Vector3> atomPositions; // Computed position of the line
    private List<Vector3> startPositions; // starting positions for lerp
    private List<Vector3> targetPositions; // ending positions for lerp
    private float currentSpeed; // internal speed
    private BalletPatternType lastPatternType;
    private bool isLerping;
    private float lerpStartTime;
    private float currentSize; // Actual size after being modified by LFO
    private List<OrbitData> orbitData; // A list of element needed by the orbit & atom pattern
    private Color dancerColor; // Store the color attribute to the debug dancer game object.

    public void Init(BalletManager balletMngr)
    {
        // Initialize list
        dancers = new List<BalletDancer>();
        cirlePositions = new List<Vector3>();
        linePositions = new List<Vector3>();
        orbitPositions = new List<Vector3>();
        atomPositions = new List<Vector3>();
        startPositions = new List<Vector3>();
        targetPositions = new List<Vector3>();

        if(manualPositions == null)
		{
            manualPositions = new List<Vector3>();
        }

        dancerColor = GenerateRandomColor();

        UpdateDancerCount(dancerCount);
        GenerateOrbits();

        lastPatternType = patternType;

        currentSpeed = speed;
    }

    public void UpdateMovement()
	{
        transform.position = position;
        transform.eulerAngles = rotation;

        // Update Speed
        currentSpeed += speed / 1000;

        // Update Size
        currentSize = size + sizeLFOAmplitude * Mathf.Sin(sizeLFOFrequency * Mathf.PI * 2f * Time.time);

        ComputeCircleMovement();
        ComputeLineMovement();
        ComputeOrbitMovement();
        ComputeAtomMovement();
    }

    public void ApplyMovement()
	{
        if (dancers == null)
            return;

        // If we change pattern type then start lerping
        if(lastPatternType != patternType)
		{
            lerpStartTime = Time.time;

            // Save starting position for lerping
            switch (lastPatternType)
            {
                case BalletPatternType.Circle:
                    startPositions = cirlePositions;
                    break;
                case BalletPatternType.Line:
                    startPositions = linePositions;
                    break;
                case BalletPatternType.Point:
                    List<Vector3> onePosition = new List<Vector3>();
                    foreach (BalletDancer d in dancers)
                        onePosition.Add(position);
                    startPositions = onePosition;
                    break;
                case BalletPatternType.Orbit:
                    startPositions = orbitPositions;
                    break;
                case BalletPatternType.Atom:
                    startPositions = atomPositions;
                    break;
                case BalletPatternType.Manual:
                    startPositions = manualPositions;
                    break;
            }

            lastPatternType = patternType;
            isLerping = true;
		}

        // Look for the good target
        switch (patternType)
		{
            case BalletPatternType.Circle:
                targetPositions = cirlePositions;
                break;
            case BalletPatternType.Line:
                targetPositions = linePositions;
                break;
            case BalletPatternType.Point:
                List<Vector3> onePosition = new List<Vector3>();
                foreach (BalletDancer d in dancers)
                    onePosition.Add(position);

                targetPositions = onePosition;
                break;
            case BalletPatternType.Orbit:
                targetPositions = orbitPositions;
                break;
            case BalletPatternType.Atom:
                targetPositions = atomPositions;
                break;
            case BalletPatternType.Manual:
                targetPositions = manualPositions;
                break;
        }

        // Lerp amount
        if (lerpDuration == 0)
            lerpDuration = 0.01f;
        float t = (Time.time - lerpStartTime) / lerpDuration;
        t = lerpCurve.Evaluate(t);
        if(t >= 1f)
		{
            isLerping = false;
		}

        // Apply movement
        for (int i = 0; i < dancers.Count; i++)
		{
            if (dancers[i] != null && !float.IsNaN(targetPositions[i].x))
			{
                if(isLerping)
				{
                    dancers[i].transform.position = Vector3.Lerp(startPositions[i], targetPositions[i], t);
                }
                else
				{
                    dancers[i].transform.position = Vector3.Lerp(dancers[i].transform.position, targetPositions[i], 0.3f);
                }
            }
                
		}
    }

	public void ResetSpeed()
	{
        currentSpeed = 0;
	}

	public void ShowDancer(bool isVisible)
	{
        if (dancers == null)
            return;

        foreach(BalletDancer d in dancers)
		{
            d.isVisible = isVisible;
		}
	}

    public BalletDancer AddDancer()
	{
        BalletDancer dancer = Instantiate(dancerPrefab) as BalletDancer;

        dancer.SetColor(dancerColor);


        // Look for the next available ID
        int newID = 0;
        foreach (BalletDancer d in dancers)
        {
            if (newID == d.id)
            {
                newID++;
            }
            else
            {
                break;
            }
        }

        // Set everything correctly before send it to the universe.
        dancer.id = newID;
        dancer.name = "Dancer " + dancer.id;
        dancer.transform.parent = this.transform;

        dancers.Add(dancer);
        cirlePositions.Add(new Vector3());
        linePositions.Add(new Vector3());
        orbitPositions.Add(new Vector3());
        atomPositions.Add(new Vector3());

        if(manualPositions.Count < dancers.Count)
		{
            manualPositions.Add(new Vector3());
        }
        

        Debug.Log("Dancer " + dancer.id + " created and added to pattern" + id);

        // Update dancerCount
        dancerCount = dancers.Count;

        return dancer;
    }

    public bool RemoveDancer(int id = -1)
	{
        bool result = false;
        BalletDancer dancerToRemove = null;

        // If no dancer here, let's get out directly.
        if (dancers.Count < 1)
            return false;

        // Remove dancer from the list of this pattern.
        if (id == -1) // If id is -1, it means we want to remove last object
        {
            dancerToRemove = dancers[dancers.Count - 1];
            dancers.RemoveAt(dancers.Count - 1);
            result = true;
        }
        else // Look for the target in the list.
        {
            foreach (BalletDancer d in dancers)
            {
                if (d.id == id)
                {
                    dancerToRemove = d;
                }
            }

            // We found it so remove it from the list.
            if(dancerToRemove != null)
			{
                result = dancers.Remove(dancerToRemove);
            }
            else
			{
                Debug.LogError("Couldn't find Dancer " + id + " in pattern " + gameObject.name);
                return false;
			}
        }

        if (dancerToRemove != null)
        {
            Debug.Log("Dancer " + dancerToRemove.id + " removed from the list and destroyed from pattern " + gameObject.name);
            Destroy(dancerToRemove.gameObject);

            cirlePositions.RemoveAt(dancerCount - 1);
            linePositions.RemoveAt(dancerCount - 1);
            orbitPositions.RemoveAt(dancerCount - 1);
            atomPositions.RemoveAt(dancerCount - 1);
            manualPositions.RemoveAt(dancerCount - 1);

            dancerCount = dancers.Count;
        }

        return result;
	}

    void ComputeCircleMovement()
	{
        for (int i = 0; i < dancers.Count; i++)
        {
            if(size != 0)
			{
                cirlePositions[i] = new Vector3(Mathf.Sin(currentSpeed * 10 + i * Mathf.PI * 2f / dancers.Count + phase * Mathf.PI * 2) * currentSize / 2,
                                                Mathf.Cos(currentSpeed * 10 + i * Mathf.PI * 2f / dancers.Count + phase * Mathf.PI * 2) * currentSize / 2,
                                                0f);
                cirlePositions[i] = transform.TransformPoint(cirlePositions[i]);
            }
            else
			{
                cirlePositions[i] = Vector3.zero;

            }
            
        }
    }

    void ComputeLineMovement()
	{
        if (dancerCount == 0)
            return;

        if(dancerCount == 1)
		{
            linePositions[0] = Vector3.zero;
            linePositions[0] = transform.TransformPoint(linePositions[0]);
            return;
        }

        // Set points positions
        for (int i = 0; i < dancers.Count; i++)
        {
            linePositions[i] = new Vector3(0,
                                            Mathf.Cos(currentSpeed*5 + Mathf.PI * i / (dancerCount - 1) + 2 * Mathf.PI * phase) * currentSize / 2,
                                            0);


            linePositions[i] = transform.TransformPoint(linePositions[i]);
        }
    }

    void ComputeOrbitMovement()
	{
        if(orbitData == null || orbitData.Count != dancerCount)
		{
            GenerateOrbits();
		}

        for (int i = 0; i < dancers.Count; i++)
        {
            if (size != 0)
            {
                orbitPositions[i] = new Vector3(Mathf.Sin(currentSpeed * orbitData[i].speed * 10 + i * Mathf.PI * 2f / dancerCount + phase * Mathf.PI * 2) * orbitData[i].radius * currentSize / 2,
                                                Mathf.Cos(currentSpeed * orbitData[i].speed * 10 + i * Mathf.PI * 2f / dancerCount + phase * Mathf.PI * 2) * orbitData[i].radius * currentSize / 2,
                                                0f);
                orbitPositions[i] = transform.TransformPoint(orbitPositions[i]);
            }
            else
            {
                orbitPositions[i] = Vector3.zero;

            }

        }
    }

    public void GenerateOrbits()
	{
        orbitData = new List<OrbitData>();
       for(int i = 1; i < dancerCount + 1; i++)
		{
            OrbitData oD = new OrbitData();

            // Compute radius for this fellow
            float radius = (float)i / (float)dancerCount;
            radius = Mathf.Clamp(Random.Range(radius - 0.1f, radius + 0.1f), 0f, 1f);
            oD.radius = radius;

            // Compute speed for this fellow
            float orbitalSpeed = Random.Range(0.9f, 1.2f);
            oD.speed = orbitalSpeed;

            // Compute angle 
            float orbitAngle = Random.Range(-180,180);
            Quaternion orbitRotation = Quaternion.AngleAxis(orbitAngle, new Vector3(Random.Range(-1, 1), Random.Range(-1, 1), Random.Range(-1, 1)));
            oD.rotation = orbitRotation;

            orbitData.Add(oD);

        }
	}

    void ComputeAtomMovement()
	{
        if (orbitData == null || orbitData.Count != dancerCount)
        {
            GenerateOrbits();
        }

        for (int i = 0; i < dancerCount; i++)
        {
            if (size != 0)
            {
                atomPositions[i] = new Vector3(Mathf.Sin(currentSpeed * orbitData[i].speed * 10 + i * Mathf.PI * 2f / dancerCount + phase * Mathf.PI * 2) * currentSize / 2,
                                                Mathf.Cos(currentSpeed * orbitData[i].speed * 10 + i * Mathf.PI * 2f / dancerCount + phase * Mathf.PI * 2) * currentSize / 2,
                                                0f);

                // Rotate position with angle degree
                atomPositions[i] = orbitData[i].rotation * atomPositions[i];

                // Apply pattern transform 
                atomPositions[i] = transform.TransformPoint(atomPositions[i]);
            }
            else
            {
                atomPositions[i] = Vector3.zero;

            }

        }
    }

    public BalletPatternData StoreData()
	{

        BalletPatternData data = new BalletPatternData();
        data.id = id;
        data.dancerCount = dancerCount;
        data.patternType = (int)patternType;
        data.position = position;
        data.rotation = rotation; 
        data.size = size; 
        data.speed = speed;
        data.lerpDuration = lerpDuration;
        data.phase = phase;
        data.sizeLFOFrequency = sizeLFOFrequency;
        data.sizeLFOAmplitude = sizeLFOAmplitude;
        data.manualPositions = manualPositions;

        return data;
	}

    public void LoadData(BalletPatternData data)
	{
        id = data.id;
        dancerCount = data.dancerCount;
        patternType = (BalletPatternType) data.patternType;
        position = data.position;
        rotation = data.rotation;
        size = data.size;
        speed = data.speed;
        lerpDuration = data.lerpDuration;
        phase = data.phase;
        sizeLFOFrequency = data.sizeLFOFrequency;
        sizeLFOAmplitude = data.sizeLFOAmplitude;
        manualPositions = data.manualPositions;

        // Check manual position size before leaving.
        // In case of loading not correct data
        if(manualPositions.Count > dancerCount)
		{
            int itemToRemove = manualPositions.Count - dancerCount;
            for(int i = 0; i < itemToRemove; i++)
			{
                manualPositions.RemoveAt(manualPositions.Count - 1);
            }
		} 
        else if (manualPositions.Count < dancerCount)
        {
            int itemToadd = dancerCount - manualPositions.Count;
            for (int i = 0; i < itemToadd; i++)
            {
                manualPositions.Add(new Vector3());
            }
        }
    }

    public Vector3 GetPosition(int idPos)
	{
        Vector3 result = Vector3.zero; 

        if(idPos >= 0 && idPos < dancerCount)
		{
            result = dancers[idPos].transform.position;
		}

        return result;
	}

    public void UpdateDancerCount(int count)
	{
        if (count > dancers.Count)
        {
            int instanceCount = count - dancers.Count;
            for (int i = 0; i < instanceCount; i++)
            {
                AddDancer();
            }
        }
        else if(count < dancers.Count)
        {
            int removeCount = dancers.Count - count;
            for (int i = 0; i < removeCount; i++)
            {
                RemoveDancer();
            }
        }
    }

    private Color GenerateRandomColor()
    {
        return new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1f);
    }
}

#region Data
[System.Serializable]
public class OrbitData
{
    public float radius; // A percentage of radius between 0 and 1
    public float speed; // A percentage of speed between 0 and 1
    public Quaternion rotation; // Orbital rotation
}

[System.Serializable]
public class BalletPatternData
{
    public int id;
    public int dancerCount;
    public int patternType;
    public Vector3 position;
    public Vector3 rotation;
    public float size; 
    public float speed; 
    public float lerpDuration;
    public float phase;
    public float sizeLFOFrequency;
    public float sizeLFOAmplitude;
    public List<Vector3> manualPositions;
}
#endregion