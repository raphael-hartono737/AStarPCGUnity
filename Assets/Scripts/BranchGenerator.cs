using UnityEngine;
using System.Collections.Generic;
using static UnityEditor.PlayerSettings;
using System.Collections;

public class BranchGenerator : MonoBehaviour
{
    [Header("Generation Settings")]
    [Range(0, 1)] public float spawnChance = 0.2f;
    public int maxLength = 15;
    [Range(0, 1)] public float turnProbability = 0.3f;

    public int branchCounter = 0;
    public int maxBranches = 3; // Maximum T-junctions to spawn
    public int minDistanceBetweenBranches = 5; // Minimum spacing between branches
    public int minDistanceFromEnd = 5; // Don't branch near start/end
    public int branchInterval = 10; // Only allow branching every N tiles
    [SerializeField] private MainRoadGenerator mainRoad;
    private TerrainCell[,] grid;

    private List<Vector2Int> branchPositions = new List<Vector2Int>();

    void OnEnable()
    {
        RoadGenerationEvents.OnRoadGenerationComplete += HandleRoadGenerationComplete;
    }

    void OnDisable()
    {
        RoadGenerationEvents.OnRoadGenerationComplete -= HandleRoadGenerationComplete;
    }

    private void HandleRoadGenerationComplete()
    {
        mainRoad = GetComponent<MainRoadGenerator>();
        if (mainRoad == null || mainRoad.Grid == null || mainRoad.RoadData?.path == null)
        {
            Debug.LogError("Missing road data references!");
            return;
        }

        GenerateBranchPaths();
    }

    public void GenerateBranchPaths()
    {
        tJunctionPositions.Clear(); 
        branchPositions.Clear();
        if (mainRoad == null || mainRoad.Grid == null || mainRoad.RoadData?.path == null)
        {
            Debug.LogError("Missing road data references!");
            return;
        }

        grid = mainRoad.Grid;
        var mainPath = mainRoad.RoadData.path;

        branchPositions.Clear();
        int branchesCreated = 0;
        int branchCounter = 0;


        for (int i = 0; i < mainPath.Length; i++)
        {
            Vector2Int pos = mainPath[i];
            branchCounter++;

            Debug.Log($"[BranchGen] Evaluating position: {pos} (Index: {i})");

            // Log every check step to find where it fails
            if (IsTooClose(pos, branchPositions))
            {
                Debug.Log($"[BranchGen] Skipped: Too close to existing branch");
                continue;
            }
            foreach (var temple in mainRoad.RoadData.endPoints)
                if (Vector2Int.Distance(pos, mainRoad.RoadData.start) < minDistanceFromEnd ||
                Vector2Int.Distance(pos, temple) < minDistanceFromEnd)
            {
                Debug.Log($"[BranchGen] Skipped: Too close to road ends");
                continue;
            }

            if (branchesCreated >= maxBranches)
            {
                Debug.Log($"[BranchGen] Skipped: Max branches reached ({maxBranches})");
                break;
            }

            if (branchCounter % branchInterval != 0)
            {
                Debug.Log($"[BranchGen] Skipped: Not at branch interval ({branchCounter}/{branchInterval})");
                continue;
            }

            if (grid[pos.x, pos.y].segment != RoadSegment.Straight)
            {
                Debug.Log($"[BranchGen] Skipped: Not a straight segment (Current: {grid[pos.x, pos.y].segment})");
                continue;
            }

            if (Random.value > spawnChance)
            {
                Debug.Log($"[BranchGen] Skipped: Random chance failed (spawnChance={spawnChance})");
                continue;
            }


            var (roadDir, branchDir) = CalculateBranchDirection(pos, mainPath);
            if (branchDir == Vector2Int.zero)
            {
                Debug.Log($"[BranchGen] Skipped: Invalid branch direction");
                continue;
            }
            Debug.Log($"[BranchGen] About to call WalkPath at {pos} with direction {branchDir}");
            bool success = WalkPath(pos, branchDir, maxLength);
            if (!success)
            {
                Debug.Log($"[BranchGen] WalkPath failed at {pos}");
                continue;
            }

            branchesCreated++;
            branchPositions.Add(pos);
            Debug.Log($"[BranchGen] Branch created at {pos}");
        }
        mainRoad.ClassifySegments(); // Reclassify after branches
        mainRoad.GetComponent<RoadVisualizer>()?.RefreshVisualization();
        GetComponent<BuildingGenerator>()?.GenerateBuildings();
    }

