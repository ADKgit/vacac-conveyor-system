using UnityEngine;

/// <summary>
/// A single conveyor piece. Defines the path products travel along it and the
/// links to the neighbouring segments in the line.
/// </summary>
public class ConveyorSegment : MonoBehaviour
{
    [SerializeField] private Transform entrySocket;
    [SerializeField] private Transform exitSocket;
    [SerializeField] private float speed = 1.5f;

    private ConveyorSegment nextSegment;
    private ConveyorSegment previousSegment;

    public Transform EntrySocket => entrySocket;
    public Transform ExitSocket => exitSocket;
    public float Speed => speed;
    public ConveyorSegment NextSegment => nextSegment;
    public ConveyorSegment PreviousSegment => previousSegment;

    public bool HasFreeEntry => previousSegment == null;
    public bool HasFreeExit => nextSegment == null;

    /// <summary>Distance in world units from the entry socket to the exit socket.</summary>
    public float Length => Vector3.Distance(entrySocket.position, exitSocket.position);

    /// <summary>Links this segment's exit to the entry of the given segment.</summary>
    public void ConnectTo(ConveyorSegment next)
    {
        if (next == null || next == this) return;

        nextSegment = next;
        next.previousSegment = this;
    }

    /// <summary>
    /// Releases this segment's links to its neighbours, leaving their free sockets
    /// available again. Called before the segment is destroyed.
    /// </summary>
    public void Disconnect()
    {
        if (previousSegment != null)
        {
            previousSegment.nextSegment = null;
            previousSegment = null;
        }

        if (nextSegment != null)
        {
            nextSegment.previousSegment = null;
            nextSegment = null;
        }
    }

    /// <summary>Returns the world position the given distance along this segment.</summary>
    public Vector3 GetPointAtDistance(float distance)
    {
        float t = Mathf.Clamp01(distance / Length);
        return Vector3.Lerp(entrySocket.position, exitSocket.position, t);
    }
}