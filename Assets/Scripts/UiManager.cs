using System.Collections.Generic; // Added for List support
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    public Image xpBar;
    public TextMeshProUGUI levelText;
    public Rigidbody boatRB;

    public RectTransform speedometerNeedle;
    public RectTransform compassWheel;

    private bool isWaitingForUpgrade = false;
    private float lastMaxXp;

    public RectTransform caughtFishHolder;
    public TextMeshProUGUI fishCaughtPrefab; // Use this as the prefab template

    // Tracks the current active text elements on screen
    private List<TextMeshProUGUI> activeFishTexts = new List<TextMeshProUGUI>();

    void Start()
    {
        lastMaxXp = GlobalStats.expTonNextLevel;
    }

    void Update()
    {
        updateXpBar();
        updateLevelDisplay();
        SpeedOmeter();
        compass();
    }

    void updateXpBar()
    {
        float currentXp = GlobalStats.Experince;
        float currentMaxXp = GlobalStats.expTonNextLevel;

        if (currentMaxXp > lastMaxXp || (currentXp < 5f && lastMaxXp > currentMaxXp))
        {
            isWaitingForUpgrade = true;
            lastMaxXp = currentMaxXp;
        }

        if (isWaitingForUpgrade && currentXp == 0)
        {
            isWaitingForUpgrade = false;
        }

        if (isWaitingForUpgrade)
        {
            xpBar.fillAmount = 1f;
        }
        else
        {
            float percentage = currentXp / currentMaxXp;
            xpBar.fillAmount = Mathf.Clamp01(percentage);
            lastMaxXp = currentMaxXp;
        }
    }

    void updateLevelDisplay()
    {
        levelText.text = GlobalStats.Level.ToString();
    }

    void SpeedOmeter()
    {
        float currentSpeed = boatRB.linearVelocity.magnitude;
        currentSpeed = Mathf.Clamp(currentSpeed, 0, GlobalStats.maxSpeed);

        float t = currentSpeed / GlobalStats.maxSpeed;
        float desiredAngle = Mathf.Lerp(112, -105, t);
        speedometerNeedle.localRotation = Quaternion.Euler(0, 0, desiredAngle);
    }

    void compass()
    {
        Vector3 forward = boatRB.transform.forward;
        forward.y = 0;

        float angle = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;
        compassWheel.localRotation = Quaternion.Euler(0, 0, angle);
    }

    public void updateFishCaught(string fishText)
    {
        TextMeshProUGUI newFishText = Instantiate(fishCaughtPrefab, caughtFishHolder);
        newFishText.text = fishText;

        activeFishTexts.Add(newFishText);

        if (activeFishTexts.Count > 5)
        {
            Destroy(activeFishTexts[0].gameObject);
            activeFishTexts.RemoveAt(0);
        }

        for (int i = 0; i < activeFishTexts.Count; i++)
        {
            float newYPosition = 40f - (20f * i);

            activeFishTexts[i].rectTransform.anchoredPosition = new Vector2(0, newYPosition);
        }
    }
}