using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class HexHighlight : MonoBehaviour
{
    #region Attributes
    [Range(1f, 2f)]
    [SerializeField]
    private float _size = 1f;
    [Range(0, 1f)]
    [SerializeField]
    private float _width = 0.2f;
    [SerializeField]
    private float _height = 1f;
    #endregion

    #region Properties
    #endregion

    private LineRenderer _lineRenderer;
    private Vector3[] _vertices = new Vector3[7];
    private bool _calculatedVertices = false;

    private readonly int _sides = 6;
    int points => _sides + 1;


    #region Unity Methods
    void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.loop = false;

        CalculateAllPoints();
    }
    #endregion


    #region Custom Methods
    public void ToggleOutline(bool selected, Color? color = null)
    {
        // NOTE: Line renderer will not be associated in Edit mode!
        if (_lineRenderer == null)
            _lineRenderer = GetComponent<LineRenderer>();

        if (selected && color.HasValue)
        {
            _lineRenderer.startColor = color.Value;
            _lineRenderer.endColor = color.Value;
        }

        if (selected)
            SetProgress(1);

        _lineRenderer.enabled = selected;
    }

    public void SetProgress(float percent = 1f, Color? color = null)
    {
        percent = percent.Clamp(0, 1);
        _lineRenderer.enabled = percent > Mathf.Epsilon;

        if (color.HasValue)
        {
            _lineRenderer.startColor = color.Value;
            _lineRenderer.endColor = color.Value;
        }

        Vector3[] points = CalculatePartialPoints(percent);
        SetRenderPoints(points);
    }

    /// <summary>
    /// Calculate which line renderer points should be included to show progress
    ///   over a hexagon shape (including final partial line).
    /// </summary>
    private Vector3[] CalculatePartialPoints(float percent = 1f)
    {
        if (!_calculatedVertices)
            CalculateAllPoints();

        // Determine which points are fully included in the partial line, and what
        //   percentage of the final line should be included.
        percent = percent.Clamp(0, 1);
        int fullPoints = Mathf.CeilToInt(points * percent);
        if (fullPoints == points) return _vertices;
        float remainingPercent = points * percent - Mathf.Floor(points * percent);

        Vector3[] test = new Vector3[fullPoints + 1];
        Vector3 lastFullPoint = _vertices[0];
        int idx = 0;
        while (idx < fullPoints)
        {
            lastFullPoint = _vertices[idx];
            test[idx] = lastFullPoint;
            idx++;
        }

        Vector3 nextPoint = _vertices[idx];
        Vector3 middlePoint = Vector3Extensions.PointAlongLine(lastFullPoint, nextPoint, remainingPercent);
        test[idx] = middlePoint;

        return test;
    }

    /// <summary>
    /// Hex highlight vertice points only need to be calculated once (startup)
    /// </summary>
    private void CalculateAllPoints()
    {
        _vertices = HexTile.CalculateHexPoints(_size).ToList().Select((point) => new Vector3(point.x, _height, point.y)).ToArray();
        _calculatedVertices = true;
    }

    private void SetRenderPoints(Vector3[] points)
    {
        // NOTE: Very important that position count comes before setting positions!
        //         Previously took two draw calculation cycles to properly assign
        //         points to line renderer when setting positions first!
        _lineRenderer.positionCount = points.Length;
        _lineRenderer.SetPositions(points);

        _lineRenderer.startWidth = _width;
        _lineRenderer.endWidth = _width;
    }
    #endregion

    #region Input Actions
    #endregion
}
