using System.Collections;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    private GameObject[] cards;
    [SerializeField] private GameObject[] rarityPrefabs;
    public Transform container;
    // Array to keep track of the actual prefabs we spawn
    private GameObject[] activeSpawnedCards = new GameObject[3];

    bool LeveldUp = false;
    bool calledOnce = false;

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
        testInputs();

        if (checkLevel() || LeveldUp)
        {
            if (!calledOnce)
            {
                TriggerCardDisplay();
                calledOnce = true;
                LeveldUp = false;
            }
        }
    }

    void TriggerCardDisplay()
    {
        GameObject[] selectedPrefabs = pickCard(GlobalStats.rarityChance);

        for (int i = 0; i < 3; i++)
        {
            if (activeSpawnedCards[i] != null)
            {
                Destroy(activeSpawnedCards[i]);
            }

            activeSpawnedCards[i] = Instantiate(selectedPrefabs[i], cards[i].transform.position, cards[i].transform.rotation);

            activeSpawnedCards[i].SetActive(false);

            cards[i].SetActive(false);

            StartCoroutine(ExecuteAfterDelay(activeSpawnedCards[i], i));
        }
    }

    IEnumerator ExecuteAfterDelay(GameObject card, int number)
    {
        yield return new WaitForSeconds(0.25f * number);
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
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            float currentRotation = Mathf.Lerp(startRotation, targetRotation, t);
            card.transform.eulerAngles = new Vector3(0, currentRotation, 0);

            yield return null;
        }

        card.transform.eulerAngles = new Vector3(40, targetRotation, 0);
    }

    GameObject[] pickCard(float cardRarityChance)
    {
        GameObject[] selected = new GameObject[3];

        for (int i = 0; i < 3; i++)
        {
            float roll = Random.Range(0f, 1f);

            float finalScore = roll * cardRarityChance;

            if (finalScore >= 0.9f)
            {
                selected[i] = rarityPrefabs[3];
            }
            else if (finalScore >= 0.75f)
            {
                selected[i] = rarityPrefabs[2];
            }
            else if (finalScore >= 0.45f)
            {
                selected[i] = rarityPrefabs[1];
            }
            else
            {
                selected[i] = rarityPrefabs[0];
            }
        }

        return selected;
    }

    void testInputs()
    {
        //remove
        if (Input.GetKeyDown(KeyCode.H))
        {
            LeveldUp = true;
        }
        if (Input.GetKeyDown(KeyCode.J))
        {
            GlobalStats.Experince = 0;
        }
    }
    bool checkLevel()
    {
        if (GlobalStats.Experince < GlobalStats.expTonNextLevel)
        {
            return false;
        }
        GlobalStats.expTonNextLevel = GlobalStats.expTonNextLevel * 1.33f;
        Debug.Log(GlobalStats.expTonNextLevel);
        return true;
    }
}
