using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using OSCQuery;

public class DancerGroup : MonoBehaviour
{
    [Header("Pattern Settings")]
    [Range(1, 10)]
    public float count = 1;

    public GameObject dancerPrefab;
    public List<Dancer> dancers;

    [Header("Patterns")]
    [Range(0, 10)]
    public float patternSize = 1; // Size of this pattern

    [Range(0, 1)]
    public float patternSizeSpread = 0;
    [Range(0, 1)]
    public float patternAxisSpread = 0;



    [Header("Animation")]
    [Range(-5, 5)]
    public float patternSpeed = 1f; // speed of the choreography
    [Range(0, 1)]
    public float patternSpeedRandom = 0;

    //[DoNotExpose]
    [HideInInspector]
    public float patternTime = 0;

    [Range(0, 1)]
    public float patternTimeOffset = 0; // offset phase


    [Range(0, 10)]
    public float patternSizeLFOFrequency;
    [Range(0, 10)]
    public float patternSizeLFOAmplitude;

    [SerializeField]
    public List<DancePattern> patterns;

    private void OnEnable()
    {
        init();
    }

    void Start()
    {
    }

    void init()
    {
        dancers = GetComponentsInChildren<Dancer>().ToList();
        patterns = new List<DancePattern>();
        patterns.Add(new LinePattern());
        patterns.Add(new CirclePattern());
    }

    void Update()
    {

        //if (dancers.Count == 0) dancers = GetComponentsInChildren<Dancer>().ToList(); //Resync here if needed

        while (Mathf.Ceil(count) < dancers.Count) removeLastDancer();
        while (Mathf.Ceil(count) > dancers.Count) addDancer();

        patternTime += Time.deltaTime * patternSpeed;

        for (int i = 0; i < dancers.Count; i++)
        {
            dancers[i].weight = i <= count - 1 ? 1 : getCountRelativeProgression();
            dancers[i].transform.localPosition = Vector3.zero;
            dancers[i].localPatternTime += Time.deltaTime * (patternSpeed * Mathf.Lerp(1, .5f+dancers[i].randomFactor, patternSpeedRandom));
        }


        foreach (var p in patterns)
        {
            p.updatePattern(this);
        }
    }


    //Dancer Management
    void addDancer()
    {
        GameObject dancer = Instantiate(dancerPrefab, transform);
        dancers.Add(dancer.GetComponent<Dancer>());

    }

    void removeLastDancer()
    {
        if (dancers[dancers.Count - 1] != null) dancers[dancers.Count - 1].kill(getKillTime());
        dancers.RemoveAt(dancers.Count - 1);
    }



    //Virtual to override by child classes
    protected virtual float getKillTime() { return 1; }

    //Helpers
    public int getFullDancersCount() { return Mathf.FloorToInt(count); }
    public float getCountRelativeProgression() { return count - getFullDancersCount(); }
}
