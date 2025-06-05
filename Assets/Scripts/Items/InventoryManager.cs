using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [SerializeField] public GameObject InventoryMenu;
    [SerializeField] public GameObject ConsumableMenu;
    [SerializeField] public GameObject NotesMenu;
    [SerializeField] public GameObject KeysMenu; 
    [SerializeField] private bool menuActivated;
    [SerializeField] private ItemSlot[] itemSlot;
    [SerializeField] private NotesSlot[] notesSlot; 
    [SerializeField] private KeySlot[] keysSlot;
    [SerializeField] private GameObject PlayerUI;
    [SerializeField] private int countKeys;
    [SerializeField] private Player player; 

    [Header("ConsumableItems")]
    public ItemsSO[] itemSOs;

    [Header("NotesItems")]
    public NotesSO[] NotesSOs;

    [Header("KeysItems")]
    public KeysSO[] keysSOs; 

    // Start is called before the first frame update
    void Start()
    {
        PlayerUI = GameObject.Find("PlayerUI");
        player = GameObject.Find("Player").GetComponent<Player>();
        countKeys = 0; 
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I) && menuActivated)
        {
            PlayerUI.SetActive(true);
            Time.timeScale = 1;
            DisableCursor(); 
            InventoryMenu.SetActive(false);
            ConsumableMenu.SetActive(false);
            NotesMenu.SetActive(false);
            KeysMenu.SetActive(false);
            menuActivated = false;

        }
        else if (Input.GetKeyDown(KeyCode.I) && !menuActivated)
        {
            PlayerUI.SetActive(false);
            Time.timeScale = 0;
            EnableCursor(); 
            InventoryMenu.SetActive(true);
            menuActivated = true;

        }
        if (countKeys == 2)
        {
            player.OnAllKeysCollected(); 
        }
        
    }

    #region Items/Consumables
    public bool UseItem(string itemName)
    {
        for (int i = 0; i < itemSOs.Length; i++)
        {
            if (itemSOs[i].name == itemName) 
            {
                bool usable = itemSOs[i].UseItem();
                return usable; 
            }
        }
        return false;
    }

    public int AddItem(string itemName, int quantity, Sprite itemSprite, string itemDescription)
    {
        for (int i = 0; i < itemSlot.Length; i++)
        {
            if (itemSlot[i].isFull == false && itemSlot[i].itemName == itemName || itemSlot[i].quantity == 0)
            {
                int leftOverItems = itemSlot[i].AddItem(itemName, quantity, itemSprite, itemDescription);
                if (leftOverItems > 0)
                {
                    leftOverItems = AddItem(itemName, leftOverItems, itemSprite, itemDescription); 
                    
                }
                return leftOverItems;
            }
        }
        return quantity; 
    }

    public void DeselectAllSlots()
    {
        for (int i = 0; i < itemSlot.Length; i++)
        {
            itemSlot[i].selectedShader.SetActive(false);
            itemSlot[i].isSelected = false; 
        }
    }

    #endregion

    #region Notes

    public void DeselectAllNotesSlots()
    {
        for (int i = 0; i < notesSlot.Length; i++)
        {
            notesSlot[i].selectedShader.SetActive(false);
            notesSlot[i].isSelected = false;
        }
    }

    public int AddNote(string notesName, Sprite noteSprite, string noteDescription)
    {
        for (int i = 0; i < notesSlot.Length; i++)
        {
            if (string.IsNullOrEmpty(notesSlot[i].notesName))
            {
                int leftover = notesSlot[i].AddNote(notesName, noteSprite, noteDescription);
                return leftover; 
            }
        }
        return 1; 
    }

    #endregion

    #region Keys

    public int AddKey(string keyName, Sprite keySprite, string keyDescription)
    {
        for (int i = 0; i < keysSlot.Length; i++)
        {
            if (string.IsNullOrEmpty(keysSlot[i].keyName))
            {
                countKeys++; 
                return keysSlot[i].AddKey(keyName, keySprite, keyDescription);
            }
        }
        return 1;
    }

    public void DeselectAllKeysSlots()
    {
        for (int i = 0; i < keysSlot.Length; i++)
        {
            keysSlot[i].selectedShader.SetActive(false);
            keysSlot[i].isSelected = false;
        }
    }

    public int GetKeysCollectedCount()
    {
        int count = 0;
        for (int i = 0; i < keysSlot.Length; i++)
        {
            if (!string.IsNullOrEmpty(keysSlot[i].keyName))
                count++;
        }
        return count;
    }

    #endregion 

    #region Extra Settings
    void EnableCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true; 
    }
    void DisableCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    #endregion
}
