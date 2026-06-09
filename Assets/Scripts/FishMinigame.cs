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
        if (!fishInAnyZone && Input.GetKeyDown(KeyCode.Space))
        {
            FishEscaped();
        }
        if (spacePressedOnce)
        {
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
        Debug.Log("Fish caught");
        GlobalStats.Experince = (GlobalStats.Experince + (GlobalStats.fishDifficulty * 3)) * GlobalStats.multiFishAmount;
        closeUI();
    }

    void FishEscaped()
    {
        Debug.Log("Fish escaped");
        closeUI();
    }

    public void openUi()
    {
        // called from fishinController
        closeUI();
        //GlobalStats.fishDifficulty = GlobalStats.fishDifficulty + 1;
        //Debug.Log(GlobalStats.fishDifficulty);
        isUIOpen = true;

        fishe.anchoredPosition = initialFishPos;

        uiPanel.SetActive(true);

        generategreenZones(GlobalStats.fishDifficulty, 4);
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
        float yOffset = 65f;

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
}