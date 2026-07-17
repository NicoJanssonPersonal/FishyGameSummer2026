using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class SlotMachine : MonoBehaviour
{
    public GameObject[] slotWheels;
    public GameObject button;

    [Header("Button Animation Settings")]
    [SerializeField] private float pressDepth = 0.05f;
    [SerializeField] private float pressSpeed = 0.5f;

    [Header("3rd Spin Settings")]
    [SerializeField] private float slowStrength = 6f;
    [SerializeField] private float additionalSpins = 6f;

    public GameObject[] offLights;

    private bool isAnimating = false;
    private float[] currentWheelAngles;

    public GameObject[] unlitLetters;
    public Button increaseBet;
    public Button decreaseBet;
    public TextMeshProUGUI betAmountText;
    int currentbet = 100;
    bool canBet;

    void Start()
    {
        currentWheelAngles = new float[slotWheels.Length];
        for (int i = 0; i < slotWheels.Length; i++)
        {
            currentWheelAngles[i] = 0f;
            slotWheels[i].transform.localRotation = Quaternion.Euler(0, 0, 0);
        }

        increaseBet.onClick.AddListener(upBet);
        decreaseBet.onClick.AddListener(downBet);

        // Make sure the default bet is valid and clamped right away
        currentbet = Mathf.Clamp(100, 10, Mathf.Min(10000, GlobalStats.money));
        betAmountText.text = "Bet amount : " + currentbet;

        // Check if we can bet initially
        CheckCanBet();
    }

    void OnMouseDown()
    {
        // Prevent spinning if already animating, if no button, or if canBet is false
        if (isAnimating || button == null || !canBet) return;

        StartCoroutine(AnimateButton());
        Spin();
    }

    // --- Centralized CanBet Safety Check ---
    void CheckCanBet()
    {
        // You can spin if:
        // 1. You have enough money to cover your CURRENT bet.
        // 2. Your current bet is greater than 0.
        if (GlobalStats.money >= currentbet && currentbet > 0)
        {
            canBet = true;
        }
        else
        {
            canBet = false;
        }
    }

    void upBet()
    {
        int increment = 10;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            increment = 100;
        }
        else if (Input.GetKey(KeyCode.LeftControl))
        {
            increment = 1000;
        }

        currentbet += increment;

        // Cap the bet at 10k or total money, whichever is lower
        int maxAllowedBet = Mathf.Min(10000, GlobalStats.money);
        if (currentbet > maxAllowedBet)
        {
            currentbet = maxAllowedBet;
        }

        betAmountText.text = "Bet amount : " + currentbet;
        CheckCanBet(); // Validate status after modifying
    }

    void downBet()
    {
        int increment = 10;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            increment = 100;
        }
        else if (Input.GetKey(KeyCode.LeftControl))
        {
            increment = 1000;
        }

        currentbet -= increment;

        if (currentbet < 100)
        {
            currentbet = 100;
        }

        betAmountText.text = "Bet amount : " + currentbet;
        CheckCanBet();
    }

    void Spin()
    {
        GlobalStats.money = GlobalStats.money - currentbet;
        GlobalStats.SaveMoneyAndSkillpoints();

        CheckCanBet();

        foreach (var letter in unlitLetters)
        {
            if (letter != null) letter.SetActive(true);
        }
        foreach (var light in offLights)
        {
            if (light != null) light.SetActive(true);
        }

        float[] possiblestops = { 0f, 45f, 90f, 135f, 180f, 225f, 270f, 315f };
        float[] targetAngles = new float[slotWheels.Length];

        for (int i = 0; i < slotWheels.Length; i++)
        {
            targetAngles[i] = possiblestops[Random.Range(0, possiblestops.Length)];
        }

        if (Mathf.RoundToInt(targetAngles[1]) == Mathf.RoundToInt(targetAngles[0]))
        {
            if (Random.value < 0.5f)
            {
                targetAngles[2] = targetAngles[0];

                if (targetAngles[0] == 0)
                {
                    StartCoroutine(JACKPOT());
                    reward(Mathf.RoundToInt(targetAngles[2]));
                }
                else
                {
                    StartCoroutine(WINLights());
                    reward(Mathf.RoundToInt(targetAngles[2]));
                }
            }
            else
            {
                targetAngles[2] = (targetAngles[0] + 45f) % 360f;
                StartCoroutine(AnimateLights(8f));
            }
        }

        Debug.Log("1st: " + targetAngles[0] + " | 2nd: " + targetAngles[1] + " | 3rd: " + targetAngles[2]);

        for (int i = 0; i < slotWheels.Length; i++)
        {
            float startDelay = i * 0.2f;
            float duration = 1.5f;

            if (i == 2 && Mathf.RoundToInt(targetAngles[1]) == Mathf.RoundToInt(targetAngles[0]))
            {
                StartCoroutine(AnimateSpin(i, slotWheels[i], targetAngles[i], startDelay, duration + slowStrength));
            }
            else
            {
                StartCoroutine(AnimateSpin(i, slotWheels[i], targetAngles[i], startDelay, duration));
            }
        }
    }

    IEnumerator AnimateButton()
    {
        isAnimating = true;

        Vector3 originalPosition = button.transform.localPosition;
        Vector3 targetPosition = originalPosition + new Vector3(0, -pressDepth, 0);

        while (Vector3.Distance(button.transform.localPosition, targetPosition) > 0.01f)
        {
            button.transform.localPosition = Vector3.MoveTowards(
                button.transform.localPosition,
                targetPosition,
                pressSpeed * Time.deltaTime
            );
            yield return null;
        }

        while (Vector3.Distance(button.transform.localPosition, originalPosition) > 0.01f)
        {
            button.transform.localPosition = Vector3.MoveTowards(
                button.transform.localPosition,
                originalPosition,
                pressSpeed * Time.deltaTime
            );
            yield return null;
        }

        button.transform.localPosition = originalPosition;
        isAnimating = false;
    }

    IEnumerator AnimateSpin(int wheelIndex, GameObject wheel, float targetAngle, float delay, float duration)
    {
        yield return new WaitForSeconds(delay);

        float extraSpins = additionalSpins;
        float elapsed = 0f;

        float startX = currentWheelAngles[wheelIndex];

        float deltaAngle = targetAngle - startX;
        if (deltaAngle < 0)
        {
            deltaAngle += 360f;
        }

        float endX = startX + (extraSpins * 360f) + deltaAngle;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / duration;

            float curve = 1f - Mathf.Pow(1f - percent, 3f);

            float currentX = Mathf.Lerp(startX, endX, curve);
            wheel.transform.localRotation = Quaternion.Euler(currentX, 0, 0);

            yield return null;
        }

        float finalCleanAngle = targetAngle % 360f;
        wheel.transform.localRotation = Quaternion.Euler(finalCleanAngle, 0, 0);

        currentWheelAngles[wheelIndex] = finalCleanAngle;
    }

    IEnumerator AnimateLights(float totalDuration)
    {
        yield return new WaitForSeconds(2f);
        float elapsed = 0f;
        float stepDelay = 0.1f;
        int pairIndex = 0;
        int totalPairs = offLights.Length / 2;

        if (offLights.Length < 2) yield break;

        foreach (var light in offLights)
        {
            if (light != null) light.SetActive(true);
        }

        while (elapsed < totalDuration)
        {
            int leftLightIndex = pairIndex * 2;
            int rightLightIndex = (pairIndex * 2) + 1;

            if (offLights[leftLightIndex] != null) offLights[leftLightIndex].SetActive(false);
            if (offLights[rightLightIndex] != null) offLights[rightLightIndex].SetActive(false);

            yield return new WaitForSeconds(stepDelay);
            elapsed += stepDelay;

            if (offLights[leftLightIndex] != null) offLights[leftLightIndex].SetActive(true);
            if (offLights[rightLightIndex] != null) offLights[rightLightIndex].SetActive(true);

            pairIndex = (pairIndex + 1) % totalPairs;
        }

        foreach (var light in offLights)
        {
            if (light != null) light.SetActive(true);
        }
    }

    IEnumerator FlashAllLights(int flashCount, float flashInterval)
    {
        if (offLights == null || offLights.Length == 0) yield break;

        for (int i = 0; i < flashCount; i++)
        {
            foreach (var light in offLights)
            {
                if (light != null) light.SetActive(false);
            }

            yield return new WaitForSeconds(flashInterval);

            foreach (var light in offLights)
            {
                if (light != null) light.SetActive(true);
            }

            yield return new WaitForSeconds(flashInterval);
        }
    }

    public float sequnceLightsDuration = 6f;
    public int numberOfflashes = 12;
    public float timeBetweenFlashes = 0.1f;

    IEnumerator WINLights()
    {
        yield return StartCoroutine(AnimateLights(sequnceLightsDuration));
        yield return StartCoroutine(FlashAllLights(numberOfflashes, timeBetweenFlashes));
    }

    IEnumerator JACKPOT()
    {
        yield return StartCoroutine(AnimateLights(sequnceLightsDuration));
        StartCoroutine(FlashAllLights(numberOfflashes, timeBetweenFlashes));
        yield return StartCoroutine(LetterWave(0.1f));
    }

    IEnumerator LetterWave(float delayBetweenLights)
    {
        for (int i = 0; i < unlitLetters.Length; i++)
        {
            if (unlitLetters[i] != null)
            {
                unlitLetters[i].SetActive(false);
            }
            yield return new WaitForSeconds(delayBetweenLights);
        }
    }

    void reward(int angle)
    {
        if (angle == 0)
        {
            print("JACKPOT");
            GlobalStats.money += currentbet * 100;
        }
        else if (angle == 45)
        {
            //"Flaskpost";
        }
        else if (angle == 90)
        {
            GlobalStats.skillpoints += (currentbet / 100);
        }
        else if (angle == 135)
        {
            GlobalStats.money += currentbet * 10;
        }
        else if (angle == 180)
        {
            //"Anchor";
        }
        else if (angle == 225)
        {
            GlobalStats.money += currentbet * 2;
        }
        else if (angle == 270)
        {
            GlobalStats.skillpoints += (currentbet / 100) * 2;
        }
        else if (angle == 315)
        {
            //"Crab";
        }
        GlobalStats.SaveMoneyAndSkillpoints();
    }
}