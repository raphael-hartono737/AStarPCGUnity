using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    public TextMeshProUGUI interactPrompt; // Assign in Inspector
    public KeyCode interactKey = KeyCode.E;

    private GameObject currentInteractable;
    [SerializeField] private Player player; 
    private string currentTag = "";
    //[SerializeField] private GameObject berriesPrefab;
    //[SerializeField] private GameObject waterBottlePrefab; 

    void Start()
    {
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
        {
            player = playerGO.GetComponent<Player>();
        }

        if (player == null)
        {
            Debug.LogError("Player script not found on tagged 'Player' object!");
        }
    }
    void Update()
    {
        if (interactPrompt != null)
            interactPrompt.gameObject.SetActive(currentInteractable != null);

        if (currentInteractable != null && Input.GetKeyDown(interactKey))
        {
            HandleInteraction();
        }
    }

    public void SetCurrentInteractable(GameObject go, string tag)
    {
        currentInteractable = go;
        currentTag = tag;
    }

    public void ClearCurrentInteractable()
    {
        currentInteractable = null;
        currentTag = "";
    }

    private void HandleInteraction()
    {
        int dropRate = 0; 
        switch (currentTag)
        {
            case "Water":
                Debug.Log("Drinking Water");
                dropRate = UnityEngine.Random.Range(2, 100); 
                player.DrinkWater(dropRate);
                break;

            case "Food":
                Debug.Log("Eating Food");
                dropRate = UnityEngine.Random.Range(2, 25);
                player.ConsumeFood(dropRate);
                Destroy(currentInteractable);
                Debug.Log("Player Consumed Food: " + dropRate); 
                break;

            case "Door":
                Debug.Log("Opening door...");
                // Call your Door interaction logic here
                break;

            default:
                Debug.LogWarning($"No interaction defined for tag: {currentTag}");
                break;
        }
    }
}
