using UnityEngine;
using System.Collections;
using System;
using System.Threading;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{

    public enum DrawMode { NoiseMap, Mesh, FalloffMap };
    public DrawMode drawMode;

    private int currentSeed; // Variabel baru untuk menyimpan seed acak

    public TerrainData terrainData;
    public NoiseData noiseData;
    public TextureData textureData;

    public Material terrainMaterial;

    //[Header("Quest Orb Settings")]
    //[SerializeField] private GameObject questOrbPrefab;
    //[SerializeField] private string questOrbTag = "QuestOrb";
    //[SerializeField] private float verticalOffset = 1.5f; // Tambahkan offset disini

    [Range(0, MeshGenerator.numSupportedChunkSizes - 1)]
    public int chunkSizeIndex;
    [Range(0, MeshGenerator.numSupportedFlatshadedChunkSizes - 1)]
    public int flatshadedChunkSizeIndex;

    [Range(0, MeshGenerator.numSupportedLODs - 1)]
    public int editorPreviewLOD;
    public bool autoUpdate;

    float[,] falloffMap;

    Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

    [SerializeField]
    private GameObject generator_Mangrove;
    [SerializeField]
    private GameObject generator_Palm ;
    [SerializeField]
    private GameObject generator_Bush;
    [SerializeField]
    private GameObject generator_Temple;
    [SerializeField]
    private GameObject generator_Pine;
    [SerializeField]
    private GameObject generator_Start; 

    public static event System.Action OnMapGenerationComplete;
    void Awake()
    {
        currentSeed = UnityEngine.Random.Range(1, 1001); // Perbaikan di sini
        textureData.ApplyToMaterial(terrainMaterial);
        textureData.UpdateMeshHeights(terrainMaterial, terrainData.minHeight, terrainData.maxHeight);
        RequestMapData(Vector2.zero, OnMapDataReceived);
    }

    void OnMapDataReceived(MapData mapData)
    {
        RequestMeshData(mapData, editorPreviewLOD, OnMeshDataReceived);
    }

    void OnMeshDataReceived(MeshData meshData)
    {
        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (display != null)
        {
            display.DrawMesh(meshData);
        }

        StartCoroutine(GenerateObjectsAfterDelay());
    }

    //void SpawnQuestOrb()
    //{
    //    Vector3 spawnPosition = FindValidQuestOrbPosition();
    //    if (spawnPosition != Vector3.zero)
    //    {
    //        Instantiate(questOrbPrefab, spawnPosition, Quaternion.identity);
    //    }
    //    else
    //    {
    //        Debug.LogWarning("QuestOrb tidak ditempatkan: Posisi valid tidak ditemukan.");
    //    }
    //}

    //Vector3 FindValidQuestOrbPosition()
    //{
    //    int maxAttempts = 1000;
    //    float mapWorldSize = (mapChunkSize - 1) * terrainData.uniformScale;
    //    float halfMapSize = mapWorldSize / 2;

    //    for (int i = 0; i < maxAttempts; i++)
    //    {
    //        float x = UnityEngine.Random.Range(-halfMapSize, halfMapSize);
    //        float z = UnityEngine.Random.Range(-halfMapSize, halfMapSize);
    //        Vector3 rayOrigin = new Vector3(x, 500, z);

    //        RaycastHit hit;
    //        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, Mathf.Infinity))
    //        {
    //            float height = hit.point.y;
    //            if (height >= 40 && height <= 47)
    //            {
    //                float normalizedHeight = height / terrainData.meshHeightMultiplier;
    //                foreach (var layer in textureData.layers)
    //                {
    //                    if (normalizedHeight >= layer.startHeight && normalizedHeight <= (layer.startHeight + layer.blendStrength))
    //                    {
    //                        // Tambahkan offset vertikal ke posisi Y
    //                        Vector3 spawnPos = hit.point + Vector3.up * verticalOffset;
    //                        return spawnPos;
    //                    }
    //                }
    //            }
    //        }
    //    }
    //    return Vector3.zero;
    //}

    //void PlaceExistingQuestOrb()
    //{
    //    GameObject questOrb = GameObject.FindGameObjectWithTag(questOrbTag);
    //    if (questOrb != null)
    //    {
    //        Vector3 spawnPosition = FindValidQuestOrbPosition();
    //        if (spawnPosition != Vector3.zero)
    //        {
    //            questOrb.transform.position = spawnPosition; 
    //        }
    //        else
    //        {
    //            Debug.LogWarning("QuestOrb tidak dipindahkan: Posisi valid tidak ditemukan.");
    //        }
    //    }
    //    else
    //    {
    //        Debug.LogError("QuestOrb tidak ditemukan di scene! Pastikan ada objek dengan tag " + questOrbTag);
    //    }
    //}

    IEnumerator GenerateObjectsAfterDelay()
    {
        yield return null;
        GenerateAllObjects(); 
        OnMapGenerationComplete?.Invoke(); 
    }

    void GenerateAllObjects()
    {
        PlacementGenerator[] generators = new PlacementGenerator[]
        {
        generator_Mangrove.GetComponent<PlacementGenerator>(),
        generator_Palm.GetComponent<PlacementGenerator>(),
        generator_Bush.GetComponent<PlacementGenerator>(),
        generator_Temple.GetComponent<PlacementGenerator>(),
        generator_Pine.GetComponent<PlacementGenerator>(),
        generator_Start.GetComponent<PlacementGenerator>()
        };

        foreach (var generator in generators)
        {
            if (generator != null)
            {
                generator.Generate();
            }
            else
            {
                Debug.LogError("Missing PlacementGenerator component!");
            }
        }

        //PlaceExistingQuestOrb(); 
    }

    void OnValuesUpdated()
    {
        if (!Application.isPlaying)
        {
            DrawMapInEditor();
        }
    }

    void OnTextureValuesUpdated()
    {
        textureData.ApplyToMaterial(terrainMaterial);
    }

    public int mapChunkSize
    {
        get
        {
            if (terrainData.useFlatShading)
            {
                return MeshGenerator.supportedFlatshadedChunkSizes[flatshadedChunkSizeIndex] - 1;
            }
            else
            {
                return MeshGenerator.supportedChunkSizes[chunkSizeIndex] - 1;
            }
        }
    }

    public void DrawMapInEditor()
    {
        textureData.UpdateMeshHeights(terrainMaterial, terrainData.minHeight, terrainData.maxHeight);
        MapData mapData = GenerateMapData(Vector2.zero);

        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (drawMode == DrawMode.NoiseMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
        }
        else if (drawMode == DrawMode.Mesh)
        {
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, terrainData.meshHeightMultiplier, terrainData.meshHeightCurve, editorPreviewLOD, terrainData.useFlatShading));
        }
        else if (drawMode == DrawMode.FalloffMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(FalloffGenerator.GenerateFalloffMap(mapChunkSize)));
        }
    }

    public void RequestMapData(Vector2 centre, Action<MapData> callback)
    {
        ThreadStart threadStart = delegate {
            MapDataThread(centre, callback);
        };

        new Thread(threadStart).Start();
    }

    void MapDataThread(Vector2 centre, Action<MapData> callback)
    {
        MapData mapData = GenerateMapData(centre);
        lock (mapDataThreadInfoQueue)
        {
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
        }
    }

    public void RequestMeshData(MapData mapData, int lod, Action<MeshData> callback)
    {
        ThreadStart threadStart = delegate {
            MeshDataThread(mapData, lod, callback);
        };

        new Thread(threadStart).Start();
    }

    void MeshDataThread(MapData mapData, int lod, Action<MeshData> callback)
    {
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, terrainData.meshHeightMultiplier, terrainData.meshHeightCurve, lod, terrainData.useFlatShading);
        lock (meshDataThreadInfoQueue)
        {
            meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
        }
    }

    void Update()
    {
        if (mapDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < mapDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }

        if (meshDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < meshDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
    }

    MapData GenerateMapData(Vector2 centre)
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(
            mapChunkSize + 2,
            mapChunkSize + 2,
            currentSeed, // Gunakan currentSeed, bukan noiseData.seed
            noiseData.noiseScale,
            noiseData.octaves,
            noiseData.persistance,
            noiseData.lacunarity,
            centre + noiseData.offset
        );

        if (terrainData.useFalloff)
        {

            if (falloffMap == null)
            {
                falloffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize + 2);
            }

            for (int y = 0; y < mapChunkSize + 2; y++)
            {
                for (int x = 0; x < mapChunkSize + 2; x++)
                {
                    if (terrainData.useFalloff)
                    {
                        noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - falloffMap[x, y]);
                    }

                }
            }

        }

        return new MapData(noiseMap);
    }

    void OnValidate()
    {

        if (terrainData != null)
        {
            terrainData.OnValuesUpdated -= OnValuesUpdated;
            terrainData.OnValuesUpdated += OnValuesUpdated;
        }
        if (noiseData != null)
        {
            noiseData.OnValuesUpdated -= OnValuesUpdated;
            noiseData.OnValuesUpdated += OnValuesUpdated;
        }
        if (textureData != null)
        {
            textureData.OnValuesUpdated -= OnTextureValuesUpdated;
            textureData.OnValuesUpdated += OnTextureValuesUpdated;
        }

    }

    struct MapThreadInfo<T>
    {
        public readonly Action<T> callback;
        public readonly T parameter;

        public MapThreadInfo(Action<T> callback, T parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }

    }

}


public struct MapData
{
    public readonly float[,] heightMap;


    public MapData(float[,] heightMap)
    {
        this.heightMap = heightMap;
    }
}
