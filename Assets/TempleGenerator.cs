using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Finds and places exactly two Temple instances by sampling random (x,z) positions,
/// raycasting downward to ensure they lie within [minHeight, maxHeight], and (optionally)
/// enforcing a minimum separation between them. Attach this script to any GameObject,
/// assign the templePrefab + parameters in the Inspector, then call GenerateTemples() 
/// (or let it run in Start).
/// </summary>
public class TempleGenerator : MonoBehaviour
{
    [Header("Prefabs & Parents")]
    [Tooltip("Assign a prefab that is tagged \"Temple\" and contains a child named \"Gateway\".")]
    public GameObject templePrefab;
    [Tooltip("Parent transform under which the two temples will be instantiated.")]
    public Transform templeParent;

    [Header("Sampling Settings (matches PlacementGenerator)")]
    [Tooltip("Number of random attempts to find each Temple location.")]
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

    [Header("Optional: Enforce distance between temples")]
    [Tooltip("If > 0, ensures the two temples are at least this far apart.")]
    public float minSeparation = 0f;

    [Header("Layer Mask for Terrain")]
    [Tooltip("Layers that the downward?raycast should hit (e.g. your terrain layer).")]
    public LayerMask terrainLayerMask = ~0;

    private const int REQUIRED_COUNT = 2;

    void Start()
    {
        // If you want it automatic, uncomment:
        // GenerateTemples();
    }

    /// <summary>
    /// Call this to place exactly two temples. Logs an error if fewer than 2 valid spots are found.
    /// </summary>
    public void GenerateTemples()
    {
        List<Vector3> chosenPositions = FindValidLocations(REQUIRED_COUNT, maxSamples);
        if (chosenPositions.Count < REQUIRED_COUNT)
        {
            Debug.LogError($"[TempleGenerator] Only found {chosenPositions.Count} valid spots (needed {REQUIRED_COUNT}).");
            return;
        }

        foreach (Vector3 pos in chosenPositions)
        {
            GameObject t = Instantiate(templePrefab, pos, Quaternion.identity, templeParent);
            // The prefab itself should already carry tag="Temple" and contain a child named "Gateway".
        }

        Debug.Log("[TempleGenerator] Successfully placed 2 temples.");
    }

    /// <summary>
    /// Samples random (x,z) points up to maxSamples. For each:
    ///   - Raycast down from (x, raycastStartY, z).
    ///   - If hit on terrainLayerMask and hit.point.y ? [minHeight, maxHeight], consider valid.
    ///   - If minSeparation > 0, ensure candidate is far enough from any previously chosen point.
    /// Returns a list of up to 'requiredCount' world positions.
    /// </summary>
    private List<Vector3> FindValidLocations(int requiredCount, int maxSamples)
    {
        List<Vector3> validList = new List<Vector3>();
        int attempts = 0;

        while (validList.Count < requiredCount && attempts < maxSamples)
        {
            attempts++;
            float sampleX = Random.Range(xRange.x, xRange.y);
            float sampleZ = Random.Range(zRange.x, zRange.y);
            Vector3 rayOrigin = new Vector3(sampleX, raycastStartY, sampleZ);

            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, Mathf.Infinity, terrainLayerMask))
            {
                float y = hit.point.y;
                if (y < minHeight || y > maxHeight)
                    continue;

                Vector3 candidate = hit.point;

                if (minSeparation > 0f)
                {
                    bool tooClose = false;
                    foreach (Vector3 existing in validList)
                    {
                        if (Vector3.Distance(existing, candidate) < minSeparation)
                        {
                            tooClose = true;
                            break;
                        }
                    }
                    if (tooClose)
                        continue;
                }

                validList.Add(candidate);
            }
        }

        if (validList.Count < requiredCount)
            Debug.LogWarning($"[TempleGenerator] Found only {validList.Count} valid spots after {attempts} attempts.");

        return validList;
    }
}