using UnityEngine;

public class Product : MonoBehaviour
{
    [SerializeField] private ConveyorSegment startSegment;
    [SerializeField] private float heightOffset = 0f;

    private ConveyorSegment current;
    private float distance;

    public void PlaceOn(ConveyorSegment segment)
    {
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

        if (distance >= current.Length)
        {
            transform.position = current.GetPointAtDistance(current.Length) + Vector3.up * heightOffset;
            current = null;
            return;
        }

        transform.position = current.GetPointAtDistance(distance) + Vector3.up * heightOffset;
    }
}