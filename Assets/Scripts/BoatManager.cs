using UnityEngine;

public class BoatManager : MonoBehaviour
{
    public GameObject fishradius;
    private float fishrangeRatio = 1.4f;

    void Start()
    {
        
    }

    void Update()
    {
        updateFishingRange();
    }
    void updateFishingRange()
    {
        float fishRange = GlobalStats.fishingRange * fishrangeRatio;
        fishradius.transform.localScale = new Vector3(fishRange, 1, fishRange);
    }
}
