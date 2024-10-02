using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceManager : MonoBehaviour
{
    public List<StandardForceController> Forces
    {
        get
		{
            if (_forces == null)
                UpdateForceList();
            return _forces;
		}
    }

    private List<StandardForceController> _forces;
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
        foreach (StandardForceController force in _forces)
		{
            /*if(data.forceControllerData != null)
			{
                foreach (ForceControllerData forceData in data.forceControllerData)
                {
                    if (force.Key == forceData.key)
                    {
                        force.LoadData(forceData);
                        break;
                    }
                }
            }*/

            force.Initiliaze(_orbsMngr, _balletMngr);
		}
	}

	// Update is called once per frame
	void UpdateForceList()
    {
        _forces = new List<StandardForceController>();

        StandardForceController[] forcesChildren = GetComponentsInChildren<StandardForceController>();
        foreach (StandardForceController f in forcesChildren)
        {
            _forces.Add(f);
        }
    }
}
