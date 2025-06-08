using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Initilization")]
    [SerializeField] private GameObject player;
    [SerializeField] private Transform portLocation;
    [SerializeField] private MapGenerator mapGenerator;
    [SerializeField] private MainRoadGenerator roadGen;
    public static event System.Action OnGameManagerComplete; 

    void OnEnable()
    {
        RoadGenerationEvents.OnRoadGenerationComplete += HandleGameInitialization;
    }

    void OnDisable()
    {
        RoadGenerationEvents.OnRoadGenerationComplete -= HandleGameInitialization;
    }
    void HandleGameInitialization()
    {
        if (roadGen != null)
        {
            portLocation = roadGen.selected.transform;
            Debug.Log($"[GameManager] Found StartPoint at world-position {portLocation.position}");
            Vector3 spawnPos = portLocation.position + new Vector3(0f, 2f, 0f);
            Debug.Log($"[GameManager] About to Instantiate player at spawnPos = {spawnPos}");
            Instantiate(player, spawnPos, Quaternion.identity);
            OnGameManagerComplete?.Invoke(); 
        }
        else
        {
            Debug.Log("Game Manager: Start Point is not found!");
        }
        if (player == null)
        {
            Debug.LogError("Player GameObject not found!");
            return;
        }
    }
}