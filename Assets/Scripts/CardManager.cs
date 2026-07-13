using System.Collections;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    private GameObject[] cards;
    [SerializeField] private GameObject[] rarityPrefabs;
    public Transform container;
    private GameObject[] activeSpawnedCards = new GameObject[3];

    private bool calledOnce = false;

    void Start()
    {
        cards = new GameObject[container.childCount];

        for (int i = 0; i < container.childCount; i++)
        {
            cards[i] = container.GetChild(i).gameObject;
        }

        foreach (var card in cards)
        {
            card.SetActive(false);
        }

        calledOnce = false;
    }

    void Update()
    {
        // Only run checkLevel if the upgrade screen isn't already active
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
        Time.timeScale = 1f;
        GameObject[] selectedPrefabs = pickCard(GlobalStats.rarityChance);

        for (int i = 0; i < 3; i++)
        {
            if (activeSpawnedCards[i] != null)
            {
                Destroy(activeSpawnedCards[i]);
            }

            // 1. Spawn the 3D card prefab
            activeSpawnedCards[i] = Instantiate(selectedPrefabs[i], cards[i].transform.position, cards[i].transform.rotation);
            activeSpawnedCards[i].transform.localScale = selectedPrefabs[i].transform.localScale * 2f;
            activeSpawnedCards[i].SetActive(false);

            // 2. WIRED CONNECTION: Find your CardUpgrade script on the spawned object and give it this manager instance
            CardUpgrade upgradeScript = activeSpawnedCards[i].GetComponent<CardUpgrade>();
            if (upgradeScript != null)
            {
                upgradeScript.SetupCard(this);
            }
            else
            {
                Debug.LogWarning($"CardUpgrade script missing on prefab: {activeSpawnedCards[i].name}!");
            }

            cards[i].SetActive(false);
            StartCoroutine(ExecuteAfterDelay(activeSpawnedCards[i], i));
        }
    }

    public void HideCards()
    {
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
        yield return new WaitForSecondsRealtime(0.25f * number);
        StartCoroutine(RotateOverTime(1.0f, card));
    }

    IEnumerator RotateOverTime(float duration, GameObject card)
    {
        card.SetActive(true);
        float elapsed = 0f;
        float startRotation = card.transform.eulerAngles.y;
        float targetRotation = startRotation + 360f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            float currentRotation = Mathf.Lerp(startRotation, targetRotation, t);
            if (card == null) yield break;
            card.transform.eulerAngles = new Vector3(0, currentRotation, 0);

            yield return null;
        }

        if (card != null)
        {
            card.transform.eulerAngles = new Vector3(40, targetRotation, 0);
        }
    }

    GameObject[] pickCard(float cardRarityChance)
    {
        GameObject[] selected = new GameObject[3];

        for (int i = 0; i < 3; i++)
        {
            float roll = Random.Range(0f, 1f);
            float finalScore = roll * cardRarityChance;

            if (finalScore >= 0.97f) { selected[i] = rarityPrefabs[4]; }
            else if (finalScore >= 0.9f) { selected[i] = rarityPrefabs[3]; }
            else if (finalScore >= 0.75f) { selected[i] = rarityPrefabs[2]; }
            else if (finalScore >= 0.45f) { selected[i] = rarityPrefabs[1]; }
            else { selected[i] = rarityPrefabs[0]; }
        }

        return selected;
    }

    bool checkLevel()
    {
        if (GlobalStats.Experince < GlobalStats.expTonNextLevel)
        {
            return false;
        }
        if (FishMinigame.isUIOpen)
        {
            return false;
        }

        GlobalStats.Level++;
        GlobalStats.Experince -= GlobalStats.expTonNextLevel;
        GlobalStats.expTonNextLevel = GlobalStats.expTonNextLevel * 1.33f;

        Debug.Log("Leveled up to " + GlobalStats.Level + "! Exp needed for next: " + GlobalStats.expTonNextLevel);
        return true;
    }
}