using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles runtime placement and removal of conveyor pieces. A translucent ghost
/// preview follows the mouse across the ground plane and snaps to the free exit
/// socket of a nearby segment. Left click commits a piece, right click removes the
/// piece under the cursor, R rotates the preview, F reverses its direction, and
/// number keys change the type.
/// </summary>
public class ConveyorPlacer : MonoBehaviour
{
    [Header("Prefabs")]
    [Tooltip("Selectable conveyor types, in the order the number keys select them.")]
    [SerializeField] private GameObject[] conveyorPrefabs;

    [Header("Raycasting")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private LayerMask conveyorMask;
    [SerializeField] private float maxRayDistance = 200f;

    [Header("Placement")]
    [SerializeField] private float rotationStep = 90f;
    [SerializeField] private float snapRadius = 1.5f;
    [SerializeField] private float autoConnectTolerance = 0.25f;

    private readonly List<ConveyorSegment> placedSegments = new List<ConveyorSegment>();

    /// <summary>Every conveyor placed so far, in placement order.</summary>
    public IReadOnlyList<ConveyorSegment> PlacedSegments => placedSegments;

    /// <summary>Index of the conveyor type currently selected for placement.</summary>
    public int SelectedIndex { get; private set; }

    /// <summary>Whether the next piece placed will have its direction reversed.</summary>
    public bool IsFlipped => ghostFlipped;

    /// <summary>How many conveyor types are available.</summary>
    public int TypeCount => conveyorPrefabs != null ? conveyorPrefabs.Length : 0;

    private Camera cam;
    private GameObject ghost;
    private ConveyorSegment ghostSegment;
    private GhostVisual ghostVisual;
    private ConveyorSegment snapTarget;
    private float ghostYRotation;
    private bool ghostFlipped;

    private void Awake()
    {
        cam = Camera.main;
    }

    private void Start()
    {
        RebuildGhost();
    }

    private void Update()
    {
        HandleTypeSelection();

        if (ghost == null) return;

        HandleFlip();
        UpdateGhost();

        if (Input.GetKeyDown(KeyCode.R))
        {
            ghostYRotation += rotationStep;
        }

        if (Input.GetMouseButtonDown(0))
        {
            PlaceGhost();
        }

        if (Input.GetMouseButtonDown(1))
        {
            RemoveSegmentUnderCursor();
        }
    }

    /// <summary>Selects a conveyor type by index and rebuilds the preview.</summary>
    public void SelectType(int index)
    {
        if (index < 0 || index >= TypeCount) return;
        if (index == SelectedIndex && ghost != null) return;

        SelectedIndex = index;
        RebuildGhost();
    }

    private void HandleTypeSelection()
    {
        for (int i = 0; i < TypeCount && i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SelectType(i);
            }
        }
    }

    /// <summary>Reverses the preview's travel direction, so it can descend as well as climb.</summary>
    private void HandleFlip()
    {
        if (!Input.GetKeyDown(KeyCode.F)) return;

        ghostFlipped = !ghostFlipped;

        if (ghostSegment != null)
        {
            ghostSegment.SetFlipped(ghostFlipped);
        }
    }

    /// <summary>Destroys the current preview and spawns one for the selected type.</summary>
    private void RebuildGhost()
    {
        if (ghost != null)
        {
            Destroy(ghost);
        }

        if (TypeCount == 0) return;

        ghost = Instantiate(conveyorPrefabs[SelectedIndex]);
        ghost.name = "GhostPreview";

        ghostSegment = ghost.GetComponent<ConveyorSegment>();
        SetCollidersEnabled(ghost, false);

        ghostVisual = ghost.AddComponent<GhostVisual>();

        // Carry the current flip state onto the new preview.
        if (ghostSegment != null)
        {
            ghostSegment.SetFlipped(ghostFlipped);
        }
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
        if (ghostSegment.EntrySocket == null || ghostSegment.ExitSocket == null) return null;

        ConveyorSegment best = null;
        float bestDistance = snapRadius;

        foreach (ConveyorSegment candidate in placedSegments)
        {
            if (candidate == null || candidate.ExitSocket == null) continue;
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

    /// <summary>
    /// Returns a segment whose free entry socket is sitting on the given segment's
    /// exit socket, so a replacement piece can rejoin the line ahead of it.
    /// </summary>
    private ConveyorSegment FindForwardConnection(ConveyorSegment segment)
    {
        ConveyorSegment best = null;
        float bestDistance = autoConnectTolerance;

        foreach (ConveyorSegment candidate in placedSegments)
        {
            if (candidate == null || candidate == segment) continue;
            if (!candidate.HasFreeEntry || candidate.EntrySocket == null) continue;

            float d = Vector3.Distance(segment.ExitSocket.position, candidate.EntrySocket.position);
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
        GameObject placed = Instantiate(conveyorPrefabs[SelectedIndex], ghost.transform.position, ghost.transform.rotation);
        placed.name = $"Conveyor_{placedSegments.Count:00}";

        ConveyorSegment segment = placed.GetComponent<ConveyorSegment>();
        if (segment == null) return;

        segment.SetFlipped(ghostFlipped);

        if (snapTarget != null)
        {
            snapTarget.ConnectTo(segment);
        }

        ConveyorSegment forward = FindForwardConnection(segment);
        if (forward != null)
        {
            segment.ConnectTo(forward);
        }

        placedSegments.Add(segment);
    }

    /// <summary>
    /// Removes the conveyor under the cursor, unlinking it from its neighbours and
    /// clearing any products riding it first.
    /// </summary>
    private void RemoveSegmentUnderCursor()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit, maxRayDistance, conveyorMask)) return;

        ConveyorSegment segment = hit.collider.GetComponentInParent<ConveyorSegment>();
        if (segment == null) return;

        RemoveProductsOn(segment);

        segment.Disconnect();
        placedSegments.Remove(segment);
        Destroy(segment.gameObject);
    }

    /// <summary>Destroys any product currently travelling on the given segment.</summary>
    private static void RemoveProductsOn(ConveyorSegment segment)
    {
        foreach (Product product in FindObjectsByType<Product>(FindObjectsSortMode.None))
        {
            if (product.CurrentSegment == segment)
            {
                Destroy(product.gameObject);
            }
        }
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