using UnityEngine;

/// <summary>
/// Finds and places exactly one StartingPoint instance by sampling random (x,z) positions,
/// raycasting downward to ensure they lie within [minHeight, maxHeight], and choosing
/// the spot that maximizes the average distance to each existing Temple’s “Gateway” child.
/// Attach this to its own GameObject, assign parameters in the Inspector, then call GenerateStartPoint().
/// </summary>
public class StartPointGenerator : MonoBehaviour
{
    [Header("Prefabs & Parents")]
    [Tooltip("Assign a prefab that is tagged \"StartingPoint\".")]
    public GameObject startPointPrefab;
    [Tooltip("Parent transform under which the StartingPoint will be instantiated.")]
    public Transform startParent;

    [Header("Sampling Settings (matches PlacementGenerator)")]
    [Tooltip("Number of random attempts to find the StartingPoint.")]
    public int maxSamples = 1000;
    [Tooltip("World?space X range for sampling (min, max).")]
    public Vector2 xRange = new Vector2(-50f, 50f);
    [Tooltip("World?space Z range for sampling (min, max).")]
    public Vector2 zRange = new Vector2(-50f, 50f);
    [Tooltip("Vertical start height for raycasts.")]
    public float raycastStartY = 100f;
    [Tooltip("Minimum allowed terrain height (world Y).")]
    public float minHeight = 0f;
    [Tooltip("Maximum allowed terrain height (world Y).")]
    public float maxHeight = 100f;

    [Header("Layer Mask for Terrain")]
    [Tooltip("Layers that the downward?raycast should hit (e.g. your terrain layer).")]
    public LayerMask terrainLayerMask = ~0;

    void Start()
    {
        // If you want it automatic, uncomment:
        // GenerateStartPoint();
    }

    /// <summary>
    /// Call this to place one StartingPoint. It will look for all GameObjects tagged "Temple",
    /// find their "Gateway" child transforms, then sample up to maxSamples random (x,z) positions.
    /// For each valid raycast spot (hit ? [minHeight,maxHeight]), it computes the average distance
    /// to all temple?gateway positions. Finally, it picks the candidate with the largest average
    /// and instantiates startPointPrefab there. Logs an error if none found.
    /// </summary>
    public void GenerateStartPoint()
    {
        GameObject[] temples = GameObject.FindGameObjectsWithTag("Temple");
        if (temples.Length == 0)
        {
            Debug.LogError("[StartPointGenerator] No GameObject with tag \"Temple\" found in scene.");
            return;
        }

        Transform[] gatewayTransforms = CollectGatewayTransforms(temples);
        if (gatewayTransforms.Length == 0)
        {
            Debug.LogError("[StartPointGenerator] None of the Temple GameObjects contain a child named \"Gateway\".");
            return;
        }

        Vector3? bestLocation = FindFarthestLocation(gatewayTransforms, maxSamples);
        if (!bestLocation.HasValue)
        {
            Debug.LogError("[StartPointGenerator] Failed to find any valid StartingPoint spot after sampling.");
            return;
        }

        Instantiate(startPointPrefab, bestLocation.Value, Quaternion.identity, startParent);
        // The prefab itself should be tagged "StartingPoint".
        Debug.Log("[StartPointGenerator] Successfully placed 1 StartingPoint.");
    }

    /// <summary>
    /// Given all temples, find their child named "Gateway". Returns an array of those transforms.
    /// </summary>
    private Transform[] CollectGatewayTransforms(GameObject[] temples)
    {
        var tempList = new System.Collections.Generic.List<Transform>();
        foreach (GameObject t in temples)
        {
            Transform gateway = t.transform.Find("Gateway");
            if (gateway != null)
                tempList.Add(gateway);
        }
        return tempList.ToArray();
    }

    /// <summary>
    /// Samples up to maxSamples random (x,z) points. For each, raycasts down:
    ///   - If hit ? [minHeight, maxHeight] on terrainLayerMask,
    ///     compute average distance to each Transform in gatewayTransforms.
    ///   - Track the candidate with the largest average distance.
    /// Returns the best spot or null if none found.
    /// </summary>
    private Vector3? FindFarthestLocation(Transform[] gatewayTransforms, int maxSamples)
    {
        Vector3 bestPos = Vector3.zero;
        float bestAvgDist = float.MinValue;
        int attempts = 0;

        while (attempts < maxSamples)
        {
            attempts++;
            float sampleX = Random.Range(xRange.x, xRange.y);
            float sampleZ = Random.Range(zRange.x, zRange.y);
            Vector3 rayOrigin = new Vector3(sampleX, raycastStartY, sampleZ);

            if (!Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, Mathf.Infinity, terrainLayerMask))
                continue;

            float y = hit.point.y;
            if (y < minHeight || y > maxHeight)
                continue;

            Vector3 candidate = hit.point;

            // Compute average distance to all gateways
            float sumDist = 0f;
            foreach (Transform gw in gatewayTransforms)
            {
                sumDist += Vector3.Distance(candidate, gw.position);
            }
            float avgDist = sumDist / gatewayTransforms.Length;

            if (avgDist > bestAvgDist)
            {
                bestAvgDist = avgDist;
                bestPos = candidate;
            }
        }

        if (bestAvgDist == float.MinValue)
            Debug.LogWarning($"[StartPointGenerator] No valid spot found in {attempts} attempts.");

        return (bestAvgDist == float.MinValue) ? (Vector3?)null : bestPos;
    }
}