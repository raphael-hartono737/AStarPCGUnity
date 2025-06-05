using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class KeySlot : MonoBehaviour, IPointerClickHandler
{
    [Header("key Data")]
    [SerializeField] public string keyName;
    [SerializeField] public Sprite keySprite;
    [SerializeField] public string keyDescription;
    public Sprite emptySprite;

    [Header("key Slot")]
    [SerializeField] private Image keyImage;
    [SerializeField] public GameObject selectedShader;
    [SerializeField] public bool isSelected;


    [Header("Item Description")]
    [SerializeField] public Image keyDescriptionImage;
    [SerializeField] public TMP_Text keyDescriptionNameText;
    [SerializeField] public TMP_Text keyDescriptionText;

    [SerializeField] private InventoryManager inventoryManager;

    public int AddKey(string keyName, Sprite keySprite, string keyDescription)
    {
        if (!string.IsNullOrEmpty(this.keyName))
            return 1;

        this.keyName = keyName;
        this.keySprite = keySprite;
        this.keyDescription = keyDescription;

        keyImage.sprite = keySprite;
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
        if (string.IsNullOrEmpty(keyName))
            return;

        if (!isSelected)
        {
            inventoryManager.DeselectAllKeysSlots();

            selectedShader.SetActive(true);
            isSelected = true;

            keyDescriptionNameText.text = keyName;
            keyDescriptionText.text = keyDescription;
            keyDescriptionImage.sprite = keySprite;

        }

    }

    public void ClearSlot()
    {
        keyName = "";
        keySprite = null;
        keyDescription = "";

        keyImage.sprite = emptySprite;
        selectedShader.SetActive(false);
        isSelected = false;
    }
}
