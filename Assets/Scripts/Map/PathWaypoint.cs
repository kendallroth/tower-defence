#nullable enable

using Drawing;
using Sirenix.OdinInspector;
using UnityEngine;


public class PathWaypoint : MonoBehaviourGizmos
{
    #region Attributes
    [SerializeField]
    [ReadOnly]
    private int number = 0;

    [TitleGroup("Position")]
    [HorizontalGroup("Position/Split")]
    [ShowInInspector]
    [ReadOnly]
    [ToggleLeft]
    public bool FirstWaypoint => NextWaypoint == null && PreviousWaypoint != null;
    [HorizontalGroup("Position/Split")]
    [ShowInInspector]
    [ReadOnly]
    [ToggleLeft]
    public bool LastWaypoint => PreviousWaypoint == null && NextWaypoint != null;
    #endregion


    #region Properties
    /// <summary>
    /// Next waypoint in path
    /// </summary>
    public PathWaypoint? NextWaypoint => nextWaypoint;
    /// <summary>
    /// Previous waypoint in path
    /// </summary>
    public PathWaypoint? PreviousWaypoint => previousWaypoint;
    /// <summary>
    /// Path waypoint number (1-based)
    /// </summary>
    public int Number => number;
    /// <summary>
    /// Whether waypoint is connected to any other waypoints
    /// </summary>
    public bool Connected => NextWaypoint != null || PreviousWaypoint != null;
    /// <summary>
    /// Parent hex tile
    /// </summary>
    public HexTile HexTile => hexTile;
    #endregion


    [SerializeField, HideInInspector]
    private PathWaypoint? previousWaypoint = null;
    [SerializeField, HideInInspector]
    private PathWaypoint? nextWaypoint = null;
    [SerializeField, HideInInspector]
    private HexTile hexTile;

    private float waypointMarkerRadius = 0.1f;
    public int Index => Number - 1;


    #region Unity Methods
    #endregion


    #region Accessors
    public void SetTile(HexTile tile)
    {
        hexTile = tile;
    }

    /// <summary>
    /// Set waypoint number
    /// </summary>
    /// <param name="number">Waypoint number</param>
    public void SetNumber(int number)
    {
        this.number = number;
    }

    /// <summary>
    /// Link the next waypoint
    /// </summary>
    /// <param name="waypoint">Next waypoint</param>
    public void SetNextWaypoint(PathWaypoint waypoint)
    {
        nextWaypoint = waypoint;
    }

    /// <summary>
    /// Link the previous waypoint
    /// </summary>
    /// <param name="waypoint">Previous waypoint</param>
    public void SetPreviousWaypoint(PathWaypoint waypoint)
    {
        previousWaypoint = waypoint;
    }
    #endregion


    #region Custom Methods
    /// <summary>
    /// Initialize a path waypoint
    /// </summary>
    /// <param name="number">Waypoint number</param>
    /// <param name="previous">Previous waypoint</param>
    public void Init(int number, PathWaypoint? previous)
    {
        SetNumber(number);

        if (previous != null)
            SetPreviousWaypoint(previous);
    }

    /// <summary>
    /// Reset a waypoint (clears linked waypoints, etc)
    /// </summary>
    public void Reset()
    {
        nextWaypoint = null;
        previousWaypoint = null;
        number = 0;
    }
    #endregion


    #region Debug Methods
    public override void DrawGizmos()
    {
        bool showWaypoints = hexTile?.HexMap?.ShowWaypoints ?? true;
        if (!showWaypoints || hexTile?.TileType != HexTileType.PATH) return;

        Draw.WireSphere(transform.position, waypointMarkerRadius, Connected ? Color.blue : Color.red);
        Vector3 labelPosition = transform.position + Vector3.up * waypointMarkerRadius * 2 + Vector3.forward * waypointMarkerRadius * 2;
        Draw.Label2D(labelPosition, $"{Number}", 16, Color.black);

        if (NextWaypoint != null)
        {
            Vector3 directionToNext = transform.DirectionTo(NextWaypoint.transform);
            Draw.Arrow(
                transform.position + directionToNext * waypointMarkerRadius * 2 + Vector3.up * 0.01f,
                NextWaypoint.transform.position - directionToNext * waypointMarkerRadius * 2 + Vector3.up * 0.01f,
                transform.up,
                waypointMarkerRadius,
                Color.blue
            );
        }
    }
    #endregion
}
