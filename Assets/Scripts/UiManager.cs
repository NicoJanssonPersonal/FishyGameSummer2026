using System.Collections;
using System.Collections.Generic; // Added for List support
//using System.Numerics;
using TMPro;
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
    private List<TextMeshProUGUI> activeFishTexts = new List<TextMeshProUGUI>();

    public GameObject coinSpawnPoint;
    [Header("XP Bar")]
    public Image xpBarGreen; // The main bar that fills smoothly
    public Image xpBarWhite; // The background bar that snaps instantly
    public float fillLerpSpeed = 10f; // How fast the green catches up to the white

    public Image hpBarWhite;
    public Image hpBarRed;

    public Image NitroBar;
    private Vector2 orignalPosFishHolder;
    [Header("Fish Holder Animation")]
    public float slideDuration = 0.4f; // Time in seconds for the slide animation
    private Coroutine slideCoroutine;
    private bool isHidden = false; // Tracks if the box is currently off-screen


    void Start()
    {
        lastMaxXp = GlobalStats.expTonNextLevel;
        lastLevel = GlobalStats.Level;
        orignalPosFishHolder = caughtFishHolder.anchoredPosition;
    }

    void Update()
    {
        updateXpBar();
        updateHpBar();
        updateLevelDisplay();
        SpeedOmeter();
        compass();
        updateNitroBar();
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
    void updateHpBar()
    {
        float currentHp = GlobalStats.currentHealth;
        float MaxHp = GlobalStats.maxHealth;

        float targetPercentage = Mathf.Clamp01(currentHp / MaxHp);

        hpBarRed.fillAmount = targetPercentage;

        hpBarWhite.fillAmount = Mathf.Lerp(hpBarWhite.fillAmount, targetPercentage, Time.deltaTime * fillLerpSpeed);

        if (Mathf.Abs(hpBarWhite.fillAmount - targetPercentage) < 0.001f)
        {
            hpBarWhite.fillAmount = targetPercentage;
        }
    }
    void updateNitroBar()
    {
        float targetPercentage = Mathf.Clamp01(GlobalStats.currentNitro / GlobalStats.maxNitro);
        NitroBar.fillAmount = targetPercentage * 0.5f;
    }

    void updateLevelDisplay()
    {
        levelText.text = GlobalStats.Level.ToString();
    }

    void SpeedOmeter()
    {
        float currentSpeed = (boatRB != null) ? boatRB.linearVelocity.magnitude : 0f;
        if (float.IsNaN(currentSpeed) || float.IsInfinity(currentSpeed))
        {
            currentSpeed = 0f;
        }

        float maxSpeed = GlobalStats.maxSpeed;

        float t = 0f;
        if (maxSpeed > 0f)
        {
            currentSpeed = Mathf.Clamp(currentSpeed, 0, maxSpeed);
            t = currentSpeed / maxSpeed;
        }

        float desiredAngle = Mathf.Lerp(112f, -105f, t);

        if (!float.IsNaN(desiredAngle) && !float.IsInfinity(desiredAngle))
        {
            speedometerNeedle.localRotation = Quaternion.Euler(0, 0, desiredAngle);
        }
        else
        {
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

    public void updateFishCaught(int moneyFromFish, int xpFromFish, int fishdiff)
    {
        string fishName;

        switch (fishdiff)
        {
            case 1:
                fishName = "MINNOW";
                break;
            case 2:
                fishName = "SOGGY BOOT";
                break;
            case 3:
                fishName = "SILVER CARP";
                break;
            case 4:
                fishName = "GHOST SQUID";
                break;
            case 5:
                fishName = "EMILBERT";
                break;
            case 6:
                fishName = "NEON ANGLER";
                break;
            case 7:
                fishName = "CRYSTAL SALMON";
                break;
            case 8:
                fishName = "MAGMA TUNA";
                break;
            case 9:
                fishName = "ABYSSAL KRAKEN";
                break;
            case 10:
                fishName = "CELESTIAL LEVIATHAN";
                break;
            default:
                fishName = "MYSTERY FISH";
                break;
        }

        TextMeshProUGUI newFishText = Instantiate(fishCaughtPrefab, caughtFishHolder);
        newFishText.text = $"YOU CAUGHT A {fishName}, {moneyFromFish} GOLD {xpFromFish} XP";

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
        ShowFishCaught(moneyFromFish);
    }
    public RectTransform slideBoxButton;

    public void ShowFishCaught(int coinAmount)
    {
        // här kommer fisken som visas före den ska exploderas
        // updatera money counter till smooth steps
        // adda ljud ti minigamet DONE
        explodedFishToCoin(coinAmount);
    }
    public void explodedFishToCoin(int coinAmount)
    {
        Vector2 screenCenterPixels = new Vector2(Screen.width / 2f, Screen.height / 2f);

        for (int i = 0; i < Mathf.Min(coinAmount, 1000); i++)
        {
            RectTransform rt = Instantiate(peng, coinSpawnPoint.transform);
            rt.anchoredPosition = new Vector2(
                UnityEngine.Random.Range(-40f, 40f),
                40f
            );

            StartCoroutine(FallCoin(rt));
        }
    }
    public RectTransform coinTargetLocation;
    IEnumerator FallCoin(RectTransform coin)
    {
        if (coin == null || coinTargetLocation == null) yield break;

        Vector3 startPos = coin.position;

        Vector3 burstOffset = new Vector3(
            UnityEngine.Random.Range(-100f, 100f),
            UnityEngine.Random.Range(-50f, 120f),
            0f
        );
        Vector3 popPos = startPos + burstOffset;

        Vector3 controlPoint = (popPos + coinTargetLocation.position) * 0.5f
                             + new Vector3(UnityEngine.Random.Range(-200f, 200f), UnityEngine.Random.Range(100f, 300f), 0f);

        float duration = UnityEngine.Random.Range(0.6f, 0.9f);
        float elapsed = 0f;

        float rotationSpeed = UnityEngine.Random.Range(-500f, 500f);
        Vector3 initialScale = coin.localScale;

        while (elapsed < duration)
        {
            if (coin == null) yield break;

            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            Vector3 currentPos = Mathf.Pow(1 - t, 2) * popPos +
                                 2 * (1 - t) * t * controlPoint +
                                 Mathf.Pow(t, 2) * coinTargetLocation.position;

            coin.position = currentPos;

            coin.Rotate(0, 0, rotationSpeed * Time.deltaTime);

            coin.localScale = Vector3.Lerp(initialScale, initialScale * 0.4f, t);

            yield return null;
        }

        if (coin != null)
        {
            coin.position = coinTargetLocation.position;
            Destroy(coin.gameObject);
        }
    }
    public void slideInBox()
    {
        if (slideCoroutine != null)
        {
            StopCoroutine(slideCoroutine);
        }

        Vector2 targetPos = isHidden ? orignalPosFishHolder : new Vector2(-310f, orignalPosFishHolder.y);

        Vector3 targetRot = isHidden ? new Vector3(0f, 0f, 180f) : Vector3.zero;

        isHidden = !isHidden;

        slideCoroutine = StartCoroutine(AnimateBoxSlide(targetPos, targetRot, slideDuration));
    }

    private IEnumerator AnimateBoxSlide(Vector2 targetPosition, Vector3 targetRotation, float duration)
    {
        Vector2 startPosition = caughtFishHolder.anchoredPosition;
        Quaternion startRotation = slideBoxButton.localRotation;
        Quaternion endRotation = Quaternion.Euler(targetRotation);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float percent = Mathf.Clamp01(elapsed / duration);

            float smoothPercent = Mathf.SmoothStep(0f, 1f, percent);

            caughtFishHolder.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, smoothPercent);

            slideBoxButton.localRotation = Quaternion.Lerp(startRotation, endRotation, smoothPercent);

            yield return null;
        }

        caughtFishHolder.anchoredPosition = targetPosition;
        slideBoxButton.localRotation = endRotation;

        slideCoroutine = null;
    }
}