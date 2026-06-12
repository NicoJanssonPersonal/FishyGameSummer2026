using UnityEngine;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    public Image xpBar;
    
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
    }

    void updateXpBar()
    {
        float currentXp = GlobalStats.Experince;
        float currentMaxXp = GlobalStats.expTonNextLevel;

        // IF the max XP suddenly jumped up, a level up happened this frame
        if (currentMaxXp > lastMaxXp)
        {
            // Calculate percentage using the OLD max cap so it hits 100%+
            float levelUpPercentage = currentXp / lastMaxXp;
            xpBar.fillAmount = Mathf.Clamp01(levelUpPercentage);

            // Update our tracker to the new max XP for the next frame
            lastMaxXp = currentMaxXp;
        }
        else
        {
            // Normal frame: calculate normally
            float percentage = currentXp / currentMaxXp;
            xpBar.fillAmount = Mathf.Clamp01(percentage);

            // Keep tracking the max XP
            lastMaxXp = currentMaxXp;
        }
    }
}