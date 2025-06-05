using UnityEngine;

public class Interactable : MonoBehaviour
{
    public virtual void Interact()
    {
        // Contoh: Tampilkan pesan di console
        Debug.Log("Berinteraksi dengan: " + gameObject.name);

        // Override method ini di script turunan untuk logika spesifik objek
    }
}