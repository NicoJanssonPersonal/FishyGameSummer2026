using UnityEngine;

public class FishingController1 : MonoBehaviour
{
    public FishMinigame fishMinigame;
    public Animator animator;
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
                fishDiff = CalculateFishDifficultyAndColor();
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
    void Awake()
    {
        GameObject playerObj = GameObject.FindWithTag("fiskarDude");

        if (playerObj != null)
        {
            // Search child objects for a specific child named "ModelAnimator"
            animator = playerObj.GetComponent<Animator>();
        }
    }
    void OnMouseDown()
    {
        if (!CardManager.isUpgrading)
        {
            StartFishingMinigame();
            if (animator != null) animator.SetTrigger("cast");
        }
    }

    void StartFishingMinigame()
    {
        totalScore++;

        if (fishMinigame != null)
        {
            float distanceToBoat = Vector3.Distance(boat.transform.position, transform.position);

            if (distanceToBoat <= GlobalStats.fishingRange)
            {
                Destroy(gameObject);
                fishMinigame.openUi(fishDiff);
            }
            else
            {
                Debug.Log("Get closer to the fishing spot!");
                // TODO: Add UI feedback for out of range
            }
        }
        else
        {
            Debug.LogError("CRITICAL: There is no FishMinigame present in the scene!");
        }
    }

    int CalculateFishDifficultyAndColor()
    {
        if (tubeMaterial == null) return GlobalStats.fishDifficulty;

        float luckBonus = GlobalStats.fishRarity * 0.5f;
        float roll = Mathf.Clamp(Random.Range(0f, 100f) + luckBonus, 0f, 100f);
        // add crazy special legendary fishes if in specific area
        int difficultyBoost = 0;
        Color targetColor = CommonColor;

        // EXACT BASE PERCENTAGES (when fishRarity = 0):
        if (roll >= 99.5f)        // 0.5% Chance (Rolls 99.5 to 100.0)
        {
            difficultyBoost = 10;
            targetColor = ChaoticColor;
        }
        else if (roll >= 97.5f)   // 2.0% Chance (Rolls 97.5 to 99.49)
        {
            difficultyBoost = 7;
            targetColor = LegendaryColor;
        }
        else if (roll >= 85.0f)   // 12.5% Chance (Rolls 85.0 to 97.49)
        {
            difficultyBoost = 4;
            targetColor = RareColor;
        }
        else if (roll >= 50.0f)   // 35.0% Chance (Rolls 50.0 to 84.99)
        {
            difficultyBoost = 2;
            targetColor = UncommonColor;
        }
        else                      // 50.0% Chance (Rolls 0.0 to 49.99)
        {
            difficultyBoost = 0;
            targetColor = CommonColor;
        }

        tubeMaterial.SetColor("_EmissionColor", targetColor);
        return GlobalStats.fishDifficulty + difficultyBoost;
    }

    private void OnDestroy()
    {
        if (tubeMaterial != null)
        {
            Destroy(tubeMaterial);
        }
    }

    private void OnValidate()
    {
        if (Application.isPlaying && tubeMaterial != null)
        {
            fishDiff = CalculateFishDifficultyAndColor();
        }
    }
}