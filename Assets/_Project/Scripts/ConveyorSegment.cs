using UnityEngine;

public class ConveyorSegment : MonoBehaviour
{
    [SerializeField] private Transform entrySocket;
    [SerializeField] private Transform exitSocket;
    [SerializeField] private float speed = 1.5f;
    [SerializeField] private ConveyorSegment nextSegment;

    private ConveyorSegment previousSegment;

    public Transform EntrySocket => entrySocket;
    public Transform ExitSocket => exitSocket;
    public float Speed => speed;
    public ConveyorSegment NextSegment => nextSegment;
    public ConveyorSegment PreviousSegment => previousSegment;

    public bool HasFreeEntry => previousSegment == null;
    public bool HasFreeExit => nextSegment == null;

    public float Length => Vector3.Distance(entrySocket.position, exitSocket.position);

    public void ConnectTo(ConveyorSegment next)
    {
        if (next == null || next == this) return;

        nextSegment = next;
        next.previousSegment = this;
    }

    public Vector3 GetPointAtDistance(float distance)
    {
        float t = Mathf.Clamp01(distance / Length);
        return Vector3.Lerp(entrySocket.position, exitSocket.position, t);
    }
}