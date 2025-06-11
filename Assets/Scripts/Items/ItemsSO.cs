using UnityEngine;

[CreateAssetMenu] 
public class ItemsSO : ScriptableObject
{
    [Header("Basic Info")]
    public string itemName;
    public Sprite icon;
    [TextArea] public string description;
    public int amountToChangeStat; 

    public bool UseItem()
    {
        if (statToChange == StatToChange.hydration)
        {
            Player playerHydration = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
            if (playerHydration == null)
            {
                Debug.Log("Items: Player Null!");
            }
            if (playerHydration.playerHydrate >= playerHydration.maxHydrate)
            {
                return false; 
            }
            else
            {
                playerHydration.ChangeHydration(amountToChangeStat);
                return true; 
            }
            
        }
        if (statToChange == StatToChange.hunger)
        {
            Player playerHunger = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
            if (playerHunger == null)
            {
                Debug.Log("Items: Player Null!");
            }
            if (playerHunger.playerHunger >= playerHunger.maxHunger)
            {
                return false;
            }
            else
            {
                playerHunger.ChangeHunger(amountToChangeStat);
                return true; 
            }
        }
        return false; 
    }
    public StatToChange statToChange = new StatToChange(); 

    public enum StatToChange
    {
        none,
        health,
        hydration,
        hunger
    }

}
