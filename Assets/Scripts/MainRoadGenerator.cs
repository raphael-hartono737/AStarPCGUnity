using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public enum TerrainType { Water, Flat, Steep }
public enum RoadType { None, MainRoad, Branch }
public enum RoadSegment { Straight, Curve, TJunc, Cross, DeadEnd }

[System.Serializable]
public class TerrainCell
{
    public float height;
    public TerrainType terrain;
    public RoadType road;
    public RoadSegment segment;
}

[System.Serializable]
public class RoadData
{
    public Vector2Int[] path;
    public Vector2Int start;
    public List<Vector2Int> endPoints = new List<Vector2Int>();
}

public static class RoadGenerationEvents
{
    public static System.Action OnRoadGenerationComplete;
}

public class MainRoadGenerator : MonoBehaviour
{
    [Header("Water Detection")]
    public LayerMask waterLayer;
    public float waterMinY = 25f;
    public float waterMaxY = 30f;
    public Color waterCellColor = new Color(0, 0.5f, 1f, 0.3f);

    [Header("Grid Settings")]
    public int gridSize = 2500;
    public float noiseScale = 0.1f;

    [Header("Pathfinding")]
    public float steepCost = 10f;
    public float flatCost = 1f;
    public bool processClosestFirst = true;
    private Vector3 worldPos;

    [Header("Temple Settings")]
    public string templeTag = "Temple";
    public string gatewayChildName = "Gateway";

    private TerrainCell[,] grid;
    private RoadData roadData = new RoadData();

    public TerrainCell[,] Grid => grid;
    public RoadData RoadData => roadData;
    [SerializeField] private MapGenerator mapGenerator;
    [SerializeField] private EndlessTerrain endlessTerrain;

    [Header("Grid Visualization")]
    public bool showGrid = true;
    public Color gridColor = Color.gray;
    [Range(1, 100)] public int gridStep = 100;
    public Color startPointColor = Color.green;
    public Color endPointColor = Color.red;
    public float pointRadius = 10f; 
    private HashSet<Vector2Int> waterCells = new HashSet<Vector2Int>();

    [Header("Debug")]
    public int debugStep = 100;

    private bool isGenerated = false;
    private bool isClassified = false;
    private bool mapReady = false;
    private bool chunksReady = false;
    public bool IsInitialized { get; private set; }
    [SerializeField] private string startPointTag = "StartingPoint";

    public static readonly Vector2Int[] Directions = new[]
    {
        new Vector2Int(-1, 0),
        new Vector2Int(1, 0),
        new Vector2Int(0, -1),
        new Vector2Int(0, 1)
    };

    void OnEnable()
    {
        MapGenerator.OnMapGenerationComplete += OnMapReady;
        EndlessTerrain.OnChunksUpdated += OnChunksReady;
    }

    void OnDisable()
    {
        MapGenerator.OnMapGenerationComplete -= OnMapReady;
        EndlessTerrain.OnChunksUpdated -= OnChunksReady;
    }

    private void OnMapReady()
    {
        mapReady = true;
        TryGenerateRoads();
    }

    private void OnChunksReady()
    {
        // First build water cells from the heightmaps
        BuildWaterCellsFromChunks();
        chunksReady = true;
        TryGenerateRoads();
    }

    // Only run once both map and chunks are ready
    private void TryGenerateRoads()
    {
        if (!IsInitialized && mapReady && chunksReady)
        {
            GenerateRoadSystem();
            IsInitialized = true;
        }
    }

    void BuildWaterCellsFromChunks()
    {
        waterCells.Clear();

        int chunkSize = mapGenerator.mapChunkSize;
        Debug.Log("chunkSize: " + chunkSize + " (Map Generator's chunk size)");
        float scale = mapGenerator.terrainData.uniformScale;
        Debug.Log("Uniform scale: " + scale);

        foreach (var kv in EndlessTerrain.terrainChunkDictionary)
        {
            Vector2 chunkCoord = kv.Key;
            var chunk = kv.Value;
            if (chunk == null)
            {
                Debug.LogError($"Null chunk found at coordinate {kv.Key}");
                continue;
            }
            var mapData = chunk.mapData;
            if (mapData.heightMap == null) continue;

            // loop local chunk coords
            for (int localY = 0; localY < mapData.heightMap.GetLength(1); localY++)
            {
                for (int localX = 0; localX < mapData.heightMap.GetLength(0); localX++)
                {
                    float h = mapData.heightMap[localX, localY];
                    
                    // choose your threshold; e.g. terrainData.minHeight:
                    if (h <= mapGenerator.terrainData.minHeight)
                    {
                        // convert to global grid index:
                        int gx = Mathf.RoundToInt(chunkCoord.x * chunkSize + localX);
                        int gy = Mathf.RoundToInt(chunkCoord.y * chunkSize + localY);
                        var cell = new Vector2Int(gx, gy);
                        waterCells.Add(cell);
                    }
                }
            }
        }
    }
    private Dictionary<Vector2, EndlessTerrain.TerrainChunk> chunkDict
    => EndlessTerrain.terrainChunkDictionary;
    private void BuildHeightSampler() { /* no-op for now */ }

