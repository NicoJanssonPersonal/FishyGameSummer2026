using UnityEngine;

public class FishingController1 : MonoBehaviour
{
    public FishMinigame fishMinigame;
    private static int totalScore = 0;
    public GameObject boat;
    void Start()
    {
        if (fishMinigame == null)
        {
            fishMinigame = FindAnyObjectByType<FishMinigame>();
        }
        if (boat == null)
        {
            boat = GameObject.Find("BoatHolder");
        }
    }
    void OnMouseDown()
    {
        startFisingMinigame();  
    }

    void startFisingMinigame()
    {
        totalScore++;
        
        if (fishMinigame != null)
        {
            //fishMinigame.openUi();
            //Debug.Log("boat pos " + boat.transform.position + " Plop location " + transform.position);
            float DistanceFromBoatToFish = Vector3.Distance(boat.transform.position, transform.position);
            Debug.Log(DistanceFromBoatToFish);
            if(DistanceFromBoatToFish <= GlobalStats.fishingRange)
            {
                Destroy(gameObject);
                fishMinigame.openUi();  
            }
            else
            {
                Debug.Log("get closer to the plop");
                // Behövs feedback åt spelaren att dom är för långt borta
            }
        }
        else
        {
            Debug.LogError("CRITICAL: There is no MinigameManager present in the scene!");
        }

    }
    

}
