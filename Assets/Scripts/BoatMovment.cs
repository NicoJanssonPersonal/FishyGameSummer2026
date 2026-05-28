using UnityEngine;

public class BoatController : MonoBehaviour
{
    public float thrustForce = 150f;
    public float maxSpeed = 3f;

    public float turnTorque = 500f;
    public float maxAngularVelocity = 2f;

    public float sidewaysGrip = 2.5f;
    public float forwardDrag = 0.5f;

    private Rigidbody rb;
    private float moveInput;
    private float turnInput;

    public Transform rudderTransform;
    public float maxRudderAngle = 35f;
    public float rudderTurnSpeed = 5f;

    public Camera boatCam;
    private Vector3 originalLocalPosition;
    private float smoothSpeedPercentage;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        originalLocalPosition = boatCam.transform.localPosition;

        rb.maxAngularVelocity = maxAngularVelocity;
    }

    void Update()
    {
        moveInput = Input.GetAxis("Vertical");     // W/S or Up/Down
        turnInput = Input.GetAxis("Horizontal");   // A/D or Left/Right

        AnimateVisuals();
        CameraZoomer();
    }

    void FixedUpdate()
    {
        ApplyThrust();
        ApplySteering();
        ApplyWaterResistance();
        float currentSpeed = rb.linearVelocity.magnitude;
        smoothSpeedPercentage = Mathf.Clamp01(currentSpeed / maxSpeed);
    }

    void ApplyThrust()
    {
        float currentSpeed = rb.linearVelocity.magnitude;
        float thrustFactor = Mathf.Clamp01(1f - (currentSpeed / maxSpeed));

        if (moveInput >= 0)
        {
            Vector3 forwardThrust = transform.forward * moveInput * thrustForce * thrustFactor;
            rb.AddForce(forwardThrust, ForceMode.Force);
        }
        else
        {
            Vector3 forwardThrust = (transform.forward * moveInput * thrustForce * thrustFactor) * 0.05f;
            rb.AddForce(forwardThrust, ForceMode.Force);
        }

    }

    void ApplySteering()
    {
        float forwardSpeed = Vector3.Dot(rb.linearVelocity, transform.forward);
        float speedFactor = Mathf.Clamp01(Mathf.Abs(forwardSpeed) / maxSpeed);

        float turnAmount = turnInput * turnTorque * speedFactor;
        rb.AddTorque(transform.up * turnAmount, ForceMode.Force);
    }

    void ApplyWaterResistance()
    {
        Vector3 forwardVelocity = transform.forward * Vector3.Dot(rb.linearVelocity, transform.forward);
        rb.AddForce(-forwardVelocity * forwardDrag, ForceMode.Force);

        Vector3 rightVelocity = transform.right * Vector3.Dot(rb.linearVelocity, transform.right);

        rb.AddForce(-rightVelocity * sidewaysGrip, ForceMode.Force);
    }

    void AnimateVisuals()
    {

        if (rudderTransform != null)
        {
            float directionMultiplier;
            if(moveInput >= 0)
            {
                directionMultiplier = -1;
            }
            else
            {
                directionMultiplier = 1;
            }
            
            float targetYRotation = turnInput * maxRudderAngle * directionMultiplier;

            Quaternion targetRudderRot = Quaternion.Euler(0f, targetYRotation, 0f);
            rudderTransform.localRotation = Quaternion.Lerp(
                rudderTransform.localRotation,
                targetRudderRot,
                Time.deltaTime * rudderTurnSpeed
            );
        }
    }
    void CameraZoomer()
    {
        if (boatCam == null) return;

        float targetFOV = Mathf.Lerp(50, 60, smoothSpeedPercentage);
        boatCam.fieldOfView = Mathf.Lerp(boatCam.fieldOfView, targetFOV, Time.deltaTime * 2f);

        float targetXShift = -turnInput * 1.5f;

        float forwardSpeed = Vector3.Dot(rb.linearVelocity, transform.forward);
        if (Mathf.Abs(forwardSpeed) < 0.1f) targetXShift = 0;

        Vector3 targetPosition = originalLocalPosition;
        targetPosition.x += targetXShift;

        boatCam.transform.localPosition = Vector3.Lerp(
            boatCam.transform.localPosition,
            targetPosition,
            Time.deltaTime * 0.5f
        );
    }

}