    public float SampleTerrainHeight(Vector3 worldPos)
    {
        // 1) Convert to chunk‐local units (unscaled)
        float uscale = mapGenerator.terrainData.uniformScale;
        float luaX = worldPos.x / uscale;
        float luaZ = worldPos.z / uscale;

        // 2) Determine which chunk we're in
        int chunkSize = mapGenerator.mapChunkSize - 1;   // matches EndlessTerrain
        int cx = Mathf.RoundToInt(luaX / chunkSize);
        int cz = Mathf.RoundToInt(luaZ / chunkSize);
        var key = new Vector2(cx, cz);

        if (!chunkDict.TryGetValue(key, out var chunk) || chunk.mapData.heightMap == null)
        {
            Debug.Log("[MainRoadGenerator] No Value");
            return 0f;  // fallback if outside
        }
           

        // 3) Compute local offsets inside that chunk
        float offsetX = luaX - cx * chunkSize;
        float offsetZ = luaZ - cz * chunkSize;

        var hm = chunk.mapData.heightMap;
        int hms = hm.GetLength(0);  // heightMap is square [hms × hms]

        // 4) Map offset → heightMap sample coordinates
        //    (if your mapData was generated at size N, then samples go 0..N-1)
        float sampleX = offsetX / chunkSize * (hms - 1);
        float sampleZ = offsetZ / chunkSize * (hms - 1);

        // 5) Bilinear‐interpolate between the four nearest samples
        int x0 = Mathf.Clamp(Mathf.FloorToInt(sampleX), 0, hms - 1);
        int z0 = Mathf.Clamp(Mathf.FloorToInt(sampleZ), 0, hms - 1);
        int x1 = Mathf.Clamp(x0 + 1, 0, hms - 1);
        int z1 = Mathf.Clamp(z0 + 1, 0, hms - 1);

        float tx = sampleX - x0;
        float tz = sampleZ - z0;

        float h00 = hm[x0, z0];
        float h10 = hm[x1, z0];
        float h01 = hm[x0, z1];
        float h11 = hm[x1, z1];

        float h0 = Mathf.Lerp(h00, h10, tx);
        float h1 = Mathf.Lerp(h01, h11, tx);
        float raw = Mathf.Lerp(h0, h1, tz);

        // 6) Apply your same height‐curve & multiplier used in mesh generation
        var td = mapGenerator.terrainData;
        float curved = td.meshHeightCurve.Evaluate(raw);
        return curved * td.meshHeightMultiplier;
    }

    public void GenerateRoadSystem()
    {
        if (isGenerated)
        {
            Debug.LogWarning("[MainRoadGenerator] Already generated. Skipping.");
            return;
        }

        Debug.Log("[MainRoadGenerator] Generating road system...");

        isGenerated = true;
        InitializeGrid();
        if (!FindStartPoint())
        {
            Debug.Log("No Start Points, idiots!");
            return;
        }
        
        FindEndpoints();
        if (processClosestFirst)
            OrderEndpointsByDistance();
        CalculateMainRoad();
        

        ClassifySegments();

        IsInitialized = true;
        var branchGen = GetComponent<BranchGenerator>();
        if (branchGen != null)
            branchGen.GenerateBranchPaths();
        RoadGenerationEvents.OnRoadGenerationComplete?.Invoke();
    }

