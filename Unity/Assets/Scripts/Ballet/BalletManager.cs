using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalletManager : MonoBehaviour
{
    public bool showDancers; // Show debug object to visualize easier
    public DataManager dataMngr; // Load stored data
    public BalletPattern patternPrefab;
    public List<BalletPattern> orbsPatterns; // a list of orbs patterns 
    public List<BalletPattern> forcePatterns; // A list of forse patterns
    public enum PatternGroup { Orb, Force }

    private OxipitalData data;

    // Start is called before the first frame update
    void Start()
    {
        // Initialize lists
        if(orbsPatterns == null)
            orbsPatterns = new List<BalletPattern>();
        if (forcePatterns == null)
            forcePatterns = new List<BalletPattern>();

        data = dataMngr.LoadData();

        showDancers = (PlayerPrefs.GetInt("ShowDancer") != 0);
    }

    // Update is called once per frame
    void Update()
    {
        foreach(BalletPattern pattern in orbsPatterns)
		{
            pattern.UpdateMovement();
            pattern.ApplyMovement();
            pattern.ShowDancer(showDancers);
		}

        foreach (BalletPattern pattern in forcePatterns)
        {
            pattern.UpdateMovement();
            pattern.ApplyMovement();
            pattern.ShowDancer(showDancers);
        }
    }

	private void OnDestroy()
	{
        PlayerPrefs.SetInt("ShowDancer", showDancers ? 1 : 0);
	}

	public BalletPattern AddPattern(PatternGroup group = PatternGroup.Orb)
	{
        BalletPattern pattern = Instantiate(patternPrefab) as BalletPattern;
        int newID = 0;
        string patternName = "";
        List<BalletPattern> groupList = GetGroupList(group);   

        // Look for the next available ID
        foreach (BalletPattern p in groupList)
        {
            if (newID == p.id)
            {
                newID++;
            }
            else
            {
                break;
            }
        }

        patternName = "["+group.ToString()+"] Pattern " + newID;

        if (pattern != null)
		{
            // Set everything correctly before send it to the universe.
            pattern.id = newID;
            pattern.name = patternName;
            pattern.transform.parent = this.transform;

            // Check if we have available data for this guy.
            if (data == null)
                data = dataMngr.LoadData();

            if(group == PatternGroup.Orb)
			{
                if(data.balletMngrData != null && data.balletMngrData.orbsPatternData != null)
				{
                    foreach (BalletPatternData patternData in data.balletMngrData.orbsPatternData)
                    {
                        if (patternData.id == newID)
                        {
                            pattern.LoadData(patternData);
                        }
                    }
                }
            }
            else if(group == PatternGroup.Force)
			{
                if(data.balletMngrData != null && data.balletMngrData.forcePatternData != null)
				{
                    foreach (BalletPatternData patternData in data.balletMngrData.forcePatternData)
                    {
                        if (patternData.id == newID)
                        {
                            pattern.LoadData(patternData);
                        }
                    }
                }
            }

            pattern.Init(this);

            // Add to main list
            groupList.Add(pattern);
        }

        return pattern;
    }

    public bool RemovePattern(PatternGroup group, int id=-1)
	{
        bool result = false;
        BalletPattern patternToRemove = null;
        List<BalletPattern> groupList = GetGroupList(group);

        // If no patterns here, let's get out directly.
        if (groupList.Count == 0)
            return false;

        // If id is -1, it means we want to remove last object
        if (id == -1)
        {
            patternToRemove = groupList[groupList.Count - 1];
            groupList.RemoveAt(groupList.Count - 1);
            result = true;
        }
        else // Let's find the right guy to kill
        {
            foreach (BalletPattern p in groupList)
            {
                if (p.id == id)
                {
                    patternToRemove = p;
                    result = groupList.Remove(p);
                    break;
                }
            }
        }

        if (patternToRemove != null)
        {
            Debug.Log("Pattern " + patternToRemove.id + " removed from the list " + group.ToString() + " and destroyed.");
            Destroy(patternToRemove.gameObject);
        }

        return result;
    }

    public BalletPattern GetPattern(PatternGroup group, int id)
	{
        BalletPattern p = null;
        List<BalletPattern> groupList = GetGroupList(group);

        if (id < 0 || id >= groupList.Count)
		{
            Debug.LogError("Trying to get pattern out of bound in "+ group.ToString() +".");
            return p;
		}

        foreach(BalletPattern pa in groupList)
		{
            if (pa.id == id)
                p = pa;
		}

        if(p == null)
		{
            Debug.LogError("Could find Pattern " + id + " of group " + groupList.ToString());
		}

        return p;
	}

    public void SynchronizePattern()
	{
        foreach(BalletPattern bp in orbsPatterns)
		{
            bp.ResetSpeed();
		}

        foreach (BalletPattern bp in forcePatterns)
        {
            bp.ResetSpeed();
        }
    }

    private List<BalletPattern> GetGroupList(PatternGroup g)
	{
        List<BalletPattern> result = null;

        // Define where to add this baby : is it a force or an orbs ?
        switch (g)
        {
            case PatternGroup.Orb:
                result = orbsPatterns;
                break;
            case PatternGroup.Force:
                result = forcePatterns;
                break;
            default:
                Debug.LogError("Cannot find the requested pattern group.");
                break;
        }

        return result;
    }
}

[System.Serializable]
public class BalletManagerData
{
    public List<BalletPatternData> orbsPatternData = new List<BalletPatternData>();
    public List<BalletPatternData> forcePatternData = new List<BalletPatternData>();
}