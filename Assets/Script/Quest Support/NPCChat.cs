using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCChat : MonoBehaviour
{
    QuestObject questObj;
    private InteractionManager interactionManager;

    void Start()
    {
        questObj = GetComponent<QuestObject>();
        interactionManager = FindObjectOfType<InteractionManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            interactionManager.SetCurrentInteractable(gameObject, "NPC");
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            interactionManager.ClearCurrentInteractable();
    }

    public void TryChat()
    {
        Debug.Log("Interacted!");
        if (questObj.HasQuest())
        {
            questObj.AdvanceQuest();
            Destroy(this.gameObject); 
        }
        else
        {
            Debug.Log("No Quests in this NPC!");
        }
    }

}
