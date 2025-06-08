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

    [SerializeField] private int maxQuests = 10;
    private int questsCompleted = 0;
    private bool allQuestsFinished = false;

    [SerializeField] private InteractionManager interactionManager;
    public Waypoint waypoint;
    public Text questComplete;
    [SerializeField] private GameObject findQuestMasterUI; 

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
    }

    private void Update()
    {
        if (allQuestsFinished)
        {
            // Nonaktifkan waypoint jika semua quest selesai
            waypoint.target = null;
            return;
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
            return;

        int questIndex = (DebugIndex >= questPrefabs.Length || DebugIndex < 0)
                         ? Random.Range(0, questPrefabs.Length)
                         : DebugIndex;

        GameObject questGO;
        if (questPrefabs[questIndex].transform.parent != null)
        {
            currentQuest = questPrefabs[questIndex].GetComponent<QuestBase>();
            currentQuest.Initiate();
        }
        else
        {
            Transform point = questPoints[Random.Range(0, questPoints.Length)];
            questGO = Instantiate(questPrefabs[questIndex], point.position, Quaternion.identity);
            questGO.transform.SetParent(point);
            currentQuest = questGO.GetComponent<QuestBase>();
            currentQuest.Initiate();
        }
    }

    private void OnGUI()
    {
        if (!allQuestsFinished && currentQuest == null)
        {
            findQuestMasterUI.SetActive(true); 
        }
    }

    IEnumerator QuestComplete()
    {
        questComplete.enabled = true;
        Color c = questComplete.color;
        c.a = 1.0f;
        questComplete.color = c;

        float t = 0.0f;

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
}