using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        if (TryGetComponent<SharkController>(out SharkController shark))
        {
            SharkController.SharkDeath(shark);
            return; 
        }

        // 2. Default death logic for non-shark enemies
        Destroy(gameObject);
    }
}