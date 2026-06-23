using UnityEngine;

public class FishingController1 : MonoBehaviour
{
    public FishMinigame fishMinigame;
    private static int totalScore = 0;
    public GameObject boat;
    private int fishDiff;
    private Renderer tubeRenderer;
    void Start()
    {
        Transform tubeChild = transform.Find("tube");

        if (tubeChild != null)
        {
            tubeRenderer = tubeChild.GetComponent<Renderer>();

            if (tubeRenderer != null)
            {
                fishDiff = fishDifficultyBasedOfRarity();
            }
        }

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
            if (DistanceFromBoatToFish <= GlobalStats.fishingRange)
            {
                Destroy(gameObject);
                fishMinigame.openUi(fishDiff);
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
    int fishDifficultyBasedOfRarity()
    {
        float roll = Random.Range(0f, 100f);
        if (GlobalStats.fishRarity > roll)
        {
            int fishDiff;
            if (GlobalStats.fishRarity > 90)
            {
                fishDiff = GlobalStats.fishDifficulty + Random.Range(5, 10);
                tubeRenderer.material.color = Color.red;
                return fishDiff;
            }
            if (GlobalStats.fishRarity > 75)
            {
                fishDiff = GlobalStats.fishDifficulty + Random.Range(4, 5);
                tubeRenderer.material.color = Color.yellow;
                return fishDiff;
            }
            if (GlobalStats.fishRarity > 50)
            {
                fishDiff = GlobalStats.fishDifficulty + Random.Range(3, 4);
                tubeRenderer.material.color = Color.pink;
                return fishDiff;
            }
            if (GlobalStats.fishRarity > 25)
            {
                fishDiff = GlobalStats.fishDifficulty + Random.Range(2, 3);
                tubeRenderer.material.color = Color.green;
                return fishDiff;
            }
            if (GlobalStats.fishRarity > 10)
            {
                fishDiff = GlobalStats.fishDifficulty + Random.Range(1, 2);
                tubeRenderer.material.color = Color.cyan;
                return fishDiff;
            }

        }
        return GlobalStats.fishDifficulty;
    }
}

