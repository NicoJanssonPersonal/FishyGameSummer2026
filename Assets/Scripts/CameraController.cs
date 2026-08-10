using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform boatTarget;       // Assign your Boat GameObject here
    private float distance = 15f;
    private float heightOffset = 2.0f;

    public float xSpeed = 120.0f;      // SENSITIVTY SIDEWAYS
    public float ySpeed = 120.0f;      // SENSITIVTY upDOwn

    private float yMinLimit = 5f;     // How low can the camera look
    private float yMaxLimit = 80f;      // How high can the camera look

    public float returnSpeed = 3.0f;       // How fast the camera snaps back behind the boat
    public float movementThreshold = 0.5f; // Minimum boat speed required to trigger auto-center
    private float defaultYRotation = 40.0f;

    public float zoomSpeed = 50.0f;
    public float minFOV = 20.0f;
    public float maxFOV = 80.0f;
    public float zoomSmoothness = 10f;

    private float xRotation = 0.0f;
    private float yRotation = 0.0f;
    private float targetFOV;
    private Camera cam;
    private Rigidbody boatRb;

    [HideInInspector] public bool isTransitioning = false; 

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        xRotation = angles.y;
        yRotation = angles.x;

        cam = GetComponent<Camera>();
        if (cam != null)
        {
            targetFOV = cam.fieldOfView;
        }

        if (boatTarget != null)
        {
            boatRb = boatTarget.GetComponentInChildren<Rigidbody>();
        }
    }
    public void ResetAngles()
    {
        Vector3 angles = transform.eulerAngles;
        xRotation = angles.y;
        yRotation = angles.x;
    }

    void LateUpdate()
    {
        if (isTransitioning || boatTarget == null) return;

        if (boatRb == null) boatRb = boatTarget.GetComponent<Rigidbody>();

        if (Input.GetMouseButton(1) && !CardManager.isUpgrading)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            xRotation += Input.GetAxis("Mouse X") * xSpeed * Time.deltaTime;
            yRotation -= Input.GetAxis("Mouse Y") * ySpeed * Time.deltaTime;
            yRotation = Mathf.Clamp(yRotation, yMinLimit, yMaxLimit);

            if (cam != null)
            {
                float scrollInput = Input.GetAxis("Mouse ScrollWheel");
                if (Mathf.Abs(scrollInput) > 0.01f)
                {
                    targetFOV -= scrollInput * zoomSpeed;
                    targetFOV = Mathf.Clamp(targetFOV, minFOV, maxFOV);
                }
            }
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (boatRb != null && boatRb.linearVelocity.magnitude > movementThreshold)
            {
                float targetXAngle = boatTarget.eulerAngles.y;
                float targetYAngle = defaultYRotation;

                xRotation = Mathf.LerpAngle(xRotation, targetXAngle, Time.deltaTime * returnSpeed);
                yRotation = Mathf.LerpAngle(yRotation, targetYAngle, Time.deltaTime * returnSpeed);
            }
        }

        if (cam != null)
        {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * zoomSmoothness);
        }

        Quaternion rotation = Quaternion.Euler(yRotation, xRotation, 0);
        Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
        Vector3 targetLookAtPosition = boatTarget.position + new Vector3(0, heightOffset, 0);

        Vector3 finalPosition = (rotation * negDistance) + targetLookAtPosition;

        transform.position = finalPosition;
        transform.rotation = rotation;
    }
}