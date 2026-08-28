using UnityEngine;

public class LeafSpawner : MonoBehaviour
{
    #region Attributes
    [Header("Assets")]
    [Tooltip("Drag your animated leaf UI prefabs here")]
    [SerializeField] private GameObject[] leafPrefabs;

    [Header("Spawn Controls")]
    [SerializeField] private float minSpeed = 500f; // Pixels per second
    [SerializeField] private float maxSpeed = 350f;
    [SerializeField] private float spawnIntervalMin = 0.1f; // Decreased slightly to account for full screen spread
    [SerializeField] private float spawnIntervalMax = 0.1f;
    [SerializeField] private Vector2 leafScaleRange = new Vector2(0.8f, 1.2f);

    private RectTransform canvasRect;
    private float nextSpawnTime;
    private const float Padding = 150f; // Off-screen clearance offset margin
    #endregion

    #region Properties
    #endregion

    #region Methods
    private void Awake()
    {
        canvasRect = GetComponent<RectTransform>();
        CalculateNextSpawnTime();
    }

    private void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            SpawnLeaf();
            CalculateNextSpawnTime();
        }
    }

    private void SpawnLeaf()
    {
        if (leafPrefabs == null || leafPrefabs.Length == 0)
            return;

        // Calculate parent Canvas dimensions bounds
        float halfWidth = canvasRect.rect.width / 2f;
        float halfHeight = canvasRect.rect.height / 2f;

        Vector2 startPoint = Vector2.zero;
        Vector2 endPoint = Vector2.zero;

        // Full-Screen Coverage Calculation:
        // Randomly pick whether a leaf spawns along the Top boundary or the Right boundary.
        // This distributes them evenly across the entire diagonal flow.
        if (Random.value > 0.5f)
        {
            // TYPE A: Spawns along the TOP edge (travels to the BOTTOM or LEFT edge)
            float randomX = Random.Range(-halfWidth, halfWidth + Padding);
            startPoint = new Vector2(randomX, halfHeight + Padding);

            // Project the target point diagonally down and to the left
            endPoint = new Vector2(randomX - (halfWidth * 2f), -halfHeight - Padding);
        }
        else
        {
            // TYPE B: Spawns along the RIGHT edge (travels to the LEFT or BOTTOM edge)
            float randomY = Random.Range(-halfHeight - Padding, halfHeight);
            startPoint = new Vector2(halfWidth + Padding, randomY);

            // Project the target point diagonally down and to the left
            endPoint = new Vector2(-halfWidth - Padding, randomY - (halfHeight * 2f));
        }

        // Select and Instantiate a random animated Prefab asset
        GameObject chosenPrefab = leafPrefabs[Random.Range(0, leafPrefabs.Length)];
        GameObject leafGo = Instantiate(chosenPrefab, transform, false);
        leafGo.name = "FallingLeaf_Animated";

        // Force uniform UI positioning and randomized scale parameters
        RectTransform leafRect = leafGo.GetComponent<RectTransform>();
        leafRect.anchorMin = new Vector2(0.5f, 0.5f);
        leafRect.anchorMax = new Vector2(0.5f, 0.5f);
        leafRect.pivot = new Vector2(0.5f, 0.5f);

        float randomScale = Random.Range(leafScaleRange.x, leafScaleRange.y);
        leafRect.localScale = new Vector3(randomScale, randomScale, 1f);

        // Initialize the movement logic tracking
        if (!leafGo.TryGetComponent(out FallingLeaf leaf))
        {
            leaf = leafGo.AddComponent<FallingLeaf>();
        }

        float randomSpeed = Random.Range(minSpeed, maxSpeed);
        leaf.Initialize(startPoint, endPoint, randomSpeed);
    }

    private void CalculateNextSpawnTime()
    {
        nextSpawnTime = Time.time + Random.Range(spawnIntervalMin, spawnIntervalMax);
    }
    #endregion
}