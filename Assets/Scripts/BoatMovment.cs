using UnityEngine;

public class BoatController : MonoBehaviour
{
    [Header("Motor Settings")]
    public float thrustForce = 150f;
    public float maxSpeed = 3f;
    public float reverseSpeedDebuff = 0.05f;

    public float turnTorque = 500f;
   

    public float sidewaysGrip = 2.5f;
    public float forwardDrag = 0.5f;

    [Header("Steering Limits")]
    public float maxTurnTorque = 50f; // Caps the raw force applied
    public float minTurningRadius = 10f; // The tightest circle the boat can make
    public float maxAngularVelocity = 2f; // Caps how fast the boat can spin (rad/s)

    private Rigidbody rb;
    private float moveInput;
    private float turnInput;

    [Header("Rudder Settings (Visual)")]
    public Transform rudderTransform;
    public float maxRudderAngle = 35f;
    public float rudderTurnSpeed = 5f;

    [Header("Camera Settings")]
    public Camera boatCam;
    public float maxCamFOV = 80f;
    public float minCamFOV = 70f;
    private Vector3 originalLocalPosition;
    private float smoothSpeedPercentage;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        originalLocalPosition = boatCam.transform.localPosition;

        
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
            Vector3 forwardThrust = (transform.forward * moveInput * thrustForce * thrustFactor) * reverseSpeedDebuff;
            rb.AddForce(forwardThrust, ForceMode.Force);
        }

    }



    void ApplySteering()
    {
        // 1. Calculate forward speed and speed factor
        float forwardSpeed = Vector3.Dot(rb.linearVelocity, transform.forward);
        float speedFactor = Mathf.Clamp01(Mathf.Abs(forwardSpeed) / maxSpeed);

        // 2. Calculate and clamp the torque (Max Turn Thrust)
        float turnAmount = turnInput * turnTorque * speedFactor;
        turnAmount = Mathf.Clamp(turnAmount, -maxTurnTorque, maxTurnTorque);

        // 3. Apply the torque
        rb.AddTorque(transform.up * turnAmount, ForceMode.Force);

        // 4. Enforce Max Turn Speed & Max Turn Radius
        LimitRotationSpeed(forwardSpeed);
    }

    void LimitRotationSpeed(float forwardSpeed)
    {
        Vector3 currentAngularVelocity = rb.angularVelocity;

        float allowedAngularSpeed = maxAngularVelocity;

        // We only calculate this if the boat is actually moving to avoid dividing by zero
        if (Mathf.Abs(forwardSpeed) > 0.2f && minTurningRadius > 0f)
        {
            float radiusLimitedAngularSpeed = Mathf.Abs(forwardSpeed) / minTurningRadius;
            // Keep whichever limit is stricter (smaller)
            allowedAngularSpeed = Mathf.Min(allowedAngularSpeed, radiusLimitedAngularSpeed);
        }

        // Clamp the Y-axis angular velocity (the steering axis)
        float clampedYRotation = Mathf.Clamp(currentAngularVelocity.y, -allowedAngularSpeed, allowedAngularSpeed);

        // Apply the clamped velocity back to the rigidbody
        rb.angularVelocity = new Vector3(currentAngularVelocity.x, clampedYRotation, currentAngularVelocity.z);
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

        float targetFOV = Mathf.Lerp(minCamFOV, maxCamFOV, smoothSpeedPercentage);
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
