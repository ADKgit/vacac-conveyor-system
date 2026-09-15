using UnityEngine;

/// <summary>
/// RTS-style camera. WASD or arrow keys pan, middle mouse drags, the scroll
/// wheel zooms, and Q/E orbit. Movement is clamped so the camera cannot leave
/// the build area.
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("Speeds")]
    [SerializeField] private float panSpeed = 15f;
    [SerializeField] private float dragPanSpeed = 0.03f;
    [SerializeField] private float zoomSpeed = 500f;
    [SerializeField] private float rotationSpeed = 90f;

    [Header("Limits")]
    [SerializeField] private float minHeight = 3f;
    [SerializeField] private float maxHeight = 40f;
    [SerializeField] private float panLimit = 40f;

    private Vector3 lastMousePosition;

    private void Update()
    {
        HandleKeyboardPan();
        HandleDragPan();
        HandleZoom();
        HandleRotation();
        ClampPosition();
    }

    private void HandleKeyboardPan()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        if (horizontal == 0f && vertical == 0f) return;

        Vector3 move = (GroundForward() * vertical + GroundRight() * horizontal).normalized;
        transform.position += move * (panSpeed * Time.deltaTime);
    }

    private void HandleDragPan()
    {
        if (Input.GetMouseButtonDown(2))
        {
            lastMousePosition = Input.mousePosition;
        }

        if (!Input.GetMouseButton(2)) return;

        Vector3 delta = Input.mousePosition - lastMousePosition;
        lastMousePosition = Input.mousePosition;

        transform.position -= (GroundRight() * delta.x + GroundForward() * delta.y) * dragPanSpeed;
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll == 0f) return;

        transform.position += transform.forward * (scroll * zoomSpeed * Time.deltaTime);
    }

    private void HandleRotation()
    {
        float direction = 0f;
        if (Input.GetKey(KeyCode.Q)) direction = -1f;
        if (Input.GetKey(KeyCode.E)) direction = 1f;

        if (direction == 0f) return;

        transform.RotateAround(transform.position, Vector3.up, direction * rotationSpeed * Time.deltaTime);
    }

    private void ClampPosition()
    {
        Vector3 position = transform.position;
        position.x = Mathf.Clamp(position.x, -panLimit, panLimit);
        position.y = Mathf.Clamp(position.y, minHeight, maxHeight);
        position.z = Mathf.Clamp(position.z, -panLimit, panLimit);
        transform.position = position;
    }

    /// <summary>Camera forward flattened onto the ground plane.</summary>
    private Vector3 GroundForward() => Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;

    /// <summary>Camera right flattened onto the ground plane.</summary>
    private Vector3 GroundRight() => Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;
}