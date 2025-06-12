using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct MirrorDebugData
{
    public Vector2Int originalPos;
    public Vector2Int mirroredPos;
    public bool isValid;
    public string reason;
}

public class BuildingGenerator : MonoBehaviour
{
    [Header("Settings")]
    public int maxBuildings = 10;
    public float buildingYOffset = 0.5f;
    public float placementHeightCheck = 5f;

    [Header("Building Prefabs")]
    public GameObject[] buildingPrefabs;

    [Header("Visualization")]
    public Color[] buildingZoneColors;
    public bool drawGizmos = true;

    [SerializeField] private MainRoadGenerator roadGenerator;
    private TerrainCell[,] grid;
    private List<BuildingZone> buildingZones = new List<BuildingZone>();
    private List<MirrorDebugData> mirrorDebugLogs = new List<MirrorDebugData>();
    private int currentPrefabIndex = 0;
    private int currentColorIndex = 0;

    private class BuildingZone
    {
        public Vector2Int gridPosition;
        public Vector3Int size;
        public Color zoneColor;
        public GameObject buildingInstance;
        public List<Vector2Int> occupiedCells = new List<Vector2Int>();
    }

    void OnEnable() => RoadGenerationEvents.OnRoadGenerationComplete += GenerateBuildings;
    void OnDisable() => RoadGenerationEvents.OnRoadGenerationComplete -= GenerateBuildings;

    public void GenerateBuildings()
    {
        if (!ValidateDependencies()) return;

        CleanupPreviousBuildings();
        AttemptBuildingPlacement();
    }

    void AttemptBuildingPlacement()
    {
        int buildingsPlaced = 0;
        List<Vector2Int> potentialSites = FindPotentialBuildingSites();

        while (buildingsPlaced < maxBuildings && potentialSites.Count > 0)
        {
            Vector2Int site = GetRandomSite(ref potentialSites);
            GameObject nextPrefab = GetNextPrefab();

            if (TryPlaceBuilding(site, nextPrefab, out BuildingZone newZone))
            {
                buildingZones.Add(newZone);
                buildingsPlaced++;
            }
        }

        Debug.Log($"Successfully placed {buildingsPlaced}/{maxBuildings} buildings");
    }

    bool TryPlaceBuilding(Vector2Int site, GameObject prefab, out BuildingZone zone)
    {
        zone = null;
        // 1) Figure out prefab’s footprint size
        Vector3Int size = CalculatePrefabSize(prefab);

        // 2) Can we place here?
        if (!CanPlaceBuilding(site, size))
            return false;

        // 3) Compute world‐space position (centers over the grid cell and Y‐raycasts) 
        Vector3 worldPosition = CalculateWorldPosition(site, size);

        // 4) Detect which neighbouring cell is road‐occupied
        Vector2Int roadDir = Vector2Int.zero;
        foreach (var dir in MainRoadGenerator.Directions)
        {
            Vector2Int neighbour = site + dir;
            if (roadGenerator.IsInBounds(neighbour) && grid[neighbour.x, neighbour.y].road != RoadType.None)
            {
                roadDir = dir;
                break;
            }
        }

        // 5) Build rotation so +Z “forward” faces toward the road
        Quaternion rotation;
        if (roadDir != Vector2Int.zero)
        {
            Vector3 lookDir = new Vector3(roadDir.x, 0, roadDir.y);
            rotation = Quaternion.LookRotation(lookDir, Vector3.up);
        }
        else
        {
            // fallback: no adjacent road found
            rotation = Quaternion.identity;
        }

        // 6) Instantiate with the computed rotation
        GameObject buildingInstance = Instantiate(prefab, worldPosition, rotation, transform);
        buildingInstance.name = $"Building_{currentPrefabIndex}";

        // 7) Register zone for occupancy and gizmos
        zone = CreateBuildingZone(site, size, buildingInstance);
        return true;
    }

