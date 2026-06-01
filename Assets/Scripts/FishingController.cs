using UnityEngine;

public class FishingController1 : MonoBehaviour
{
    public MinigameManager minigameManager;
    private static int totalScore = 0;
    void Start()
    {
        if (minigameManager == null)
        {
            minigameManager = FindAnyObjectByType<MinigameManager>();
        }
    }
    void OnMouseDown()
    {
        startFisingMinigame();
        Destroy(gameObject);
    }

    void startFisingMinigame()
    {
        totalScore++;
        
        // Safety check to prevent crashing if it's STILL missing
        if (minigameManager != null)
        {
            minigameManager.openUi();
        }
        else
        {
            Debug.LogError("CRITICAL: There is no MinigameManager present in the scene!");
        }

        //Debug.Log("Plop Clicked! Current Score: " + totalScore);
    }
    

}
