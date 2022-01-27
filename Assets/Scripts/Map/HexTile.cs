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
[RequireComponent(typeof(HexHighlight))]
public class HexTile : MonoBehaviourGizmos
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
    public int NeighbourCount => neighbours.Count((tile) => tile != null);

    [Title("Attributes")]
    [SerializeField]
    private HexTileType tileType = HexTileType.EMPTY;
    [SerializeField]
    private HexPathingType pathingType = HexPathingType.NORMAL;
    [SerializeField]
    private int height = 0;

    [Title("Game Objects")]
#nullable disable warnings
    [Required]
    [SerializeField]
    private Transform terrainParent;
    [Required]
    [SerializeField]
    private Transform propsParent;
#nullable enable warnings

    [Title("Tower")]
    [SerializeField]
    [ReadOnly]
    private Tower? _tower;
    #endregion


    #region Properties
    /// <summary>
    /// Hex tile height
    /// </summary>
    public int Height => height;
    /// <summary>
    /// Hex tile type (terrain)
    /// </summary>
    public HexTileType TileType => tileType;
    /// <summary>
    /// Hex tile pathfinding type
    /// </summary>
    public HexPathingType PathingType => pathingType;
    /// <summary>
    /// All hex tile neighbours (clockwise from NE)
    /// <br /><br />
    /// Note that tile neighbours will be null/empty on map borders!
    /// </summary>
    public HexTile?[] Neighbours => neighbours;
    /// <summary>
    /// Hex tile coordinates
    /// </summary>
    public HexCoordinates Coordinates => coordinates;
    /// <summary>
    /// Hex tile parent map
    /// </summary>
    public HexMap HexMap => _hexMap ??= GetComponentInChildren<HexMap>();
    public PathWaypoint Waypoint => _waypoint ??= GetComponentInChildren<PathWaypoint>();
    public Tower? tower => _tower ??= GetComponentInChildren<Tower>();
    #endregion


#nullable disable warnings
    [SerializeField, HideInInspector]
    private HexTile?[] neighbours = new HexTile[6];
    [SerializeField, HideInInspector]
    private HexMap _hexMap;
    private PathWaypoint _waypoint;
    private HexHighlight _hexHighlight;
#nullable enable warnings


    #region Unity Methods
    void Awake()
    {
        _hexHighlight = GetComponent<HexHighlight>();

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
    /// Get a tile's neighbour in a direction
    /// </summary>
    public HexTile? GetNeighbour(HexDirection direction)
    {
        return neighbours[(int)direction];
    }

    /// <summary>
    /// Get all neighbouring path tiles
    /// </summary>
    public HexTile[] GetNeighbourPaths()
    {
#nullable disable warnings
        return neighbours.Where((n) => n != null && n.tileType == HexTileType.PATH).ToArray();
#nullable enable warnings
    }

    public void ClearNeighbours()
    {
        neighbours = new HexTile[6];
    }

    public void SetNeighbour(HexDirection direction, HexTile? tile)
    {
        neighbours[(int)direction] = tile;
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
    /// Initialize a Hex tile
    /// </summary>
    public void Init(HexCoordinates coordinates, HexTileType tileType = HexTileType.EMPTY)
    {
        SetCoordinates(coordinates);
        SetTypes(tileType);

        gameObject.name = $"Hex Tile {coordinates}";

        if (terrainParent == null)
            terrainParent = transform.Find("Terrain");
        if (propsParent == null)
            propsParent = transform.Find("Props");

        DrawTile();
    }

    /// <summary>
    /// Draw the hex tile contents
    /// <br /><br />
    /// Note that this will clear tile terrain/props!
    /// </summary>
    public void DrawTile()
    {
        PositionTile();
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
                RotateProps(coordinates.Direction(Waypoint.NextWaypoint.HexTile.Coordinates));
            }
        }
        // Create destination point and face towards the path
        if (pathingType == HexPathingType.DESTINATION)
        {
            Instantiate(generator.DestinationBlock, propsParent);
            if (Waypoint.PreviousWaypoint != null)
            {
                RotateProps(coordinates.Direction(Waypoint.PreviousWaypoint.HexTile.Coordinates));
            }
        }
    }

    /// <summary>
    /// Rotate props towards a direction
    /// </summary>
    public void RotateProps(HexDirection? direction)
    {
        int angle = direction != null ? Coordinates.Angle((HexDirection)direction) : 0;

        // TODO: Consider applying rotation to specific objects if rotating props parent is bad?
        propsParent.rotation = Quaternion.Euler(0, angle, 0);
    }

    /// <summary>
    /// Update hex tile position
    /// </summary>
    public void PositionTile()
    {
        float yPosition = height * HexConstants.HeightMultiplier;
        transform.position = coordinates.GetPosition(yPosition);
    }

    /// <summary>
    /// Remove the tile props
    /// </summary>
    public void ClearTerrain()
    {
        terrainParent.RemoveChildren();
    }

    /// <summary>
    /// Remove the tile props (spawned objects)
    /// </summary>
    public void ClearProps()
    {
        propsParent.RemoveChildren();
    }

    public void ShowProgress(float progress, Color? color = null)
    {
        _hexHighlight.SetProgress(progress, color);
    }

    public void ToggleSelection(bool selected, Color? color = null)
    {
        // NOTE: Hex highlight will not be associated in Edit mode!
        if (_hexHighlight == null)
            _hexHighlight = GetComponent<HexHighlight>();

        _hexHighlight.ToggleOutline(selected, color);
    }

    /// <summary>
    /// Calculate points of a hex (pointy-side up, clockwise)
    /// </summary>
    public static Vector2[] CalculateHexPoints(float size, Vector2? position = null)
    {
        Vector2 offsetPosition = position.HasValue ? position.Value : Vector2.zero;
        int points = 7;
        Vector2[] vertices = new Vector2[points];

        float angle = 2 * Mathf.PI / 6;

        for (int i = 0; i < points; i++)
        {
            vertices[i] = new Vector2(
                offsetPosition.x + size * Mathf.Sin(angle * i),
                offsetPosition.y + size * Mathf.Cos(angle * i)
            );
        }

        return vertices;
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
