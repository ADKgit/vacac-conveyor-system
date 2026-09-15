using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles runtime placement of conveyor pieces. A translucent ghost preview
/// follows the mouse across the ground plane and snaps to the free exit socket
/// of a nearby segment. Left click commits the piece, R rotates it.
/// </summary>
public class ConveyorPlacer : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject conveyorPrefab;

    [Header("Raycasting")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float maxRayDistance = 200f;

    [Header("Placement")]
    [SerializeField] private float rotationStep = 90f;
    [SerializeField] private float snapRadius = 1.5f;

    private readonly List<ConveyorSegment> placedSegments = new List<ConveyorSegment>();

    /// <summary>Every conveyor placed so far, in placement order.</summary>
    public IReadOnlyList<ConveyorSegment> PlacedSegments => placedSegments;

    private Camera cam;
    private GameObject ghost;
    private ConveyorSegment ghostSegment;
    private GhostVisual ghostVisual;
    private ConveyorSegment snapTarget;
    private float ghostYRotation;

    private void Awake()
    {
        cam = Camera.main;
    }

    private void Start()
    {
        CreateGhost();
    }

    private void Update()
    {
        UpdateGhost();

        if (Input.GetKeyDown(KeyCode.R))
        {
            ghostYRotation += rotationStep;
        }

        if (Input.GetMouseButtonDown(0))
        {
            PlaceGhost();
        }
    }

    /// <summary>Spawns the preview instance and strips it of anything a real piece needs.</summary>
    private void CreateGhost()
    {
        ghost = Instantiate(conveyorPrefab);
        ghost.name = "GhostPreview";

        ghostSegment = ghost.GetComponent<ConveyorSegment>();
        SetCollidersEnabled(ghost, false);

        ghostVisual = ghost.AddComponent<GhostVisual>();
    }

    /// <summary>Moves the ghost to the cursor, then snaps it if a socket is in range.</summary>
    private void UpdateGhost()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, maxRayDistance, groundMask))
        {
            ghost.transform.position = hit.point;
            ghost.transform.rotation = Quaternion.Euler(0f, ghostYRotation, 0f);
        }

        snapTarget = FindSnapTarget();

        if (snapTarget != null)
        {
            AlignSocketTo(ghost.transform, ghostSegment.EntrySocket, snapTarget.ExitSocket);
        }

        if (ghostVisual != null)
        {
            ghostVisual.SetSnapping(snapTarget != null);
        }
    }

    /// <summary>
    /// Returns the nearest placed segment with a free exit socket within the snap
    /// radius, or null if there isn't one.
    /// </summary>
    private ConveyorSegment FindSnapTarget()
    {
        if (ghostSegment == null) return null;

        ConveyorSegment best = null;
        float bestDistance = snapRadius;

        foreach (ConveyorSegment candidate in placedSegments)
        {
            if (!candidate.HasFreeExit) continue;

            float d = Vector3.Distance(ghost.transform.position, candidate.ExitSocket.position);
            if (d < bestDistance)
            {
                bestDistance = d;
                best = candidate;
            }
        }

        return best;
    }

    /// <summary>Commits the ghost's current transform as a real conveyor piece.</summary>
    private void PlaceGhost()
    {
        GameObject placed = Instantiate(conveyorPrefab, ghost.transform.position, ghost.transform.rotation);
        placed.name = $"Conveyor_{placedSegments.Count:00}";

        ConveyorSegment segment = placed.GetComponent<ConveyorSegment>();
        if (segment == null) return;

        if (snapTarget != null)
        {
            snapTarget.ConnectTo(segment);
        }

        placedSegments.Add(segment);
    }

    /// <summary>
    /// Moves <paramref name="piece"/> so that <paramref name="pieceSocket"/> ends up at
    /// exactly the same world position and rotation as <paramref name="targetSocket"/>.
    /// Rotation must be applied before the offset is read, because rotating the root
    /// moves its children in world space.
    /// </summary>
    private static void AlignSocketTo(Transform piece, Transform pieceSocket, Transform targetSocket)
    {
        Quaternion socketLocalRotation = Quaternion.Inverse(piece.rotation) * pieceSocket.rotation;
        piece.rotation = targetSocket.rotation * Quaternion.Inverse(socketLocalRotation);

        Vector3 socketOffset = pieceSocket.position - piece.position;
        piece.position = targetSocket.position - socketOffset;
    }

    /// <summary>Ghost pieces must not block raycasts or collide with anything.</summary>
    private static void SetCollidersEnabled(GameObject target, bool enabled)
    {
        foreach (Collider col in target.GetComponentsInChildren<Collider>())
        {
            col.enabled = enabled;
        }
    }
}