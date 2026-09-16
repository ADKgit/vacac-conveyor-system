using UnityEngine;

/// <summary>
/// A single conveyor piece. Defines the path products travel along it and the
/// links to the neighbouring segments in the line.
///
/// A segment can be flipped, which swaps which end acts as the entry and which
/// acts as the exit. Everything else reads the sockets through the properties
/// below, so flipping reverses travel direction without moving the model — an
/// inclined piece becomes a declined one.
/// </summary>
public class ConveyorSegment : MonoBehaviour
{
    [SerializeField] private Transform entrySocket;
    [SerializeField] private Transform exitSocket;
    [SerializeField] private float speed = 1.5f;

    private ConveyorSegment nextSegment;
    private ConveyorSegment previousSegment;
    private bool isFlipped;

    public Transform EntrySocket => isFlipped ? exitSocket : entrySocket;
    public Transform ExitSocket => isFlipped ? entrySocket : exitSocket;

    public float Speed => speed;
    public bool IsFlipped => isFlipped;
    public ConveyorSegment NextSegment => nextSegment;
    public ConveyorSegment PreviousSegment => previousSegment;

    public bool HasFreeEntry => previousSegment == null;
    public bool HasFreeExit => nextSegment == null;

    /// <summary>Distance in world units from the entry socket to the exit socket.</summary>
    public float Length => Vector3.Distance(EntrySocket.position, ExitSocket.position);

    /// <summary>Swaps which end of this piece products enter and leave by.</summary>
    public void SetFlipped(bool flipped)
    {
        isFlipped = flipped;
    }

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
        return Vector3.Lerp(EntrySocket.position, ExitSocket.position, t);
    }
}