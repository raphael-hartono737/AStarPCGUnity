using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NotesSlot : MonoBehaviour, IPointerClickHandler
{
    [Header("Notes Data")]
    [SerializeField] public string notesName;
    [SerializeField] public Sprite notesSprite;
    [SerializeField] public string notesDescription;
    public Sprite emptySprite;

    [Header("Notes Slot")]
    [SerializeField] private TMP_Text noteName;
    [SerializeField] private Image notesImage;
    [SerializeField] public GameObject selectedShader;
    [SerializeField] public bool isSelected;


    [Header("Item Description")]
    public TMP_Text notesDescriptionNameText;
    public TMP_Text notesDescriptionText;

    [SerializeField] private InventoryManager inventoryManager;

    public int AddNote(string notesName, Sprite notesSprite, string notesDescription)
    {
        if (!string.IsNullOrEmpty(this.notesName))
            return 1;

        this.notesName = notesName;
        this.notesSprite = notesSprite;
        this.notesDescription = notesDescription;

        notesImage.sprite = notesSprite;
        return 0; 
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            OnLeftClick(); 
        }
    }
    void OnLeftClick()
    {
        if (string.IsNullOrEmpty(notesName))
            return;

        if (!isSelected)
        {
            inventoryManager.DeselectAllNotesSlots();

            selectedShader.SetActive(true);
            isSelected = true;

            notesDescriptionNameText.text = notesName;
            notesDescriptionText.text = notesDescription;
        }
        
    }

    public void ClearSlot()
    {
        notesName = "";
        notesSprite = null;
        notesDescription = "";

        notesImage.sprite = emptySprite;
        selectedShader.SetActive(false);
        isSelected = false;
    }

}
