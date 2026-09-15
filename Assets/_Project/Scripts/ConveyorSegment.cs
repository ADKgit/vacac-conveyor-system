using UnityEngine;

public class ConveyorSegment : MonoBehaviour
{
    [SerializeField] private Transform entrySocket;
    [SerializeField] private Transform exitSocket;
    [SerializeField] private float speed = 1.5f;

    public Transform EntrySocket => entrySocket;
    public Transform ExitSocket => exitSocket;
    public float Speed => speed;

    public float Length => Vector3.Distance(entrySocket.position, exitSocket.position);

    public Vector3 GetPointAtDistance(float distance)
    {
        float t = Mathf.Clamp01(distance / Length);
        return Vector3.Lerp(entrySocket.position, exitSocket.position, t);
    }

    private void Start()
    {
        Debug.Log($"{name} length: {Length}");
    }
}