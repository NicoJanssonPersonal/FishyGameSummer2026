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

        // Roll a random value between 0 and 100
        float roll = Random.Range(0f, 100f);

        // Check if the rolled number falls within the player's fishRarity stat boost
        // The higher GlobalStats.fishRarity is, the higher the chance to get rare tiers
        float effectiveRoll = roll + (GlobalStats.fishRarity * 0.5f); // Example scaling factor

        if (effectiveRoll > 95f)
        {
            tubeMaterial.SetColor("_EmissionColor", ChaoticColor);
            return GlobalStats.fishDifficulty + 4;
        }
        if (effectiveRoll > 80f)
        {
            tubeMaterial.SetColor("_EmissionColor", LegendaryColor);
            return GlobalStats.fishDifficulty + 3;
        }
        if (effectiveRoll > 60f)
        {
            tubeMaterial.SetColor("_EmissionColor", RareColor);
            return GlobalStats.fishDifficulty + 2;
        }
        if (effectiveRoll > 35f)
        {
            tubeMaterial.SetColor("_EmissionColor", UncommonColor);
            return GlobalStats.fishDifficulty + 1;
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

    private void OnValidate()
    {
        if (Application.isPlaying && tubeMaterial != null)
        {
            fishDiff = CalculateFishDifficultyAndColor();
        }
    }
}