using UnityEngine;

public class BoatController : MonoBehaviour
{
    [Header("Motor Settings")]
    public float thrustForce, maxSpeed, reverseSpeedDebuff;
    public float sidewaysGrip = 2.5f; 
    public float forwardDrag = 0.5f; // Water resistance

    [Header("Steering Limits")]
    public float minTurningRadius, maxAngularVelocity, turnTorque, maxTurnTorque;
    private Rigidbody rb;
    private float moveInput;
    private float turnInput;

    [Header("Nitro / Boost Tank Settings")]
    [Tooltip("Maximum Nitro Capacity (seconds of continuous boost)")]
    public float nitroRegenRate = 15f; 
    [Tooltip("Delay in seconds before Nitro starts regenerating after release")]
    public float regenDelay = 0.5f;
    private float regenTimer = 0f;

    // Internal state
    public bool isBoosting { get; private set; }
    private float activeBoostFactor = 0f;

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
    [SerializeField] private Transform visualMesh; 
    [SerializeField] private float rollAngle = 15f;  
    [SerializeField] private float pitchAngle = 10f; 
    [SerializeField] private float tiltSpeed = 5f;  

    private Vector3 lastVelocity;
    private Quaternion meshInitialRotation;
    private float targetY;

    void Start()
    {
        GlobalStats.currentNitro = GlobalStats.maxNitro; // Fill nitro tank on spawn

        if (visualMesh != null)
        {
            meshInitialRotation = visualMesh.localRotation;
        }
        GlobalStats.LoadStats();
        getStatsFromGlobalStats();

        rb = GetComponent<Rigidbody>();

        if (boatCam != null)
        {
            originalLocalPosition = boatCam.transform.localPosition;
        }
        targetY = transform.position.y;
    }

    void Update()
    {
        moveInput = Input.GetAxis("Vertical");     // W/S or Up/Down
        turnInput = Input.GetAxis("Horizontal");   // A/D or Left/Right

        HandleNitroInput();
        AnimateVisuals();
        CameraZoomer();
    }

    void HandleNitroInput()
    {
        bool wantsToBoost = Input.GetKey(KeyCode.LeftShift) && GlobalStats.currentNitro > 0f && moveInput >= 0f;

        if (wantsToBoost)
        {
            isBoosting = true;
            regenTimer = regenDelay;

            GlobalStats.currentNitro -= GlobalStats.nitroDrainRate * Time.deltaTime;
            GlobalStats.currentNitro = Mathf.Max(0f, GlobalStats.currentNitro);
        }
        else
        {
            isBoosting = false;

            if (regenTimer > 0f)
            {
                regenTimer -= Time.deltaTime;
            }
            else if (GlobalStats.currentNitro < GlobalStats.maxNitro)
            {
                GlobalStats.currentNitro += nitroRegenRate * Time.deltaTime;
                GlobalStats.currentNitro = Mathf.Min(GlobalStats.maxNitro, GlobalStats.currentNitro);
            }
        }
    }

    void getStatsFromGlobalStats()
    {
        maxTurnTorque = GlobalStats.maxTurnTorque; 
        turnTorque = GlobalStats.turnTorque; 
        minTurningRadius = GlobalStats.minTurningRadius; 
        maxAngularVelocity = GlobalStats.maxAngularVelocity;
        thrustForce = GlobalStats.thrustForce; 
        maxSpeed = GlobalStats.maxSpeed; 
        reverseSpeedDebuff = GlobalStats.reverseSpeedDebuff;
        rudderTurnSpeed = GlobalStats.rudderTurnSpeed;
    }

    void FixedUpdate()
    {
        getStatsFromGlobalStats();

        // Smooth transitions into and out of boost state
        float targetBoostFactor = isBoosting ? (GlobalStats.nitroSpeedMultiplier - 1.0f) : 0f;
        activeBoostFactor = Mathf.Lerp(activeBoostFactor, targetBoostFactor, Time.fixedDeltaTime * 6f);

        if (!CardManager.isUpgrading)
        {
            ApplyThrust();
            ApplySteering();
            ApplyVisualTilt();
        }
        
        ApplyWaterResistance();

        float effectiveMaxSpeed = maxSpeed * (1f + activeBoostFactor);
        float currentSpeed = rb.linearVelocity.magnitude;
        smoothSpeedPercentage = Mathf.Clamp01(currentSpeed / effectiveMaxSpeed);
    }

