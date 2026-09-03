using Unity.Mathematics;
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

    public GameObject bobber;
    [SerializeField][ColorUsage(true, true)] private Color ChaoticColor;
    [SerializeField][ColorUsage(true, true)] private Color LegendaryColor;
    [SerializeField][ColorUsage(true, true)] private Color RareColor;
    [SerializeField][ColorUsage(true, true)] private Color UncommonColor;
    [SerializeField][ColorUsage(true, true)] private Color CommonColor;

    public FishingLine fishingLine;
    public GameObject activeBobber;
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
                activeBobber = Instantiate(bobber, transform.position, Quaternion.identity);
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

        float luckBonus = (GlobalStats.fishRarity * 0.5f) / 100f;
        float normalizedRoll = Mathf.Clamp01(UnityEngine.Random.Range(0f, 1f) + luckBonus);

        float curveExponent = 3.0f;
        float curvedRoll = Mathf.Pow(normalizedRoll, curveExponent);

        int difficultyBoost = Mathf.RoundToInt(1f + (curvedRoll * 9f));

        Color targetColor = CommonColor;
        if (normalizedRoll >= 0.995f) targetColor = ChaoticColor;
        else if (normalizedRoll >= 0.975f) targetColor = LegendaryColor;
        else if (normalizedRoll >= 0.850f) targetColor = RareColor;
        else if (normalizedRoll >= 0.500f) targetColor = UncommonColor;

        tubeMaterial.SetColor("_EmissionColor", targetColor);

        return difficultyBoost;
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