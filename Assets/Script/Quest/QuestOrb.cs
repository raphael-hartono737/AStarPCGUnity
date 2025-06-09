using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestOrb : MonoBehaviour
{
    public int DebugIndex = 0;

    public GameObject[] questPrefabs;
    public Transform[] questPoints;
    [SerializeField] QuestBase currentQuest;
    [SerializeField] private SphereCollider questActivation;
    [SerializeField] private int questTracker;
    [SerializeField] private GameObject[] notePrefabs;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private int prefabIndex;

    [SerializeField] private int maxQuests = 10;
    private int questsCompleted = 0;
    private bool allQuestsFinished = false;

    [SerializeField] private InteractionManager interactionManager;
    public Waypoint waypoint;
    public Text questComplete;
    [SerializeField] private GameObject findQuestMasterUI;

    public static event System.Action OnQuestTracker6; 

    private void Awake()
    {
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        Transform playerUITransform = playerGO.transform.Find("PlayerUI");

        waypoint = playerUITransform.Find("WayPoint").GetComponent<Waypoint>();
        if (waypoint == null)
        {
            waypoint = GameObject.Find("Player/PlayerUI/WayPoint").GetComponent<Waypoint>();
        }
        questComplete = playerUITransform.Find("QuestComplete").GetComponent<Text>();
        if (questComplete == null)
        {
            questComplete = GameObject.Find("Player/PlayerUI/QuestComplete").GetComponent<Text>();
        }
        interactionManager = FindObjectOfType<InteractionManager>();
        if (interactionManager == null)
            Debug.Log("No InteractionManager in scene!");
    }
    private void Start()
    {
        // Set batas maksimal quest secara random
        maxQuests = Random.Range(5, 11);
        questTracker = 0;
        prefabIndex = 0; 
        //questPoints = GameObject.FindGameObjectWithTag("questLoc"); 
    }

    private void Update()
    {
        if (allQuestsFinished)
        {
            // Nonaktifkan waypoint jika semua quest selesai
            waypoint.target = null;
            return;
        }

        if (questTracker >= 6)
        {
            OnQuestTracker6?.Invoke();
            Destroy(this.gameObject); 
        }

        if (currentQuest)
        {
            waypoint.target = currentQuest.transform;
            if (currentQuest.isComplete)
            {
                if (!currentQuest.permanentQuest) Destroy(currentQuest.gameObject);
                currentQuest = null;
                questsCompleted++;

                // Cek apakah sudah mencapai limit quest
                if (questsCompleted >= maxQuests)
                {
                    allQuestsFinished = true;
                    waypoint.target = null;
                }
                StartCoroutine(QuestComplete());
            }
        }
        else if (!allQuestsFinished)
        {
            waypoint.target = transform;
            
        }
        if (/*!allQuestsFinished && */currentQuest == null)
        {
            findQuestMasterUI.SetActive(true);
        }
        else
        {
            findQuestMasterUI.SetActive(false);
        }


    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            interactionManager.SetCurrentInteractable(this.gameObject, "questOrbTag");
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            interactionManager.ClearCurrentInteractable();
    }

    public void TryAssignQuest()
    {
        if (allQuestsFinished || currentQuest != null)
        {
            return;

        }
        int questIndex = (DebugIndex >= questPrefabs.Length || DebugIndex < 0)
                         ? Random.Range(0, questPrefabs.Length)
                         : DebugIndex;

        GameObject questGO;
        if (questPrefabs[questIndex].transform.parent != null)
        {
            Debug.Log("Yes 1");
            currentQuest = questPrefabs[questIndex].GetComponent<QuestBase>();
            currentQuest.Initiate();
        }
        else
        {
            Debug.Log("Yes 2");
            Transform point = questPoints[Random.Range(0, questPoints.Length)];
            questGO = Instantiate(questPrefabs[questIndex], point.position, Quaternion.identity);

            //Debug.Log($"Instantiated: {questGO.name} at {point.position}");

            questGO.transform.SetParent(point);
            currentQuest = questGO.GetComponent<QuestBase>();
            if (currentQuest == null)
            {
                Debug.Log("currentQuest: null!");
            }
            currentQuest.Initiate();
        }
    }

    IEnumerator QuestComplete()
    {
        questComplete.enabled = true;
        Color c = questComplete.color;
        c.a = 1.0f;
        questComplete.color = c;

        float t = 0.0f;
        OnCompleteQuestSpawn(); 
        yield return new WaitForSeconds(2.0f);

        while (t < 1.0f)
        {
            float alpha = Mathf.Lerp(1.0f, 0.0f, t);
            c.a = alpha;
            questComplete.color = c;
            t += Time.deltaTime;
            yield return null;
        }
        
        questComplete.enabled = false;
    }

    private void OnQuestTrackerNumberChange()
    {
        if (questTracker > 0 && questTracker % 3 == 0)
        {
            prefabIndex = (questTracker / 3) - 1;

            // Safety check in case you run out of prefabs
            if (prefabIndex >= 0 && prefabIndex < notePrefabs.Length)
            {
                Instantiate(notePrefabs[prefabIndex], spawnPoint != null ? spawnPoint.position : transform.position, spawnPoint != null ? spawnPoint.rotation : transform.rotation);
            }
        }
    }

    private void OnCompleteQuestSpawn()
    {
        questTracker++; 
        OnQuestTrackerNumberChange();
    }
}