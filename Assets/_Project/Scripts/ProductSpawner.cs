using UnityEngine;

/// <summary>
/// Spawns products at a fixed interval onto the head of the conveyor line.
/// The head is the first placed segment whose entry socket is unconnected.
/// </summary>
public class ProductSpawner : MonoBehaviour
{
    [SerializeField] private ConveyorPlacer placer;
    [SerializeField] private GameObject productPrefab;
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private KeyCode toggleKey = KeyCode.Space;

    private bool isRunning;
    private float timeUntilNextSpawn;

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            isRunning = !isRunning;
            timeUntilNextSpawn = 0f;
        }

        if (!isRunning) return;

        timeUntilNextSpawn -= Time.deltaTime;

        if (timeUntilNextSpawn <= 0f)
        {
            timeUntilNextSpawn = spawnInterval;
            SpawnProduct();
        }
    }

    private void SpawnProduct()
    {
        ConveyorSegment head = FindLineHead();
        if (head == null) return;

        GameObject spawned = Instantiate(productPrefab);
        spawned.name = "Product";

        Product product = spawned.GetComponent<Product>();
        if (product != null)
        {
            product.PlaceOn(head);
        }
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