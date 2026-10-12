using UnityEngine;
using System.Collections;

public class SharkController : MonoBehaviour
{
    public Transform target;

    [Header("Movement Speeds")]
    public float normalSpeed = 5f;
    public float wanderSpeed = 2.5f;
    public float rotationSpeed = 10f;

    [Header("Detection & Combat")]
    public float detectionRange = 15f;
    public float attackRange = 2f;
    public float attackCooldown = 1.5f;

    [Header("Circle Idle Settings")]
    public float defaultCircleRadius = 10f;
    public float baseCircleSpeed = 0.5f;

    [Header("Randomization Settings")]
    [Tooltip("Vary the size of the pattern per shark")]
    public float sizeVariance = 3f;
    [Tooltip("Vary the swimming speed per shark")]
    public float speedVariance = 0.15f;
    [Tooltip("Randomly rotate the circular path angle")]
    public bool randomizeRotation = true;

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
    private float circleTimer = 0f;

    public float swimRadius;
    private Quaternion patternRotation = Quaternion.identity;

    private float actualSpeed;
    private float actualRadius;

    void Start()
    {
        animator = GetComponent<Animator>();
        currentSpeed = normalSpeed;
        spawnPoint = transform.position;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
        }

        ApplyRandomVariation();
        FindPlayerTarget();
    }

    void ApplyRandomVariation()
    {
        circleTimer = Random.Range(0f, Mathf.PI * 2f);

        actualSpeed = baseCircleSpeed + Random.Range(-speedVariance, speedVariance);
        actualRadius = defaultCircleRadius + Random.Range(-sizeVariance, sizeVariance);

        if (swimRadius <= 0f)
        {
            swimRadius = actualRadius;
        }

        if (randomizeRotation)
        {
            patternRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
        }
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
            SwimCircle();
        }
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

    void SwimCircle()
    {
        float speedMultiplier = isSlowed ? slowMultiplier : 1f;

        circleTimer += Time.deltaTime * actualSpeed * speedMultiplier;

        float x = Mathf.Cos(circleTimer) * swimRadius;
        float z = Mathf.Sin(circleTimer) * swimRadius;

        Vector3 targetPosition = spawnPoint + (patternRotation * new Vector3(x, 0, z));

        // Move directly toward the target point along the circle
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, wanderSpeed * speedMultiplier * Time.deltaTime);

        // Rotate to face the direction of movement along the circle
        Vector3 moveDirection = (targetPosition - transform.position);
        moveDirection.y = 0;

        if (moveDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(-moveDirection.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
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
        Gizmos.color = Color.magenta;
        Vector3 center = Application.isPlaying ? spawnPoint : transform.position;
        float radius = Application.isPlaying ? actualRadius : defaultCircleRadius;
        Quaternion rot = Application.isPlaying ? patternRotation : Quaternion.identity;

        Vector3 lastPos = center + (rot * new Vector3(radius, 0, 0));
        int steps = 50;
        for (int i = 1; i <= steps; i++)
        {
            float t = (i / (float)steps) * Mathf.PI * 2f;
            float x = Mathf.Cos(t) * radius;
            float z = Mathf.Sin(t) * radius;
            Vector3 currentPos = center + (rot * new Vector3(x, 0, z));

            Gizmos.DrawLine(lastPos, currentPos);
            lastPos = currentPos;
        }
    }
}