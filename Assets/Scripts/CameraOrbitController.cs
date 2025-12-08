using UnityEngine;

[DisallowMultipleComponent]
public class CameraOrbitController : MonoBehaviour
{
    public static CameraOrbitController Instance { get; private set; }

    [Header("Camera")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private float orbitSpeed = 120f;   // degrees per second
    [SerializeField] private float zoomSpeed = 5f;      // units per scroll
    [SerializeField] private float damping = 10f;       // lerp for smooth follow

    [Header("Input")]
    [Tooltip("Mouse button used to rotate (0 = LMB, 1 = RMB, 2 = MMB).")]
    [SerializeField] private int rotateMouseButton = 0;

    // Current target model & config
    private Transform target;
    private ModelViewEntry currentConfig;

    // Orbit state
    private float yaw;
    private float pitch;
    private float distance;

    // Limits from the current model
    private float minYaw, maxYaw, minPitch, maxPitch, minDistance, maxDistance;

    // Smooth focus state
    private bool isFocusing;
    private Vector3 focusStartPos;
    private Quaternion focusStartRot;
    private Vector3 focusTargetPos;
    private Quaternion focusTargetRot;
    private float focusTime;
    private float focusDuration;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    /// <summary>
    /// Called by ModelViewEntry when its button is clicked.
    /// Moves the camera smoothly to the front of the model and
    /// sets up orbit limits & zoom.
    /// </summary>
    public void FocusOn(ModelViewEntry config)
    {
        if (targetCamera == null) return;

        target = config.transform;
        currentConfig = config;

        // Copy limits from the model
        minYaw = config.MinYaw;
        maxYaw = config.MaxYaw;
        minPitch = config.MinPitch;
        maxPitch = config.MaxPitch;
        minDistance = config.MinDistance;
        maxDistance = config.MaxDistance;

        distance = Mathf.Clamp(config.DefaultDistance, minDistance, maxDistance);

        // Start facing the "forward" of the model
        yaw = target.eulerAngles.y;
        // Tilt slightly downward (or whatever is configured)
        pitch = Mathf.Clamp(config.StartPitch, minPitch, maxPitch);

        // Compute where we want the camera to end up
        Vector3 pivot = GetPivot();
        Vector3 offset = Quaternion.Euler(pitch, yaw, 0f) * (Vector3.back * distance);
        Vector3 desiredPos = pivot + offset;
        Quaternion desiredRot = Quaternion.LookRotation(pivot - desiredPos, Vector3.up);

        // Setup focus interpolation
        focusStartPos = targetCamera.transform.position;
        focusStartRot = targetCamera.transform.rotation;
        focusTargetPos = desiredPos;
        focusTargetRot = desiredRot;
        focusDuration = Mathf.Max(0.01f, config.FocusDuration);
        focusTime = 0f;
        isFocusing = true;
    }

    private Vector3 GetPivot()
    {
        if (target == null || currentConfig == null)
            return Vector3.zero;

        // Pivot is model position + per-model offset
        return target.position + target.TransformVector(currentConfig.PivotOffset);
    }

    private void LateUpdate()
    {
        if (targetCamera == null)
            return;

        // First: if we are currently "auto moving" to a model, just lerp
        if (isFocusing)
        {
            focusTime += Time.deltaTime;
            float t = Mathf.Clamp01(focusTime / focusDuration);
            t = Mathf.SmoothStep(0f, 1f, t);

            targetCamera.transform.position = Vector3.Lerp(focusStartPos, focusTargetPos, t);
            targetCamera.transform.rotation = Quaternion.Slerp(focusStartRot, focusTargetRot, t);

            if (t >= 1f)
                isFocusing = false;

            return;
        }

        // After focus: enable manual orbit / zoom
        if (target == null)
            return;

        HandleInput();
        UpdateCameraTransform();
    }

    private void HandleInput()
    {
        // Pan/orbit & tilt
        if (Input.GetMouseButton(rotateMouseButton))
        {
            float dx = Input.GetAxis("Mouse X");
            float dy = Input.GetAxis("Mouse Y");

            yaw += dx * orbitSpeed * Time.deltaTime;
            pitch -= dy * orbitSpeed * Time.deltaTime;

            yaw = Mathf.Clamp(yaw, minYaw, maxYaw);
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }

        // Zoom with mouse wheel
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > Mathf.Epsilon)
        {
            distance -= scroll * zoomSpeed;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }
    }

    private void UpdateCameraTransform()
    {
        Vector3 pivot = GetPivot();
        Quaternion orbitRot = Quaternion.Euler(pitch, yaw, 0f);

        // Camera is always on a sphere around the pivot
        Vector3 targetPos = pivot + orbitRot * (Vector3.back * distance);
        Quaternion targetRot = Quaternion.LookRotation(pivot - targetPos, Vector3.up);

        targetCamera.transform.position = Vector3.Lerp(
            targetCamera.transform.position,
            targetPos,
            Time.deltaTime * damping
        );
        targetCamera.transform.rotation = Quaternion.Slerp(
            targetCamera.transform.rotation,
            targetRot,
            Time.deltaTime * damping
        );
    }
}
