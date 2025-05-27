using UnityEngine;

[CreateAssetMenu(fileName = "HealthData", menuName = "ScriptableObjects/HealthData", order = 1)]
public class HealthData : ScriptableObject
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    // Property to get or set current health with validation
    public float CurrentHealth
    {
        get => currentHealth;
        set
        {
            currentHealth = Mathf.Clamp(value, 0f, MaxHealth);
            OnHealthChanged?.Invoke(currentHealth); // Notify listeners of health change
        }
    }

    // Property to get or set max health
    public float MaxHealth
    {
        get => maxHealth;
        set
        {
            maxHealth = Mathf.Max(value, 0f); // Ensure max health is not negative
            currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth); // Clamp current health if needed
        }
    }

    // Event to notify when health changes
    public System.Action<float> OnHealthChanged;

    // Initialize health (call this when starting the game)
    public void Initialize()
    {
        CurrentHealth = MaxHealth; // Reset current health to max health
    }

    // Method to take damage
    public void TakeDamage(float amount)
    {
        if (amount > 0)
        {
            CurrentHealth -= amount;
        }
    }

    // Method to heal
    public void Heal(float amount)
    {
        if (amount > 0)
        {
            CurrentHealth += amount;
        }
    }
}