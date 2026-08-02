using System.Collections;
using TMPro;
using UnityEngine;

public class FishMinigame : MonoBehaviour
{
    [Header("UI Canvas Elements")]
    public GameObject uiPanel;
    public RectTransform greenZoneHolder;
    public GameObject greenZone;
    public GameObject fisheGameObject;
    public GameObject[] fishesGameObjects;
    private RectTransform fishe;
    public Rigidbody2D fishRB;
    public static bool isUIOpen = false;

    [Header("Fish Stats")]
    private GameObject[] greenZoneObjects = new GameObject[0];
    private Vector3[] zoneBasePositions = new Vector3[0];
    private Vector2 initialFishPos;
    public RectTransform killZone;

    public RectTransform baitHitBox;
    public UiManager uiManagerScript;

    [Header("Zone Movement Settings")]
    public float zoneMoveSpeed = 1.5f;       // Speed of moving zones
    public float zoneMoveAmplitude = 15f;    // Pixels moved up/down
    private int currentDifficulty = 1;
    private int thisFishDifficulty;

    void Start()
    {
        fishe = fisheGameObject.GetComponent<RectTransform>();
        fishRB = fisheGameObject.GetComponent<Rigidbody2D>();
        initialFishPos = fishe.anchoredPosition;
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

        if (!isUIOpen)
            return;

        // Animate middle green zones at difficulty 5+
        if (currentDifficulty >= 5)
        {
            AnimateZones(currentDifficulty);
        }

        bool spacePressedOnce = false;
        bool fishInAnyZone = false;

        for (int i = 0; i < greenZoneObjects.Length; i++)
        {
            if (greenZoneObjects[i] == null)
                continue;

            RectTransform zoneRect = greenZoneObjects[i].GetComponent<RectTransform>();

            if (zoneRect != null && IsOverlapping(fishe, zoneRect))
            {
                fishInAnyZone = true;

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

        if (IsOverlapping(baitHitBox, killZone))
        {
            FishEscaped();
        }

        if (!fishInAnyZone && Input.GetKeyDown(KeyCode.Space))
        {
            FishEscaped();
        }

        if (spacePressedOnce)
        {
            fishRB.linearVelocity = new Vector2(fishRB.linearVelocity.x, -GlobalStats.constantSpeed);
        }
    }

    void AnimateZones(int level)
    {
        float levelMultiplier = level / 5;
        int zoneCount = greenZoneObjects.Length;
        if (zoneCount <= 2) return;

        for (int i = 1; i < zoneCount - 1; i++)
        {
            if (greenZoneObjects[i] == null) continue;

            float phaseOffset = i * 1.5f;
            float yOffset = Mathf.Sin(Time.time * zoneMoveSpeed * levelMultiplier + phaseOffset) * zoneMoveAmplitude;

            Vector3 newPos = zoneBasePositions[i];
            newPos.y += yOffset;

            greenZoneObjects[i].transform.localPosition = newPos;
        }
    }

    void MoveFishToZone(int zoneIndex)
    {
        if (zoneIndex >= greenZoneObjects.Length)
        {
            CatchFish();
            return;
        }

        RectTransform zone = greenZoneObjects[zoneIndex].GetComponent<RectTransform>();
        Vector3 targetPos = zone.position;
        targetPos.y += Random.Range(5f, 30f);
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
            fishCorners[2].y - fishHeight,
            fishWidth,
            fishHeight
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
        float xpFromFish = thisFishDifficulty * 3 * GlobalStats.xpGain;
        GlobalStats.Experince += xpFromFish;

        float moneyFromFish = thisFishDifficulty* 2 * GlobalStats.moneyGain;
        GlobalStats.money += Mathf.RoundToInt(moneyFromFish);

        uiManagerScript.updateFishCaught(Mathf.RoundToInt(moneyFromFish), Mathf.RoundToInt(xpFromFish));
        StartCoroutine(delay());
    }

    IEnumerator delay()
    {
        fishRB.linearVelocity = Vector3.zero;
        yield return new WaitForSeconds(0.5f);
        closeUI();
    }

    void FishEscaped()
    {
        Debug.Log("Fish escaped");
        closeUI();
    }

    public void openUi(int fishDifficulty)
    {
        thisFishDifficulty = fishDifficulty;
        closeUI();
        isUIOpen = true;

        foreach (var fish in fishesGameObjects)
        {
            fish.SetActive(false);
        }

        //GameObject activeFish = fishesGameObjects[fishDifficulty - 1]; ta tibaka när det finns 10 olika sprites
        GameObject activeFish = fishesGameObjects[0];
        activeFish.SetActive(true);
        fishe.anchoredPosition = initialFishPos;

        uiPanel.SetActive(true);
        generategreenZones(fishDifficulty, fishDifficulty);
    }

    void closeUI()
    {
        isUIOpen = false;
        deletegreenZones();
        uiPanel.SetActive(false);

        foreach (var fish in fishesGameObjects)
        {
            fish.SetActive(false);
        }
    }

    void generategreenZones(int difficulty, int greenZones)
    {
        currentDifficulty = difficulty;
        int zoneCount = greenZones + 1;

        greenZoneObjects = new GameObject[zoneCount];
        zoneBasePositions = new Vector3[zoneCount];

        float minY = 140.5f;
        float maxY = 435.0f;
        float totalDistance = maxY - minY;

        float scaleShrinkFactor = 0.075f * (difficulty * Random.Range(0.5f, 1.2f));

        for (int i = 0; i < zoneCount; i++)
        {
            float dynamicScaleY = 0.45f - (i * scaleShrinkFactor);
            if (dynamicScaleY < 0.05f)
                dynamicScaleY = 0.05f;

            GameObject newGreenZone = Instantiate(greenZone, greenZoneHolder, false);
            newGreenZone.transform.localScale = new Vector3(1f, dynamicScaleY, 1f);

            float t = (zoneCount > 1) ? (float)i / (zoneCount - 1) : 0f;
            float targetY = Mathf.Lerp(minY, maxY, t);

            if (i > 0 && i < zoneCount - 1)
            {
                float maxJitter = (totalDistance / (zoneCount - 1)) * 0.15f;
                targetY += Random.Range(-maxJitter, maxJitter);
            }

            Vector3 position = new Vector3(1127f, targetY, 0f);
            newGreenZone.transform.localPosition = position;

            greenZoneObjects[i] = newGreenZone;
            zoneBasePositions[i] = position;
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
        zoneBasePositions = new Vector3[0];
    }

}