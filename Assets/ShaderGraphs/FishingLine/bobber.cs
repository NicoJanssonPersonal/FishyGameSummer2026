using UnityEngine;

public class bobber : MonoBehaviour
{
    public FishingLine fishingLine;
    public GameObject splashEffect;
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
