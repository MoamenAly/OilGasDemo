using RTLTMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class ModelViewEntry : MonoBehaviour
{
    public enum ViewDirection { Front, Back, Left, Right, Top, Custom }

    [Header("--- UI SETTINGS ---")]
    [SerializeField] private bool showInMenu = true;
    [SerializeField] private string displayName = "";
    [Tooltip("Path inside Resources folder for the button prefab")]
    [SerializeField] private string buttonPrefabPath = "Prefabs/ButtonPart";

    [Header("--- INITIAL FOCUS VIEW ---")]
    [SerializeField] private ViewDirection startDirection = ViewDirection.Front;
    [SerializeField] private Vector3 pivotOffset = Vector3.zero;

    [Space(5)]
    [Tooltip("Starting distance from the model.")]
    [SerializeField] private float defaultDistance = 3f;
    [Tooltip("Starting vertical tilt.")]
    [SerializeField] private float startPitch = 15f;
    [Tooltip("Seconds to transition to this model.")]
    [SerializeField] private float focusDuration = 1.5f;

    [Header("--- ORBIT LIMITS (ROTATION) ---")]
    [Tooltip("Left/Right rotation range (-180 to 180 for full circle)")]
    [SerializeField] private float minYaw = -180f;
    [SerializeField] private float maxYaw = 180f;

    [Tooltip("Up/Down tilt range (Avoid -90/90 to prevent gimbal lock)")]
    [SerializeField] private float minPitch = -10f;
    [SerializeField] private float maxPitch = 80f;

    [Header("--- PAN LIMITS (MOVEMENT) ---")]
    [Tooltip("How far left/right the camera can slide from center.")]
    [SerializeField] private float maxPanHorizontal = 2.0f;
    [Tooltip("How far up/down the camera can slide from center.")]
    [SerializeField] private float maxPanVertical = 2.0f;

    [Header("--- ZOOM LIMITS ---")]
    [SerializeField] private float minDistance = 2.0f;
    [SerializeField] private float maxDistance = 4.0f;

    // Update the Enum to include Custom
    public enum UIPosition { Top, Bottom, Left, Right, Front, Custom }

    [Header("--- UI PLACEMENT ---")]
    [SerializeField] private UIPosition panelPosition = UIPosition.Top;
    [SerializeField] private float uiDistance = 0.5f;

    [Tooltip("This offset is ADDED to the preset position above. Useful for fine-tuning.")]
    [SerializeField] private Vector3 additionalOffset = Vector3.zero;

    // Properties
    public UIPosition PanelPosition => panelPosition;
    public float UIDistance => uiDistance;
    public Vector3 AdditionalOffset => additionalOffset;

    [Header("--- PART INFORMATION ---")]
    [SerializeField] private string partTitle = "";
    [TextArea(3, 10)]
    [SerializeField] private string partInfo = "Details about this part...";

    public string PartTitle => string.IsNullOrEmpty(partTitle) ? displayName : partTitle;
    public string PartInfo => partInfo;

    // --- Private Variables ---
    private Button buttonPrefab;
    private Transform buttonParent;

    // --- Properties for Camera Controller ---
    public Vector3 PivotOffset => pivotOffset;
    public float DefaultDistance => defaultDistance;
    public float StartPitch => startPitch;
    public float FocusDuration => focusDuration;
    public float MinYaw => minYaw;
    public float MaxYaw => maxYaw;
    public float MinPitch => minPitch;
    public float MaxPitch => maxPitch;
    public float MaxPanHorizontal => maxPanHorizontal;
    public float MaxPanVertical => maxPanVertical;
    public float MinDistance => minDistance;
    public float MaxDistance => maxDistance;

    public float GetStartingYaw()
    {
        switch (startDirection)
        {
            case ViewDirection.Front: return 0f;
            case ViewDirection.Back: return 180f;
            case ViewDirection.Left: return -90f;
            case ViewDirection.Right: return 90f;
            default: return 0f;
        }
    }

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(displayName))
            displayName = gameObject.name;
    }

    private void Start()
    {
        if (!showInMenu) return;

        buttonParent = FindObjectOfType<VerticalLayoutGroup>()?.transform;
        if (buttonParent == null)
        {
            Debug.LogWarning($"[{name}] VerticalLayoutGroup (Button Parent) not found.");
            return;
        }

        Button prefab = Resources.Load<Button>(buttonPrefabPath);
        if (prefab == null)
        {
            Debug.LogError($"[{name}] Button prefab missing at Resources/{buttonPrefabPath}");
            return;
        }

        Button btn = Instantiate(prefab, buttonParent);
        RTLTextMeshPro label = btn.GetComponentInChildren<RTLTextMeshPro>();
        if (label != null) label.text = displayName;

        btn.onClick.AddListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        if (CameraOrbitController.Instance != null)
            CameraOrbitController.Instance.FocusOn(this);
    }
}