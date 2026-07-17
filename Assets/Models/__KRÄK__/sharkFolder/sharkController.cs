using UnityEngine;
using System.Collections;

public class SharkController : MonoBehaviour
{
    public Transform target;
    public float normalSpeed = 5f;
    public float rotationSpeed = 10f;

    public float attackRange = 2f;
    public float attackCooldown = 1.5f;

    public int maxHits = 5;
    public float slowDuration = 3f;
    [Range(0f, 1f)]
    public float slowMultiplier = 0.5f;

    private Animator animator;
    private float currentSpeed;
    private float nextAttackTime = 0f;
    private int hitCount = 0;
    private bool isSlowed = false;
    public GameObject bloodPuddlePredaf;
    public GameObject sharkExplodePrefab;


    void Start()
    {
        animator = GetComponent<Animator>();
        currentSpeed = normalSpeed;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
        }

        FindPlayerTarget();
    }

    void Update()
    {
        if (target == null)
        {
            FindPlayerTarget();
            if (target == null) return;
        }

        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        if (distanceToTarget > attackRange)
        {
            SwimTowardsTarget();
        }
        else
        {
            TryChomp();
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
            animator.SetTrigger("CHOMP");
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
    }

    void TakeHit()
    {
        hitCount++;
        Debug.Log($"Shark hit! Total hits: {hitCount}/{maxHits}");

        if (hitCount >= maxHits)
        {
            Debug.Log("Shark defeated!");
            GameObject blood = Instantiate(bloodPuddlePredaf, transform.position, transform.rotation);
            GameObject explosion = Instantiate(sharkExplodePrefab, transform.position, transform.rotation);

            // 2. Tell Unity to automatically delete these spawned effects after 3 seconds
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

    IEnumerator SlowDownRoutine()
    {
        isSlowed = true;
        currentSpeed = normalSpeed * slowMultiplier;

        yield return new WaitForSeconds(slowDuration);

        currentSpeed = normalSpeed;
        isSlowed = false;
    }
}