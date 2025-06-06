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


    public Waypoint waypoint;
    public Text questComplete;

    private void Awake()
    {
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        Transform playerUITransform = playerGO.transform.Find("PlayerUI");

        waypoint = playerUITransform.Find("WayPoint").GetComponent<Waypoint>();
        questComplete = playerUITransform.Find("QuestComplete").GetComponent<Text>();
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

    private void OnTriggerStay(Collider other)
    {
        // Hanya aktif jika belum mencapai limit quest
        if (allQuestsFinished) return;

        if (currentQuest == null && other.CompareTag("Player") && Input.GetButtonDown("Use"))
        {
            int questIndex = (DebugIndex >= questPrefabs.Length || DebugIndex < 0) ?
                Random.Range(0, questPrefabs.Length) : DebugIndex;

            if (questPrefabs[questIndex].transform.parent != null)
            {
                currentQuest = questPrefabs[questIndex].GetComponent<QuestBase>();
                currentQuest.Initiate();
            }
            else
            {
                Transform point = questPoints[Random.Range(0, questPoints.Length)];
                GameObject quest = Instantiate(questPrefabs[questIndex], point.position, Quaternion.identity);
                quest.transform.SetParent(point);
                currentQuest = quest.GetComponent<QuestBase>();
                currentQuest.Initiate();
            }
        }
    }

    private void OnGUI()
    {
        // Hanya tampilkan GUI jika belum menyelesaikan semua quest
        if (!allQuestsFinished && currentQuest == null)
        {
            GUI.Label(new Rect(10, 10, 300, 20), "Find the Quest Master for a new quest!");
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