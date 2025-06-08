using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; 

public abstract class QuestBase : MonoBehaviour
{
    [Tooltip("{0} = parts complete, {1} out of\nex:killed {0} out of {1} zombies")]
    public string questText;
    public TMP_Text questTextDisplay; 
    public bool permanentQuest = false;
    public GameObject questObject;
    public bool isComplete = false;

    public abstract void Initiate(); //quest setup
    public abstract void Advance();  //quest progress advancement
    public virtual void Complete() //quest complete action
    {
        isComplete = true;
        Destroy(this.gameObject); 
        
    }
}
