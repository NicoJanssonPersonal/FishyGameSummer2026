using UnityEngine;

public class FishingController1 : MonoBehaviour
{

    private static int totalScore = 0;

    void OnMouseDown()
    {
        startFisingMinigame();
        Destroy(gameObject);
    }

    void startFisingMinigame()
    {
        totalScore++;

        Debug.Log("Plop Clicked! Current Score: " + totalScore);
    }
}
