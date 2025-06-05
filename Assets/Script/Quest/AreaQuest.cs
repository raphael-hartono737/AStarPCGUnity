using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaQuest : QuestBase
{
    //must have QuestObject script

    public float radius = 20.0f;
    public int numObjects = 10;
    [SerializeField] int questProgress = 0;

    public override void Initiate()
    {
        // Pastikan questObject sudah di-assign
        if (questObject == null)
        {
            Debug.LogError("QuestObject prefab is not assigned!");
            return;
        }

        numObjects = Random.Range(5, 15);
        for (int i = 0; i < numObjects;)
        {
            Vector2 spawnPoint = Random.insideUnitCircle * radius;
            Quaternion rot = Quaternion.Euler(0, Random.Range(0, 360), 0);

            RaycastHit hit;
            Vector3 rayStart = transform.position + new Vector3(spawnPoint.x, 20, spawnPoint.y);

            // Tambahkan pengecekan raycast
            if (Physics.Raycast(rayStart, Vector3.down, out hit, Mathf.Infinity, LayerMask.GetMask("Ground")))
            {
                GameObject temp = Instantiate(questObject, hit.point + Vector3.up, rot);
                temp.transform.SetParent(transform);

                // Pastikan komponen QuestObject ada
                QuestObject qo = temp.GetComponent<QuestObject>();
                if (qo != null)
                {
                    qo.SetQuest(this);
                    i++;
                }
                else
                {
                    Debug.LogError("Instantiated object missing QuestObject component!");
                    Destroy(temp);
                }
            }
            else
            {
                Debug.LogWarning("Raycast failed to hit ground at position: " + rayStart);
            }
        }
    }

    public override void Advance()
    {
        ++questProgress;
        Debug.Log("Quest Progress: " + questProgress + "/" + numObjects);
        if (questProgress >= numObjects)
        {
            Complete();
        }
    }

    public override void Complete()
    {
        base.Complete();
        Debug.Log("Quest Complete!");
    }

    private void OnGUI()
    {
        if (!isComplete) GUI.Label(new Rect(10, 10, 300, 20), string.Format("Quest: " + questText, questProgress, numObjects));
    }
}
