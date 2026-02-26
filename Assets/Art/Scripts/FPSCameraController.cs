using UnityEngine;

public class FPSCameraController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintMultiplier = 2f;
    [SerializeField] private float crouchMultiplier = 0.5f;

    [Header("Mouse Look Settings")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float smoothing = 1.5f;

    [Header("Tilt Settings")]
    [SerializeField] private float tiltAngle = 15f;
    [SerializeField] private float tiltSpeed = 5f;

    [Header("Zoom Settings")]
    [SerializeField] private float normalFOV = 60f;
    [SerializeField] private float zoomedFOV = 30f;
    [SerializeField] private float zoomSpeed = 10f;

    [Header("Look Constraints")]
    [SerializeField] private float maxLookAngle = 90f;
    [SerializeField] private float minLookAngle = -90f;

    // Private variables
    private Camera cam;
    private Vector2 smoothedMouseDelta;
    private Vector2 currentMouseDelta;
    private float cameraPitch = 0f;
    private float currentTilt = 0f;
    private float targetFOV;
    private bool isZooming = false;

    private void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            cam = GetComponentInChildren<Camera>();
        }

        // Lock and hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        targetFOV = normalFOV;
        cam.fieldOfView = normalFOV;
    }

    private void Update()
    {
        HandleMouseLook();
        HandleMovement();
        HandleTilt();
        HandleZoom();

        // Toggle cursor lock with Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleCursorLock();
        }
    }

    private void HandleMouseLook()
    {
        // Get raw mouse input
        Vector2 mouseDelta = new Vector2(
            Input.GetAxisRaw("Mouse X"),
            Input.GetAxisRaw("Mouse Y")
        );

        // Smooth the mouse movement
        currentMouseDelta = Vector2.Lerp(currentMouseDelta, mouseDelta, 1f / smoothing);
        smoothedMouseDelta += currentMouseDelta * mouseSensitivity;

        // Clamp vertical rotation
        smoothedMouseDelta.y = Mathf.Clamp(smoothedMouseDelta.y, minLookAngle, maxLookAngle);

        // Apply rotation
        cameraPitch = -smoothedMouseDelta.y;
        float yaw = smoothedMouseDelta.x;

        transform.localRotation = Quaternion.Euler(cameraPitch, yaw, currentTilt);
    }

    private void HandleMovement()
    {
        float speed = moveSpeed;

        // Sprint
        if (Input.GetKey(KeyCode.LeftShift))
        {
            speed *= sprintMultiplier;
        }

        // Crouch
        if (Input.GetKey(KeyCode.LeftControl))
        {
            speed *= crouchMultiplier;
        }

        // Get input
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Calculate movement direction relative to camera
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        // Keep movement on horizontal plane
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = (forward * vertical + right * horizontal).normalized;

        // Apply movement
        transform.position += moveDirection * speed * Time.deltaTime;

        // Vertical movement (up/down)
        if (Input.GetKey(KeyCode.Space))
        {
            transform.position += Vector3.up * speed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.C))
        {
            transform.position += Vector3.down * speed * Time.deltaTime;
        }
    }

    private void HandleTilt()
    {
        float targetTilt = 0f;

        // Tilt with Q and E keys
        if (Input.GetKey(KeyCode.Q))
        {
            targetTilt = tiltAngle;
        }
        else if (Input.GetKey(KeyCode.E))
        {
            targetTilt = -tiltAngle;
        }

        // Smoothly interpolate to target tilt
        currentTilt = Mathf.Lerp(currentTilt, targetTilt, tiltSpeed * Time.deltaTime);
    }

    private void HandleZoom()
    {
        // Right mouse button to zoom
        if (Input.GetMouseButtonDown(1))
        {
            isZooming = true;
            targetFOV = zoomedFOV;
        }
        else if (Input.GetMouseButtonUp(1))
        {
            isZooming = false;
            targetFOV = normalFOV;
        }

        // Scroll wheel zoom
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0f)
        {
            targetFOV -= scrollInput * 10f;
            targetFOV = Mathf.Clamp(targetFOV, 20f, 90f);
        }

        // Smoothly interpolate FOV
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, zoomSpeed * Time.deltaTime);
    }

    private void ToggleCursorLock()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}