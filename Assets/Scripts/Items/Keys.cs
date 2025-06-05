using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Keys : MonoBehaviour
{
    [SerializeField] private string keyName;
    [SerializeField] private Sprite keyIcon;
    [TextArea][SerializeField] private string keyDescription;

    private InteractionManager interactionManager;
    private InventoryManager inventoryManager;

    private void Start()
    {
        interactionManager = FindObjectOfType<InteractionManager>();
        if (interactionManager == null)
            Debug.LogError("key: No InteractionManager found in the scene!");
        inventoryManager = GameObject.Find("InventorySystem").GetComponent<InventoryManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            interactionManager.SetCurrentInteractable(this.gameObject, "Key");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            interactionManager.ClearCurrentInteractable();
        }
    }

    public void PickUpKey()
    {
        int leftover = inventoryManager.AddKey(
            keyName,
            keyIcon,
            keyDescription
        );

        if (leftover <= 0)
        {
            Destroy(gameObject);
        }
        else
        {
            Debug.Log($"key: No empty slot for “{keyName}.”");
        }
    }

}
