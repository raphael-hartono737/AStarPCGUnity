using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public enum TerrainType { Water, Flat, Steep }
public enum RoadType { None, MainRoad, Branch }
public enum RoadSegment { Straight, Curve, TJunc, Cross, DeadEnd }

[System.Serializable]
public class TerrainCell
{
    // Now holds the exact Y‐coordinate from the raycast hit.
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
    [Header("Water Detection (Raycast)")]
    [Tooltip("Raycast origin Y‐coordinate.")]
    public float rayOriginY = 100f;
    [Tooltip("Raycast checks down to this Y‐coordinate.")]
    public float rayTargetY = 37f;

    [Header("Grid Settings")]
    public int gridSize = 250;
    public float noiseScale = 0.1f;

    [Header("Pathfinding")]
    public float steepCost = 10f;
    public float flatCost = 1f;
    public bool processClosestFirst = true;
    private Vector3 worldPos;

    [Header("Temple Settings")]
    public string templeTag = "Temple";
    public string gatewayChildName = "Gateway";

    // **Replace** the full‐grid array with a dictionary for lazy initialization:
    private Dictionary<Vector2Int, TerrainCell> _cellDict = new Dictionary<Vector2Int, TerrainCell>();
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

    [Header("Terrain Classification")]
    [Tooltip("Any cell with height above this value is considered ‘Steep’.")]
    public float steepThreshold = 10f;

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
        BuildWaterCellsFromChunks();
        chunksReady = true;
        TryGenerateRoads();
    }

    private void TryGenerateRoads()
    {
        if (!IsInitialized && mapReady && chunksReady)
        {
            GenerateRoadSystem();
            IsInitialized = true;
        }
    }

    private void BuildWaterCellsFromChunks()
    {
        waterCells.Clear();

        int chunkSize = mapGenerator.mapChunkSize;
        float scale = mapGenerator.terrainData.uniformScale;

        foreach (var kv in EndlessTerrain.terrainChunkDictionary)
        {
            Vector2 chunkCoord = kv.Key;
            var chunk = kv.Value;
            if (chunk == null || chunk.mapData.heightMap == null) continue;

            for (int localY = 0; localY < chunk.mapData.heightMap.GetLength(1); localY++)
            {
                for (int localX = 0; localX < chunk.mapData.heightMap.GetLength(0); localX++)
                {
                    float h = chunk.mapData.heightMap[localX, localY];
                    if (h <= mapGenerator.terrainData.minHeight)
                    {
                        int gx = Mathf.RoundToInt(chunkCoord.x * chunkSize + localX);
                        int gy = Mathf.RoundToInt(chunkCoord.y * chunkSize + localY);
                        waterCells.Add(new Vector2Int(gx, gy));
                    }
                }
            }
        }
    }

    public float SampleTerrainHeight(Vector3 worldPos)
    {
        // (Unchanged; used only for deciding steep/flat.)
        float uscale = mapGenerator.terrainData.uniformScale;
        float luaX = worldPos.x / uscale;
        float luaZ = worldPos.z / uscale;

        int chunkSize = mapGenerator.mapChunkSize - 1;
        int cx = Mathf.RoundToInt(luaX / chunkSize);
        int cz = Mathf.RoundToInt(luaZ / chunkSize);
        var key = new Vector2(cx, cz);

        if (!EndlessTerrain.terrainChunkDictionary.TryGetValue(key, out var chunk) ||
            chunk.mapData.heightMap == null)
        {
            return 0f;
        }

        float offsetX = luaX - cx * chunkSize;
        float offsetZ = luaZ - cz * chunkSize;

        var hm = chunk.mapData.heightMap;
        int hms = hm.GetLength(0);

        float sampleX = offsetX / chunkSize * (hms - 1);
        float sampleZ = offsetZ / chunkSize * (hms - 1);

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

        if (!FindStartPoint())
        {
            Debug.LogError("[MainRoadGenerator] No valid start point found.");
            return;
        }

        FindEndpoints();
        if (processClosestFirst)
            OrderEndpointsByDistance();

        CalculateMainRoad();                    // A* will fill only the visited cells in _cellDict
        BuildFullGridFromDictionary();          // Now build a full 2D array from those visited cells
        ClassifySegments();                     // Classify road segments on the completed array

        IsInitialized = true;
        var branchGen = GetComponent<BranchGenerator>();
        if (branchGen != null)
            branchGen.GenerateBranchPaths();

        RoadGenerationEvents.OnRoadGenerationComplete?.Invoke();
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

        foreach (var sp in starts)
            if (sp != selected)
                Destroy(sp);

        worldPos = selected.transform.position;
        roadData.start = WorldToGridPosition(worldPos);

        if (!IsInBounds(roadData.start))
        {
            Debug.LogError($"[MainRoadGenerator] Start {roadData.start} out of grid bounds!");
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
                Debug.LogError($"[MainRoadGenerator] No gateway in temple {temple.name}");
                continue;
            }
            var gp = WorldToGridPosition(gw.position);
            if (!IsInBounds(gp))
            {
                Debug.LogError($"[MainRoadGenerator] Gateway {gp} out of bounds");
                continue;
            }
            roadData.endPoints.Add(gp);
        }
        if (roadData.endPoints.Count == 0)
            Debug.LogError("[MainRoadGenerator] No valid gateways found.");
    }

    void CalculateMainRoad()
    {
        // Mark the start cell as MainRoad (this will lazy‐init it in _cellDict)
        var startCell = GetCellAt(roadData.start);
        startCell.road = RoadType.MainRoad;

        var fullList = new List<Vector2Int> { roadData.start };

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
                Debug.LogError("[MainRoadGenerator] No path found to endpoint " + end);
                continue;
            }

            fullList.AddRange(path);
            foreach (var p in path)
            {
                var c = GetCellAt(p);
                c.road = RoadType.MainRoad;
            }
        }

        roadData.path = fullList.Distinct().ToArray();
    }

    public List<Vector2Int> GetNeighbors(Vector2Int pos)
    {
        var list = new List<Vector2Int>();
        foreach (var d in Directions)
        {
            var n = pos + d;
            if (!IsInBounds(n)) continue;

            // Treat any cell classified as Water as impassable:
            if (GetCellAt(n).terrain == TerrainType.Water)
                continue;

            list.Add(n);
        }
        return list;
    }

    public bool IsInBounds(Vector2Int pos) =>
        pos.x >= 0 && pos.y >= 0 && pos.x < gridSize && pos.y < gridSize;

    public void ClassifySegments()
    {
        if (isClassified || grid == null) return;

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                if (grid[x, y].road == RoadType.MainRoad)
                {
                    grid[x, y].segment = CalculateSegmentType(new Vector2Int(x, y));
                }
            }
        }

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

    // -------------------------------------------------------------------------
    // Modified: Use the stored 'height' from each TerrainCell instead of re-sampling.
    public Vector3 GridToWorldPosition(Vector2Int gridPos)
    {
        float half = gridSize / 2f;
        var p = new Vector3(
            gridPos.x - half,
            0,
            gridPos.y - half
        );

        // subtract 0.4f to sink the road by 0.4 units:
        float storedHeight = grid[gridPos.x, gridPos.y].height;
        p.y = storedHeight - 0.4f;
        return p;
    }

    void OrderEndpointsByDistance()
    {
        roadData.endPoints = roadData.endPoints
            .OrderBy(e => Vector2Int.Distance(roadData.start, e))
            .ToList();
    }

    // -------------------------------------------------------------------------
    // “Lazy initialization” helper. Creates a TerrainCell on first access,
    // running a downward raycast from y=rayOriginY to y=rayTargetY. If no hit,
    // classify as Water. Otherwise use the exact hit.point.y as 'height', and
    // still apply steepThreshold on the sampled height to decide Flat/Steep.
    private TerrainCell GetCellAt(Vector2Int pos)
    {
        if (_cellDict.TryGetValue(pos, out var existing))
            return existing;

        // --- Instead of calling GridToWorldPosition(pos), compute world X/Z directly:
        float half = gridSize / 2f;
        float worldX = pos.x - half;
        float worldZ = pos.y - half;
        Vector3 samplePoint = new Vector3(worldX, 0, worldZ);
        float rawHeightMap = SampleTerrainHeight(samplePoint);

        // Raycast from (worldX, rayOriginY, worldZ) down to (worldX, rayTargetY, worldZ):
        Vector3 rayOrigin = new Vector3(worldX, rayOriginY, worldZ);
        float rayDistance = rayOriginY - rayTargetY;

        bool hitSomething = Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hitInfo, rayDistance);

        TerrainType type;
        float storedWorldY = rayTargetY; // fallback if no hit

        if (!hitSomething)
        {
            type = TerrainType.Water;
        }
        else
        {
            storedWorldY = hitInfo.point.y;

            // Decide Flat vs. Steep using rawHeightMap:
            if (rawHeightMap > steepThreshold)
                type = TerrainType.Steep;
            else
                type = TerrainType.Flat;
        }

        var newCell = new TerrainCell
        {
            height = storedWorldY,
            terrain = type,
            road = RoadType.None,
            segment = RoadSegment.Straight
        };

        _cellDict[pos] = newCell;
        return newCell;
    }

    // After A* finishes, build a full 2D array from all visited cells
    private void BuildFullGridFromDictionary()
    {
        grid = new TerrainCell[gridSize, gridSize];

        // First fill everything with a default “flat, no‐road” cell:
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                grid[x, y] = new TerrainCell
                {
                    height = 0f,
                    terrain = TerrainType.Flat,
                    road = RoadType.None,
                    segment = RoadSegment.Straight
                };
            }
        }

        // Overwrite only the positions we actually “touched”:
        foreach (var kv in _cellDict)
        {
            Vector2Int p = kv.Key;
            if (p.x < 0 || p.y < 0 || p.x >= gridSize || p.y >= gridSize)
                continue;
            grid[p.x, p.y] = kv.Value;
        }
    }
}
