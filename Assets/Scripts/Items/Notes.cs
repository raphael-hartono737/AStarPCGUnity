using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Notes : MonoBehaviour
{
    [SerializeField] private string notesName;
    [SerializeField] private Sprite notesIcon;
    [TextArea][SerializeField] private string notesDescription;

    private InteractionManager interactionManager;
    private InventoryManager inventoryManager; 

    private void Start()
    {
        interactionManager = FindObjectOfType<InteractionManager>();
        if (interactionManager == null)
            Debug.LogError("Notes: No InteractionManager found in the scene!");
        inventoryManager = GameObject.Find("InventorySystem").GetComponent<InventoryManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            interactionManager.SetCurrentInteractable(this.gameObject, "Note");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            interactionManager.ClearCurrentInteractable();
        }
    }

    public void PickUpNote()
    {
        int leftover = inventoryManager.AddNote(
            notesName,
            notesIcon,
            notesDescription
        );

        if (leftover <= 0)
        {
            Destroy(gameObject);
        }
        else
        {
            Debug.Log($"Notes: No empty slot for “{notesName}.”");
        }
    }

}
