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

    [Header("--- UI MODE SELECTION ---")]
    [SerializeField] private UIMode currentUIMode = UIMode.UI_3D;
    public enum UIMode { UI_3D, UI_2D }

    [Header("--- INFO UI PANEL 3D---")]
    [SerializeField] private CanvasGroup infoPanel3D;
    [SerializeField] private RTLTextMeshPro titleText3D;
    [SerializeField] private Transform contentData3D;
    [SerializeField] private string HorizontalDataPrefabPath3D = "Prefabs/HorizontalData3D";

    [Header("--- INFO UI PANEL 2D---")]
    [SerializeField] private CanvasGroup infoPanel2D;
    [SerializeField] private RTLTextMeshPro titleText2D;
    [SerializeField] private Transform contentData2D;
    [SerializeField] private string HorizontalDataPrefabPath2D = "Prefabs/HorizontalData2D";

    [Header("--- UI SETTINGS ---")]
    [SerializeField] private float uiFadeSpeed = 5f;

    [Header("--- FLOATING UI CONFIG (3D Only) ---")]
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

    // Public method to switch UI mode
    public void SetUIMode(UIMode mode)
    {
        currentUIMode = mode;

        // Hide both panels first
        if (infoPanel3D != null) infoPanel3D.alpha = 0;
        if (infoPanel2D != null) infoPanel2D.alpha = 0;

        // Refresh the current panel if we have a config
        if (currentConfig != null)
        {
            UpdateUIContent(currentConfig);
        }
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

        // Update UI based on current mode
        UpdateUIContent(config);

        isPanelVisible = true;

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

    private void UpdateUIContent(ModelViewEntry config)
    {
        if (currentUIMode == UIMode.UI_3D)
        {
            // Update 3D UI
            if (titleText3D != null) titleText3D.text = config.PartTitle;

            // Clear previous data
            foreach (Transform child in contentData3D)
            {
                Destroy(child.gameObject);
            }

            // Populate new data
            if (config._data != null)
            {
                GameObject horizontalDataprefab = Resources.Load<GameObject>(HorizontalDataPrefabPath3D);
                if (horizontalDataprefab != null)
                {
                    foreach (var data in config._data)
                    {
                        GameObject HorizontalData = Instantiate(horizontalDataprefab, contentData3D);
                        HorizontalData.transform.GetChild(0).GetComponent<RTLTextMeshPro>().text = data.key;
                        HorizontalData.transform.GetChild(1).GetComponent<RTLTextMeshPro>().text = data.value;
                    }
                }
            }
        }
        else // UI_2D
        {
            // Update 2D UI
            if (titleText2D != null) titleText2D.text = config.PartTitle;

            // Clear previous data
            foreach (Transform child in contentData2D)
            {
                Destroy(child.gameObject);
            }

            // Populate new data
            if (config._data != null)
            {
                GameObject horizontalDataprefab = Resources.Load<GameObject>(HorizontalDataPrefabPath2D);
                if (horizontalDataprefab != null)
                {
                    foreach (var data in config._data)
                    {
                        GameObject HorizontalData = Instantiate(horizontalDataprefab, contentData2D);
                        HorizontalData.transform.GetChild(0).GetComponent<RTLTextMeshPro>().text = data.key;
                        HorizontalData.transform.GetChild(1).GetComponent<RTLTextMeshPro>().text = data.value;
                    }
                }
            }
        }
    }

    private void LateUpdate()
    {
        if (targetCamera == null || isStartup) return;

        if (isFocusing)
        {
            HandleFocusTransition();
            // Keep UI hidden while moving
            if (infoPanel3D != null) infoPanel3D.alpha = 0;
            if (infoPanel2D != null) infoPanel2D.alpha = 0;
            return;
        }

        if (target == null) return;

        HandleInput();
        UpdateCameraTransform();

        // Handle UI based on mode
        if (currentUIMode == UIMode.UI_3D)
        {
            PositionUINearPart();
            HandleUIVisibility(infoPanel3D);
            // Hide 2D panel
            if (infoPanel2D != null)
            {
                infoPanel2D.alpha = 0;
                infoPanel2D.blocksRaycasts = false;
                infoPanel2D.interactable = false;
            }
        }
        else // UI_2D
        {
            HandleUIVisibility(infoPanel2D);
            // Hide 3D panel
            if (infoPanel3D != null)
            {
                infoPanel3D.alpha = 0;
                infoPanel3D.blocksRaycasts = false;
                infoPanel3D.interactable = false;
            }
        }
    }

    private void PositionUINearPart()
    {
        if (infoPanel3D == null || target == null || currentConfig == null) return;

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
            case ModelViewEntry.UIPosition.Custom: directionVec = Vector3.zero; break;
        }

        // 2. Add the custom fine-tuning offset
        Vector3 tweakVec = target.TransformVector(currentConfig.AdditionalOffset);

        // 3. Final Position
        Vector3 finalPosition = basePivot + directionVec + tweakVec;

        // 4. Smoothly move the panel
        infoPanel3D.transform.position = Vector3.Lerp(infoPanel3D.transform.position, finalPosition, Time.deltaTime * damping);

        // 5. Billboard rotation
        infoPanel3D.transform.LookAt(infoPanel3D.transform.position + targetCamera.transform.rotation * Vector3.forward,
                                   targetCamera.transform.rotation * Vector3.up);
    }

    private void HandleUIVisibility(CanvasGroup panel)
    {
        if (panel == null) return;

        // Only show panel if we have a target and aren't moving/starting up
        float targetAlpha = (isPanelVisible && !isFocusing && !isStartup) ? 1f : 0f;

        // Smooth fade
        panel.alpha = Mathf.MoveTowards(panel.alpha, targetAlpha, Time.deltaTime * uiFadeSpeed);

        // Disable interactions if hidden
        panel.blocksRaycasts = (panel.alpha > 0.8f);
        panel.interactable = (panel.alpha > 0.8f);
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

        yield return StartCoroutine(Fade(1f, 0f));

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