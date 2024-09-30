using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceManager : MonoBehaviour
{
    public List<ForceController> Forces
    {
        get
		{
            if (_forces == null)
                UpdateForceList();
            return _forces;
		}
    }

    private List<ForceController> _forces;
    private OrbsManager _orbsMngr;
    private BalletManager _balletMngr;
    private DataManager _dataMngr;


    // Start is called before the first frame update
    void OnEnable()
    {
        UpdateForceList();
    }

	private void Start()
	{
        // Get Managers reference
        _orbsMngr = GameObject.FindGameObjectWithTag("Orb Manager").GetComponent<OrbsManager>();
        _balletMngr = GameObject.FindGameObjectWithTag("Ballet Manager").GetComponent<BalletManager>();
        _dataMngr = GameObject.FindGameObjectWithTag("Data Manager").GetComponent<DataManager>();

        OxipitalData data = _dataMngr.LoadData();

        // Initialize each force and load its datas
        foreach (ForceController force in _forces)
		{
            if(data.forceControllerData != null)
			{
                foreach (ForceControllerData forceData in data.forceControllerData)
                {
                    if (force.Key == forceData.key)
                    {
                        force.LoadData(forceData);
                        break;
                    }
                }
            }

            force.Initiliaze(_orbsMngr, _balletMngr);
		}
	}

	// Update is called once per frame
	void UpdateForceList()
    {
        _forces = new List<ForceController>();

        ForceController[] forcesChildren = GetComponentsInChildren<ForceController>();
        foreach (ForceController f in forcesChildren)
        {
            _forces.Add(f);
        }
    }
}
