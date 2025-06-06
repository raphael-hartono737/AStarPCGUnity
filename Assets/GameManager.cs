using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Initilization")]
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject portLocation;
    [SerializeField] private MapGenerator mapGenerator;

    void OnEnable()
    {
        QuestOrbFinder.questOrbFinderComplete += HandleGameInitialization;
    }

    void OnDisable()
    {
        QuestOrbFinder.questOrbFinderComplete -= HandleGameInitialization;
    }
    void HandleGameInitialization()
    {
        portLocation = GameObject.FindGameObjectWithTag("StartPoint"); 
        if (portLocation != null)
        {
            Vector3 basePos = portLocation.transform.position;
            Vector3 spawnPos = basePos + new Vector3(0f, 2f, 0f); 
            Instantiate(player, spawnPos, Quaternion.identity);
        }
        if (player == null)
        {
            Debug.LogError("Player GameObject not found!");
            return;
        }
    }
}