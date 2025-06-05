using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;
using UnityEngine.UI; 

public class Player : MonoBehaviour
{
    public bool isGrounded = false;
    [Header("Player Stats")]
    [SerializeField] public HealthData playerHealth;
    [SerializeField] public Temperature playerTemperature;
    [SerializeField] private RigidbodyFirstPersonController playerMovement; 
    [SerializeField] public float playerHunger = 1000.0f;
    [SerializeField] public float playerHydrate = 1000.0f; 
    [SerializeField] private bool playerStarving;
    public float maxHunger = 1000.0f;
    public float maxHydrate = 1000.0f;


    [Header("Player Stats Multiplication")]
    [SerializeField] private float dehydrationAmount;

    [Header("Player Interact & Inventory Manager")]
    [SerializeField] private GameObject interactCollider;
    [SerializeField] public GameObject InventoryMenu;

    [Header("Player Sliders")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider hungerSlider;
    [SerializeField] private Slider hydrateSlider; 

    private float tempAmount; 
    private void Start()
    {
        playerHunger = 1000.0f;
        playerHydrate = 1000.0f; 
        dehydrationAmount = 1.0f; 
        // Initialize health at the start of the game
        if (playerHealth != null)
        {
            playerHealth.Initialize();

            // Subscribe to the health change event
            playerHealth.OnHealthChanged += OnHealthChanged;
        }

        StartCoroutine(PlayerHunger());
        StartCoroutine(PlayerDehydration());
        UpdateSliders(); 
    }

    void Update()
    {
        if (this.gameObject != null)
        {
            if (playerMovement.movementSettings.Running == true)
            {
                dehydrationAmount = 2.5f; 
            }
            else
            {
                dehydrationAmount = 1.0f; 
            }
            if (playerTemperature.temperature < playerTemperature.minTemperature)
            {
                Debug.Log("Player is Freezing!");
                Freezing();
            }
        }

        if (playerHealth.CurrentHealth == 0)
        {
            Destroy(this.gameObject); 
        }
        
    }

    void UpdateSliders()
    {
        if (hungerSlider != null)
            hungerSlider.value = playerHunger / maxHunger;

        if (hydrateSlider != null)
            hydrateSlider.value = playerHydrate / maxHydrate;
    }
    // Update is called once per frame
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;

        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }


    private void OnHealthChanged(float newHealth)
    {
        Debug.Log($"Player Health Changed: {newHealth}/{playerHealth.MaxHealth}");

        if (newHealth <= 0)
        {
            Debug.Log("Player has died!");
            Die();
        }
    }

    private void Die()
    {
        // Handle player death logic here
        Debug.Log("Game Over!");
        Destroy(gameObject); // Example: Destroy the player object
    }

    public void TakeDamage(float amount)
    {
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(amount);
        }
    }

    //=================Player Freezing====================

    private void Freezing()
    {
        if (playerTemperature.temperature < 0)
        {
            StartCoroutine(PlayerTakingDamage(0.5f)); 
        }
    }

    private IEnumerator PlayerTakingDamage(float amount)
    {
        while (playerHealth.CurrentHealth != 0)
        {
            TakeDamage(amount);
            yield return null; 
        }
    }

    private IEnumerator PlayerHunger()
    {
        while (this.gameObject != null && playerHunger > 0)
        {
            playerHunger = playerHunger - 1.0f;
            UpdateSliders(); 
            yield return new WaitForSeconds(1.0f); 
        }
        if (playerHunger < 0)
        {
            playerHealth.TakeDamage(1.0f); 
        }
    }

    private IEnumerator PlayerDehydration()
    {
        while (this.gameObject != null && playerHydrate > 0)
        {
            playerHydrate = playerHydrate - dehydrationAmount;
            UpdateSliders();
            yield return new WaitForSeconds(1.0f);
        }
    }

    public void ChangeHunger(float foodValue)
    {
        Debug.Log("Hunger: + " + foodValue); 
        playerHunger = Mathf.Clamp(playerHunger + foodValue, 0, maxHunger);
        UpdateSliders();
    }

    public void ChangeHydration(float waterValue)
    {
        Debug.Log("Hydration: + " + waterValue);
        playerHydrate = Mathf.Clamp(playerHydrate + waterValue, 0, maxHydrate);
        UpdateSliders();
    }

    public void OnAllKeysCollected()
    {
        Debug.Log("All Keys Collected!");
    }

}
