using System.Collections;
using Unity.Mathematics;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    private GameObject[] cards;
    public Transform container;
    bool LeveldUp = false;
    bool calledOnce;
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

    // Update is called once per frame
    void Update()
    {
        
        testInputs();
        for (int i = 0; i < cards.Length; i++)
        {
            {
            if (LeveldUp)
            {
                if (!calledOnce)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        StartCoroutine(ExecuteAfterDelay(cards[j], j));
                    } 
                    calledOnce = true;
                }
            }
            else if (!LeveldUp)
            {
                cards[i].SetActive(false);
                calledOnce = false;
            }
        }
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

        // We use a target that is exactly 360 degrees more than where we started
        float targetRotation = startRotation + 360f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Use Lerp to move from start to target directly
            float currentRotation = Mathf.Lerp(startRotation, targetRotation, t);
            card.transform.eulerAngles = new Vector3(0, currentRotation, 0);

            yield return null;
        }

        // Force the final rotation
        card.transform.eulerAngles = new Vector3(40, targetRotation, 0);
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
            LeveldUp = false;
        }
    }
}
