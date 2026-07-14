using System;
using System.Collections;
using System.Collections.Generic; // Added for List support
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    public TextMeshProUGUI levelText;
    public Rigidbody boatRB;

    public RectTransform speedometerNeedle;
    public RectTransform compassWheel;

    private bool isWaitingForUpgrade = false;
    private float lastMaxXp;
    private int lastLevel;

    public RectTransform caughtFishHolder;
    public TextMeshProUGUI fishCaughtPrefab; // Use this as the prefab template
    public RectTransform peng;
    // Tracks the current active text elements on screen
    private List<TextMeshProUGUI> activeFishTexts = new List<TextMeshProUGUI>();

    public GameObject coinSpawnPoint;
    [Header("XP Bar")]
    public Image xpBarGreen; // The main bar that fills smoothly
    public Image xpBarWhite; // The background bar that snaps instantly
    public float fillLerpSpeed = 10f; // How fast the green catches up to the white
    void Start()
    {
        lastMaxXp = GlobalStats.expTonNextLevel;
        lastLevel = GlobalStats.Level; // Track the starting level
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

        if (GlobalStats.Level > lastLevel)
        {
            xpBarWhite.fillAmount = 1f;
            xpBarGreen.fillAmount = Mathf.Lerp(xpBarGreen.fillAmount, 1f, Time.deltaTime * fillLerpSpeed);

            if (1f - xpBarGreen.fillAmount < 0.1f)
            {
                xpBarGreen.fillAmount = 0f;
                xpBarWhite.fillAmount = 0f;
                lastLevel = GlobalStats.Level;
            }
        }
        else
        {
            float targetPercentage = Mathf.Clamp01(currentXp / currentMaxXp);

            xpBarWhite.fillAmount = targetPercentage;

            xpBarGreen.fillAmount = Mathf.Lerp(xpBarGreen.fillAmount, targetPercentage, Time.deltaTime * fillLerpSpeed);

            if (Mathf.Abs(xpBarGreen.fillAmount - targetPercentage) < 0.001f)
            {
                xpBarGreen.fillAmount = targetPercentage;
            }
        }
    }

    void updateLevelDisplay()
    {
        levelText.text = GlobalStats.Level.ToString();
    }

    void SpeedOmeter()
    {
        // 1. Get the boat's speed (fallback to 0 if the rigidbody is missing or speed is NaN)
        float currentSpeed = (boatRB != null) ? boatRB.linearVelocity.magnitude : 0f;
        if (float.IsNaN(currentSpeed) || float.IsInfinity(currentSpeed))
        {
            currentSpeed = 0f;
        }

        // 2. Fetch the max speed safely
        float maxSpeed = GlobalStats.maxSpeed;

        // 3. Prevent division by zero if maxSpeed is 0 or negative
        float t = 0f;
        if (maxSpeed > 0f)
        {
            currentSpeed = Mathf.Clamp(currentSpeed, 0, maxSpeed);
            t = currentSpeed / maxSpeed;
        }

        // 4. Interpolate the angle
        float desiredAngle = Mathf.Lerp(112f, -105f, t);

        // 5. Final safety check: only assign the rotation if it is a valid number
        if (!float.IsNaN(desiredAngle) && !float.IsInfinity(desiredAngle))
        {
            speedometerNeedle.localRotation = Quaternion.Euler(0, 0, desiredAngle);
        }
        else
        {
            // Fallback to the default starting angle (112 degrees) if something goes wrong
            speedometerNeedle.localRotation = Quaternion.Euler(0, 0, 112f);
        }
    }

    void compass()
    {
        Vector3 forward = boatRB.transform.forward;
        forward.y = 0;

        float angle = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;
        compassWheel.localRotation = Quaternion.Euler(0, 0, angle);
    }

    public void updateFishCaught(int moneyFromFish, int xpFromFish)
    {
        TextMeshProUGUI newFishText = Instantiate(fishCaughtPrefab, caughtFishHolder);
        newFishText.text = "YOU CAUGHT A EMILBERT, " + moneyFromFish + " GOLD " + xpFromFish + " XP";

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
        coinsFallInChest(moneyFromFish);

    }
    void coinsFallInChest(int coinAmount)
    {
        for (int i = 0; i < coinAmount; i++)
        {
            RectTransform rt = Instantiate(peng, coinSpawnPoint.transform);
            rt.anchoredPosition = new Vector2(
                UnityEngine.Random.Range(-40f, 40f),
                40f
            );

            StartCoroutine(FallCoin(rt));
        }
    }
    IEnumerator FallCoin(RectTransform coin)
    {
        float speed = UnityEngine.Random.Range(200f, 350f);      // Different fall speed
        float drift = UnityEngine.Random.Range(-30f, 30f);       // Horizontal drift
        float rotation = UnityEngine.Random.Range(-180f, 180f);  // Spin

        while (coin != null && coin.anchoredPosition.y > -650f)
        {
            coin.anchoredPosition += new Vector2(
                drift * Time.deltaTime,
                -speed * Time.deltaTime
            );

            coin.Rotate(0, 0, rotation * Time.deltaTime);

            yield return null;
        }

        if (coin != null)
            Destroy(coin.gameObject);
    }
}