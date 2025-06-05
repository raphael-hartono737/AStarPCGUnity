using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FirstQuestOrb : MonoBehaviour
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
        if (allQuestsFinished) return;

        if (currentQuest == null && other.CompareTag("Player") && Input.GetButtonDown("Use"))
        {
            int questIndex = (DebugIndex >= questPrefabs.Length || DebugIndex < 0) ?
                Random.Range(0, questPrefabs.Length) : DebugIndex;

            // Pastikan prefab tidak null
            if (questPrefabs[questIndex] == null)
            {
                Debug.LogError("Quest prefab at index " + questIndex + " is null!");
                return;
            }

            // Perbaikan logika spawn
            Vector3 spawnPosition = other.transform.position + other.transform.forward * 1f;
            spawnPosition.y += 0.5f;

            GameObject quest = Instantiate(
                questPrefabs[questIndex],
                spawnPosition,
                Quaternion.identity
            );

            currentQuest = quest.GetComponent<QuestBase>();

            // Tambahkan pengecekan null
            if (currentQuest != null)
            {
                currentQuest.Initiate();
            }
            else
            {
                Debug.LogError("Instantiated quest missing QuestBase component!");
                Destroy(quest);
            }
        }
    }

    private void OnGUI()
    {
        // Hanya tampilkan GUI jika belum menyelesaikan semua quest
        if (!allQuestsFinished && currentQuest == null)
        {
            GUI.Label(new Rect(10, 10, 300, 20), "Find the Letter!");
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