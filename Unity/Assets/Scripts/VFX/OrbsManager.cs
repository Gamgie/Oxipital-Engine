using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.VFX;
using System;

public class OrbsManager : MonoBehaviour
{
    public List<OrbGroup> orbs;

    [Header("Orbs Positions")]
    public int orbGroupCount;
    public OrbGroup orbGroupPrefab;
    public Transform emitterOrbRoot;
    public DataManager dataManager;
    public BalletManager balletMngr;

    private UnityEvent OnOrbCreated;

	void OnEnable()
	{
        OnOrbCreated = new UnityEvent();
        Application.targetFrameRate = 60;
    }

	// Start is called before the first frame update
	void Start()
    {
        if(orbs != null)
        {
            foreach(OrbGroup o in orbs)
            {
                Destroy(o.gameObject);
            }
        }

        InitOrbs();
    }

    // Update is called once per frame
    void Update()
    {

        if (orbGroupCount < 0)
            return;

        if(orbGroupCount > orbs.Count)
        {
            AddOrbGroup();
        }
        else if(orbGroupCount < orbs.Count)
        {
            DestroyOrbGroup(orbs.Count - 1);
        }
    }


    private void OnDestroy()
    {
        // Save how many orbs were there
        dataManager.SaveData();
    }

    public void KillAllParticles()
    {
        if (orbs == null)
            return;

        foreach (OrbGroup o in orbs)
        {
            o.Reinit();
        }
    }

    private void InitOrbs()
	{
        orbs = new List<OrbGroup>();
        OxipitalData loadedData = dataManager.LoadData();
        orbGroupCount = loadedData.orbCount;

        for (int i = 0; i < orbGroupCount; i++)
        {
            if (orbGroupCount > orbs.Count)
            {
                AddOrbGroup(loadedData.orbGroupData[i]);
            }
        }
    }
    public OrbGroup AddOrbGroup(OrbGroupData orbData = null)
    {
        if (orbs == null)
        {
            Debug.LogError("Try to add an orb in a null list");
            return null;
        }

        OrbGroup o = Instantiate(orbGroupPrefab);
        if(orbData == null)
		{
            o.gameObject.name = "OrbGroup" + orbs.Count;
            o.orbGroupId = orbs.Count;
        }
        else
		{
            o.data = orbData;
            o.LoadData();
		}
        
        o.transform.parent = emitterOrbRoot;
        o.Initialize(this);
        orbs.Add(o);

        return o;
    }

	public BalletPattern GetPattern(int patternID)
	{
        return balletMngr.GetPattern(BalletManager.PatternGroup.Orb,patternID);
	}

	public void DestroyOrbGroup(int index)
    {
        if (orbs == null)
        {
            Debug.LogError("Try to destroy an orb in a null list");
            return;
        }

        OrbGroup orbToBeDestroyed = orbs[index];
        Destroy(orbToBeDestroyed.gameObject);
        orbs.RemoveAt(index);
    }

    public UnityEvent GetOnOrbCreated()
	{
        if(OnOrbCreated == null)
		{
            OnOrbCreated = new UnityEvent();
        }

        return OnOrbCreated;
	}
}