using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    public TextMeshProUGUI interactPrompt; // Assign in Inspector
    public KeyCode interactKey = KeyCode.E;
    [SerializeField] private GameObject interactionpromptGO; 
    private GameObject currentInteractable;
    [SerializeField] private Player player; 
    private string currentTag = "";
    [SerializeField] private GameObject foodPrefab;
    //[SerializeField] private GameObject waterBottlePrefab; 

    void Start()
    {
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        interactionpromptGO = GameObject.Find("PlayerUI/InteractPrompt");
        if (interactionpromptGO != null)
        {
            interactPrompt = interactionpromptGO.GetComponent<TextMeshProUGUI>();
        }
        else
        {
            Debug.Log("Interaction Prompt is null!");
        }
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
            case "NPC":
                Debug.Log("NPC Collides!");
                var npcChat = currentInteractable.GetComponent<NPCChat>();
                if (npcChat != null)
                    npcChat.TryChat();
                else
                    Debug.LogWarning("Interactable tagged 'NPC' has no NPCChat component");
                break;

            case "questOrbTag":
                var orb = currentInteractable.GetComponent<QuestOrb>();
                if (orb != null)
                    orb.TryAssignQuest();
                else
                    Debug.LogWarning("Interactable tagged QuestOrb has no QuestOrb component");
                break;

            case "Water":
                Debug.Log("Drinking Water");
                dropRate = UnityEngine.Random.Range(2, 100); 
                break;

            case "Food":
                Debug.Log("Eating Food");
                dropRate = UnityEngine.Random.Range(2, 25);
                Debug.Log("Drop Rate: " + dropRate); 
                if (dropRate <= 10)
                {
                    Instantiate(foodPrefab, player.transform.position, Quaternion.identity);
                    Destroy(currentInteractable);
                }
                Destroy(currentInteractable);
                break;

            case "Door":
                Debug.Log("Opening door...");
                // Call your Door interaction logic here
                break;

            case "Note":
                {
                    // Attempt to find the NoteInteractable component on the current interactable
                    var noteComp = currentInteractable.GetComponent<Notes>();
                    if (noteComp != null)
                    {
                        noteComp.PickUpNote();
                    }
                    else
                    {
                        Debug.LogWarning($"Interactable tagged 'Note' but missing NoteInteractable: {currentInteractable.name}");
                    }
                    break;
                }

            case "Key":
                {
                    var keyComp = currentInteractable.GetComponent<Keys>();
                    if (keyComp != null)
                    {
                        keyComp.PickUpKey();
                        Debug.Log("Collected the key");
                    }
                    else
                    {
                        Debug.LogWarning("invalid Keys");
                    }
                    break; 
                }
            default:
                Debug.LogWarning($"No interaction defined for tag: {currentTag}");
                break;
        }
    }
}
