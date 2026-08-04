using UnityEngine;

public class BoatController : MonoBehaviour
{
    [Header("Motor Settings")]
    public float thrustForce, maxSpeed, reverseSpeedDebuff;
    public float sidewaysGrip = 2.5f; // ??? (De s�ger PANG om den e f� l�g)
    public float forwardDrag = 0.5f; // Water resistance
    [Header("Steering Limits")]
    public float minTurningRadius, maxAngularVelocity, turnTorque, maxTurnTorque;
    private Rigidbody rb;
    private float moveInput;
    private float turnInput;

    [Header("Rudder Settings (Visual)")]
    public Transform rudderTransform;
    public float rudderTurnSpeed = GlobalStats.rudderTurnSpeed;

    [Header("Camera Settings")]
    public Camera boatCam;
    public float maxCamFOV = 80f;
    public float minCamFOV = 70f;
    private Vector3 originalLocalPosition;
    private float smoothSpeedPercentage;
    private float currentXRotation = 40f;
    public float reverseTiltAngle = 45f;

    [Header("Visual Tilt Settings")]
    [SerializeField] private Transform visualMesh; // Drag your child mesh object here
    [SerializeField] private float rollAngle = 15f;  // Maximum banking angle when turning
    [SerializeField] private float pitchAngle = 10f; // Maximum nose-up/down angle when accelerating
    [SerializeField] private float tiltSpeed = 5f;  // How fast the boat tilts (smoothness)

    private Vector3 lastVelocity;
    private Quaternion meshInitialRotation;
    private float targetY;

    void Start()
    {
        if (visualMesh != null)
        {
            meshInitialRotation = visualMesh.localRotation;
        }
        GlobalStats.LoadStats();
        getStatsFromGlobalStats();

        rb = GetComponent<Rigidbody>();

        originalLocalPosition = boatCam.transform.localPosition;
        targetY = transform.position.y;
    }

    void Update()
    {
        moveInput = Input.GetAxis("Vertical");     // W/S or Up/Down
        turnInput = Input.GetAxis("Horizontal");   // A/D or Left/Right

        AnimateVisuals();
        CameraZoomer();
    }
    void getStatsFromGlobalStats()
    {
        maxTurnTorque = GlobalStats.maxTurnTorque; // Caps the raw force applied
        turnTorque = GlobalStats.turnTorque; // Turn acceleration
        minTurningRadius = GlobalStats.minTurningRadius; // The tightest circle the boat can make
        maxAngularVelocity = GlobalStats.maxAngularVelocity;
        thrustForce = GlobalStats.thrustForce; // Acceleration
        maxSpeed = GlobalStats.maxSpeed; // Top speed
        reverseSpeedDebuff = GlobalStats.reverseSpeedDebuff;
        reverseSpeedDebuff = GlobalStats.reverseSpeedDebuff;
        rudderTurnSpeed = GlobalStats.rudderTurnSpeed;
    }

    void FixedUpdate()
    {
        getStatsFromGlobalStats();
        if (!CardManager.isUpgrading)
        {
            ApplyThrust();
            ApplySteering();
            ApplyVisualTilt();
        }
        ApplyWaterResistance();
        float currentSpeed = rb.linearVelocity.magnitude;
        smoothSpeedPercentage = Mathf.Clamp01(currentSpeed / maxSpeed);
    }
    void ApplyVisualTilt()
    {
        if (visualMesh == null) return;

        float forwardSpeed = Vector3.Dot(rb.linearVelocity, transform.forward);
        float normalizedSpeed = (maxSpeed > 0f) ? Mathf.Clamp01(Mathf.Abs(forwardSpeed) / maxSpeed) : 0f;

        float targetPitch = moveInput * pitchAngle * normalizedSpeed;
        float targetRoll = turnInput * rollAngle * normalizedSpeed;

        Quaternion tiltRotation = Quaternion.Euler(targetPitch, 0f, targetRoll);
        Quaternion targetRotation = meshInitialRotation * tiltRotation;

        visualMesh.localRotation = Quaternion.Slerp(
            visualMesh.localRotation,
            targetRotation,
            Time.fixedDeltaTime * tiltSpeed
        );
    }

    void ApplyThrust()
    {
        float currentSpeed = rb.linearVelocity.magnitude;

        float thrustFactor = (maxSpeed > 0f) ? Mathf.Clamp01(1f - (currentSpeed / maxSpeed)) : 0f;

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
        float forwardSpeed = Vector3.Dot(rb.linearVelocity, transform.forward);
        float speedFactor = (maxSpeed > 0f) ? Mathf.Clamp01(Mathf.Abs(forwardSpeed) / maxSpeed) : 0f;

        float currentTurnInput = turnInput;
        if (forwardSpeed < 0f)
        {
            currentTurnInput = -turnInput;
        }

        // --- ADDED: Active Straightening / Steering Damping ---
        if (Mathf.Abs(turnInput) < 0.05f)
        {
            // When not turning, rapidly bleed off Y angular velocity so the boat travels straight
            Vector3 angVel = rb.angularVelocity;
            angVel.y = Mathf.Lerp(angVel.y, 0f, Time.fixedDeltaTime * 10f);
            rb.angularVelocity = angVel;
        }
        else
        {
            // Calculate and apply turn torque only when actively pressing turn keys
            float turnAmount = currentTurnInput * turnTorque * speedFactor;
            turnAmount = Mathf.Clamp(turnAmount, -maxTurnTorque, maxTurnTorque);

            if (!float.IsNaN(turnAmount))
            {
                rb.AddTorque(transform.up * turnAmount, ForceMode.Force);
            }
        }

        LimitRotationSpeed(forwardSpeed);
    }

    void LimitRotationSpeed(float forwardSpeed)
    {
        Vector3 currentAngularVelocity = rb.angularVelocity;

        float allowedAngularSpeed = maxAngularVelocity;

        if (Mathf.Abs(forwardSpeed) > 0.2f && minTurningRadius > 0f)
        {
            float radiusLimitedAngularSpeed = Mathf.Abs(forwardSpeed) / minTurningRadius;
            allowedAngularSpeed = Mathf.Min(allowedAngularSpeed, radiusLimitedAngularSpeed);
        }

        float clampedYRotation = Mathf.Clamp(currentAngularVelocity.y, -allowedAngularSpeed, allowedAngularSpeed);

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

            float forwardSpeed = Vector3.Dot(rb.linearVelocity, transform.forward);
            float directionMultiplier;
            if (forwardSpeed >= 0f)
            {
                directionMultiplier = -1f;
            }
            else
            {
                directionMultiplier = -1f;
            }

            float targetYRotation = turnInput * minTurningRadius * directionMultiplier;

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

        float targetFOV = Mathf.Lerp(boatCam.fieldOfView, boatCam.fieldOfView * 2f, smoothSpeedPercentage);
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


        float targetXRotation = 40f;


        if (forwardSpeed < -0.2f)
        {
            targetXRotation = reverseTiltAngle;
        }


        float maxChangePerSecond = Mathf.Abs(reverseTiltAngle) / 10f;
        currentXRotation = Mathf.MoveTowards(currentXRotation, targetXRotation, maxChangePerSecond * Time.deltaTime);

        Vector3 currentAngles = boatCam.transform.localEulerAngles;
        boatCam.transform.localEulerAngles = new Vector3(currentXRotation, currentAngles.y, currentAngles.z);
    }

}
