using RTLTMPro;
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class CameraOrbitController : MonoBehaviour
{
    public static CameraOrbitController Instance { get; private set; }

    [Header("--- CORE COMPONENTS ---")]
    [HideInInspector] private Camera targetCamera;
    [SerializeField] private CanvasGroup menu;
    [SerializeField] private CanvasGroup fadeCanvas;

    [Header("--- MOVEMENT SETTINGS ---")]
    [SerializeField] private float orbitSpeed = 120f;
    [SerializeField] private float panSpeed = 0.5f;
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float damping = 10f;

    [Header("--- INPUT CONFIGURATION ---")]
    [Tooltip("0: Left, 1: Right, 2: Middle")]
    [SerializeField] private int rotateMouseButton = 0;
    [SerializeField] private int panMouseButton = 2;

    [Header("--- INFO UI PANEL ---")]
    [SerializeField] private CanvasGroup infoPanel; // The Panel's CanvasGroup
    [SerializeField] private RTLTextMeshPro titleText;
    [SerializeField] private RTLTextMeshPro infoText;
    [SerializeField] private float uiFadeSpeed = 5f;

    [Header("--- FLOATING UI CONFIG ---")]
    [Tooltip("Offset of the panel from the part's pivot point")]
    [SerializeField] private Vector3 panelOffset = new Vector3(0.5f, 0.5f, 0f);

    private bool isPanelVisible = false;

    [Header("--- STARTUP SEQUENCE ---")]
    [SerializeField] private Transform initialTarget;
    [SerializeField] private Vector3 initialPivotOffset = Vector3.zero;
    [SerializeField] private float initialDistance = 6f;
    [SerializeField] private float initialYaw = 0f;
    [SerializeField] private float initialPitch = 20f;
    [SerializeField] private float startupMoveDuration = 1.5f;
    [SerializeField] private float fadeDuration = 1f;

    // --- INTERNAL STATE ---
    private Transform target;
    private ModelViewEntry currentConfig;

    private float yaw;
    private float pitch;
    private float distance;
    private Vector3 panOffset = Vector3.zero;

    // --- LIMITS (Synced from ModelViewEntry) ---
    private float minYaw, maxYaw, minPitch, maxPitch, minDistance, maxDistance;

    // --- TRANSITION STATE ---
    private bool isFocusing;
    private bool isStartup;
    private Vector3 focusStartPos, focusTargetPos;
    private Quaternion focusStartRot, focusTargetRot;
    private float focusTime, focusDuration;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (targetCamera == null) targetCamera = Camera.main;
    }

    private void Start()
    {
        isStartup = true;
        StartCoroutine(StartupSequence());
    }

    public void FocusOn(ModelViewEntry config)
    {
        if (targetCamera == null || isStartup) return;

        target = config.transform;
        currentConfig = config;

        // 1. Sync Limits from the Part
        minYaw = config.MinYaw;
        maxYaw = config.MaxYaw;
        minPitch = config.MinPitch;
        maxPitch = config.MaxPitch;
        minDistance = config.MinDistance;
        maxDistance = config.MaxDistance;

        // Update the UI content
        if (titleText != null) titleText.text = config.PartTitle;
        if (infoText != null) infoText.text = config.PartInfo;

        isPanelVisible = true; // Show the panel

        // 2. Setup Target State
        distance = Mathf.Clamp(config.DefaultDistance, minDistance, maxDistance);
        yaw = config.GetStartingYaw();
        pitch = Mathf.Clamp(config.StartPitch, minPitch, maxPitch);
        panOffset = Vector3.zero;

        // 3. Calculate Transition Points
        Vector3 pivot = GetPivot();
        Vector3 offset = Quaternion.Euler(pitch, yaw, 0f) * (Vector3.back * distance);

        focusStartPos = targetCamera.transform.position;
        focusStartRot = targetCamera.transform.rotation;
        focusTargetPos = pivot + offset;
        focusTargetRot = Quaternion.LookRotation(pivot - focusTargetPos, Vector3.up);

        focusDuration = Mathf.Max(0.01f, config.FocusDuration);
        focusTime = 0f;
        isFocusing = true;
    }

    private void LateUpdate()
    {
        if (targetCamera == null || isStartup) return;

        if (isFocusing)
        {
            HandleFocusTransition();
            // Keep UI hidden while moving
            if (infoPanel != null) infoPanel.alpha = 0;
            return;
        }

        if (target == null) return;

        HandleInput();
        UpdateCameraTransform();

        // NEW METHODS
        PositionUINearPart();
        HandleUIVisibility();
    }

    private void PositionUINearPart()
    {
        if (infoPanel == null || target == null || currentConfig == null) return;

        Vector3 basePivot = GetStaticPivot();
        Vector3 directionVec = Vector3.zero;

        // 1. Calculate the base direction vector
        switch (currentConfig.PanelPosition)
        {
            case ModelViewEntry.UIPosition.Top: directionVec = target.up * currentConfig.UIDistance; break;
            case ModelViewEntry.UIPosition.Bottom: directionVec = -target.up * currentConfig.UIDistance; break;
            case ModelViewEntry.UIPosition.Left: directionVec = -target.right * currentConfig.UIDistance; break;
            case ModelViewEntry.UIPosition.Right: directionVec = target.right * currentConfig.UIDistance; break;
            case ModelViewEntry.UIPosition.Front: directionVec = target.forward * currentConfig.UIDistance; break;
            case ModelViewEntry.UIPosition.Custom: directionVec = Vector3.zero; break; // Starts at pivot
        }

        // 2. Add the custom fine-tuning offset (converted to local model space)
        Vector3 tweakVec = target.TransformVector(currentConfig.AdditionalOffset);

        // 3. Final Position = Static Pivot + Direction + Tweak
        Vector3 finalPosition = basePivot + directionVec + tweakVec;

        // 4. Smoothly move the panel to the target position
        infoPanel.transform.position = Vector3.Lerp(infoPanel.transform.position, finalPosition, Time.deltaTime * damping);

        // 5. Billboard rotation (face camera)
        infoPanel.transform.LookAt(infoPanel.transform.position + targetCamera.transform.rotation * Vector3.forward,
                                   targetCamera.transform.rotation * Vector3.up);
    }

    private void HandleUIVisibility()
    {
        if (infoPanel == null) return;

        // Only show panel if we have a target and aren't moving/starting up
        float targetAlpha = (isPanelVisible && !isFocusing && !isStartup) ? 1f : 0f;

        // Smooth fade
        infoPanel.alpha = Mathf.MoveTowards(infoPanel.alpha, targetAlpha, Time.deltaTime * uiFadeSpeed);

        // Disable interactions if hidden
        infoPanel.blocksRaycasts = (infoPanel.alpha > 0.8f);
        infoPanel.interactable = (infoPanel.alpha > 0.8f);
    }

    private void HandleInput()
    {
        // --- ORBIT (Rotation) ---
        if (Input.GetMouseButton(rotateMouseButton))
        {
            yaw += Input.GetAxis("Mouse X") * orbitSpeed * Time.deltaTime;
            pitch -= Input.GetAxis("Mouse Y") * orbitSpeed * Time.deltaTime;
            yaw = Mathf.Clamp(yaw, minYaw, maxYaw);
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }

        // --- PANNING (Movement) ---
        if (Input.GetMouseButton(panMouseButton))
        {
            float factor = distance * panSpeed * Time.deltaTime;
            Vector3 move = (targetCamera.transform.right * -Input.GetAxis("Mouse X") * factor) +
                           (targetCamera.transform.up * -Input.GetAxis("Mouse Y") * factor);

            Vector3 newPan = panOffset + move;

            // Clamp Pan using ModelViewEntry limits
            if (currentConfig != null)
            {
                newPan.x = Mathf.Clamp(newPan.x, -currentConfig.MaxPanHorizontal, currentConfig.MaxPanHorizontal);
                newPan.y = Mathf.Clamp(newPan.y, -currentConfig.MaxPanVertical, currentConfig.MaxPanVertical);
                newPan.z = Mathf.Clamp(newPan.z, -currentConfig.MaxPanHorizontal, currentConfig.MaxPanHorizontal);
            }
            panOffset = newPan;
        }

        // --- ZOOM ---
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > Mathf.Epsilon)
        {
            distance = Mathf.Clamp(distance - (scroll * zoomSpeed), minDistance, maxDistance);
        }
    }

    private void UpdateCameraTransform()
    {
        Vector3 pivot = GetPivot();
        Quaternion orbitRot = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 targetPos = pivot + orbitRot * (Vector3.back * distance);
        Quaternion targetRot = Quaternion.LookRotation(pivot - targetPos, Vector3.up);

        targetCamera.transform.position = Vector3.Lerp(targetCamera.transform.position, targetPos, Time.deltaTime * damping);
        targetCamera.transform.rotation = Quaternion.Slerp(targetCamera.transform.rotation, targetRot, Time.deltaTime * damping);
    }

    private Vector3 GetPivot()
    {
        if (target == null || currentConfig == null) return Vector3.zero;
        return target.position + target.TransformVector(currentConfig.PivotOffset) + panOffset;
    }

    private Vector3 GetStaticPivot()
    {
        if (target == null || currentConfig == null) return Vector3.zero;

        // We EXCLUDE panOffset here so the UI stays stuck to the mesh
        return target.position + target.TransformVector(currentConfig.PivotOffset);
    }

    private void HandleFocusTransition()
    {
        focusTime += Time.deltaTime;
        float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(focusTime / focusDuration));

        targetCamera.transform.position = Vector3.Lerp(focusStartPos, focusTargetPos, t);
        targetCamera.transform.rotation = Quaternion.Slerp(focusStartRot, focusTargetRot, t);

        if (t >= 1f) isFocusing = false;
    }

    private IEnumerator StartupSequence()
    {
        if (fadeCanvas != null) fadeCanvas.alpha = 1f;
        if (menu != null) menu.alpha = 0f;

        yield return StartCoroutine(Fade(0f, 1f));

        Vector3 pivot = initialTarget != null ? initialTarget.position + initialTarget.TransformVector(initialPivotOffset) : Vector3.zero;
        Quaternion baseRot = initialTarget != null ? initialTarget.rotation : Quaternion.identity;
        Quaternion rot = baseRot * Quaternion.Euler(initialPitch, initialYaw, 0f);
        Vector3 pos = pivot + rot * (Vector3.back * initialDistance);

        Vector3 sPos = targetCamera.transform.position;
        Quaternion sRot = targetCamera.transform.rotation;

        float elapsed = 0f;
        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime / startupMoveDuration;
            float s = Mathf.SmoothStep(0f, 1f, elapsed);
            targetCamera.transform.position = Vector3.Lerp(sPos, pos, s);
            targetCamera.transform.rotation = Quaternion.Slerp(sRot, Quaternion.LookRotation(pivot - pos, baseRot * Vector3.up), s);
            yield return null;
        }

        if (menu != null) menu.alpha = 1f;
        yield return StartCoroutine(Fade(1f, 0f));
        if (fadeCanvas != null) fadeCanvas.gameObject.SetActive(false);
        isStartup = false;
    }

    private IEnumerator Fade(float from, float to)
    {
        if (fadeCanvas == null) yield break;
        float elapsed = 0f;
        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime / fadeDuration;
            fadeCanvas.alpha = Mathf.Lerp(from, to, elapsed);
            yield return null;
        }
        fadeCanvas.alpha = to;
    }
}