    void ApplyThrust()
    {
        float currentSpeed = rb.linearVelocity.magnitude;

        // Calculate dynamic maximum top speed ceiling while boosting
        float effectiveMaxSpeed = maxSpeed * (1f + activeBoostFactor);
        float dynamicThrustForce = thrustForce * (isBoosting ? GlobalStats.nitroThrustMultiplier : 1.0f);

        float thrustFactor = (effectiveMaxSpeed > 0f) ? Mathf.Clamp01(1f - (currentSpeed / effectiveMaxSpeed)) : 0f;

        if (moveInput >= 0)
        {
            // Continuous forward force while boosting
            Vector3 forwardThrust = transform.forward * (moveInput > 0 ? moveInput : (isBoosting ? 1f : 0f)) * dynamicThrustForce * thrustFactor;
            rb.AddForce(forwardThrust, ForceMode.Force);
        }
        else
        {
            Vector3 forwardThrust = (transform.forward * moveInput * thrustForce * thrustFactor) * reverseSpeedDebuff;
            rb.AddForce(forwardThrust, ForceMode.Force);
        }
    }

    void ApplyWaterResistance()
    {
        Vector3 forwardVelocity = transform.forward * Vector3.Dot(rb.linearVelocity, transform.forward);
        Vector3 rightVelocity = transform.right * Vector3.Dot(rb.linearVelocity, transform.right);

        // Dynamically lower drag while nitro is engaged so speed builds up smooth and fast
        float dragFactor = Mathf.Lerp(1.0f, 0.4f, activeBoostFactor / (GlobalStats.nitroSpeedMultiplier - 1.0f + 0.0001f));
        
        rb.AddForce(-forwardVelocity * (forwardDrag * dragFactor), ForceMode.Force);
        rb.AddForce(-rightVelocity * sidewaysGrip, ForceMode.Force);
    }

    void ApplySteering()
    {
        float forwardSpeed = Vector3.Dot(rb.linearVelocity, transform.forward);
        float effectiveMaxSpeed = maxSpeed * (1f + activeBoostFactor);
        float speedFactor = (effectiveMaxSpeed > 0f) ? Mathf.Clamp01(Mathf.Abs(forwardSpeed) / effectiveMaxSpeed) : 0f;

        float currentTurnInput = turnInput;
        if (forwardSpeed < 0f)
        {
            currentTurnInput = -turnInput;
        }

        if (Mathf.Abs(turnInput) < 0.05f)
        {
            Vector3 angVel = rb.angularVelocity;
            angVel.y = Mathf.Lerp(angVel.y, 0f, Time.fixedDeltaTime * 10f);
            rb.angularVelocity = angVel;
        }
        else
        {
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

    void ApplyVisualTilt()
    {
        if (visualMesh == null) return;

        float effectiveMaxSpeed = maxSpeed * (1f + activeBoostFactor);
        float forwardSpeed = Vector3.Dot(rb.linearVelocity, transform.forward);
        float normalizedSpeed = (effectiveMaxSpeed > 0f) ? Mathf.Clamp01(Mathf.Abs(forwardSpeed) / effectiveMaxSpeed) : 0f;

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

    void AnimateVisuals()
    {
        if (rudderTransform != null)
        {
            float directionMultiplier = -1f;

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

        // FOV expands wider when nitro is burning
        float extraNitroFOV = isBoosting ? 10f : 0f;
        float targetFOV = Mathf.Lerp(minCamFOV, maxCamFOV + extraNitroFOV, smoothSpeedPercentage);
        boatCam.fieldOfView = Mathf.Lerp(boatCam.fieldOfView, targetFOV, Time.deltaTime * 3f);

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