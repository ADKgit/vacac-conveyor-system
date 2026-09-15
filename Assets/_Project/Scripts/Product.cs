using UnityEngine;

/// <summary>
/// Moves a product along a chain of connected conveyor segments.
/// Movement is kinematic: the product tracks a distance travelled along its
/// current segment and is placed at the matching world position each frame.
/// </summary>
public class Product : MonoBehaviour
{
    [SerializeField] private ConveyorSegment startSegment;
    [SerializeField] private float heightOffset = 0f;
    [SerializeField] private bool destroyAtEndOfLine = true;

    private ConveyorSegment current;
    private float distance;

    /// <summary>Places this product at the start of the given segment.</summary>
    public void PlaceOn(ConveyorSegment segment)
    {
        if (segment == null) return;

        current = segment;
        distance = 0f;
        transform.position = segment.GetPointAtDistance(0f) + Vector3.up * heightOffset;
    }

    private void Start()
    {
        if (startSegment != null) PlaceOn(startSegment);
    }

    private void Update()
    {
        if (current == null) return;

        distance += current.Speed * Time.deltaTime;

        // Hand off to the next segment, carrying any overshoot with us.
        while (distance >= current.Length)
        {
            ConveyorSegment next = current.NextSegment;
            if (next == null)
            {
                ReachEndOfLine();
                return;
            }

            distance -= current.Length;
            current = next;
        }

        transform.position = current.GetPointAtDistance(distance) + Vector3.up * heightOffset;
    }

    private void ReachEndOfLine()
    {
        transform.position = current.GetPointAtDistance(current.Length) + Vector3.up * heightOffset;
        current = null;

        if (destroyAtEndOfLine)
        {
            Destroy(gameObject);
        }
    }
}