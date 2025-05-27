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
    public float raycastStartHeight = 100f;
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

    [Header("Grid Visualization")]
    public bool showGrid = true;
    public Color gridColor = Color.gray;
    [Range(1, 100)] public int gridStep = 100;
    public Color startPointColor = Color.green;
    public Color endPointColor = Color.red;
    public float pointRadius = 10f;

    [Header("Debug")]
    public bool debugRaycasts = true;
    public int debugStep = 100;

    private bool isGenerated = false;
    private bool isClassified = false;
    public bool IsInitialized { get; private set; }
    [SerializeField] private string startPointTag = "StartingPoint";

    public static readonly Vector2Int[] Directions = new[]
    {
        new Vector2Int(-1, 0),
        new Vector2Int(1, 0),
        new Vector2Int(0, -1),
        new Vector2Int(0, 1)
    };

    void Start()
    {
        MapGenerator.OnMapGenerationComplete += HandleMapGenerationComplete;
    }

    void OnEnable()
    {
        EndlessTerrain.OnChunksUpdated += HandleChunksReady;
    }
    void HandleMapGenerationComplete()
    {
        GenerateRoadSystem();
        MapGenerator.OnMapGenerationComplete -= HandleMapGenerationComplete;
    }

    void HandleChunksReady()
    {
        var chunks = EndlessTerrain.terrainChunkDictionary;
        Debug.Log("Chunks ready: " + chunks.Count);
        InitializeGrid(); 
    }
    void OnDestroy()
    {
        MapGenerator.OnMapGenerationComplete -= HandleMapGenerationComplete;
        EndlessTerrain.OnChunksUpdated -= HandleChunksReady;
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
        if (!FindStartPoint()) return;
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

                Vector3 rayOrigin = new Vector3(worldX, raycastStartHeight, worldZ);
                if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, Mathf.Infinity, waterLayer))
                {
                    bool isWater = hit.point.y >= waterMinY && hit.point.y <= waterMaxY;
                    if (debugRaycasts && x % debugStep == 0 && y % debugStep == 0)
                    {
                        Debug.Log($"Hit at {hit.point.y:F2} (water range: {waterMinY}-{waterMaxY}); Is water: {isWater}");
                    }
                    if (isWater)
                        cell.terrain = TerrainType.Water;
                }

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

    public Vector3 GridToWorldPosition(Vector2Int g)
    {
        float half = gridSize / 2f;
        return new Vector3(g.x - half, 27f, g.y - half);
    }

    void OrderEndpointsByDistance()
    {
        roadData.endPoints = roadData.endPoints
            .OrderBy(e => Vector2Int.Distance(roadData.start, e))
            .ToList();
    }
}
