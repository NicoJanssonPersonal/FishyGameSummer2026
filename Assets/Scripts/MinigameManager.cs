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

    private bool isUIOpen = false;
    private minigameTriggerForwarder fishForwarder;

    void Start()
    {
        closeUI();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isUIOpen) closeUI();
            else openUi();
        }

        if (isUIOpen && isFishInGreenZone)
        {
            // Do your fishing progress bar logic here! 
            // e.g., progress += Time.deltaTime;
            if (Input.GetKeyDown(KeyCode.Space))
            {
                fisheRB.AddForceY(1000f);
            }
        }
    }

    void openUi()
    {
        isUIOpen = true;
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
        float scaleShrinkFactor = 0.15f;

        for (int i = 0; i < 4; i++)
        {
            GameObject newgreenZone = Instantiate(greenZone, greenZoneHolder, false);

            minigameTriggerForwarder zoneForwarder = newgreenZone.AddComponent<minigameTriggerForwarder>();
            zoneForwarder.OnForwardTriggerChanged += HandleTriggerStateChanged;

            RectTransform rect = newgreenZone.GetComponent<RectTransform>();
            if (rect != null)
            {
                float dynamicY = 146.7f + (i * yOffset);
                rect.anchoredPosition3D = new Vector3(1127f, dynamicY, 0f);
                float dynamicScaleY = 0.45f - (i * scaleShrinkFactor);
                if (dynamicScaleY < 0.05f) dynamicScaleY = 0.05f;
                rect.localScale = new Vector3(1f, dynamicScaleY, 1f);
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
                Debug.Log("<color=green>Fish Entered Green Zone!</color> bool is now TRUE.");
            }
            else
            {
                Debug.Log("<color=red>Fish Left Green Zone!</color> bool is now FALSE.");
            }
        }
    }
}