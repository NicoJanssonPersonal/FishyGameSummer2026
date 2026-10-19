using UnityEngine;
using System.Collections;

public class SharkController : MonoBehaviour
{
    public Transform target;

    [Header("Movement Speeds")]
    public float normalSpeed = 5f;
    public float wanderSpeed = 2.5f;
    public float rotationSpeed = 720f;

    [Header("Detection & Combat")]
    public float detectionRange = 15f;
    public float attackRange = 2f;
    public float attackCooldown = 1.5f;

    [Header("Health & Damage")]
    public int maxHits = 5;
    public float slowDuration = 3f;
    [Range(0f, 1f)]
    public float slowMultiplier = 0.5f;

    [Header("Terrain Avoidance")]
    public float terrainIgnoreTimer = 0f;
    private bool avoidingTerrain = false;

    public GameObject bloodPuddlePredaf;
    public GameObject sharkExplodePrefab;

    private Animator animator;
    private float currentSpeed;
    private float nextAttackTime = 0f;
    private int hitCount = 0;
    private bool isSlowed = false;

    private Vector3 spawnPoint;

    public float circleRadius = 10;

    void Start()
    {
        if (Random.value < 0.33f) // 33 procent chans att int denhär hajen spawnar
        {
            Destroy(gameObject);
        }

        animator = GetComponent<Animator>();
        currentSpeed = normalSpeed;
        spawnPoint = transform.position;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
        }

        FindPlayerTarget();
    }

    void Update()
    {
        if (avoidingTerrain)
        {
            terrainIgnoreTimer -= Time.deltaTime;
            transform.Translate(Vector3.back * wanderSpeed * Time.deltaTime);

            if (terrainIgnoreTimer <= 0f)
            {
                avoidingTerrain = false;
            }
            return;
        }

        if (target == null)
        {
            FindPlayerTarget();
        }

        float distanceToTarget = target != null ? Vector3.Distance(transform.position, target.position) : float.MaxValue;

        if (distanceToTarget <= detectionRange)
        {
            if (distanceToTarget > attackRange)
            {
                SwimTowardsTarget();
            }
            else
            {
                TryChomp();
            }
        }
        else
        {
            SwimCircle(circleRadius);
        }
    }
    private float currentAngle = 0f;
    private void SwimCircle(float radius)
    {
        if (radius <= 0.0001f) return;

        float angularSpeed = currentSpeed / radius;

        currentAngle += angularSpeed * Time.deltaTime;

        float x = spawnPoint.x + (Mathf.Cos(currentAngle) * radius);
        float z = spawnPoint.z + (Mathf.Sin(currentAngle) * radius);
        Vector3 newPosition = new Vector3(x, spawnPoint.y, z);

        Vector3 moveDirection = newPosition - transform.position;

        if (moveDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(-moveDirection);
        }

        transform.position = newPosition;
    }
    void FindPlayerTarget()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            target = playerObj.transform;
        }
    }

    void SwimTowardsTarget()
    {
        Vector3 direction = (target.position - transform.position);
        direction.y = 0;
        direction.Normalize();

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(-direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        transform.Translate(Vector3.back * currentSpeed * Time.deltaTime);
    }

    void TryChomp()
    {
        Vector3 direction = (target.position - transform.position);
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(-direction), rotationSpeed * Time.deltaTime);
        }

        if (Time.time >= nextAttackTime)
        {
            if (animator != null) animator.SetTrigger("CHOMP");
            GlobalStats.currentHealth -= 10;
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("CannonBall"))
        {
            TakeHit();
        }
        else if (other.CompareTag("Terrain"))
        {
            AvoidTerrain();
        }
    }

    void AvoidTerrain()
    {
        avoidingTerrain = true;
        terrainIgnoreTimer = 2.5f;

        transform.Rotate(0f, 180f, 0f);
    }

    void TakeHit()
    {
        hitCount++;

        if (hitCount >= maxHits)
        {
            GameObject blood = Instantiate(bloodPuddlePredaf, transform.position, transform.rotation);
            GameObject explosion = Instantiate(sharkExplodePrefab, transform.position, transform.rotation);

            Destroy(blood, 3f);
            Destroy(explosion, 3f);
            Destroy(gameObject);
            return;
        }

        if (!isSlowed)
        {
            StartCoroutine(SlowDownRoutine());
        }
    }

    public static void SharkDeath(SharkController shark)
    {
        if (shark == null) return;

        GameObject blood = Instantiate(shark.bloodPuddlePredaf, shark.transform.position, shark.transform.rotation);
        GameObject explosion = Instantiate(shark.sharkExplodePrefab, shark.transform.position, shark.transform.rotation);

        Destroy(blood, 3f);
        Destroy(explosion, 3f);
        Destroy(shark.gameObject);
    }

    IEnumerator SlowDownRoutine()
    {
        isSlowed = true;
        currentSpeed = normalSpeed * slowMultiplier;

        yield return new WaitForSeconds(slowDuration);

        currentSpeed = normalSpeed;
        isSlowed = false;
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Vector3 center = Application.isPlaying ? spawnPoint : transform.position;

        float drawRadius = circleRadius;

        int segments = 36;
        Vector3 previousPoint = Vector3.zero;

        for (int i = 0; i <= segments; i++)
        {
            float angle = (i * 360f / segments) * Mathf.Deg2Rad;

            // Calculate X and Z for this point on the circle
            float x = center.x + (Mathf.Cos(angle) * drawRadius);
            float z = center.z + (Mathf.Sin(angle) * drawRadius);
            Vector3 currentPoint = new Vector3(x, center.y, z);

            if (i > 0)
            {
                Gizmos.DrawLine(previousPoint, currentPoint);
            }

            previousPoint = currentPoint;
        }
    }
}