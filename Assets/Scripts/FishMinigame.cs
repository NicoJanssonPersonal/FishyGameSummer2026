using UnityEngine;

public class FishMinigame : MonoBehaviour
{
    [Header("UI Canvas Elements")]
    public GameObject uiPanel;
    public RectTransform greenZoneHolder;
    public GameObject greenZone;
    public GameObject fisheGameObject;
    private RectTransform fishe;

    [Header("Game State")]
    private bool isFishInGreenZone = false;
    private bool isUIOpen = false;

    [Header("Fish Stats")]
    public float constantSpeed = GlobalStats.constantSpeed;
    public int fishDifficulty = GlobalStats.fishDifficulty;

    private GameObject[] greenZoneObjects = new GameObject[4];
    private Vector2 initialFishPos;

    void Start()
    {
        fishe = fisheGameObject.GetComponent<RectTransform>();
        fishDifficulty = GlobalStats.fishDifficulty;
        constantSpeed = GlobalStats.constantSpeed;
        initialFishPos = fishe.anchoredPosition;
        debugfiskpos();
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

        isFishInGreenZone = false;

        for (int i = 0; i < greenZoneObjects.Length; i++)
        {
            if (greenZoneObjects[i] == null)
                continue;

            RectTransform zoneRect =
                greenZoneObjects[i].GetComponent<RectTransform>();

            if (zoneRect != null && IsOverlapping(fishe, zoneRect))
            {
                isFishInGreenZone = true;

                Debug.Log($"Fish touching zone {i}");

                // Stop checking once one overlap is found
                break;
            }
        }
        // FIXA så att den teleporta uppåt när man trycker på space
    }

    bool IsOverlapping(RectTransform a, RectTransform b)
    {
        Vector3[] cornersA = new Vector3[4];
        Vector3[] cornersB = new Vector3[4];

        a.GetWorldCorners(cornersA);
        b.GetWorldCorners(cornersB);

        Rect rectA = new Rect(
            cornersA[0].x,
            cornersA[0].y,
            cornersA[2].x - cornersA[0].x,
            cornersA[2].y - cornersA[0].y
        );

        Rect rectB = new Rect(
            cornersB[0].x,
            cornersB[0].y,
            cornersB[2].x - cornersB[0].x,
            cornersB[2].y - cornersB[0].y
        );
        // FISK RECT/COLLIDERN ÄR HELA FISKEN INTE BARA TARGETEN
        return rectA.Overlaps(rectB);
    }

    void CatchFish()
    {
        Debug.Log("Fish caught");
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

        isUIOpen = true;

        fishe.anchoredPosition = initialFishPos;

        uiPanel.SetActive(true);

        generategreenZones(fishDifficulty, 4);
        debugfiskpos();
    }

    void closeUI()
    {
        isUIOpen = false;
        isFishInGreenZone = false;

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