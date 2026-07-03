using UnityEngine;

public class CannonBallBehavior : MonoBehaviour
{
    public float lifetime = 3f;           
    private Vector3 startPosition;
    private Vector3 targetPosition;
    
    private float speed = 15f; 
    private float arcHeight = 5f;
    private float progress = 0f;
    private bool isInitialized = false;

    [Header("Effects")]
    public GameObject impactParticlePrefab; 

    public void Initialize(Vector3 start, Vector3 target, float speedValue, float height)
    {
        startPosition = start;
        targetPosition = target;
        speed = speedValue;
        arcHeight = height;
        progress = 0f;
        isInitialized = true;

        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (!isInitialized) return;

        float distance = Vector3.Distance(startPosition, targetPosition);
        if (distance > 0)
        {
            progress += (speed / distance) * Time.deltaTime;
        }

        if (progress >= 1.0f)
        {
            TriggerImpact(transform.position);
            return;
        }

        Vector3 currentLinearPosition = Vector3.Lerp(startPosition, targetPosition, progress);
        float currentArcY = Mathf.Sin(progress * Mathf.PI) * arcHeight;
        transform.position = new Vector3(currentLinearPosition.x, currentLinearPosition.y + currentArcY, currentLinearPosition.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Cannon")) return;
        
        TriggerImpact(transform.position);
    }

    void TriggerImpact(Vector3 impactPosition)
    {
        if (impactParticlePrefab != null)
        {
            Instantiate(impactParticlePrefab, impactPosition, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}