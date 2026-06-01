using UnityEngine;

public class MinigameManager : MonoBehaviour
{
    [Header("UI Canvas Elements")]
    public GameObject uiPanel;
    public RectTransform greenZoneHolder;
    public GameObject greenZone;
    public GameObject fishe;

    [Header("Physics State")]
    // This is the boolean you want!
    bool isFishInGreenZone = false;
    public Rigidbody2D fisheRB;

    [Header("fiskarns STATS")]
    float constantSpeed = 50f;
    private bool isUIOpen = false;
    private minigameTriggerForwarder fishForwarder;
    private RectTransform[] greenZoneRects = new RectTransform[4];
    private Vector3 initalFishPos;
    void Start()
    {
        closeUI();
        initalFishPos = fishe.transform.position;
    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isUIOpen) closeUI();
            //else openUi();
        }
        if (!isUIOpen) return;

        RectTransform fishRect = fishe.GetComponent<RectTransform>();
        if (fishRect == null) return;

        float fishY = fishRect.anchoredPosition.y;

        if (greenZoneRects[0] != null)
        {
            float greenZoneBottom = greenZoneRects[0].anchoredPosition.y - (greenZoneRects[0].rect.height / 2f);

            if (fishY <= greenZoneBottom)
            {
                fishEscaped();
                closeUI();
                return;
            }
        }
        if (isUIOpen && Input.GetKeyDown(KeyCode.Space) && !isFishInGreenZone)
        {
            fishEscaped();
        }

        if (isUIOpen && isFishInGreenZone)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                bool spacePressedOnce = true;
                fishRect = fishe.GetComponent<RectTransform>();
                if (fishRect != null)
                {
                    fishY = fishRect.anchoredPosition.y;
                    float detectionRadius = 25f;
                    bool teleported = false;

                    for (int i = 0; i < greenZoneRects.Length - 1; i++)
                    {
                        float currentZoneY = greenZoneRects[i].anchoredPosition.y;

                        if (Mathf.Abs(fishY - currentZoneY) <= detectionRadius)
                        {
                            float nextZoneY = greenZoneRects[i + 1].anchoredPosition.y;
                            fishRect.anchoredPosition = new Vector2(fishRect.anchoredPosition.x, nextZoneY + Random.Range(5, 15));

                            if (fisheRB != null) fisheRB.linearVelocity = Vector2.zero;

                            teleported = true;
                            break;
                        }


                    }

                    if (!teleported)
                    {
                        int lastIndex = greenZoneRects.Length - 1;
                        float lastZoneY = greenZoneRects[lastIndex].anchoredPosition.y;

                        if (Mathf.Abs(fishY - lastZoneY) <= detectionRadius)
                        {
                            CatchFish();
                            spacePressedOnce = false;
                        }
                    }
                }
                if (spacePressedOnce)
                {
                    fisheRB.linearVelocity = new Vector2(fisheRB.linearVelocity.x, -constantSpeed);
                }

            }
        }
    }
    void CatchFish()
    {
        Debug.Log("fish caught");
        closeUI();
    }
    void fishEscaped()
    {
        Debug.Log("fisharn escpaed");
        closeUI();
    }
    public void openUi()
    {
        closeUI();
        isUIOpen = true;
        //greenZoneRects.Clear();new Vector3(366.5f, 126.5f, 0)
        fishe.transform.position = initalFishPos;
        uiPanel.SetActive(true);
        generategreenZones(1);
    }

    void closeUI()
    {
        isUIOpen = false;
        isFishInGreenZone = false;
        uiPanel.SetActive(false);
    }

    void generategreenZones(int difficulty)
    {
        float yOffset = 65f;
        float scaleShrinkFactor = 0.075f * difficulty;

        for (int i = 0; i < 4; i++)
        {
            GameObject newgreenZone = Instantiate(greenZone, greenZoneHolder, false);

            minigameTriggerForwarder zoneForwarder = newgreenZone.AddComponent<minigameTriggerForwarder>();
            zoneForwarder.OnForwardTriggerChanged += HandleTriggerStateChanged;

            RectTransform rect = newgreenZone.GetComponent<RectTransform>();
            if (rect != null)
            {
                float dynamicY = 140.5f + (i * yOffset);
                rect.anchoredPosition3D = new Vector3(1127f, dynamicY, 0f);
                float dynamicScaleY = 0.45f - (i * scaleShrinkFactor);
                if (dynamicScaleY < 0.05f) dynamicScaleY = 0.05f;
                rect.localScale = new Vector3(1f, dynamicScaleY, 1f);

                greenZoneRects[i] = rect;
            }
        }
    }

    private void OnEnable()
    {
        if (fishe != null)
        {
            fishForwarder = fishe.GetComponent<minigameTriggerForwarder>();
            if (fishForwarder == null) fishForwarder = fishe.AddComponent<minigameTriggerForwarder>();

            fishForwarder.OnForwardTriggerChanged += HandleTriggerStateChanged;
        }
    }

    private void OnDisable()
    {
        if (fishForwarder != null) fishForwarder.OnForwardTriggerChanged -= HandleTriggerStateChanged;
    }

    private void HandleTriggerStateChanged(GameObject reportedBy, Collider2D triggeredWith, bool isEntering)
    {
        if ((reportedBy == fishe && triggeredWith.gameObject.name.Contains(greenZone.name)) ||
            (reportedBy.name.Contains(greenZone.name) && triggeredWith.gameObject == fishe))
        {
            isFishInGreenZone = isEntering;

            if (isFishInGreenZone)
            {
                //Debug.Log("<color=green>Fish Entered Green Zone!</color> bool is now TRUE.");
            }
            else
            {
                //Debug.Log("<color=red>Fish Left Green Zone!</color> bool is now FALSE.");
            }
        }
    }
}