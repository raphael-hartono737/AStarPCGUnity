using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackToPort : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    private void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            gameManager.badEnding = true;
            gameManager.Outcome(); 
        }
    }
}
