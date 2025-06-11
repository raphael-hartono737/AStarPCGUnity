using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Initilization")]
    [SerializeField] private GameObject player;
    [SerializeField] private Transform portLocation;
    [SerializeField] private MapGenerator mapGenerator;
    [SerializeField] private MainRoadGenerator roadGen;
    [SerializeField] private bool firstInitialization;

    [Header("Phase 2")]
    [SerializeField] private int templeCounter = 0; //Temple Counter
    [SerializeField] private GameObject findAllTemplesUI; //UI
    
    [SerializeField] public GameObject[] keyItems;
    [SerializeField] public int keyItemsTreshold;

    [Header("Phase 3")]
    [SerializeField] private bool thirdPhase = false;
    [SerializeField] public bool goodEnding = false;
    [SerializeField] public bool badEnding = false;
    [SerializeField] private BoxCollider boxColliderPort;
    [SerializeField] private PlacementGenerator roroGenerator; 

    public static event System.Action OnGameManagerComplete; 

    void OnEnable()
    {
        RoadGenerationEvents.OnRoadGenerationComplete += HandleGameInitialization;
        QuestOrb.OnQuestTracker6 += HandleSecondPhaseInitialization;
        GatewayTrigger.OnGatewayDestroyed += HandleTempleDetector; 
    }

    void OnDisable()
    {
        RoadGenerationEvents.OnRoadGenerationComplete -= HandleGameInitialization;
    }
    private void Update()
    {
        if (firstInitialization == true && player == null)
        {
            SceneManager.LoadScene("Main Menu"); 
        }
    }
    void HandleGameInitialization()
    {
        findAllTemplesUI.SetActive(false);
        boxColliderPort = GameObject.FindGameObjectWithTag("StartPoint").GetComponent<BoxCollider>();
        boxColliderPort.enabled = false; 
        if (roadGen != null)
        {
            portLocation = roadGen.selected.transform;
            Debug.Log($"[GameManager] Found StartPoint at world-position {portLocation.position}");
            Vector3 spawnPos = portLocation.position + new Vector3(0f, 5f, 0f);
            Debug.Log($"[GameManager] About to Instantiate player at spawnPos = {spawnPos}");
            Instantiate(player, spawnPos, Quaternion.identity);
            firstInitialization = true;
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

    #region SecondPhase
    void HandleSecondPhaseInitialization()
    {
        findAllTemplesUI.SetActive(true);
        GameObject[] allGateway = GameObject.FindGameObjectsWithTag("Gateway");
    }

    void HandleTempleDetector()
    {
        TempleCounter();
        if (templeCounter >= 2)
        {
            thirdPhase = true;
            findAllTemplesUI.SetActive(false); 
            boxColliderPort.enabled = true;
            SelectRoroChan(); 
        }
        else
        {
            thirdPhase = false; 
        }
    }

    public void TempleCounter()
    {
        templeCounter++;
        keyItemsTreshold++; 
    }
    #endregion

    #region Third Phase

    public void Outcome()
    {
        if (goodEnding == true && badEnding == false && thirdPhase == true)
        {
            SceneManager.LoadScene("TrueEnding");
        }
        else if (goodEnding == false && badEnding == true && thirdPhase == true)
        {
            SceneManager.LoadScene("BadEnding");
        }
        else
        {
            return; 
        }
    }

    void SelectRoroChan()
    {
        roroGenerator.Generate(); 
        PickOneKeepDestroyRest("NyiRoroKidul"); 
    }

    public static GameObject PickOneKeepDestroyRest(string tag)
    {
        GameObject[] all = GameObject.FindGameObjectsWithTag(tag);
        if (all == null || all.Length == 0)
            return null;

        int chosenIndex = Random.Range(0, all.Length);
        GameObject selected = all[chosenIndex];

        // 3. Destroy all the others
        for (int i = 0; i < all.Length; i++)
        {
            if (i == chosenIndex) continue;
            Object.Destroy(all[i]);
        }

        return selected;
    }
    #endregion
}