    Vector3Int CalculatePrefabSize(GameObject prefab)
    {
        Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
        Renderer renderer = prefab.GetComponentInChildren<Renderer>();
        if (renderer) bounds = renderer.bounds;

        return new Vector3Int(
            Mathf.CeilToInt(bounds.size.x),
            Mathf.CeilToInt(bounds.size.y),
            Mathf.CeilToInt(bounds.size.z)
        );
    }

    GameObject InstantiateBuilding(GameObject prefab, Vector3 position)
    {
        GameObject instance = Instantiate(prefab, position, Quaternion.identity, transform);
        instance.name = $"Building_{currentPrefabIndex}";
        return instance;
    }

    BuildingZone CreateBuildingZone(Vector2Int gridPos, Vector3Int size, GameObject instance)
    {
        Color zoneColor = GetNextZoneColor();
        BuildingZone zone = new BuildingZone
        {
            gridPosition = gridPos,
            size = size,
            zoneColor = zoneColor,
            buildingInstance = instance
        };
        CalculateOccupiedCells(gridPos, size, zone.occupiedCells);
        return zone;
    }

    void CalculateOccupiedCells(Vector2Int origin, Vector3Int size, List<Vector2Int> cells)
    {
        cells.Clear();
        for (int x = origin.x; x < origin.x + size.x; x++)
        {
            for (int y = origin.y; y < origin.y + size.z; y++)
            {
                cells.Add(new Vector2Int(x, y));
            }
        }
    }

