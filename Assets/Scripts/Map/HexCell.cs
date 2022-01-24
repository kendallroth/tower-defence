#nullable enable

using Drawing;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;


/// <summary>
/// Hex tile terrain type
/// </summary>
public enum HexTileType
{
    EMPTY,
    GRASS,
    PATH,
    WATER,
}


/// <summary>
/// Hex pathfinding type
/// </summary>
public enum HexPathingType
{
    // TODO: Consider whether EMPTY would make sense (maybe redundant)?
    NORMAL,
    SPAWN,
    DESTINATION,
}


// NOTE: Set as selection base to improve experience in Unity editor (with nested prefab)
[SelectionBase]
[RequireComponent(typeof(LineRenderer))]
public class HexCell : MonoBehaviourGizmos
{
    #region Attributes
    [Title("Coordinates")]
    [PropertyOrder(-3)]
    [SerializeField]
    private HexCoordinates coordinates;
    [PropertyOrder(-2)]
    [ShowInInspector]
    public HexOffsetCoordinates OffsetCoordinates => Coordinates.ToOffset();
    [PropertyOrder(-1)]
    [DisplayAsString]
    [ShowInInspector]
    public int NeighbourCount => neighbours.Count((cell) => cell != null);

    [Title("Attributes")]
    [SerializeField]
    private HexTileType tileType = HexTileType.EMPTY;
    [SerializeField]
    private HexPathingType pathingType = HexPathingType.NORMAL;
    [SerializeField]
    private int height = 0;

    [Title("Game Objects")]
    [Required]
    [SerializeField]
    private Transform terrainParent;
    [Required]
    [SerializeField]
    private Transform propsParent;
    #endregion


    #region Properties
    /// <summary>
    /// Hex cell height
    /// </summary>
    public int Height => height;
    /// <summary>
    /// Hex cell tile type (terrain)
    /// </summary>
    public HexTileType TileType => tileType;
    /// <summary>
    /// Hex cell pathfinding type
    /// </summary>
    public HexPathingType PathingType => pathingType;
    /// <summary>
    /// All hex cell neighbours (clockwise from NE)
    /// <br /><br />
    /// Note that cell neighbours will be null/empty on map borders!
    /// </summary>
    public HexCell?[] Neighbours => neighbours;
    /// <summary>
    /// Hex cell coordinates
    /// </summary>
    public HexCoordinates Coordinates => coordinates;
    /// <summary>
    /// Hex cell parent map
    /// </summary>
    public HexMap HexMap
    {
        get
        {
            if (_hexMap == null)
                _hexMap = GetComponentInParent<HexMap>();
            return _hexMap;
        }
    }
    /// <summary>
    /// Hex cell path waypoint
    /// </summary>
    public PathWaypoint Waypoint
    {
        get
        {
            if (_waypoint == null)
                _waypoint = GetComponentInChildren<PathWaypoint>();
            return _waypoint;
        }
    }
    #endregion


    [SerializeField, HideInInspector]
    private HexCell?[] neighbours = new HexCell[6];
    private HexMap _hexMap;
    private PathWaypoint _waypoint;
    private LineRenderer lineRenderer;


    #region Unity Methods
    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();

        // Empty tiles are not shown in-game
        if (tileType == HexTileType.EMPTY)
            gameObject.SetActive(false);

