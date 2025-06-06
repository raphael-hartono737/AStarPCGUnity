using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestOrbFinder : MonoBehaviour
{
    [SerializeField] private TerrainData terrainData;
    [SerializeField] private TextureData textureData;
    [SerializeField] private MapGenerator mapGenerator;
    [SerializeField] private float verticalOffset = 1.5f;
    [SerializeField] private string questOrbTag;
    [SerializeField] private GameObject questOrbPrefab; 
    private int mapChunkSize;

    public static event System.Action questOrbFinderComplete; 

    void OnEnable()
    {
        RoadGenerationEvents.OnRoadGenerationComplete += HandleMapGenerationComplete;
    }
    private void HandleMapGenerationComplete()
    {
        
        TryPlacingQuestOrbs(); 
    }

    void OnDisable()
    {
        RoadGenerationEvents.OnRoadGenerationComplete -= HandleMapGenerationComplete; 
    }

    void TryPlacingQuestOrbs()
    {
        mapChunkSize = mapGenerator.mapChunkSize;
        SpawnQuestOrb();
        PlaceExistingQuestOrb();
        questOrbFinderComplete?.Invoke();
    }

    void SpawnQuestOrb()
    {
        Vector3 spawnPosition = FindValidQuestOrbPosition();
        if (spawnPosition != Vector3.zero)
        {
            Instantiate(questOrbPrefab, spawnPosition, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("QuestOrb tidak ditempatkan: Posisi valid tidak ditemukan.");
        }
    }
    Vector3 FindValidQuestOrbPosition()
    {
        int maxAttempts = 1000;
        float mapWorldSize = (mapChunkSize - 1) * terrainData.uniformScale;
        float halfMapSize = mapWorldSize / 2;

        for (int i = 0; i < maxAttempts; i++)
        {
            float x = UnityEngine.Random.Range(-halfMapSize, halfMapSize);
            float z = UnityEngine.Random.Range(-halfMapSize, halfMapSize);
            Vector3 rayOrigin = new Vector3(x, 500, z);

            RaycastHit hit;
            if (Physics.Raycast(rayOrigin, Vector3.down, out hit, Mathf.Infinity))
            {
                float height = hit.point.y;
                if (height >= 40 && height <= 47)
                {
                    float normalizedHeight = height / terrainData.meshHeightMultiplier;
                    foreach (var layer in textureData.layers)
                    {
                        if (normalizedHeight >= layer.startHeight && normalizedHeight <= (layer.startHeight + layer.blendStrength))
                        {
                            // Tambahkan offset vertikal ke posisi Y
                            Vector3 spawnPos = hit.point + Vector3.up * verticalOffset;
                            return spawnPos;
                        }
                    }
                }
            }
        }
        return Vector3.zero;
    }

    void PlaceExistingQuestOrb()
    {
        GameObject questOrb = GameObject.FindGameObjectWithTag(questOrbTag);
        if (questOrb != null)
        {
            Vector3 spawnPosition = FindValidQuestOrbPosition();
            if (spawnPosition != Vector3.zero)
            {
                questOrb.transform.position = spawnPosition;
            }
            else
            {
                Debug.LogWarning("QuestOrb tidak dipindahkan: Posisi valid tidak ditemukan.");
            }
        }
        else
        {
            Debug.LogError("QuestOrb tidak ditemukan di scene! Pastikan ada objek dengan tag " + questOrbTag);
        }
    }

}