    Vector3 CalculateWorldPosition(Vector2Int gridPos, Vector3Int size)
    {
        // Get bottom-left world corner of the cell
        Vector3 origin = roadGenerator.GridToWorldPosition(gridPos);
        // Center in grid-space: half extents
        Vector3 offset = new Vector3((size.x - 1) * 0.5f, 0, (size.z - 1) * 0.5f);
        Vector3 center = origin + offset;

        // Raycast for Y
        Vector3 rayStart = center + Vector3.up * placementHeightCheck;
        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, placementHeightCheck * 2))
        {
            center.y = hit.point.y + buildingYOffset;
        }
        else
        {
            center.y = buildingYOffset;
        }

        return center;
    }

    #region Helper Methods
    bool ValidateDependencies()
    {
        if (roadGenerator == null)
            roadGenerator = GetComponent<MainRoadGenerator>() ?? FindObjectOfType<MainRoadGenerator>();
        if (roadGenerator == null || !roadGenerator.IsInitialized)
        {
            Debug.LogError("MainRoadGenerator missing or not initialized!");
            return false;
        }

        grid = roadGenerator.Grid;
        if (grid == null)
        {
            Debug.LogError("Road grid not available!");
            return false;
        }

        if (buildingPrefabs == null || buildingPrefabs.Length == 0)
        {
            Debug.LogError("No building prefabs assigned!");
            return false;
        }

        return true;
    }

    void CleanupPreviousBuildings()
    {
        foreach (BuildingZone zone in buildingZones)
            if (zone.buildingInstance != null)
                Destroy(zone.buildingInstance);
        buildingZones.Clear();
        currentPrefabIndex = 0;
        currentColorIndex = 0;
    }

    Color GetNextZoneColor()
    {
        if (buildingZoneColors.Length == 0) return Color.white;
        Color color = buildingZoneColors[currentColorIndex];
        currentColorIndex = (currentColorIndex + 1) % buildingZoneColors.Length;
        return color;
    }

    GameObject GetNextPrefab()
    {
        GameObject prefab = buildingPrefabs[currentPrefabIndex];
        currentPrefabIndex = (currentPrefabIndex + 1) % buildingPrefabs.Length;
        return prefab;
    }

    Vector2Int GetRandomSite(ref List<Vector2Int> sites)
    {
        int randomIndex = Random.Range(0, sites.Count);
        Vector2Int site = sites[randomIndex];
        sites.RemoveAt(randomIndex);
        return site;
    }

    bool CanPlaceBuilding(Vector2Int origin, Vector3Int size)
    {
        // Boundary check
        if (origin.x < 0 || origin.y < 0 ||
            origin.x + size.x > grid.GetLength(0) ||
            origin.y + size.z > grid.GetLength(1))
            return false;

        // Occupancy check
        for (int x = origin.x; x < origin.x + size.x; x++)
        {
            for (int y = origin.y; y < origin.y + size.z; y++)
            {
                if (grid[x, y].road != RoadType.None || IsCellOccupied(new Vector2Int(x, y)))
                    return false;
            }
        }
        return true;
    }

    bool IsCellOccupied(Vector2Int cell)
    {
        foreach (BuildingZone zone in buildingZones)
            if (zone.occupiedCells.Contains(cell))
                return true;
        return false;
    }

    // Provides reason why a building cannot be placed at the given position
    string GetInvalidReason(Vector2Int pos)
    {
        if (!roadGenerator.IsInBounds(pos))
            return "Out of bounds";
        if (grid[pos.x, pos.y].road != RoadType.None)
            return "Road exists";
        if (IsCellOccupied(pos))
            return "Occupied by building";
        return "Unknown";
    }

    #endregion

    #region Road Connection Logic
    List<Vector2Int> FindPotentialBuildingSites()
    {
        mirrorDebugLogs.Clear();
        List<Vector2Int> sites = new List<Vector2Int>();
        int width = grid.GetLength(0), height = grid.GetLength(1);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2Int currentPos = new Vector2Int(x, y);
                if (grid[x, y].road == RoadType.None && IsAdjacentToRoad(currentPos))
                {
                    if (Random.value > 0.5f)
                    {
                        Vector2Int mirroredPos = GetMirroredPosition(currentPos);
                        bool valid = IsValidForBuilding(mirroredPos);
                        string reason = valid ? "Valid" : GetInvalidReason(mirroredPos);
                        mirrorDebugLogs.Add(new MirrorDebugData { originalPos = currentPos, mirroredPos = mirroredPos, isValid = valid, reason = reason });
                        if (valid) sites.Add(mirroredPos);
                        else sites.Add(currentPos);
                    }
                    else
                    {
                        sites.Add(currentPos);
                    }
                }
            }
        }
        return sites;
    }

    Vector2Int GetMirroredPosition(Vector2Int pos)
    {
        foreach (var dir in MainRoadGenerator.Directions)
        {
            Vector2Int neighbor = pos + dir;
            if (roadGenerator.IsInBounds(neighbor) && grid[neighbor.x, neighbor.y].road != RoadType.None)
            {
                Vector2Int mirrored = pos + dir * 2;
                if (roadGenerator.IsInBounds(mirrored)) return mirrored;
            }
        }
        return pos;
    }

    bool IsValidForBuilding(Vector2Int pos)
    {
        return roadGenerator.IsInBounds(pos) && grid[pos.x, pos.y].road == RoadType.None && !IsCellOccupied(pos);
    }

    bool IsAdjacentToRoad(Vector2Int cell)
    {
        foreach (var dir in MainRoadGenerator.Directions)
        {
            Vector2Int neighbor = cell + dir;
            if (roadGenerator.IsInBounds(neighbor) && grid[neighbor.x, neighbor.y].road != RoadType.None)
                return true;
        }
        return false;
    }
    #endregion

    #region Gizmos
    void OnDrawGizmos()
    {
        if (!drawGizmos || buildingZones == null) return;
        foreach (BuildingZone zone in buildingZones)
        {
            Gizmos.color = zone.zoneColor;
            Vector3 origin = roadGenerator.GridToWorldPosition(zone.gridPosition);
            Vector3 offset = new Vector3((zone.size.x - 1) * 0.5f, buildingYOffset, (zone.size.z - 1) * 0.5f);
            Vector3 center = origin + offset;
            Gizmos.DrawWireCube(center, new Vector3(zone.size.x, 0.1f, zone.size.z));
        }
    }
    #endregion
}
