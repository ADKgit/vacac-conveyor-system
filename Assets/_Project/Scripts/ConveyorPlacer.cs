using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles runtime placement of conveyor pieces. A ghost preview follows the
/// mouse across the ground plane; left click commits it, R rotates it.
/// </summary>
public class ConveyorPlacer : MonoBehaviour
{
    [SerializeField] private GameObject conveyorPrefab;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float maxRayDistance = 200f;
    [SerializeField] private float rotationStep = 90f;

    private readonly List<ConveyorSegment> placedSegments = new List<ConveyorSegment>();

    private Camera cam;
    private GameObject ghost;
    private float ghostYRotation;

    private void Awake()
    {
        cam = Camera.main;
    }

    private void Start()
    {
        ghost = Instantiate(conveyorPrefab);
        ghost.name = "GhostPreview";
        SetCollidersEnabled(ghost, false);
    }

    private void Update()
    {
        UpdateGhostPosition();

        if (Input.GetKeyDown(KeyCode.R))
        {
            ghostYRotation += rotationStep;
            ghost.transform.rotation = Quaternion.Euler(0f, ghostYRotation, 0f);
        }

        if (Input.GetMouseButtonDown(0))
        {
            PlaceGhost();
        }
    }

    private void UpdateGhostPosition()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, maxRayDistance, groundMask))
        {
            ghost.transform.position = hit.point;
        }
    }

    /// <summary>Commits the ghost's current position as a real conveyor piece.</summary>
    private void PlaceGhost()
    {
        GameObject placed = Instantiate(conveyorPrefab, ghost.transform.position, ghost.transform.rotation);
        placed.name = $"Conveyor_{placedSegments.Count:00}";

        ConveyorSegment segment = placed.GetComponent<ConveyorSegment>();
        if (segment != null)
        {
            placedSegments.Add(segment);
        }
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