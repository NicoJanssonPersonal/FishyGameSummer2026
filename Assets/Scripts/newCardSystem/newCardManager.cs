using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewCardManager : MonoBehaviour
{
    public GameObject cardPrefab;
    public Transform cardParent;
    public GameObject backgroundDarken;

    [Header("Juice Settings")]
    [Tooltip("Distance below target position cards start sliding in from")]
    public float slideOffsetY = -800f;

    private GameObject[] spawnedCards;
    private Vector3[] originalScales;

    private bool isLevelingUp = false;
    private bool isAnimating = false;
    private int pendingLevelUps = 0;

    void Update()
    {
        CheckForLevelUps();

        if (pendingLevelUps > 0 && !isLevelingUp && !isAnimating)
        {
            isLevelingUp = true;
            StartCoroutine(UpgradeTimeRoutine());
        }
    }

    private void CheckForLevelUps()
    {
        if (FishMinigame.isUIOpen) return;

        while (GlobalStats.Experince >= GlobalStats.expTonNextLevel)
        {
            GlobalStats.Level++;
            GlobalStats.Experince -= GlobalStats.expTonNextLevel;
            GlobalStats.expTonNextLevel = GlobalStats.expTonNextLevel * 1.33f;
            pendingLevelUps++;

            Debug.Log("Leveled up to " + GlobalStats.Level + "! Pending choices: " + pendingLevelUps);
        }
    }

    private IEnumerator UpgradeTimeRoutine()
    {
        yield return new WaitForSecondsRealtime(0.33f);

        float duration = 0.2f;
        float elapsed = 0f;
        float initialScale = Time.timeScale;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            Time.timeScale = Mathf.Lerp(initialScale, 0f, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        Time.timeScale = 0f;
        StartCoroutine(SpawnCardsRoutine());
    }

    private IEnumerator SpawnCardsRoutine()
    {
        isAnimating = true;
        pendingLevelUps--;

        float[] xPositions = new float[] { -200f, 0f, 200f };
        spawnedCards = new GameObject[xPositions.Length];
        originalScales = new Vector3[xPositions.Length];
        backgroundDarken.SetActive(true);

        for (int i = 0; i < xPositions.Length; i++)
        {
            GameObject newCard = Instantiate(cardPrefab, cardParent != null ? cardParent : transform);
            RectTransform rect = newCard.GetComponent<RectTransform>();

            originalScales[i] = rect != null ? rect.localScale : Vector3.one;

            Vector2 targetPos = new Vector2(xPositions[i], 0f);
            Vector2 startPos = targetPos + new Vector2(0f, slideOffsetY);

            if (rect != null)
            {
                rect.anchoredPosition = startPos;
            }

            CanvasGroup canvasGroup = newCard.GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = newCard.AddComponent<CanvasGroup>();

            spawnedCards[i] = newCard;

            StartCoroutine(AnimateCardSpawn(rect, canvasGroup, startPos, targetPos, originalScales[i]));

            yield return new WaitForSecondsRealtime(0.08f);
        }

        isAnimating = false;
    }

    private IEnumerator AnimateCardSpawn(RectTransform rect, CanvasGroup canvasGroup, Vector2 startPos, Vector2 targetPos, Vector3 targetScale)
    {
        float duration = 0.4f;
        float elapsed = 0f;

        canvasGroup.alpha = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            float movementProgress = EaseOutBack(t);

            if (rect != null)
            {
                rect.anchoredPosition = Vector2.LerpUnclamped(startPos, targetPos, movementProgress);
                rect.localScale = targetScale;
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Clamp01(t * 3f);
            }

            yield return null;
        }

        if (rect != null)
        {
            rect.anchoredPosition = targetPos;
            rect.localScale = targetScale;
        }
        if (canvasGroup != null) canvasGroup.alpha = 1f;
    }

    public void destroyCards()
    {
        if (isAnimating) return;
        StartCoroutine(DestroyCardsRoutine());
    }

    private IEnumerator DestroyCardsRoutine()
    {
        isAnimating = true;

        if (pendingLevelUps <= 0)
        {
            backgroundDarken.SetActive(false);
        }

        if (spawnedCards != null && spawnedCards.Length > 0)
        {
            float duration = 0.25f;
            float elapsed = 0f;

            List<RectTransform> rects = new List<RectTransform>();
            List<CanvasGroup> groups = new List<CanvasGroup>();
            List<Vector2> startPositions = new List<Vector2>();

            foreach (GameObject card in spawnedCards)
            {
                if (card != null)
                {
                    RectTransform r = card.GetComponent<RectTransform>();
                    rects.Add(r);
                    startPositions.Add(r != null ? r.anchoredPosition : Vector2.zero);

                    CanvasGroup cg = card.GetComponent<CanvasGroup>();
                    if (cg == null) cg = card.AddComponent<CanvasGroup>();
                    groups.Add(cg);
                }
            }

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float easeT = EaseInCubic(t);

                for (int i = 0; i < rects.Count; i++)
                {
                    if (rects[i] != null)
                    {
                        Vector2 endPos = startPositions[i] + new Vector2(0f, slideOffsetY);
                        rects[i].anchoredPosition = Vector2.Lerp(startPositions[i], endPos, easeT);
                    }
                    if (groups[i] != null)
                    {
                        groups[i].alpha = 1f - t;
                    }
                }

                yield return null;
            }

            foreach (GameObject card in spawnedCards)
            {
                if (card != null) Destroy(card);
            }
        }

        spawnedCards = null;
        isAnimating = false;

        if (pendingLevelUps > 0)
        {
            StartCoroutine(SpawnCardsRoutine());
        }
        else
        {
            isLevelingUp = false;
            Time.timeScale = 1f;
        }
    }

    private float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3) + c1 * Mathf.Pow(t - 1f, 2);
    }

    private float EaseInCubic(float t)
    {
        return t * t * t;
    }
}