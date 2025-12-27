using System.Collections;
using UnityEditor;
using UnityEngine;

[DisallowMultipleComponent]
public class CameraOrbitController : MonoBehaviour
{
    public static CameraOrbitController Instance { get; private set; }

    [Header("Camera")]
    private Camera targetCamera;
    [SerializeField] private float orbitSpeed = 120f;
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float damping = 10f;

    [Header("Input")]
    [Tooltip("Mouse button used to rotate (0 = LMB, 1 = RMB, 2 = MMB).")]
    [SerializeField] private int rotateMouseButton = 0;

    [Header("Menu")]
    [SerializeField] CanvasGroup menu;

    [Header("Startup Camera")]
    [SerializeField] private Transform initialTarget;
    [SerializeField] private Vector3 initialPivotOffset = Vector3.zero;
    [SerializeField] private float initialDistance = 6f;
    [SerializeField] private float initialYaw = 0f;
    [SerializeField] private float initialPitch = 20f;
    [SerializeField] private float startupMoveDuration = 1.5f;

    [Header("Fade")]
    [SerializeField] private CanvasGroup fadeCanvas;
    [SerializeField] private float fadeDuration = 1f;

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

    private bool isStartup;

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

    private void Start()
    {
        isStartup = true;
        StartCoroutine(StartupSequence());
    }

    /// <summary>
    /// Called by ModelViewEntry when its button is clicked.
    /// </summary>
    public void FocusOn(ModelViewEntry config)
    {
        if (targetCamera == null || isStartup)
            return;

        target = config.transform;
        currentConfig = config;

        minYaw = config.MinYaw;
        maxYaw = config.MaxYaw;
        minPitch = config.MinPitch;
        maxPitch = config.MaxPitch;
        minDistance = config.MinDistance;
        maxDistance = config.MaxDistance;

        distance = Mathf.Clamp(config.DefaultDistance, minDistance, maxDistance);

        yaw = target.eulerAngles.y;
        pitch = Mathf.Clamp(config.StartPitch, minPitch, maxPitch);

        Vector3 pivot = GetPivot();
        Vector3 offset = Quaternion.Euler(pitch, yaw, 0f) * (Vector3.back * distance);
        Vector3 desiredPos = pivot + offset;
        Quaternion desiredRot = Quaternion.LookRotation(pivot - desiredPos, Vector3.up);

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

        return target.position + target.TransformVector(currentConfig.PivotOffset);
    }

    private void LateUpdate()
    {
        if (targetCamera == null)
            return;

        if (isStartup)
            return;

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

        if (target == null)
            return;

        HandleInput();
        UpdateCameraTransform();
    }

    private void HandleInput()
    {
        if (Input.GetMouseButton(rotateMouseButton))
        {
            float dx = Input.GetAxis("Mouse X");
            float dy = Input.GetAxis("Mouse Y");

            yaw += dx * orbitSpeed * Time.deltaTime;
            pitch -= dy * orbitSpeed * Time.deltaTime;

            yaw = Mathf.Clamp(yaw, minYaw, maxYaw);
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }

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

    private IEnumerator StartupSequence()
    {
        if (fadeCanvas != null)
            fadeCanvas.alpha = 1f;

        menu.alpha = 0f;

        yield return Fade(0f, 1f);
        fadeCanvas.gameObject.SetActive(true);


        // Pivot position
        Vector3 pivot = initialTarget != null
            ? initialTarget.position + initialTarget.TransformVector(initialPivotOffset)
            : Vector3.zero;

        // Base rotation comes from initial target
        Quaternion baseRot = initialTarget != null
            ? initialTarget.rotation
            : Quaternion.identity;

        // Apply pitch & yaw relative to the initial target rotation
        Quaternion rot = baseRot * Quaternion.Euler(initialPitch, initialYaw, 0f);

        // Final camera position
        Vector3 pos = pivot + rot * (Vector3.back * initialDistance);

        Vector3 startPos = targetCamera.transform.position;
        Quaternion startRot = targetCamera.transform.rotation;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / startupMoveDuration;
            float s = Mathf.SmoothStep(0f, 1f, t);

            targetCamera.transform.position = Vector3.Lerp(startPos, pos, s);
            targetCamera.transform.rotation = Quaternion.Slerp(
                startRot,
                Quaternion.LookRotation(pivot - pos, baseRot * Vector3.up),
                s
            );

            yield return null;
        }
        menu.alpha = 1f;

        yield return Fade(1f, 0f);
        fadeCanvas.gameObject.SetActive(false);
        isStartup = false;
    }


    private IEnumerator Fade(float from, float to)
    {
        if (fadeCanvas == null)
            yield break;

        float t = 0f;
        fadeCanvas.alpha = from;


        while (t < 1f)
        {
            t += Time.deltaTime / fadeDuration;
            fadeCanvas.alpha = Mathf.Lerp(from, to, t);
            yield return null;
        }

        fadeCanvas.alpha = to;
    }
}
