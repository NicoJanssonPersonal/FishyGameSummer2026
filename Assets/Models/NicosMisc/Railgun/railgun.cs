using System.Collections; // Required for Coroutines
using UnityEngine;

public class railgun : MonoBehaviour
{
    public GameObject gunSwivel;
    public GameObject gunTilt;
    public GameObject recoil; // The moving barrel part

    [Header("Movement Settings")]
    [SerializeField] private float swivelSpeed = 100f;
    [SerializeField] private float tiltSpeed = 75f;

    [Header("Clamping (Optional)")]
    [SerializeField] private float minTilt = -10f;
    [SerializeField] private float maxTilt = 45f;

    [Header("Shooting Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float bulletForce = 100f;
    [SerializeField] private float destroyDelay = 5f;
    private float currentTiltRotation = 0f;
    private Vector3 originalRecoilPosition;
    private bool isRecoiling = false;

    void Start()
    {
        // Store the starting position of the recoil piece so we know where to return to
        if (recoil != null)
        {
            originalRecoilPosition = recoil.transform.localPosition;
        }
    }

    void Update()
    {
        devMovement();
    }

    void devMovement()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float swivelAmount = horizontalInput * swivelSpeed * Time.deltaTime;

        if (gunSwivel != null)
        {
            gunSwivel.transform.Rotate(Vector3.up, swivelAmount);
        }

        float verticalInput = Input.GetAxis("Vertical");

        if (gunTilt != null)
        {
            currentTiltRotation -= verticalInput * tiltSpeed * Time.deltaTime;
            currentTiltRotation = Mathf.Clamp(currentTiltRotation, minTilt, maxTilt);

            // Note: Changed back to X axis for standard tilt, change to Z if your model requires it!
            gunTilt.transform.localRotation = Quaternion.Euler(0f, 0f, currentTiltRotation);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Shoot();
        }
    }

    public void Shoot()
    {
        if (!isRecoiling && recoil != null)
        {
            StartCoroutine(PlayRecoil());
            spawnBullet();
        }
    }
    void spawnBullet()
    {
        // 1. Spawn the bullet prefab at firePoint position and rotation
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        // 2. Get the Rigidbody attached to the instantiated bullet
        Rigidbody rb = bullet.GetComponent<Rigidbody>();

        // 3. Add impulse force pushing forward relative to firePoint direction
        if (rb != null)
        {
            rb.AddForce(firePoint.forward * bulletForce, ForceMode.Impulse);
        }

        // 4. Clean up bullet to prevent memory bloat
        Destroy(bullet, destroyDelay);
    }

    private IEnumerator PlayRecoil()
    {
        isRecoiling = true;

        Vector3 targetRecoilPosition = originalRecoilPosition - (Vector3.right * 0.1f);

        float elapsedTime = 0f;
        while (elapsedTime < 0.04)
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