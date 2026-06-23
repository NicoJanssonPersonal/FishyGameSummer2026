using TMPro;
using UnityEngine;

public class FishMinigame : MonoBehaviour
{
    [Header("UI Canvas Elements")]
    public GameObject uiPanel;
    public RectTransform greenZoneHolder;
    public GameObject greenZone;
    public GameObject fisheGameObject;
    private RectTransform fishe;
    public Rigidbody2D fishRB;
    private bool isUIOpen = false;

    [Header("Fish Stats")]

    private GameObject[] greenZoneObjects = new GameObject[4];
    private Vector2 initialFishPos;
    public RectTransform killZone;

    public TextMeshProUGUI timerText;
    private float timeRemaining = 5;
    bool isTimerRunning = false;

    void Start()
    {
        fishe = fisheGameObject.GetComponent<RectTransform>();
        fishRB = fisheGameObject.GetComponent<Rigidbody2D>();
        initialFishPos = fishe.anchoredPosition;
        //debugfiskpos();
        closeUI();
    }

    void debugfiskpos()
    {
        Debug.Log("World: " + fishe.position);
        Debug.Log("Local: " + fishe.localPosition);
        Debug.Log("Anchored: " + fishe.anchoredPosition);
    }
    void Update()
    {

        if (isTimerRunning)
        {
            HandleCountdown();
        }
        bool spacePressedOnce = false;
        bool fishInAnyZone = false;
        if (!isUIOpen)
            return;


        for (int i = 0; i < greenZoneObjects.Length; i++)
        {
            if (greenZoneObjects[i] == null)
                continue;

            RectTransform zoneRect =
                greenZoneObjects[i].GetComponent<RectTransform>();

            if (zoneRect != null && IsOverlapping(fishe, zoneRect))
            {
                fishInAnyZone = true;

                //Debug.Log($"Fish touching zone {i}");
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    spacePressedOnce = true;
                    if (i < greenZoneObjects.Length - 1)
                    {
                        MoveFishToZone(i + 1);
                    }
                    else
                    {
                        CatchFish();
                        spacePressedOnce = false;
                    }
                }
                break;
            }
        }
        if(IsOverlapping(fishe, killZone))
        {
            FishEscaped();
        }
        if (!fishInAnyZone && Input.GetKeyDown(KeyCode.Space))
        {
            FishEscaped();
        }
        if (spacePressedOnce)
        {
            GlobalStats.constantSpeed = (GlobalStats.constantSpeed * (GlobalStats.fishDifficulty)) - GlobalStats.fishingStrength;
            fishRB.linearVelocity = new Vector2(fishRB.linearVelocity.x, -GlobalStats.constantSpeed);
        }

    }

    void MoveFishToZone(int zoneIndex)
    {
        if (zoneIndex >= greenZoneObjects.Length)
        {
            CatchFish();
            return;
        }

        RectTransform zone =
            greenZoneObjects[zoneIndex].GetComponent<RectTransform>();

        Vector3 targetPos = zone.position;

        targetPos.y += Random.Range(-20f, 20f);

        fishe.position = targetPos;
    }

    bool IsOverlapping(RectTransform fish, RectTransform zone)
    {
        Vector3[] fishCorners = new Vector3[4];
        Vector3[] zoneCorners = new Vector3[4];

        fish.GetWorldCorners(fishCorners);
        zone.GetWorldCorners(zoneCorners);

        float fishWidth = fishCorners[2].x - fishCorners[0].x;
        float fishHeight = fishCorners[2].y - fishCorners[0].y;

        Rect fishHitbox = new Rect(
            fishCorners[0].x,
            fishCorners[2].y - fishHeight / 8f,
            fishWidth,
            fishHeight / 8f
        );

        Rect zoneRect = new Rect(
            zoneCorners[0].x,
            zoneCorners[0].y,
            zoneCorners[2].x - zoneCorners[0].x,
            zoneCorners[2].y - zoneCorners[0].y
        );

        return fishHitbox.Overlaps(zoneRect);
    }

    void CatchFish()
    {
        //Debug.Log("Fish caught");
        float roll = Random.Range(0f, 1f);
        if (GlobalStats.multiFishChance > roll)
        {
            GlobalStats.Experince = ((GlobalStats.Experince + (GlobalStats.fishDifficulty * 3)) * GlobalStats.multiFishAmount) * GlobalStats.xpGain;
            //Debug.Log("u caught " + GlobalStats.multiFishAmount + " fishes");
        }
        else
        {
            GlobalStats.Experince = (GlobalStats.Experince + (GlobalStats.fishDifficulty * 3)) * GlobalStats.xpGain;
            //Debug.Log("u caught one fish");
        }
        //Debug.Log("chance for multi fish " + GlobalStats.multiFishChance + " Roll:" + roll);

        closeUI();
    }

    void FishEscaped()
    {
        Debug.Log("Fish escaped");
        closeUI();
    }

    public void openUi(int fishDifficulty)
    {
        // called from fishinController
        closeUI();
        timeRemaining = 5;
        isTimerRunning = true;
        //GlobalStats.fishDifficulty = GlobalStats.fishDifficulty + 1;
        //Debug.Log(GlobalStats.fishDifficulty);
        isUIOpen = true;

        fishe.anchoredPosition = initialFishPos;

        uiPanel.SetActive(true);
        Debug.Log("fiskens svårighet " + fishDifficulty);
        generategreenZones(fishDifficulty, fishDifficulty);

        //debugfiskpos();
    }

    void closeUI()
    {
        isUIOpen = false;

        deletegreenZones();

        uiPanel.SetActive(false);
    }
    void generategreenZones(int difficulty, int greenZones)
    {
        greenZones = greenZones + 1;
        float yOffset = 235f/greenZones;

        greenZoneObjects = new GameObject[greenZones];

        float scaleShrinkFactor =
            0.075f * (difficulty * Random.Range(0.5f, 1.5f));

        for (int i = 0; i < greenZones; i++)
        {
            GameObject newGreenZone =
                Instantiate(greenZone, greenZoneHolder, false);

            float dynamicY =
                140.5f + (i * yOffset) + Random.Range(0f, 15f);

            newGreenZone.transform.localPosition =
                new Vector3(1127f, dynamicY, 0f);

            float dynamicScaleY =
                0.45f - (i * scaleShrinkFactor);

            if (dynamicScaleY < 0.05f)
                dynamicScaleY = 0.05f;

            newGreenZone.transform.localScale =
                new Vector3(1f, dynamicScaleY, 1f);

            greenZoneObjects[i] = newGreenZone;
        }
    }
    void deletegreenZones()
    {
        foreach (GameObject zone in greenZoneObjects)
        {
            if (zone != null)
            {
                Destroy(zone);
            }
        }

        greenZoneObjects = new GameObject[0];
    }
    private void HandleCountdown()
    {
        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            UpdateTimerUI();
        }
        else
        {
            FinishTimer();
        }
    }

    // 2. Handles updating the text element on the screen
    private void UpdateTimerUI()
    {
        float seconds = Mathf.CeilToInt(timeRemaining);
        timerText.text = seconds.ToString();
    }

    // 3. Handles what happens when the timer hits 0
    private void FinishTimer()
    {
        timeRemaining = 0;
        isTimerRunning = false;
        timerText.text = "Fail";
        
        TriggerTimerEvents();
    }

    // 4. A dedicated place to put your gameplay events (e.g., enable player movement)
    private void TriggerTimerEvents()
    {
        FishEscaped();
    }
}