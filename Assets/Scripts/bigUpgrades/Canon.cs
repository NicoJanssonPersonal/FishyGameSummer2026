using UnityEngine;

public class canonscript : MonoBehaviour
{
    public GameObject CanonBall;
    public GameObject shootPoint;
    public GameObject barrel; 
    
    [Header("Arc Settings")]
    public float projectileSpeed = 13f;   
    public float heightPercentage = 0.1f; 

    [Header("Smoothness Settings")]
    [Tooltip("Higher numbers make the cannon rotate faster to face targets.")]
    public float rotationSpeed = 5f; 

    private GameObject[] enemies; 
    private Quaternion targetBaseRotation;
    private Quaternion targetBarrelRotation;

    void Start()
    {
        targetBaseRotation = transform.rotation;
        if (barrel != null) targetBarrelRotation = barrel.transform.localRotation;
    }

    void Update()
    {
        CalculateAimRotations();

        transform.rotation = Quaternion.Slerp(transform.rotation, targetBaseRotation, rotationSpeed * Time.deltaTime);
        
        if (barrel != null)
        {
            barrel.transform.localRotation = Quaternion.Slerp(barrel.transform.localRotation, targetBarrelRotation, rotationSpeed * Time.deltaTime);
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            ShootArc();
        }
    }

    void CalculateAimRotations()
    {
        enemies = GameObject.FindGameObjectsWithTag("enemy");
        if (enemies.Length == 0) return;

        GameObject closestEnemy = FindClosestEnemy();
        if (closestEnemy != null)
        {
            Vector3 targetDirection = transform.position - closestEnemy.transform.position;
            targetDirection.y = 0; 

            if (targetDirection != Vector3.zero)
            {
                targetBaseRotation = Quaternion.LookRotation(targetDirection);
            }

            if (barrel != null)
            {
                float targetDistance = Vector3.Distance(shootPoint.transform.position, closestEnemy.transform.position);
                float calculatedArcHeight = Mathf.Max(targetDistance * heightPercentage, 2f);

                float launchAngleRad = Mathf.Atan((calculatedArcHeight * Mathf.PI) / targetDistance);
                float launchAngleDeg = launchAngleRad * Mathf.Rad2Deg;

                targetBarrelRotation = Quaternion.Euler(launchAngleDeg, 0f, 0f);
            }
        }
    }

    void ShootArc()
    {
        GameObject closestEnemy = FindClosestEnemy();
        
        if (closestEnemy == null || CanonBall == null || shootPoint == null) return;

        GameObject spawnedBall = Instantiate(CanonBall, shootPoint.transform.position, Quaternion.identity);
        CannonBallBehavior ballBehavior = spawnedBall.GetComponent<CannonBallBehavior>();

        if (ballBehavior != null)
        {
            float targetDistance = Vector3.Distance(shootPoint.transform.position, closestEnemy.transform.position);
            float calculatedArcHeight = Mathf.Max(targetDistance * heightPercentage, 2f);

            ballBehavior.Initialize(shootPoint.transform.position, closestEnemy.transform.position, projectileSpeed, calculatedArcHeight);
        }
    }

    GameObject FindClosestEnemy()
    {
        GameObject closest = null;
        float shortestDistance = Mathf.Infinity;
        Vector3 currentPosition = transform.position;

        foreach (GameObject enemy in enemies)
        {
            float distanceToEnemy = Vector3.Distance(enemy.transform.position, currentPosition);
            if (distanceToEnemy < shortestDistance)
            {
                shortestDistance = distanceToEnemy;
                closest = enemy;
            }
        }
        return closest;
    }
}