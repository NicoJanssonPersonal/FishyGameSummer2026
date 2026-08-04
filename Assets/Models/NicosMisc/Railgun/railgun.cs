using System.Collections;
using UnityEngine;

public class railgun : MonoBehaviour
{
    public GameObject gunSwivel;
    public GameObject gunTilt;
    public GameObject recoil; // The moving barrel part

    public enum TiltAxis { AxisX, AxisZ }

    [Header("Auto Aim Settings")]
    [SerializeField] private string enemyTag = "enemy";
    [SerializeField] private float swivelSpeed = 10f;
    [SerializeField] private float tiltSpeed = 10f;
    [SerializeField] private float range = 50f;

    [Header("Model Offset Fixes")]
    [Tooltip("If the swivel faces the wrong direction, adjust this offset (e.g., 45, 90, -90, 180).")]
    [SerializeField] private float swivelAngleOffset = 0f;

    [Tooltip("Select which axis your tilt pivot rotates around.")]
    [SerializeField] private TiltAxis tiltAxis = TiltAxis.AxisZ;

    [Tooltip("Invert tilt direction if the gun points up when target goes down.")]
    [SerializeField] private bool invertTilt = false;

    [Header("Clamping")]
    [SerializeField] private float maxTiltUp = 45f;
    [SerializeField] private float maxTiltDown = 10f;

    [Header("Shooting & Raycast Settings")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private LayerMask hitLayers = ~0; // Set to ignore player layer if needed
    [SerializeField] private GameObject beamPrefab; // Cylinder primitive or custom visual mesh
    [SerializeField] private GameObject impactEffectPrefab;
    [SerializeField] private float beamThickness = 0.1f;
    [SerializeField] private float impactEffectLifetime = 2f;

    [Header("Auto Fire Settings")]
    [SerializeField] private bool autoFire = true;
    [SerializeField] private float fireRate = 1f; // Shots per second
    private float nextTimeToFire = 0f;

    private Vector3 originalRecoilPosition;
    private bool isRecoiling = false;
    private Transform currentTarget;

    void Start()
    {
        if (recoil != null)
        {
            originalRecoilPosition = recoil.transform.localPosition;
        }

        InvokeRepeating(nameof(UpdateTarget), 0f, 0.2f);
    }

    void UpdateTarget()
    {
        GameObject closestEnemy = FindClosestEnemy();
        currentTarget = closestEnemy != null ? closestEnemy.transform : null;
    }

    void Update()
    {
        if (currentTarget == null) return;

        AimAtTarget();

        if (autoFire && Time.time >= nextTimeToFire)
        {
            nextTimeToFire = Time.time + (1f / fireRate);
            Shoot();
        }
    }

    void AimAtTarget()
    {
        if (gunSwivel != null)
        {
            Vector3 targetDirSwivel = currentTarget.position - gunSwivel.transform.position;
            targetDirSwivel.y = 0;

            if (targetDirSwivel != Vector3.zero)
            {
                Quaternion rawTargetRotation = Quaternion.LookRotation(targetDirSwivel);
                Quaternion targetSwivelRotation = rawTargetRotation * Quaternion.Euler(0f, -90f, 0f);

                gunSwivel.transform.rotation = Quaternion.Slerp(
                    gunSwivel.transform.rotation,
                    targetSwivelRotation,
                    Time.deltaTime * swivelSpeed
                );
            }
        }

        if (gunTilt != null)
        {
            Vector3 localTargetDir = gunTilt.transform.parent.InverseTransformPoint(currentTarget.position) - gunTilt.transform.localPosition;
            float distance = new Vector2(localTargetDir.x, localTargetDir.z).magnitude;
            float targetPitch = Mathf.Atan2(localTargetDir.y, distance) * Mathf.Rad2Deg;

            targetPitch = Mathf.Clamp(targetPitch, -maxTiltDown, maxTiltUp);

            Quaternion targetTiltRotation = Quaternion.Euler(0f, 0f, targetPitch);

            gunTilt.transform.localRotation = Quaternion.Slerp(
                gunTilt.transform.localRotation,
                targetTiltRotation,
                Time.deltaTime * tiltSpeed
            );
        }
    }

    public void Shoot()
    {
        if (!isRecoiling && recoil != null && firePoint != null)
        {
            StartCoroutine(PlayRecoil());
            FireRaycast();
        }
    }

    void FireRaycast()
    {
        RaycastHit hit;
        Vector3 endPoint = firePoint.position + (firePoint.forward * range);
        bool hitSomething = Physics.Raycast(firePoint.position, firePoint.forward, out hit, range, hitLayers);

        if (hitSomething)
        {
            endPoint = hit.point;

            // Damage logic
            if (hit.collider.CompareTag(enemyTag))
            {
                EnemyHealth enemy = hit.collider.GetComponent<EnemyHealth>();
                if (enemy != null)
                {
                    enemy.TakeDamage(50f);
                }
            }
        }

        if (beamPrefab != null)
        {
            // Pass hitSomething along with firePoint.position, endPoint, and hit
            StartCoroutine(AnimateTracer(firePoint.position, endPoint, hitSomething, hit));
        }
    }

    private IEnumerator AnimateTracer(Vector3 start, Vector3 end, bool hitSomething, RaycastHit hit)
    {
        GameObject tracer = Instantiate(beamPrefab, start, Quaternion.LookRotation(end - start));
        tracer.transform.Rotate(90f, 0f, 0f, Space.Self);
        tracer.transform.localScale = new Vector3(beamThickness, 1f, beamThickness);

        float tracerSpeed = 250f;
        float distance = Vector3.Distance(start, end);
        float travelTime = distance / tracerSpeed;
        float elapsedTime = 0f;

        while (elapsedTime < travelTime)
        {
            if (tracer == null) yield break;

            elapsedTime += Time.deltaTime;
            float t = elapsedTime / travelTime;
            tracer.transform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }

        if (hitSomething && impactEffectPrefab != null)
        {
            GameObject impact = Instantiate(impactEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
            Destroy(impact, impactEffectLifetime);
        }

        Destroy(tracer);
    }

    GameObject FindClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        GameObject closest = null;
        float shortestDistance = range;
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

    private IEnumerator PlayRecoil()
    {
        isRecoiling = true;

        Vector3 targetRecoilPosition = originalRecoilPosition - (Vector3.right * 0.1f);

        float elapsedTime = 0f;
        while (elapsedTime < 0.04f)
        {
            elapsedTime += Time.deltaTime;
            recoil.transform.localPosition = Vector3.Lerp(originalRecoilPosition, targetRecoilPosition, elapsedTime / 0.04f);
            yield return null;
        }
        recoil.transform.localPosition = targetRecoilPosition;

        elapsedTime = 0f;
        while (elapsedTime < 0.3f)
        {
            elapsedTime += Time.deltaTime;
            recoil.transform.localPosition = Vector3.Lerp(targetRecoilPosition, originalRecoilPosition, elapsedTime / 0.3f);
            yield return null;
        }
        recoil.transform.localPosition = originalRecoilPosition;

        isRecoiling = false;
    }
}