        ToggleSelection(false);
    }

    void Update()
    {

    }
    #endregion


    #region Accessors
    /// <summary>
    /// Get a cell's neighbour in a direction
    /// </summary>
    /// <param name="direction">Target direction</param>
    /// <returns>Neighbouring cell in direction</returns>
    public HexCell? GetNeighbour(HexDirection direction)
    {
        return neighbours[(int)direction];
    }

    /// <summary>
    /// Get all neighbouring path cells
    /// </summary>
    /// <returns>Neighbouring path cells</returns>
    public HexCell[] GetNeighbourPaths()
    {
        return neighbours.Where((n) => n != null && n.tileType == HexTileType.PATH).ToArray();
    }

    public void ClearNeighbours()
    {
        neighbours = new HexCell[6];
    }

    public void SetNeighbour(HexDirection direction, HexCell? cell)
    {
        neighbours[(int)direction] = cell;
    }

    public void SetMap(HexMap map)
    {
        _hexMap = map;
    }

    public void SetCoordinates(HexCoordinates coordinates)
    {
        this.coordinates = coordinates;
    }

    public void SetHeight(int height)
    {
        this.height = height;
    }

    public void SetTypes(HexTileType tileType, HexPathingType pathingType = HexPathingType.NORMAL)
    {
        this.tileType = tileType;
        this.pathingType = pathingType;
    }
    #endregion


    #region Custom Methods
    /// <summary>
    /// Initialize a Hex cell
    /// </summary>
    /// <param name="coordinates">Cell coordinates</param>
    /// <param name="tileType">Cell tile type</param>
    public void Init(HexCoordinates coordinates, HexTileType tileType = HexTileType.EMPTY)
    {
        SetCoordinates(coordinates);
        SetTypes(tileType);

        gameObject.name = $"Hex Cell {coordinates}";

        if (terrainParent == null)
            terrainParent = transform.Find("Terrain");
        if (propsParent == null)
            propsParent = transform.Find("Props");

        DrawCell();
    }

    /// <summary>
    /// Draw the hex cell contents
    /// <br /><br />
    /// Note that this will clear cell terrain/props!
    /// </summary>
    public void DrawCell()
    {
        PositionCell();
        ClearTerrain();

        HexMapGenerator generator = HexMapGenerator.Instance;
        HexBlock block = generator.GetBlock(tileType);
        Instantiate(block.Prefab, terrainParent);

        // TODO: Only clear props if necessary (may have spawned other props)???
        ClearProps();

        // Create spawn point and face towards the path
        if (pathingType == HexPathingType.SPAWN)
        {
            Instantiate(generator.SpawnBlock, propsParent);
            if (Waypoint.NextWaypoint != null)
            {
                RotateProps(coordinates.Direction(Waypoint.NextWaypoint.HexCell.Coordinates));
            }
        }
        // Create destination point and face towards the path
        if (pathingType == HexPathingType.DESTINATION)
        {
            Instantiate(generator.DestinationBlock, propsParent);
            if (Waypoint.PreviousWaypoint != null)
            {
                RotateProps(coordinates.Direction(Waypoint.PreviousWaypoint.HexCell.Coordinates));
            }
        }
    }

    /// <summary>
    /// Rotate props towards a direction
    /// </summary>
    /// <param name="direction">Target direction</param>
    public void RotateProps(HexDirection? direction)
    {
        int angle = direction != null ? Coordinates.Angle((HexDirection)direction) : 0;

        // TODO: Consider applying rotation to specific objects if rotating props parent is bad?
        propsParent.rotation = Quaternion.Euler(0, angle, 0);
    }

    /// <summary>
    /// Update hex cell position
    /// </summary>
    public void PositionCell()
    {
        float yPosition = height * HexConstants.HeightMultiplier;
        transform.position = coordinates.GetPosition(yPosition);
    }

    /// <summary>
    /// Remove the cell props
    /// </summary>
    public void ClearTerrain()
    {
        terrainParent.RemoveChildren();
    }

    /// <summary>
    /// Remove the cell props
    /// </summary>
    public void ClearProps()
    {
        propsParent.RemoveChildren();
    }

    /// <summary>
    /// Toggle whether the hex cell is selected
    /// </summary>
    /// <param name="selected">Whether cell is selected</param>
    /// <param name="color">Selection color</param>
    public void ToggleSelection(bool selected, Color? color = null)
    {
        // NOTE: Line renderer will not be associated in Edit mode!
        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();

        if (selected && color.HasValue)
        {
            lineRenderer.startColor = color.Value;
            lineRenderer.endColor = color.Value;
        }

        lineRenderer.enabled = selected;
    }
    #endregion


    #region Debug Methods
    public override void DrawGizmos()
    {
        if (!HexMap?.ShowCoordinates ?? true) return;

        Quaternion rotation = Quaternion.Euler(90, 0, 0);
        Vector3 position = transform.position + Vector3.up;

        // DEBUG: Axial coordinates originate from the center
        Drawing.Draw.Label3D(new float3(position.x - 0.25f, position.y, position.z + 0.45f), rotation, $"{coordinates.Q}", 0.2f, LabelAlignment.Center);
        Drawing.Draw.Label3D(new float3(position.x + 0.5f, position.y, position.z), rotation, $"{coordinates.R}", 0.2f, LabelAlignment.Center);
        Drawing.Draw.Label3D(new float3(position.x - 0.25f, position.y, position.z - 0.45f), rotation, $"{coordinates.S}", 0.2f, LabelAlignment.Center, Color.gray);

        // DEBUG: Offset coordinates indicate the offset from top-left
        Drawing.Draw.Label3D(new float3(position.x, position.y, position.z), rotation, $"{OffsetCoordinates.Col}, {OffsetCoordinates.Row}", 0.15f, LabelAlignment.Center, Color.grey);
    }
    #endregion
}
