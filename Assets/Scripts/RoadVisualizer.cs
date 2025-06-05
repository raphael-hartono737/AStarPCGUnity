using System.Collections.Generic;
using UnityEngine;

public class RoadVisualizer : MonoBehaviour
{
    public GameObject[] roadPrefabs; // Order: Straight, Curve, TJunc, Cross, DeadEnd
    //public float roadHeight = 0.1f;
    public float roadScale = 0.9f;
    private Dictionary<Vector2Int, GameObject> roadInstances = new Dictionary<Vector2Int, GameObject>();
    [SerializeField] private MainRoadGenerator mainRoad;

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
        if (mainRoad == null || mainRoad.Grid == null)
        {
            Debug.LogError("Grid not initialized!");
            return;
        }

        RefreshVisualization();
    }

    public void RefreshVisualization()
    {
        if (!mainRoad.IsInitialized || mainRoad.Grid == null)
        {
            Debug.LogError("Grid not initialized!");
            return;
        }

        // Remove obsolete visuals
        foreach (var kvp in roadInstances)
            Destroy(kvp.Value);
        roadInstances.Clear();

        // Add new visuals
        for (int x = 0; x < mainRoad.gridSize; x++)
        {
            for (int y = 0; y < mainRoad.gridSize; y++)
            {
                var cell = mainRoad.Grid[x, y];
                if (cell.road == RoadType.None) continue;

                Vector2Int pos = new Vector2Int(x, y);
                var segment = cell.segment;

                if (roadInstances.ContainsKey(pos))
                {
                    Debug.LogWarning($"[RoadVisualizer] Duplicate at {pos}");
                    continue;
                }

                Vector3 worldPos = mainRoad.GridToWorldPosition(pos);
                Quaternion rotation = GetRoadRotation(segment, pos);

                GameObject roadGO = Instantiate(roadPrefabs[(int)segment],worldPos,rotation,transform);
                roadGO.transform.localScale = Vector3.one * roadScale;

                Debug.Log($"[RoadVisualizer] Instantiated road at ({x},{y}) segment: {segment}");
                roadInstances[pos] = roadGO;
            }
        }
    }

    Quaternion GetRoadRotation(RoadSegment segment, Vector2Int pos)
    {
        var directions = new List<Vector2Int>();
        foreach (var dir in MainRoadGenerator.Directions)
        {
            var neighbor = pos + dir;
            if (mainRoad.IsInBounds(neighbor) && mainRoad.Grid[neighbor.x, neighbor.y].road != RoadType.None)
                directions.Add(dir);
        }

        switch (segment)
        {
            case RoadSegment.Straight:
                return directions.Contains(new Vector2Int(0, 1)) ? Quaternion.identity : Quaternion.Euler(0, 90, 0);
            case RoadSegment.Curve:
                return Quaternion.Euler(0, Random.Range(0, 4) * 90, 0);
            case RoadSegment.TJunc:
                Vector2Int openDir = Vector2Int.zero;
                foreach (var dir in MainRoadGenerator.Directions)
                    if (!directions.Contains(dir)) { openDir = dir; break; }
                float angle = openDir switch
                {
                    var d when d == new Vector2Int(0, 1) => 0,
                    var d when d == new Vector2Int(1, 0) => 90,
                    var d when d == new Vector2Int(0, -1) => 180,
                    var d when d == new Vector2Int(-1, 0) => 270,
                    _ => 0
                };
                return Quaternion.Euler(0, angle, 0);
            case RoadSegment.Cross:
                return Quaternion.identity;
            case RoadSegment.DeadEnd:
                return Quaternion.Euler(0, Random.Range(0, 4) * 90, 0);
            default:
                return Quaternion.identity;
        }
    }
}
