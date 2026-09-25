using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(LineRenderer))]
[ExecuteInEditMode]
public class DynamicLineRenderer : MonoBehaviour
{
    [Header("Line Points")]
    [SerializeField] private List<Transform> points = new List<Transform>();
    [SerializeField] private LineRenderer lineRenderer;

    [Header("Draw Animation")]
    [SerializeField] private bool animateOnStart;
    [SerializeField, Min(0.01f)] private float animationDuration = 1f;
    [SerializeField] private AnimationCurve animationCurve =
        AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private bool useUnscaledTime;

    [Header("Events")]
    [SerializeField] private UnityEvent onAnimationFinished = new UnityEvent();

    /// <summary>
    /// C# event alternative to the Inspector UnityEvent.
    /// </summary>
    public event Action AnimationFinished;

    public bool IsAnimating { get; private set; }
    public UnityEvent OnAnimationFinished => onAnimationFinished;

    private Coroutine animationCoroutine;

    private void Awake()
    {
        CacheLineRenderer();
        UpdateLineRenderer();
    }

    private void Start()
    {
        if (Application.isPlaying && animateOnStart)
        {
            AnimateLine();
        }
    }

    private void Update()
    {
        // Keep the line dynamic in Edit Mode and while no draw animation is running.
        // During animation, the coroutine owns the LineRenderer positions.
        if (!Application.isPlaying || !IsAnimating)
        {
            UpdateLineRenderer();
        }
    }

    private void OnDisable()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }

        IsAnimating = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        foreach (Transform point in points)
        {
            if (point != null)
            {
                Gizmos.DrawSphere(point.position, 0.01f);
            }
        }
    }

    /// <summary>
    /// Immediately renders the complete line using the current point positions.
    /// </summary>
    [ContextMenu("UpdatePoints")]
    public void UpdateLineRenderer()
    {
        CacheLineRenderer();

        if (lineRenderer == null)
        {
            return;
        }

        List<Vector3> validPositions = GetCurrentLinePositions();
        lineRenderer.positionCount = validPositions.Count;

        for (int i = 0; i < validPositions.Count; i++)
        {
            lineRenderer.SetPosition(i, validPositions[i]);
        }
    }

    /// <summary>
    /// Smoothly draws the line from the first point to the last point.
    /// Call this method from code, a Button, or another UnityEvent.
    /// </summary>
    public void AnimateLine()
    {
        if (!Application.isPlaying)
        {
            UpdateLineRenderer();
            return;
        }

        CacheLineRenderer();

        if (lineRenderer == null)
        {
            return;
        }

        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        animationCoroutine = StartCoroutine(AnimateLineRoutine());
    }

    /// <summary>
    /// Stops the animation. Optionally displays the complete line.
    /// </summary>
    public void StopAnimation(bool showCompleteLine = true)
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }

        IsAnimating = false;

        if (showCompleteLine)
        {
            UpdateLineRenderer();
        }
    }

    private IEnumerator AnimateLineRoutine()
    {
        IsAnimating = true;

        // Snapshot the positions so the path remains stable for this animation.
        List<Vector3> positions = GetCurrentLinePositions();

        if (positions.Count == 0)
        {
            lineRenderer.positionCount = 0;
            CompleteAnimation();
            yield break;
        }

        if (positions.Count == 1)
        {
            lineRenderer.positionCount = 1;
            lineRenderer.SetPosition(0, positions[0]);
            CompleteAnimation();
            yield break;
        }

        float[] cumulativeLengths = new float[positions.Count];
        float totalLength = 0f;

        for (int i = 1; i < positions.Count; i++)
        {
            totalLength += Vector3.Distance(positions[i - 1], positions[i]);
            cumulativeLengths[i] = totalLength;
        }

        // All points are at the same position.
        if (totalLength <= Mathf.Epsilon)
        {
            SetCompleteLine(positions);
            CompleteAnimation();
            yield break;
        }

        lineRenderer.positionCount = 1;
        lineRenderer.SetPosition(0, positions[0]);

        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

            float normalizedTime = Mathf.Clamp01(elapsed / animationDuration);
            float curvedTime = animationCurve == null
                ? normalizedTime
                : Mathf.Clamp01(animationCurve.Evaluate(normalizedTime));
            float targetDistance = curvedTime * totalLength;

            DrawToDistance(positions, cumulativeLengths, targetDistance);
            yield return null;
        }

        SetCompleteLine(positions);
        CompleteAnimation();
    }

    private void DrawToDistance(
        IReadOnlyList<Vector3> positions,
        IReadOnlyList<float> cumulativeLengths,
        float targetDistance)
    {
        int segmentIndex = 0;

        while (segmentIndex < positions.Count - 2 &&
               targetDistance > cumulativeLengths[segmentIndex + 1])
        {
            segmentIndex++;
        }

        float segmentStartDistance = cumulativeLengths[segmentIndex];
        float segmentEndDistance = cumulativeLengths[segmentIndex + 1];
        float segmentLength = segmentEndDistance - segmentStartDistance;

        float segmentProgress = segmentLength <= Mathf.Epsilon
            ? 1f
            : Mathf.InverseLerp(segmentStartDistance, segmentEndDistance, targetDistance);

        Vector3 animatedEndPoint = Vector3.Lerp(
            positions[segmentIndex],
            positions[segmentIndex + 1],
            segmentProgress);

        // Completed points plus one temporary animated endpoint.
        lineRenderer.positionCount = segmentIndex + 2;

        for (int i = 0; i <= segmentIndex; i++)
        {
            lineRenderer.SetPosition(i, positions[i]);
        }

        lineRenderer.SetPosition(segmentIndex + 1, animatedEndPoint);
    }

    private void SetCompleteLine(IReadOnlyList<Vector3> positions)
    {
        lineRenderer.positionCount = positions.Count;

        for (int i = 0; i < positions.Count; i++)
        {
            lineRenderer.SetPosition(i, positions[i]);
        }
    }

    private void CompleteAnimation()
    {
        IsAnimating = false;
        animationCoroutine = null;

        onAnimationFinished?.Invoke();
        AnimationFinished?.Invoke();
    }

    private List<Vector3> GetCurrentLinePositions()
    {
        List<Vector3> positions = new List<Vector3>(points.Count);

        foreach (Transform point in points)
        {
            if (point == null)
            {
                continue;
            }

            positions.Add(ToLineSpace(point.position));
        }

        return positions;
    }

    private Vector3 ToLineSpace(Vector3 worldPosition)
    {
        return lineRenderer.useWorldSpace
            ? worldPosition
            : lineRenderer.transform.InverseTransformPoint(worldPosition);
    }

    private void CacheLineRenderer()
    {
        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
        }
    }

    [ContextMenu("AddPoint")]
    public void AddPoint()
    {
        Vector3 newPosition = points.Count == 0 || points[points.Count - 1] == null
            ? transform.position
            : points[points.Count - 1].position;

        GameObject newPoint = new GameObject("Point " + points.Count);
        newPoint.transform.SetParent(transform);
        newPoint.transform.position = newPosition;
        points.Add(newPoint.transform);

        StopAnimation();
    }

    public void RemovePoint(int index)
    {
        if (index < 0 || index >= points.Count)
        {
            return;
        }

        Transform point = points[index];
        points.RemoveAt(index);

        if (point != null)
        {
            if (Application.isPlaying)
            {
                Destroy(point.gameObject);
            }
            else
            {
                DestroyImmediate(point.gameObject);
            }
        }

        StopAnimation();
    }
}
