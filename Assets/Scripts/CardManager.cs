using System.Collections;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    private GameObject[] cards;
    [SerializeField] private GameObject[] rarityPrefabs;
    public Transform container;
    private GameObject[] activeSpawnedCards = new GameObject[3];
    public static bool isUpgrading = false;

    private bool calledOnce = false;

    void Start()
    {
        cards = new GameObject[container.childCount];

        for (int i = 0; i < container.childCount; i++)
        {
            cards[i] = container.GetChild(i).gameObject;
        }

        calledOnce = false;
    }

    void Update()
    {
        if (!calledOnce)
        {
            if (checkLevel())
            {
                TriggerCardDisplay();
                calledOnce = true;
            }
        }
    }

    void TriggerCardDisplay()
    {
        isUpgrading = true;
        Time.timeScale = 1f;
        GameObject[] selectedPrefabs = pickCard(GlobalStats.rarityChance);

        for (int i = 0; i < 3; i++)
        {
            if (activeSpawnedCards[i] != null)
            {
                Destroy(activeSpawnedCards[i]);
            }

            activeSpawnedCards[i] = Instantiate(selectedPrefabs[i], cards[i].transform);

            activeSpawnedCards[i].transform.localRotation = Quaternion.identity;
            activeSpawnedCards[i].transform.localScale = selectedPrefabs[i].transform.localScale * 0.5f;

            CardUpgrade upgradeScript = activeSpawnedCards[i].GetComponent<CardUpgrade>();
            if (upgradeScript != null)
            {
                upgradeScript.SetupCard(this);
            }
            else
            {
                Debug.LogWarning($"CardUpgrade script missing on prefab: {activeSpawnedCards[i].name}!");
            }

            StartCoroutine(ExecuteAfterDelay(activeSpawnedCards[i], i));
        }
    }

    public void HideCards()
    {
        isUpgrading = false;
        StopAllCoroutines();
        Time.timeScale = 1;

        for (int i = 0; i < activeSpawnedCards.Length; i++)
        {
            if (activeSpawnedCards[i] != null)
            {
                Destroy(activeSpawnedCards[i]);
            }
        }

        calledOnce = false;
    }

    IEnumerator ExecuteAfterDelay(GameObject card, int number)
    {
        yield return new WaitForSecondsRealtime(0.1f * number);

        StartCoroutine(AnimateCardIn(0.3f, card));
    }

    IEnumerator AnimateCardIn(float duration, GameObject card)
    {
        if (card == null) yield break;

        float elapsed = 0f;
        float startRotation = card.transform.localEulerAngles.y;
        float targetRotation = startRotation + 360f;

        Vector3 startLocalPos = new Vector3(0f, -300f, 0f); // Adjust -300f if your UI scale is different
        Vector3 targetLocalPos = Vector3.zero;

        card.transform.localPosition = startLocalPos;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            float easeT = Mathf.Sin(t * Mathf.PI * 0.5f);

            if (card == null) yield break;

            float currentRotation = Mathf.Lerp(startRotation, targetRotation, t);
            card.transform.localEulerAngles = new Vector3(0, currentRotation, 0);
            card.transform.localPosition = Vector3.Lerp(startLocalPos, targetLocalPos, easeT);

            yield return null;
        }

        if (card != null)
        {
            card.transform.localPosition = targetLocalPos;
            card.transform.localEulerAngles = new Vector3(40, targetRotation, 0);
        }
    }

    GameObject[] pickCard(float cardRarityChance)
    {
        GameObject[] selected = new GameObject[3];
        float luckBonus = cardRarityChance * 0.5f;

        for (int i = 0; i < 3; i++)
        {
            float roll = Mathf.Clamp(Random.Range(0f, 100f) + luckBonus, 0f, 100f);

            if (roll >= 99.5f) selected[i] = rarityPrefabs[4]; // Chaotic
            else if (roll >= 97.5f) selected[i] = rarityPrefabs[3]; // Legendary
            else if (roll >= 85.0f) selected[i] = rarityPrefabs[2]; // Rare
            else if (roll >= 50.0f) selected[i] = rarityPrefabs[1]; // Uncommon
            else selected[i] = rarityPrefabs[0]; // Common
        }

        return selected;
    }

    bool checkLevel()
    {
        if (GlobalStats.Experince < GlobalStats.expTonNextLevel) return false;
        if (FishMinigame.isUIOpen) return false;

        GlobalStats.Level++;
        GlobalStats.Experince -= GlobalStats.expTonNextLevel;
        GlobalStats.expTonNextLevel = GlobalStats.expTonNextLevel * 1.33f;

        Debug.Log("Leveled up to " + GlobalStats.Level + "! Exp needed for next: " + GlobalStats.expTonNextLevel);
        return true;
    }
}