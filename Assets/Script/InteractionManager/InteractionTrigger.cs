using UnityEngine;

public class InteractionTrigger : MonoBehaviour
{
    private InteractionManager interactionManager;

    private void Awake()
    {
        interactionManager = FindObjectOfType<InteractionManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsTaggedInteractable(other.tag))
        {
            interactionManager.SetCurrentInteractable(other.gameObject, other.tag);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsTaggedInteractable(other.tag))
        {
            interactionManager.ClearCurrentInteractable();
        }
    }

    private bool IsTaggedInteractable(string tag)
    {
        return tag == "Water" || tag == "Food" || tag == "Door";
    }
}