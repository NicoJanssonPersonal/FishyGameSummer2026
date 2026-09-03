using UnityEngine;

public class bobber : MonoBehaviour
{
    public FishingLine fishingLine;
    public GameObject splashEffect;
    public FishMinigame fishMinigame;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        if (splashEffect != null)
        {
            Instantiate(splashEffect, transform.position, Quaternion.identity);
        }
        if (fishingLine == null)
        {
            fishingLine = FindAnyObjectByType<FishingLine>();
        }
        if (fishMinigame == null)
        {
            fishMinigame = FindAnyObjectByType<FishMinigame>();
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
        
        if (fishingLine.fishingLineLength() > 15)
        {
            fishMinigame.FishEscaped();
        }
    }
    void OnDestroy()
    {
        fishingLine.ClearLine();
    }
}
