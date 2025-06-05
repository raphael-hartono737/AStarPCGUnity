using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestObject : MonoBehaviour
{
    QuestBase quest;

    // Area default untuk penempatan acak
    private Bounds placementArea = new Bounds(Vector3.zero, new Vector3(2000f, 0f, 2000f));

    void Start()
    {
        PlaceRandomly();
    }

    public void SetQuest(QuestBase qb)
    {
        quest = qb;
    }

    public void AdvanceQuest()
    {
        quest.Advance();
    }

    public bool HasQuest()
    {
        return quest != null;
    }

    // Menempatkan objek di posisi acak dengan batas ketinggian
    public void PlaceRandomly()
    {
        int maxAttempts = 50;
        for (int i = 0; i < maxAttempts; i++)
        {
            // Generate posisi acak di XZ
            Vector2 randomXZ = Random.insideUnitCircle * placementArea.extents.magnitude;
            Vector3 rayStart = new Vector3(
                randomXZ.x + placementArea.center.x,
                50f, // Posisi Y awal raycast (diatas max height)
                randomXZ.y + placementArea.center.z
            );

            // Lakukan raycast ke bawah
            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 20f))
            {
                float hitHeight = hit.point.y;

                // Cek jika ketinggian valid (40-47)
                if (hitHeight >= 44f && hitHeight <= 47f)
                {
                    transform.position = hit.point;
                    return; // Berhasil ditempatkan
                }
            }
        }

        // Fallback jika gagal
        transform.position = new Vector3(
            placementArea.center.x,
            43.5f, // Tengah-tengah ketinggian
            placementArea.center.z
        );
    }

    // Untuk mengatur area penempatan custom (opsional)
    public void SetPlacementArea(Bounds newArea)
    {
        placementArea = newArea;
    }
}