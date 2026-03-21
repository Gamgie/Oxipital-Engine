using UnityEngine;

public class SoledadManager : MonoBehaviour
{
    public bool showBogota;
	public GameObject bogotaGameobject;

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (showBogota)
        {
            bogotaGameobject.SetActive(true);
		}
		else
        {
            bogotaGameobject.SetActive(false);
		}

	}
}
