using UnityEngine;
using System.Collections;

public class SlotMachine : MonoBehaviour
{
    public GameObject[] slotWheels;
    public GameObject button;

    [Header("Button Animation Settings")]
    [SerializeField] private float pressDepth = 0.05f;
    [SerializeField] private float pressSpeed = 0.5f;

    private bool isAnimating = false;
    private float[] currentWheelAngles;

    void Start()
    {
        currentWheelAngles = new float[slotWheels.Length];
        for (int i = 0; i < slotWheels.Length; i++)
        {
            currentWheelAngles[i] = 0f;
            slotWheels[i].transform.localRotation = Quaternion.Euler(0, 0, 0);
        }
    }

    void OnMouseDown()
    {
        if (isAnimating || button == null) return;

        StartCoroutine(AnimateButton());
        Spin();
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

    void Spin()
    {
        float[] possiblestops = { 0f, 45f, 90f, 135f, 180f, 225f, 270f, 315f };
        float[] targetAngles = new float[slotWheels.Length];

        for (int i = 0; i < slotWheels.Length; i++)
        {
            //targetAngles[i] = 180f;
            targetAngles[i] = possiblestops[Random.Range(0, possiblestops.Length)];
        }

        if (Mathf.RoundToInt(targetAngles[1]) == Mathf.RoundToInt(targetAngles[0]))
        {
            if (Random.value < 0.5f)
            {
                targetAngles[2] = targetAngles[0];
                Debug.Log("JACKPOT FORCED!");
            }
            else
            {
                targetAngles[2] = (targetAngles[0] + 45f) % 360f;
                Debug.Log("UNLUCKY");
            }
        }

        Debug.Log("1st: " + targetAngles[0] + " | 2nd: " + targetAngles[1] + " | 3rd: " + targetAngles[2]);

        for (int i = 0; i < slotWheels.Length; i++)
        {
            float startDelay = i * 0.2f;
            float duration = 1.5f;
            
            if (i == 2 && Mathf.RoundToInt(targetAngles[1]) == Mathf.RoundToInt(targetAngles[0]))
            {
                StartCoroutine(AnimateSpin(i, slotWheels[i], targetAngles[i], startDelay, duration + 3f));
            }
            else
            {
                StartCoroutine(AnimateSpin(i, slotWheels[i], targetAngles[i], startDelay, duration));
            }
        }
    }

    IEnumerator AnimateSpin(int wheelIndex, GameObject wheel, float targetAngle, float delay, float duration)
    {
        yield return new WaitForSeconds(delay);

        float extraSpins = 12f; 
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
}