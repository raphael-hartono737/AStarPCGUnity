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
    [SerializeField] private RigidbodyFirstPersonController playerMovement; 
    [SerializeField] public float playerHunger = 1000.0f;
    [SerializeField] public float playerHydrate = 1000.0f; 
    [SerializeField] private bool playerStarving;
    public float maxHunger = 1000.0f;
    public float maxHydrate = 1000.0f;


    [Header("Rates")]
    public float baseDehydrationRate = 1f;
    public float runningDehydrationRate = 5f;
    private float dehydrationRate;
    private float hungerRate = 1f;

    [Header("Player Interact & Inventory Manager")]
    [SerializeField] private GameObject interactCollider;
    [SerializeField] public GameObject InventoryMenu;

    [Header("Player Sliders")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider hungerSlider;
    [SerializeField] private Slider hydrateSlider;

    private Coroutine dehydrationRoutine;
    private Coroutine hungerRoutine;
    private Coroutine damageRoutine;
    private Coroutine healingRoutine;
    private void Start()
    {
        playerHunger = 1000.0f;
        playerHydrate = 1000.0f;
        // Initialize health at the start of the game
        if (playerHealth != null)
        {
            playerHealth.Initialize();

            // Subscribe to the health change event
            playerHealth.OnHealthChanged += OnHealthChanged;
        }
        dehydrationRoutine = StartCoroutine(PlayerDehydration());
        hungerRoutine = StartCoroutine(PlayerHunger());
        UpdateSliders(); 
    }

    void Update()
    {
        if (this.gameObject != null)
        {
            dehydrationRate = playerMovement.movementSettings.Running ? runningDehydrationRate : baseDehydrationRate;
        }

        if (playerHealth.CurrentHealth == 0)
        {
            StopAllCoroutines(); 
            Destroy(this.gameObject); 
        }

        if (playerHydrate <= 0f)
        {
            playerMovement.canRun = false; 
            if (dehydrationRoutine != null)
            {
                StopCoroutine(dehydrationRoutine);
                dehydrationRoutine = null;
                hungerRate = 3f;
            }
        }
        else
        {
            playerMovement.canRun = true; 
            if (dehydrationRoutine == null)
                dehydrationRoutine = StartCoroutine(PlayerDehydration());
            hungerRate = 1f;
        }

        if (playerHunger <= 0f)
        {
            if (hungerRoutine != null)
            {
                StopCoroutine(hungerRoutine);
                hungerRoutine = null;
            }
            if (damageRoutine == null)
                damageRoutine = StartCoroutine(PlayerTakingDamage(1f));
        }
        else
        {
            if (damageRoutine != null)
            {
                StopCoroutine(damageRoutine);
                damageRoutine = null;
            }
            if (hungerRoutine == null)
                hungerRoutine = StartCoroutine(PlayerHunger());
        }

        if (playerHunger > 0f && playerHydrate > 0f && playerHealth.CurrentHealth < playerHealth.MaxHealth)
        {
            if (healingRoutine == null)
                healingRoutine = StartCoroutine(PlayerHealing());
        }
        else
        {
            if (healingRoutine != null)
            {
                StopCoroutine(healingRoutine);
                healingRoutine = null;
            }
        }

    }

    void UpdateSliders()
    {
        if (hungerSlider) hungerSlider.value = playerHunger / maxHunger;
        if (hydrateSlider) hydrateSlider.value = playerHydrate / maxHydrate;
        if (healthSlider) healthSlider.value = playerHealth.CurrentHealth / playerHealth.MaxHealth;
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
        UpdateSliders();
        if (newHealth <= 0) Die();
    }

    private void Die()
    {
        StopAllCoroutines();
        Destroy(gameObject); 
    }

    public void TakeDamage(float amount)
    {
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(amount);
        }
    }

    private IEnumerator PlayerDehydration()
    {
        while (gameObject && playerHydrate > 0f)
        {
            playerHydrate = Mathf.Clamp(playerHydrate - dehydrationRate, 0, maxHydrate);
            UpdateSliders();
            yield return new WaitForSeconds(1f);
        }
    }

    private IEnumerator PlayerHunger()
    {
        while (gameObject && playerHunger > 0f)
        {
            playerHunger = Mathf.Clamp(playerHunger - hungerRate, 0, maxHunger);
            UpdateSliders();
            yield return new WaitForSeconds(1f);
        }
    }

    private IEnumerator PlayerTakingDamage(float amount)
    {
        while (playerHealth.CurrentHealth > 0f)
        {
            TakeDamage(amount);
            yield return new WaitForSeconds(3f);
        }
    }

    private IEnumerator PlayerHealing()
    {
        while (playerHunger > 0f && playerHydrate > 0f && playerHealth.CurrentHealth < playerHealth.MaxHealth)
        {
            playerHealth.Heal(5f);
            UpdateSliders();
            yield return new WaitForSeconds(3f);
        }
    }


    #region Items Coordination
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
    #endregion

}
