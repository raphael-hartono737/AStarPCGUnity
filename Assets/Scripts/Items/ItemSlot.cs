using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class ItemSlot : MonoBehaviour, IPointerClickHandler
{
    [Header("Item Data")]
    [SerializeField] public string itemName;
    [SerializeField] public int quantity;
    [SerializeField] public Sprite itemSprite;
    [SerializeField] public bool isFull;
    [SerializeField] public string itemDescription;
    public Sprite emptySprite; 

    [Header("Item Slot")]
    [SerializeField] private TMP_Text quantity_Text;
    [SerializeField] private Image itemImage;
    [SerializeField] public GameObject selectedShader;
    [SerializeField] public bool isSelected;
    [SerializeField] private int maxNumberOfItems;


    [Header("Item Description")]
    public Image itemDescriptionImage;
    public TMP_Text itemDescriptionNameText;
    public TMP_Text itemDescriptionText;

    [SerializeField] private InventoryManager inventoryManager;
    
    void Start()
    {
        inventoryManager = GameObject.Find("InventorySystem").GetComponent<InventoryManager>();
    }
    // Start is called before the first frame update
    public int AddItem(string itemName, int quantity, Sprite itemSprite, string itemDescription)
    {
        if (isFull)
        {
            return quantity; 
        }

        this.itemName = itemName;

        this.itemSprite = itemSprite;
        itemImage.sprite = itemSprite;
        this.itemDescription = itemDescription;
        this.quantity += quantity;
        if (this.quantity >= maxNumberOfItems)
        {
            quantity_Text.text = maxNumberOfItems.ToString();
            quantity_Text.enabled = true;
            isFull = true;

            int extraItems = this.quantity - maxNumberOfItems;
            this.quantity = maxNumberOfItems;
            return extraItems;
        }

        quantity_Text.text = this.quantity.ToString();
        quantity_Text.enabled = true;

        return 0; 
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            OnLeftClick(); 
        }
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            OnRightClick(); 
        }
    }

    void OnLeftClick()
    {
        if (isSelected)
        {
            bool usable = inventoryManager.UseItem(itemName);
            if (usable)
            {
                this.quantity -= 1;
                quantity_Text.text = this.quantity.ToString();
                if (this.quantity <= 0)
                {
                    EmptySlot();
                }
            }

        }
        else
        {
            inventoryManager.DeselectAllSlots();
            selectedShader.SetActive(true);
            isSelected = true;
            itemDescriptionNameText.text = itemName;
            itemDescriptionText.text = itemDescription;
            itemDescriptionImage.sprite = itemSprite;
            if (itemDescriptionImage.sprite == null)
            {
                itemDescriptionImage.sprite = emptySprite;
            }
        }
        
    }

    private void EmptySlot()
    {
        quantity_Text.enabled = false;
        itemImage.sprite = emptySprite;
        itemDescriptionNameText.text = "";
        itemDescriptionText.text = "";
        itemDescriptionImage.sprite = emptySprite;
    }

    void OnRightClick()
    {

    }
}
