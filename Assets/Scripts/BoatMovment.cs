using UnityEngine;

public class BoatController : MonoBehaviour
{
    public float thrustForce = 150f;
    public float maxSpeed = 1f;

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

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        rb.maxAngularVelocity = maxAngularVelocity;
    }

    void Update()
    {
        moveInput = Input.GetAxis("Vertical");     // W/S or Up/Down
        turnInput = Input.GetAxis("Horizontal");   // A/D or Left/Right

        AnimateVisuals();
    }

    void FixedUpdate()
    {
        ApplyThrust();
        ApplySteering();
        ApplyWaterResistance();
    }

    void ApplyThrust()
    {
        if (rb.linearVelocity.magnitude < maxSpeed)
        {
            Vector3 forwardThrust = transform.forward * moveInput * thrustForce;
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
            float directionMultiplier = -1;
            float targetYRotation = turnInput * maxRudderAngle * directionMultiplier;

            Quaternion targetRudderRot = Quaternion.Euler(0f, targetYRotation, 0f);
            rudderTransform.localRotation = Quaternion.Lerp(
                rudderTransform.localRotation,
                targetRudderRot,
                Time.deltaTime * rudderTurnSpeed
            );
        }
    }
}
