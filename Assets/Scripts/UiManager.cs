using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    public Image xpBar;
    public TextMeshProUGUI levelText;

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
}