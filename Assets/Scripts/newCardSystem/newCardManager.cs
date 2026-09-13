using UnityEngine;

public class NewCardManager : MonoBehaviour
{
    public GameObject cardPrefab;
    public Transform cardParent; // Drag your Canvas or UI Panel here in Inspector
    private GameObject[] spawnedCards;

    private bool calledOnce = false;
    void Update()
    {
        if (!calledOnce)
        {
            if (checkLevel())
            {
                upgradeTime();
                calledOnce = true;
            }
        }
    }

    private void upgradeTime()
    {
        Time.timeScale = 0;
        float[] xPositions = new float[] { -240f, 0f, 240f };

        spawnedCards = new GameObject[xPositions.Length];

        for (int i = 0; i < xPositions.Length; i++)
        {
            GameObject newCard = Instantiate(cardPrefab, cardParent != null ? cardParent : transform);

            RectTransform rect = newCard.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchoredPosition = new Vector2(xPositions[i], 0f);
            }

            spawnedCards[i] = newCard;
        }
    }

    public void destroyCards()
    {
        Time.timeScale = 1;
        if (spawnedCards == null) return;

        // Loop through the stored cards and destroy them
        foreach (GameObject card in spawnedCards)
        {
            if (card != null)
            {
                Destroy(card);
            }
        }

        // Optional: Reset array reference after destroying
        spawnedCards = null;
        calledOnce = false;
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