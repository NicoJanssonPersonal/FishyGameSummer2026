using UnityEngine;

public class FishingController1 : MonoBehaviour
{
    public FishMinigame fishMinigame;
    private static int totalScore = 0;
    public GameObject boat;
    private int fishDiff;
    private Renderer tubeRenderer;
    private Material tubeMaterial;

    [SerializeField][ColorUsage(true, true)] private Color ChaoticColor;
    [SerializeField][ColorUsage(true, true)] private Color LegendaryColor;
    [SerializeField][ColorUsage(true, true)] private Color RareColor;
    [SerializeField][ColorUsage(true, true)] private Color UncommonColor;
    [SerializeField][ColorUsage(true, true)] private Color CommonColor;

    void Start()
    {
        Transform tubeChild = transform.Find("tube");

        if (tubeChild != null)
        {
            tubeRenderer = tubeChild.GetComponent<Renderer>();

            if (tubeRenderer != null)
            {
                
                tubeMaterial = tubeRenderer.material;
                tubeMaterial.EnableKeyword("_EMISSION");

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
            float DistanceFromBoatToFish = Vector3.Distance(boat.transform.position, transform.position);
            //Debug.Log(DistanceFromBoatToFish);
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
        if (tubeMaterial == null) return GlobalStats.fishDifficulty;

        float roll = Random.Range(0f, 100f);

        if (GlobalStats.fishRarity > roll)
        {
            int fishDiff;
            if (GlobalStats.fishRarity > 90)
            {
                fishDiff = GlobalStats.fishDifficulty + 4;
                tubeMaterial.SetColor("_EmissionColor", ChaoticColor);
                return fishDiff;
            }
            if (GlobalStats.fishRarity > 75)
            {
                fishDiff = GlobalStats.fishDifficulty + 3;
                // FIXED: Now correctly changes emission color
                tubeMaterial.SetColor("_EmissionColor", LegendaryColor);
                return fishDiff;
            }
            if (GlobalStats.fishRarity > 50)
            {
                fishDiff = GlobalStats.fishDifficulty + 2;
                // FIXED: Now correctly changes emission color
                tubeMaterial.SetColor("_EmissionColor", RareColor);
                return fishDiff;
            }
            if (GlobalStats.fishRarity > 25)
            {
                fishDiff = GlobalStats.fishDifficulty + 1;
                tubeMaterial.SetColor("_EmissionColor", UncommonColor);
                return fishDiff;
            }
        }

        tubeMaterial.SetColor("_EmissionColor", CommonColor);
        return GlobalStats.fishDifficulty;
    }

    private void OnDestroy()
    {
        if (tubeMaterial != null)
        {
            Destroy(tubeMaterial);
        }
    }
    //kommentera bort när tonarn e nöjd
    private void OnValidate()
    {
        if (Application.isPlaying && tubeMaterial != null)
        {
            fishDifficultyBasedOfRarity();
        }
    }

}