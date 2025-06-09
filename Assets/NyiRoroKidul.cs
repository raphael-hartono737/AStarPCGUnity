using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NyiRoroKidul : MonoBehaviour
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
            gameManager.goodEnding = true;
            gameManager.Outcome(); 
        }
    }
}
