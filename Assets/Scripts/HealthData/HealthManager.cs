using UnityEngine;

public class HealthManager : MonoBehaviour
{
    [Header("Health Data")]
    [SerializeField] private HealthData healthData;

    private void Start()
    {
        // Initialize health at the start of the game
        healthData.Initialize();

        // Subscribe to the health change event
        healthData.OnHealthChanged += OnHealthChanged;
    }

    private void OnDestroy()
    {
        // Unsubscribe from the event to prevent memory leaks
        healthData.OnHealthChanged -= OnHealthChanged;
    }

    private void OnHealthChanged(float newHealth)
    {
        Debug.Log($"Health Changed: {newHealth}/{healthData.MaxHealth}");

        if (newHealth <= 0)
        {
            Debug.Log("Player has died!");
            Die();
        }
    }

    private void Die()
    {
        // Handle death logic here
        Debug.Log("Game Over!");
    }

    // Example method to take damage (can be called from other scripts or UI buttons)
    public void DealDamage(float amount)
    {
        healthData.TakeDamage(amount);
    }

    // Example method to heal (can be called from other scripts or UI buttons)
    public void RestoreHealth(float amount)
    {
        healthData.Heal(amount);
    }
}