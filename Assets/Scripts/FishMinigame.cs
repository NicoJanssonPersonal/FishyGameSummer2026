using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FishMinigame : MonoBehaviour
{
    [Header("UI Canvas Elements")]
    public GameObject uiPanel;
    public RectTransform greenZoneHolder;
    public GameObject greenZone;
    public GameObject fisheGameObject;
    public GameObject[] fishesGameObjects;
    private RectTransform fishe;
    public Rigidbody2D fishRB;
    public static bool isUIOpen = false;

    [Header("Fish Stats")]
    private GameObject[] greenZoneObjects = new GameObject[0];
    private Vector3[] zoneBasePositions = new Vector3[0];
    private Vector2 initialFishPos;
    public RectTransform killZone;

    public RectTransform baitHitBox;
    public UiManager uiManagerScript;

    [Header("Zone Movement Settings")]
    public float zoneMoveSpeed = 1.5f;       // Speed of moving zones
    public float zoneMoveAmplitude = 10f;    // Pixels moved up/down
    private int currentDifficulty = 1;
    private int thisFishDifficulty;

    public TextMeshProUGUI money;

    public TextMeshProUGUI multText;
    public RectTransform multTextHolder;
    private int currentFishMult;
    private int timesMissed;

    public AudioManager audioManager;



    void Start()
    {
        fishe = fisheGameObject.GetComponent<RectTransform>();
        fishRB = fisheGameObject.GetComponent<Rigidbody2D>();
        initialFishPos = fishe.anchoredPosition;
        money.text = GlobalStats.money.ToString();
        currentFishMult = 0;
        timesMissed = 0;
        GlobalStats.LoadStats();
        money.text = GlobalStats.money.ToString();
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

        //Debug.Log(currentFishMult);
        if (!isUIOpen)
            return;

        // Animate middle green zones at difficulty 5+
        if (currentDifficulty >= 5)
        {
            AnimateZones(currentDifficulty);
        }

        bool spacePressedOnce = false;
        bool fishInAnyZone = false;

        for (int i = 0; i < greenZoneObjects.Length; i++)
        {
            if (greenZoneObjects[i] == null)
                continue;

            RectTransform zoneRect = greenZoneObjects[i].GetComponent<RectTransform>();

            if (zoneRect != null && IsOverlapping(fishe, zoneRect))
            {
                fishInAnyZone = true;

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    spacePressedOnce = true;
                    if (i < greenZoneObjects.Length - 1)
                    {
                        audioManager.PlayHitSound(currentFishMult);
                        MoveFishToZone(i + 1);
                    }
                    else
                    {
                        CatchFish();
                        thisFishCaught = true;
                        spacePressedOnce = false;
                    }
                }
                break;
            }
        }

        if (IsOverlapping(baitHitBox, killZone))
        {
            FishEscaped();
        }

        if (!fishInAnyZone && Input.GetKeyDown(KeyCode.Space))
        {
            multText.text = "";
            currentFishMult = 0;
            timesMissed = timesMissed + 1;
            audioManager.PlayMissSound();
            missedFeedback();
        }
        if (timesMissed == 3)
        {
            FishEscaped();
            timesMissed = 0;
        }
        if (spacePressedOnce)
        {
            fishRB.linearVelocity = new Vector2(fishRB.linearVelocity.x, -GlobalStats.constantSpeed);
        }
    }

    void AnimateZones(int level)
    {
        float levelMultiplier = level / 5;
        int zoneCount = greenZoneObjects.Length;
        if (zoneCount <= 2) return;

        for (int i = 1; i < zoneCount - 1; i++)
        {
            if (greenZoneObjects[i] == null) continue;

            float phaseOffset = i * 1.5f;
            float yOffset = Mathf.Sin(Time.time * zoneMoveSpeed * levelMultiplier + phaseOffset) * zoneMoveAmplitude;

            Vector3 newPos = zoneBasePositions[i];
            newPos.y += yOffset;

            greenZoneObjects[i].transform.localPosition = newPos;
        }
    }
    private bool thisFishCaught = false;
    void MoveFishToZone(int zoneIndex)
    {
        if (zoneIndex >= greenZoneObjects.Length && !thisFishCaught)
        {
            CatchFish();
            thisFishCaught = true;  
            return;
        }

        RectTransform zone = greenZoneObjects[zoneIndex].GetComponent<RectTransform>();
        Vector3 targetPos = zone.position;
        targetPos.y += Random.Range(5f, 30f);
        currentFishMult = Mathf.Min(10, currentFishMult + 1); // cappar multen ti 10
        spawnMultText(currentFishMult, greenZoneObjects[zoneIndex - 1].GetComponent<RectTransform>().position); //måst ha ett bättre sätt att skirva mult, inte bara ta zoneIndex
        fishe.position = targetPos;
    }

    bool IsOverlapping(RectTransform fish, RectTransform zone)
    {
        Vector3[] fishCorners = new Vector3[4];
        Vector3[] zoneCorners = new Vector3[4];

        fish.GetWorldCorners(fishCorners);
        zone.GetWorldCorners(zoneCorners);

        float fishWidth = fishCorners[2].x - fishCorners[0].x;
        float fishHeight = fishCorners[2].y - fishCorners[0].y;

        Rect fishHitbox = new Rect(
            fishCorners[0].x,
            fishCorners[2].y - fishHeight,
            fishWidth,
            fishHeight
        );

        Rect zoneRect = new Rect(
            zoneCorners[0].x,
            zoneCorners[0].y,
            zoneCorners[2].x - zoneCorners[0].x,
            zoneCorners[2].y - zoneCorners[0].y
        );

        return fishHitbox.Overlaps(zoneRect);
    }

    void CatchFish()
    {
        float xpFromFish = thisFishDifficulty * 3 * GlobalStats.xpGain;
        GlobalStats.Experince += xpFromFish;

        float moneyFromFish = thisFishDifficulty * 2 * GlobalStats.moneyGain * Mathf.Max(1f, currentFishMult);
        int amountToAdd = Mathf.RoundToInt(moneyFromFish);
        if (!thisFishCaught)
        {
            uiManagerScript.updateFishCaught(amountToAdd, Mathf.RoundToInt(xpFromFish), thisFishDifficulty);
        }
        StartCoroutine(AddMoneySmoothly(amountToAdd, 0.5f));

        StartCoroutine(delay());
    }
    IEnumerator AddMoneySmoothly(int moneyToAdd, float duration)
    {
        yield return new WaitForSeconds(1f);
        int startMoney = GlobalStats.money;
        int targetMoney = startMoney + moneyToAdd;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            int currentDisplayMoney = (int)Mathf.Lerp(startMoney, targetMoney, t);

            // Update the visual UI string
            money.text = currentDisplayMoney.ToString();

            yield return null;
        }

        GlobalStats.money = targetMoney;
        money.text = GlobalStats.money.ToString();
        GlobalStats.SaveMoneyAndSkillpoints();
    }

    IEnumerator delay()
    {
        fishRB.linearVelocity = Vector3.zero;
        yield return new WaitForSeconds(0.5f);
        closeUI();
    }

    private Coroutine escapeCoroutine;

    void FishEscaped()
    {
        if (escapeCoroutine != null)
            StopCoroutine(escapeCoroutine);

        escapeCoroutine = StartCoroutine(AnimateFishEscaped());
    }

    private IEnumerator AnimateFishEscaped()
    {
        RectTransform panelRect = uiPanel.GetComponent<RectTransform>();
        CanvasGroup canvasGroup = uiPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = uiPanel.AddComponent<CanvasGroup>();

        GameObject flashObj = new GameObject("TempRedFlash", typeof(RectTransform), typeof(Image));
        flashObj.transform.SetParent(uiPanel.transform, false);

        RectTransform flashRect = flashObj.GetComponent<RectTransform>();
        flashRect.anchorMin = Vector2.zero;
        flashRect.anchorMax = Vector2.one;
        flashRect.offsetMin = Vector2.zero;
        flashRect.offsetMax = Vector2.one;

        Image flashImage = flashObj.GetComponent<Image>();
        Color startFlashColor = new Color(1f, 0f, 0f, 0.4f);
        flashImage.color = startFlashColor;
        flashImage.raycastTarget = false;

        Vector2 startPos = panelRect.anchoredPosition;
        float elapsed = 0f;
        float duration = 0.5f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            float decay = 1f - progress;

            panelRect.anchoredPosition = startPos + (Random.insideUnitCircle * 35f * decay);

            float scale = 1f + (Mathf.Sin(progress * Mathf.PI * 3f) * 0.05f * decay);
            panelRect.localScale = Vector3.one * scale;

            flashImage.color = new Color(1f, 0f, 0f, startFlashColor.a * decay);

            if (progress > 0.6f)
            {
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, (progress - 0.6f) / 0.4f);
            }

            yield return null;
        }

        Destroy(flashObj);
        panelRect.anchoredPosition = startPos;
        panelRect.localScale = Vector3.one;
        canvasGroup.alpha = 1f;

        closeUI();
        escapeCoroutine = null;
    }

    public void openUi(int fishDifficulty)
    {
        thisFishCaught = false;
        multText.text = "";
        currentFishMult = 0;
        thisFishDifficulty = fishDifficulty;
        closeUI();
        isUIOpen = true;

        foreach (var fish in fishesGameObjects)
        {
            fish.SetActive(false);
        }

        GameObject activeFish = fishesGameObjects[fishDifficulty - 1]; //ta tibaka när det finns 10 olika sprites
        //GameObject activeFish = fishesGameObjects[0];
        activeFish.SetActive(true);
        fishe.anchoredPosition = initialFishPos;

        uiPanel.SetActive(true);
        generategreenZones(fishDifficulty, fishDifficulty);
    }

    void closeUI()
    {
        thisFishCaught = false;
        isUIOpen = false;
        deletegreenZones();
        uiPanel.SetActive(false);
        //currentFishMult = 0;
        foreach (var fish in fishesGameObjects)
        {
            fish.SetActive(false);
        }
    }

    void generategreenZones(int difficulty, int greenZones)
    {
        currentDifficulty = difficulty;
        int zoneCount = greenZones + 1;

        greenZoneObjects = new GameObject[zoneCount];
        zoneBasePositions = new Vector3[zoneCount];

        float minY = 160.5f;
        float maxY = 435.0f;
        float totalDistance = maxY - minY;

        float scaleShrinkFactor = 0.075f * (difficulty * Random.Range(0.5f, 1.2f));

        for (int i = 0; i < zoneCount; i++)
        {
            float dynamicScaleY = 0.45f - (i * scaleShrinkFactor);
            if (dynamicScaleY < 0.05f)
                dynamicScaleY = 0.05f;

            GameObject newGreenZone = Instantiate(greenZone, greenZoneHolder, false);
            newGreenZone.transform.localScale = new Vector3(1f, dynamicScaleY, 1f);

            float t = (zoneCount > 1) ? (float)i / (zoneCount - 1) : 0f;
            float targetY = Mathf.Lerp(minY, maxY, t);

            if (i > 0 && i < zoneCount - 1)
            {
                float maxJitter = (totalDistance / (zoneCount - 1)) * 0.15f;
                targetY += Random.Range(-maxJitter, maxJitter);
            }

            Vector3 position = new Vector3(1127f, targetY, 0f);
            newGreenZone.transform.localPosition = position;

            greenZoneObjects[i] = newGreenZone;
            zoneBasePositions[i] = position;
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
        zoneBasePositions = new Vector3[0];
    }
    [Header("Juice Settings")]
    [SerializeField] private Color baseColor = Color.yellow;
    [SerializeField] private Color maxColor = Color.red;

    private Coroutine activeJuiceCoroutine;
    private Vector2 originalAnchoredPosition;

    void Awake()
    {
        if (multTextHolder != null)
        {
            originalAnchoredPosition = multTextHolder.anchoredPosition;
        }
    }

    void spawnMultText(int multAmount, Vector3 prevZonePos)
    {
        if (multAmount > 1)
        {
            multTextHolder.position = prevZonePos;
            multText.text = multAmount.ToString() + "x";

            float t = Mathf.Clamp01((multAmount - 2f) / 8f);

            multText.color = Color.Lerp(baseColor, maxColor, t);

            if (activeJuiceCoroutine != null)
            {
                StopCoroutine(activeJuiceCoroutine);
            }

            activeJuiceCoroutine = StartCoroutine(AnimateJuice(t));
        }
        else
        {
            multText.text = " ";
        }
    }

    private IEnumerator AnimateJuice(float intensity)
    {
        float duration = Mathf.Lerp(0.15f, 0.45f, intensity);
        float maxPunchScale = Mathf.Lerp(1.3f, 1.8f, intensity);
        float shakePixelStrength = Mathf.Lerp(10f, 40f, intensity);

        Vector2 startAnchorPos = multTextHolder.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            float decay = 1f - progress;

            float punchFactor = Mathf.Sin(progress * Mathf.PI * 4f) * decay;
            float currentScale = 1f + (maxPunchScale - 1f) * Mathf.Max(0f, punchFactor);
            multTextHolder.localScale = Vector3.one * currentScale;

            Vector2 randomOffset = Random.insideUnitCircle * shakePixelStrength * decay;
            multTextHolder.anchoredPosition = startAnchorPos + randomOffset;

            yield return null;
        }

        multTextHolder.localScale = Vector3.one;
        multTextHolder.anchoredPosition = startAnchorPos;
        activeJuiceCoroutine = null;
    }
    [Header("Miss Settings")]
    [SerializeField] private Color missColor = new Color(0.8f, 0.2f, 0.2f, 1f);
    [SerializeField] private float dropDistance = 60f;

    private Coroutine activeBreakCoroutine;
    void missedFeedback()
    {
        if (activeBreakCoroutine != null)
        {
            StopCoroutine(activeBreakCoroutine);
        }

        activeBreakCoroutine = StartCoroutine(AnimateComboBreak());
    }

    private IEnumerator AnimateComboBreak()
    {
        multText.text = "MISS!";
        Vector2 startAnchorPos = multTextHolder.anchoredPosition;

        float duration = 0.45f;
        float elapsed = 0f;

        float randomTilt = Random.Range(-15f, 15f);
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, randomTilt);

        float popUpHeight = 20f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            float yOffset;
            if (progress < 0.2f)
            {
                float popProgress = progress / 0.2f;
                yOffset = Mathf.Lerp(0f, popUpHeight, popProgress);
            }
            else
            {
                float fallProgress = (progress - 0.2f) / 0.8f;
                float gravityEase = fallProgress * fallProgress;
                yOffset = Mathf.Lerp(popUpHeight, -dropDistance, gravityEase);
            }

            multTextHolder.anchoredPosition = startAnchorPos + new Vector2(0f, yOffset);

            multTextHolder.localScale = Vector3.one * Mathf.Lerp(1.2f, 0.8f, progress);
            multTextHolder.localRotation = Quaternion.Lerp(Quaternion.identity, targetRotation, progress * 2f);

            Color c = missColor;
            c.a = Mathf.Lerp(1f, 0f, progress);
            multText.color = c;

            yield return null;
        }

        multTextHolder.anchoredPosition = startAnchorPos;
        multTextHolder.localScale = Vector3.one;
        multTextHolder.localRotation = Quaternion.identity;

        multText.text = " ";
        activeBreakCoroutine = null;
    }

}