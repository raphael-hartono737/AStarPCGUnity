using UnityEngine;

public class JunctionDetector : MonoBehaviour
{
    public MainRoadGenerator roadGenerator;

    void Start()
    {
        if (roadGenerator == null || !roadGenerator.IsInitialized)
            return;

        var grid = roadGenerator.Grid;
        for (int x = 0; x < roadGenerator.gridSize; x++)
        {
            for (int y = 0; y < roadGenerator.gridSize; y++)
            {
                if (grid[x, y].segment == RoadSegment.TJunc)
                {
                    Debug.Log($"Found T-Junction at ({x}, {y})");
                    // Example: Place a marker
                    GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    marker.transform.position = new Vector3(x, 0.5f, y);
                    marker.transform.localScale = Vector3.one * 0.5f;
                }
                else if (grid[x, y].segment == RoadSegment.Cross)
                {
                    Debug.Log($"Found Crossroads at ({x}, {y})");
                    // Example: Place a different marker
                    GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    marker.transform.position = new Vector3(x, 0.5f, y);
                    marker.transform.localScale = Vector3.one * 0.5f;
                }
            }
        }
    }
}