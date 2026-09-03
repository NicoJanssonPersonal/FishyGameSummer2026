using UnityEngine;

public class bobber : MonoBehaviour
{   
    public FishingLine fishingLine;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (fishingLine == null)
        {
            fishingLine = FindAnyObjectByType<FishingLine>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (FishMinigame.isUIOpen)
        {
          fishingLine.DrawLine(gameObject);  
        }
        else
        {
            Destroy(gameObject);
        }
        
    }
    void OnDestroy()
    {
        fishingLine.ClearLine();
    }
}
