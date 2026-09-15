using UnityEngine;

/// <summary>
/// Spawns products at a fixed interval onto the head of the conveyor line.
/// The head is the first placed segment whose entry socket is unconnected.
/// A product type is chosen at random from the configured prefabs.
/// </summary>
public class ProductSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ConveyorPlacer placer;

    [Tooltip("Product types to spawn. One is chosen at random each time.")]
    [SerializeField] private GameObject[] productPrefabs;

    [Header("Spawning")]
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private KeyCode toggleKey = KeyCode.Space;

    /// <summary>How many products have reached the end of a line.</summary>
    public int DeliveredCount { get; private set; }

    /// <summary>Whether products are currently being spawned.</summary>
    public bool IsRunning { get; private set; }

    private float timeUntilNextSpawn;

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            IsRunning = !IsRunning;
            timeUntilNextSpawn = 0f;
        }

        if (!IsRunning) return;

        timeUntilNextSpawn -= Time.deltaTime;

        if (timeUntilNextSpawn <= 0f)
        {
            timeUntilNextSpawn = spawnInterval;
            SpawnProduct();
        }
    }

    private void SpawnProduct()
    {
        if (productPrefabs == null || productPrefabs.Length == 0) return;

        ConveyorSegment head = FindLineHead();
        if (head == null) return;

        GameObject prefab = productPrefabs[Random.Range(0, productPrefabs.Length)];
        if (prefab == null) return;

        GameObject spawned = Instantiate(prefab);
        spawned.name = prefab.name;

        Product product = spawned.GetComponent<Product>();
        if (product == null) return;

        product.Delivered += OnProductDelivered;
        product.PlaceOn(head);
    }

    private void OnProductDelivered(Product product)
    {
        product.Delivered -= OnProductDelivered;
        DeliveredCount++;
    }

    /// <summary>Returns the first segment with nothing feeding into it.</summary>
    private ConveyorSegment FindLineHead()
    {
        foreach (ConveyorSegment segment in placer.PlacedSegments)
        {
            if (segment.HasFreeEntry) return segment;
        }

        return null;
    }
}