    public (Vector2Int roadDir, Vector2Int branchDir) CalculateBranchDirection(Vector2Int pos, Vector2Int[] mainPath)
    {
        int index = System.Array.IndexOf(mainPath, pos);
        if (index < 0 || index >= mainPath.Length)
        {
            Debug.Log($"[BranchGen] Position {pos} not found in main path");
            return (Vector2Int.zero, Vector2Int.zero);
        }
            
        Vector2Int roadDir = index == 0
            ? mainPath[1] - mainPath[0]
            : mainPath[index] - mainPath[index - 1];

        // Try both directions
        Vector2Int leftDir = new Vector2Int(-roadDir.y, roadDir.x);
        Vector2Int rightDir = new Vector2Int(roadDir.y, -roadDir.x);
        Debug.Log($"[BranchGen] Calculating branch from {pos}, road direction: {roadDir}");
        Debug.Log($"[BranchGen] Checking Left: {pos + leftDir}");
        Debug.Log($"[BranchGen] Checking Right: {pos + rightDir}");
        bool leftValid = IsValidBranchDirection(pos + leftDir);
        bool rightValid = IsValidBranchDirection(pos + rightDir);
        // Check which direction is valid
        if (leftValid && rightValid)
        {
            return (roadDir, Random.value > 0.5f ? leftDir : rightDir);
        }
        // Fallback to whichever is valid
        else if (leftValid)
        {
            return (roadDir, leftDir);
        }
        else if (rightValid)
        {
            return (roadDir, rightDir);
        }

        Debug.LogWarning($"No valid branch direction at {pos}!");
        return (roadDir, Vector2Int.zero);
    }

    // Helper to check if branch can be placed here
    private bool IsValidBranchDirection(Vector2Int testPos)
    {
        return IsValidCell(testPos) && grid[testPos.x, testPos.y].road == RoadType.None;
    }

    private List<Vector2Int> tJunctionPositions = new List<Vector2Int>();

    bool WalkPath(Vector2Int current, Vector2Int direction, int maxSteps)
    {
        if (grid == null)
        {
            Debug.LogError("[BranchGen] Grid is null in WalkPath()");
            return false;
        }
        Debug.Log($"[BranchGen] Starting WalkPath at {current} with direction {direction}");
        Vector2Int branchStart = current + direction;

        // At this point, direction should already be validated
        Debug.Log($"[BranchGen] Attempting to place branch at {branchStart}");
        if (!IsValidCell(branchStart) || grid[branchStart.x, branchStart.y].terrain == TerrainType.Water || grid[branchStart.x, branchStart.y].road != RoadType.None)
        {
            Debug.Log($"[BranchGenerator] ❌ Can't place branch at {branchStart}");
            return false;
        }
        // Place branch tiles
        if (grid[current.x, current.y].segment == RoadSegment.Straight)
        {
            grid[current.x, current.y].segment = RoadSegment.TJunc;
            Debug.Log($"✅ [BranchGenerator] Marked TJunction at ({current.x}, {current.y})");
        }
        else
        {
            Debug.LogWarning($"❌ [BranchGenerator] Cannot mark TJunction — segment is {grid[current.x, current.y].segment}");
            return false;
        }
        grid[branchStart.x, branchStart.y].road = RoadType.Branch;
        Vector2Int currentBranch = branchStart;

        for (int steps = 0; steps < maxSteps; steps++)
        {
            Vector2Int next = currentBranch + direction;
            if (!IsValidCell(next) ||
                grid[next.x, next.y].terrain == TerrainType.Water ||
                grid[next.x, next.y].road != RoadType.None)
            {
                break;
            }

            grid[next.x, next.y].road = RoadType.Branch;
            currentBranch = next;

            // Add random turn
            if (Random.value < turnProbability)
            {
                direction = Random.value > 0.5f
                    ? new Vector2Int(-direction.y, direction.x)
                    : new Vector2Int(direction.y, -direction.x);
            }
        }
        return true;
    }

    bool IsValidCell(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < grid.GetLength(0) &&
               pos.y >= 0 && pos.y < grid.GetLength(1);
    }

    bool IsTooClose(Vector2Int pos, List<Vector2Int> branchPositions)
    {
    foreach (var branchPos in branchPositions)
    {
        if (Vector2.Distance(pos, branchPos) < minDistanceBetweenBranches)
        {
            return true;
        }
    }
    return false;
    }
}