    void InitializeGrid()
    {
        grid = new TerrainCell[gridSize, gridSize];
        float half = gridSize / 2f;
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                float worldX = x - half;
                float worldZ = y - half;
                float noise = Mathf.PerlinNoise(worldX * noiseScale, worldZ * noiseScale);
                var cell = new TerrainCell
                {
                    terrain = TerrainType.Flat,
                    road = RoadType.None
                };

                if (waterCells.Contains(new Vector2Int(x, y)))
                    cell.terrain = TerrainType.Water;

                grid[x, y] = cell;
            }
        }
    }

    bool FindStartPoint()
    {
        var starts = GameObject.FindGameObjectsWithTag(startPointTag);
        var temples = GameObject.FindGameObjectsWithTag(templeTag);
        GameObject selected = null;
        float maxAvgDist = -1f;

        foreach (var sp in starts)
        {
            float total = 0f;
            int count = 0;
            foreach (var temple in temples)
            {
                var gw = temple.transform.Find(gatewayChildName);
                if (gw == null) continue;
                total += Vector3.Distance(sp.transform.position, gw.position);
                count++;
            }
            if (count == 0)
            {
                Debug.LogError("[MainRoadGenerator] No valid gateways in temples.");
                return false;
            }
            float avg = total / count;
            if (avg > maxAvgDist)
            {
                maxAvgDist = avg;
                selected = sp;
            }
        }

        if (selected == null)
        {
            Debug.LogError("[MainRoadGenerator] No starting point selected.");
            return false;
        }

        // Destroy extras
        foreach (var sp in starts)
            if (sp != selected)
                Destroy(sp);

        worldPos = selected.transform.position;
        roadData.start = WorldToGridPosition(worldPos);

        if (!IsInBounds(roadData.start))
        {
            Debug.LogError($"Start {roadData.start} out of grid bounds!");
            return false;
        }

        Debug.Log($"[MainRoadGenerator] Start at grid {roadData.start}, avgDist: {maxAvgDist:F2}");
        return true;
    }

    void FindEndpoints()
    {
        roadData.endPoints.Clear();
        var temples = GameObject.FindGameObjectsWithTag(templeTag);
        foreach (var temple in temples)
        {
            var gw = temple.transform.Find(gatewayChildName);
            if (gw == null)
            {
                Debug.LogError($"No gateway in temple {temple.name}");
                continue;
            }
            var gp = WorldToGridPosition(gw.position);
            if (!IsInBounds(gp))
            {
                Debug.LogError($"Gateway {gp} out of bounds");
                continue;
            }
            roadData.endPoints.Add(gp);
        }
        if (roadData.endPoints.Count == 0)
            Debug.LogError("[MainRoadGenerator] No valid gateways found.");
    }

    void CalculateMainRoad()
    {
        grid[roadData.start.x, roadData.start.y].road = RoadType.MainRoad;
        var full = new List<Vector2Int> { roadData.start };
        foreach (var end in roadData.endPoints)
        {
            var path = AStar.FindPath(
                start: roadData.start,
                end: end,
                getNeighbors: GetNeighbors,
                getCost: (current, neighbor) => flatCost,
                getHeuristic: (a, b) => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y)
            );
            if (path == null || path.Count == 0)
            {
                Debug.LogError("No path found!");
                continue;
            }
            full.AddRange(path);
            foreach (var p in path)
                grid[p.x, p.y].road = RoadType.MainRoad;
        }
        roadData.path = full.Distinct().ToArray();
    }

    public List<Vector2Int> GetNeighbors(Vector2Int pos)
    {
        var list = new List<Vector2Int>();
        foreach (var d in Directions)
        {
            var n = pos + d;
            if (IsInBounds(n)) list.Add(n);
        }
        return list;
    }

    public bool IsInBounds(Vector2Int pos) =>
        pos.x >= 0 && pos.y >= 0 && pos.x < gridSize && pos.y < gridSize;

    public void ClassifySegments()
    {
        if (isClassified) return;
        for (int x = 0; x < gridSize; x++)
            for (int y = 0; y < gridSize; y++)
                if (grid[x, y].road == RoadType.MainRoad)
                    grid[x, y].segment = CalculateSegmentType(new Vector2Int(x, y));
        isClassified = true;
    }

    RoadSegment CalculateSegmentType(Vector2Int pos)
    {
        var neigh = GetNeighbors(pos)
            .Where(n => grid[n.x, n.y].road != RoadType.None)
            .ToList();
        return neigh.Count switch
        {
            1 => RoadSegment.DeadEnd,
            2 => IsStraight(neigh[0], pos, neigh[1]) ? RoadSegment.Straight : RoadSegment.Curve,
            3 => RoadSegment.TJunc,
            4 => RoadSegment.Cross,
            _ => RoadSegment.Straight
        };
    }

    bool IsStraight(Vector2Int a, Vector2Int c, Vector2Int b)
        => (a.x == c.x && b.x == c.x) || (a.y == c.y && b.y == c.y);

    public Vector2Int WorldToGridPosition(Vector3 w)
    {
        float half = gridSize / 2f;
        int gx = Mathf.FloorToInt(w.x + half);
        int gy = Mathf.FloorToInt(w.z + half);
        return new Vector2Int(gx, gy);
    }

    public Vector3 GridToWorldPosition(Vector2Int gridPos)
    {
        float half = gridSize / 2f;
        var p = new Vector3(gridPos.x - half,
                            0,
                            gridPos.y - half);

        // sample the real terrain height here:
        float rawHeight = SampleTerrainHeight(p);

        // apply the same 20× scale your terrain GameObject has on Y:
        p.y = rawHeight * 40f;

        return p;
    }

    void OrderEndpointsByDistance()
    {
        roadData.endPoints = roadData.endPoints
            .OrderBy(e => Vector2Int.Distance(roadData.start, e))
            .ToList();
    }
}
