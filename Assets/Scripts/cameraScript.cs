using Unity.Cinemachine;
using UnityEngine;

public class cameraScript : MonoBehaviour
{
    private CinemachineInputAxisController axisController;

    void Awake()
    {
        axisController = GetComponent<CinemachineInputAxisController>();
    }

    void Update()
    {
        bool isRightClicking = Input.GetMouseButton(1);
        
        // Toggle Cinemachine axis controller
        if (axisController != null)
        {
            axisController.enabled = isRightClicking;
        }

        // Lock and hide mouse cursor while right-clicking
        if (isRightClicking)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}