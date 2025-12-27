using RTLTMPro;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
#if SIRENIX_ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

[DisallowMultipleComponent]
public class ModelViewEntry : MonoBehaviour
{
    public enum ViewDirection { Front, Back, Left, Right, Top, Custom }
    public enum UIPosition { Top, Bottom, Left, Right, Front, Custom }

    [Header("--- UI SETTINGS ---")]
    [SerializeField] private bool showInMenu = true;

    [OnValueChanged("SyncTitle")] // Odin: Syncs when you type in the inspector
    [SerializeField] private string displayName = "";

    [Tooltip("Path inside Resources folder for the button prefab")]
    [SerializeField] private string buttonPrefabPath = "Prefabs/ButtonPart";

    [Header("--- INITIAL FOCUS VIEW ---")]
    [SerializeField] private ViewDirection startDirection = ViewDirection.Front;
    [SerializeField] private Vector3 pivotOffset = Vector3.zero;

    [Space(5)]
    [SerializeField] private float defaultDistance = 3f;
    [SerializeField] private float startPitch = 15f;
    [SerializeField] private float focusDuration = 1.5f;

    [Header("--- ORBIT LIMITS (ROTATION) ---")]
    [SerializeField] private float minYaw = -180f;
    [SerializeField] private float maxYaw = 180f;
    [SerializeField] private float minPitch = -10f;
    [SerializeField] private float maxPitch = 80f;

    [Header("--- PAN LIMITS (MOVEMENT) ---")]
    [SerializeField] private float maxPanHorizontal = 2.0f;
    [SerializeField] private float maxPanVertical = 2.0f;

    [Header("--- ZOOM LIMITS ---")]
    [SerializeField] private float minDistance = 2.0f;
    [SerializeField] private float maxDistance = 4.0f;

    [Header("--- UI PLACEMENT ---")]
    [SerializeField] private UIPosition panelPosition = UIPosition.Top;
    [SerializeField] private float uiDistance = 0.5f;
    [SerializeField] private Vector3 additionalOffset = Vector3.zero;

    [Header("--- PART INFORMATION ---")]
#if SIRENIX_ODIN_INSPECTOR
    [HorizontalGroup("TitleGroup")]
    [Button(ButtonSizes.Small, Name = "Reset to Display Name")]
    public void ResetTitle() => SyncTitle();

    [HorizontalGroup("TitleGroup")]
#endif
    [SerializeField] private string partTitle = "";

    [TextArea(3, 10)]
    [SerializeField] private string partInfo = "Details about this part...";

    // Properties
    public string PartTitle => string.IsNullOrEmpty(partTitle) ? displayName : partTitle;
    public string PartInfo => partInfo;
    public UIPosition PanelPosition => panelPosition;
    public float UIDistance => uiDistance;
    public Vector3 AdditionalOffset => additionalOffset;
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

    // --- Internal Sync Logic ---
    private void SyncTitle()
    {
        partTitle = displayName;
    }

    private void OnValidate()
    {
        // Set displayName to object name if empty
        if (string.IsNullOrEmpty(displayName))
            displayName = gameObject.name;

        // Automatically push displayName to partTitle in editor
        if (string.IsNullOrEmpty(partTitle))
            SyncTitle();
    }

    // --- Logic ---

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

    private void Start()
    {
        if (!showInMenu) return;

        Transform buttonParent = FindObjectOfType<VerticalLayoutGroup>()?.transform;
        if (buttonParent == null) return;

        Button prefab = Resources.Load<Button>(buttonPrefabPath);
        if (prefab == null) return;

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