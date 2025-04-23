using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class StoneFragmentManager : MonoBehaviour
{
    [Header("Stone Fragments")]
    [SerializeField] private List<StoneFragment> stoneFragments = new List<StoneFragment>();
    
    [Header("Events")]
    public UnityEvent onAllFragmentsCollected;
    
    private int collectedFragments = 0;
    
    void Start()
    {
        if (stoneFragments.Count == 0)
        {
            FindAllStoneFragments();
        }
    }
    
    public void FindAllStoneFragments()
    {
        StoneFragment[] fragments = FindObjectsOfType<StoneFragment>();
        stoneFragments.Clear();
        stoneFragments.AddRange(fragments);
        
        foreach (StoneFragment fragment in stoneFragments)
        {
            fragment.onCollect.AddListener(OnFragmentCollected);
        }
    }
    
    public void OnFragmentCollected()
    {
        collectedFragments++;
        
        if (AllFragmentsCollected())
        {
            onAllFragmentsCollected.Invoke();
        }
    }
    
    public bool AllFragmentsCollected()
    {
        return collectedFragments >= stoneFragments.Count && stoneFragments.Count > 0;
    }
    
    public int GetCollectedCount()
    {
        return collectedFragments;
    }
    
    public int GetTotalCount()
    {
        return stoneFragments.Count;
    }
} 