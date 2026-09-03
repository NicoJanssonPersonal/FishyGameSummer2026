using System.Collections;
using System.Collections.Generic;
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
    public TextMeshProUGUI fishCaughtPrefab;
    public RectTransform peng;
    private List<TextMeshProUGUI> activeFishTexts = new List<TextMeshProUGUI>();

    public GameObject coinSpawnPoint;

    [Header("XP Bar")]
    public Image xpBarGreen;
    public Image xpBarWhite;
    public float fillLerpSpeed = 10f;

    public Image hpBarWhite;
    public Image hpBarRed;

    public Image NitroBar;
    private Vector2 orignalPosFishHolder;

    [Header("Fish Holder Animation")]
    public float slideDuration = 0.4f;
    private Coroutine slideCoroutine;
    private bool isHidden = false;

    [Header("Juiced Fish & Coin Effects")]
    public RectTransform slideBoxButton;
    public GameObject[] fishSprites;
    public RectTransform uiCanvas;
    public RectTransform coinTargetLocation;

    public AudioManager audioManager;

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
            case 1: fishName = "MINNOW"; break;
            case 2: fishName = "SOGGY BOOT"; break;
            case 3: fishName = "SILVER CARP"; break;
            case 4: fishName = "GHOST SQUID"; break;
            case 5: fishName = "EMILBERT"; break;
            case 6: fishName = "NEON ANGLER"; break;
            case 7: fishName = "CRYSTAL SALMON"; break;
            case 8: fishName = "MAGMA TUNA"; break;
            case 9: fishName = "ABYSSAL KRAKEN"; break;
            case 10: fishName = "CELESTIAL LEVIATHAN"; break;
            default: fishName = "MYSTERY FISH"; break;
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

        ShowFishCaught(moneyFromFish, fishdiff);
    }

    public void ShowFishCaught(int coinAmount, int fishdiff)
    {
        int fishIndex = Mathf.Clamp(fishdiff - 1, 0, fishSprites.Length - 1);

        if (fishSprites.Length > 0 && fishSprites[fishIndex] != null && uiCanvas != null)
        {
            GameObject spawnedFish = Instantiate(fishSprites[fishIndex]);

            spawnedFish.transform.SetParent(uiCanvas, false);
            spawnedFish.transform.SetAsLastSibling();

            RectTransform fishRect = spawnedFish.GetComponent<RectTransform>();
            if (fishRect != null)
            {
                fishRect.anchorMin = new Vector2(0.5f, 0.5f);
                fishRect.anchorMax = new Vector2(0.5f, 0.5f);
                fishRect.pivot = new Vector2(0.5f, 0.5f);
                fishRect.anchoredPosition = Vector2.zero;
            }

            StartCoroutine(AnimateFishSequence(spawnedFish, coinAmount));
        }
        else
        {
            Debug.LogError("ShowFishCaught Error: fishSprites is empty or uiCanvas is not assigned!");
            explodedFishToCoin(coinAmount, Vector3.zero);
        }
    }

    private IEnumerator AnimateFishSequence(GameObject spawnedFish, int coinAmount)
    {
        Vector3 baseScale = spawnedFish.transform.localScale * 100f;
        spawnedFish.transform.localScale = Vector3.zero;

        float popDuration = 0.25f;
        float elapsed = 0f;

        while (elapsed < popDuration)
        {
            if (spawnedFish == null) yield break;
            elapsed += Time.deltaTime;
            float t = elapsed / popDuration;

            float elasticT = Mathf.Sin(t * Mathf.PI * 0.5f) + Mathf.Sin(t * Mathf.PI) * 0.35f;
            spawnedFish.transform.localScale = Vector3.LerpUnclamped(Vector3.zero, baseScale, elasticT);

            yield return null;
        }

        spawnedFish.transform.localScale = baseScale;

        yield return new WaitForSeconds(0.25f);

        elapsed = 0f;
        float swellTime = 0.12f;
        while (elapsed < swellTime)
        {
            if (spawnedFish == null) yield break;
            elapsed += Time.deltaTime;
            spawnedFish.transform.localScale = Vector3.Lerp(baseScale, baseScale * 1.35f, elapsed / swellTime);
            yield return null;
        }

        Vector3 explosionOrigin = spawnedFish.transform.position;
        Destroy(spawnedFish);

        explodedFishToCoin(coinAmount, explosionOrigin);
    }

    public void explodedFishToCoin(int coinAmount)
    {
        Vector3 defaultPos = coinSpawnPoint != null ? coinSpawnPoint.transform.position : Vector3.zero;
        explodedFishToCoin(coinAmount, defaultPos);
    }

    public void explodedFishToCoin(int coinAmount, Vector3 spawnPosition)
    {
        int visualCoins = Mathf.Clamp(coinAmount, 1, 50);

        for (int i = 0; i < visualCoins; i++)
        {
            if (peng != null && uiCanvas != null)
            {
                RectTransform rt = Instantiate(peng, uiCanvas);
                rt.position = spawnPosition;
                rt.localScale = Vector3.one;
                rt.transform.SetAsLastSibling();

                StartCoroutine(FallCoin(rt));
            }
        }
    }

    IEnumerator FallCoin(RectTransform coin)
    {
        if (coin == null || coinTargetLocation == null) yield break;

        Vector3 startPos = coin.position;

        Vector3 burstOffset = new Vector3(
            UnityEngine.Random.Range(-140f, 140f),
            UnityEngine.Random.Range(-70f, 160f),
            0f
        );
        Vector3 popPos = startPos + burstOffset;

        Vector3 controlPoint = (popPos + coinTargetLocation.position) * 0.5f
                             + new Vector3(UnityEngine.Random.Range(-250f, 250f), UnityEngine.Random.Range(100f, 350f), 0f);

        float duration = UnityEngine.Random.Range(0.55f, 0.85f);
        float elapsed = 0f;

        float rotationSpeed = UnityEngine.Random.Range(-600f, 600f);
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

            coin.localScale = Vector3.Lerp(initialScale, initialScale * 0.35f, t);

            yield return null;
        }

        if (coin != null)
        {
            coin.position = coinTargetLocation.position;

            StartCoroutine(PunchScaleTarget());
            audioManager.PlayCoinPickupAudio();

            Destroy(coin.gameObject);
        }
    }

    private Coroutine punchScaleCoroutine;
    public TextMeshProUGUI moneyText;
    private IEnumerator PunchScaleTarget()
    {
        if (moneyText == null) yield break;

        Transform textTransform = moneyText.transform;

        Vector3 originalTargetScale = Vector3.one;
        Vector3 punchScale = Vector3.one * 1.3f;

        textTransform.localScale = punchScale;

        float duration = 0.12f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (moneyText == null) yield break;
            elapsed += Time.deltaTime;

            textTransform.localScale = Vector3.Lerp(punchScale, originalTargetScale, elapsed / duration);
            yield return null;
        }

        if (moneyText != null)
        {
            textTransform.localScale = originalTargetScale;
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