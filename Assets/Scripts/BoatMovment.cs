using UnityEngine;

public class BoatController : MonoBehaviour
{
    public float engineForce = 20f;
    public float rudderTorque = 5f;
    public float waterDrag = 0.98f;
    public float angularDragWater = 0.95f;
    public float gravity = 9.81f;

    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        float throttle = Input.GetAxis("Vertical");     // W/S
        float rudder = Input.GetAxis("Horizontal");     // A/D

        rb.AddForce(transform.forward * throttle * engineForce);

        float forwardSpeed =
            Vector3.Dot(rb.linearVelocity, transform.forward);

        float steeringPower =
            rudder * rudderTorque * Mathf.Abs(forwardSpeed);

        rb.AddTorque(Vector3.up * steeringPower);

        rb.linearVelocity *= waterDrag;
        rb.angularVelocity *= angularDragWater;
    }
}
