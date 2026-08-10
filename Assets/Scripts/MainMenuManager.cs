using System.Collections;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    [Header("Cameras")]
    public Camera mainCamera;       // Child of Player (Boat)
    public Camera harborCamera;     // Standalone Camera at the Harbor

    [Header("UI & Settings")]
    public GameObject mainMenuUI;
    public float transitionDuration = 2.5f;

    private Vector3 defaultLocalPos;
    private Quaternion defaultLocalRot;
    private CameraController cameraController;

    public GameObject UIManager;

    void Start()
    {
        UIManager.SetActive(false);
        Time.timeScale = 0f;

        defaultLocalPos = mainCamera.transform.localPosition;
        defaultLocalRot = mainCamera.transform.localRotation;

        cameraController = mainCamera.GetComponent<CameraController>();

        harborCamera.gameObject.SetActive(true);
        mainCamera.gameObject.SetActive(false);
    }

    public void PlayGame()
    {
        if (mainMenuUI != null)
            mainMenuUI.SetActive(false);

        Time.timeScale = 1f;

        StartCoroutine(SmoothCameraTransition());
    }

    IEnumerator SmoothCameraTransition()
    {
        if (cameraController != null)
            cameraController.isTransitioning = true;

        Transform parentTransform = mainCamera.transform.parent;

        Vector3 startLocalPos = parentTransform.InverseTransformPoint(harborCamera.transform.position);
        Quaternion startLocalRot = Quaternion.Inverse(parentTransform.rotation) * harborCamera.transform.rotation;

        mainCamera.transform.localPosition = startLocalPos;
        mainCamera.transform.localRotation = startLocalRot;

        mainCamera.gameObject.SetActive(true);
        harborCamera.gameObject.SetActive(false);

        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / transitionDuration);

            mainCamera.transform.localPosition = Vector3.Lerp(startLocalPos, defaultLocalPos, t);
            mainCamera.transform.localRotation = Quaternion.Slerp(startLocalRot, defaultLocalRot, t);

            yield return null;
        }

        mainCamera.transform.localPosition = defaultLocalPos;
        mainCamera.transform.localRotation = defaultLocalRot;

        if (cameraController != null)
        {
            cameraController.ResetAngles();
            UIManager.SetActive(true);
            cameraController.isTransitioning = false;
        }
    }
}