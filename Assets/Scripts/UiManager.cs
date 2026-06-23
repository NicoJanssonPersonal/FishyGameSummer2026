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
    // Keeps track of the max XP from the previous frame
    private float lastMaxXp;

    void Start()
    {
        // Initialize it so it doesn't start at 0
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

        // 1. DETECT THE LEVEL UP
        // If the max XP requirement jumped, or if XP suddenly dropped drastically
        if (currentMaxXp > lastMaxXp || (currentXp < 5f && lastMaxXp > currentMaxXp))
        {
            isWaitingForUpgrade = true;
            lastMaxXp = currentMaxXp; // Update our tracker
        }

        // 2. DETECT WHEN THE UPGRADE IS CHOSEN (RESET)
        // If we were waiting for an upgrade, and XP is now perfectly 0, the menu closed!
        if (isWaitingForUpgrade && currentXp == 0)
        {
            isWaitingForUpgrade = false;
        }

        // 3. APPLY THE VISUALS
        if (isWaitingForUpgrade)
        {
            // Lock the bar at 100% while the player is picking their upgrade
            xpBar.fillAmount = 1f;
        }
        else
        {
            // Normal state: Calculate the percentage perfectly
            float percentage = currentXp / currentMaxXp;
            xpBar.fillAmount = Mathf.Clamp01(percentage);

            // Keep our tracker updated during normal gameplay
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
        float desiredAngle = Mathf.Lerp(112, -105, t);// 112min -105max
        speedometerNeedle.localRotation = Quaternion.Euler(0, 0, desiredAngle);
    }
    void compass()
    {
        Vector3 forward = boatRB.transform.forward;
        forward.y = 0;

        float angle = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;
        compassWheel.localRotation = Quaternion.Euler(0, 0, angle);
    }
}