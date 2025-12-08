using RTLTMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class ModelViewEntry : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private string displayName = "";
    private Button buttonPrefab;   // Prefab of your menu button
    private Transform buttonParent; // ScrollView Content transform

    [Tooltip("Path inside Resources folder")]
    private string buttonPrefabPath = "Prefabs/ButtonPart";

    [Header("Camera Focus")]
    [Tooltip("Pivot offset relative to the model (usually center of the mesh).")]
    [SerializeField] private Vector3 pivotOffset = Vector3.zero;

    [Tooltip("Camera distance when focusing this model.")]
    [SerializeField] private float defaultDistance = 4f;

    [Tooltip("Initial pitch angle when focusing this model.")]
    [SerializeField] private float startPitch = 15f;

    [Tooltip("Time in seconds for the camera to move to this model.")]
    [SerializeField] private float focusDuration = 1.0f;

    [Header("Orbit Limits")]
    [Tooltip("Yaw range; for full 360° set -180 and 180.")]
    [SerializeField] private float minYaw = -180f;
    [SerializeField] private float maxYaw = 180f;

    [Tooltip("Tilt range. E.g. -80 to 80.")]
    [SerializeField] private float minPitch = -80f;
    [SerializeField] private float maxPitch = 80f;

    [Tooltip("Zoom distance limits.")]
    [SerializeField] private float minDistance = 1.0f;
    [SerializeField] private float maxDistance = 10.0f;

    // Expose as read-only properties for the camera controller
    public Vector3 PivotOffset => pivotOffset;
    public float DefaultDistance => defaultDistance;
    public float StartPitch => startPitch;
    public float FocusDuration => focusDuration;

    public float MinYaw => minYaw;
    public float MaxYaw => maxYaw;
    public float MinPitch => minPitch;
    public float MaxPitch => maxPitch;
    public float MinDistance => minDistance;
    public float MaxDistance => maxDistance;

    private void OnValidate()
    {
        // Called when the component is first added (used in the inspector)
        if (string.IsNullOrEmpty(displayName))
        {
            displayName = gameObject.name;
        }
    }

    private void Start()
    {
        if (string.IsNullOrEmpty(displayName))
        {
            displayName = gameObject.name;
        }

        buttonParent = FindObjectOfType<VerticalLayoutGroup>().transform;
        if (buttonParent == null)
        {
            Debug.LogWarning($"[{name}] Button parent is not assigned.");
            return;

        }

        // Load the prefab from Resources/Prefabs
        Button buttonPrefab = Resources.Load<Button>(buttonPrefabPath);
        if (buttonPrefab == null)
        {
            Debug.LogError($"[{name}] Could not load Button prefab at Resources/{buttonPrefabPath}");
            return;
        }

        // Instantiate button in the scroll view
        Button btn = Instantiate(buttonPrefab, buttonParent);

        // Label the button
        RTLTextMeshPro label = btn.GetComponentInChildren<RTLTextMeshPro>();
        if (label != null)
        {
            label.text = displayName;
        }

        // Wire up the click to focus this model
        btn.onClick.AddListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        if (CameraOrbitController.Instance == null)
        {
            Debug.LogWarning("CameraOrbitController.Instance not found in scene.");
            return;
        }

        CameraOrbitController.Instance.FocusOn(this);
    }
}
