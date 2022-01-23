#nullable enable

using RotaryHeart.Lib.SerializableDictionary;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[Serializable]
public class HexCellDictionary : SerializableDictionaryBase<string, HexCell> { }


public class HexMap : GameSingleton<MonoBehaviour>
{
    #region Attributes
    [Title("Cells")]
    [DisplayAsString]
    [ShowInInspector]
    public int CellCount => cells.Count;
    [DisplayAsString]
    [ShowInInspector]
    public int SpawnLocations => SpawnCells.Count;
    [DisplayAsString]
    [ShowInInspector]
    public int DestinationLocations => DestinationCells.Count;

    [Title("Waypoints")]
    [DisplayAsString]
    [ShowInInspector]
    public int WaypointCount => waypoints.Count;

    [Title("Debug")]
    [SerializeField]
    private bool showCoordinates = false;
    [SerializeField]
    private bool showWaypoints = false;

    [TitleGroup("Cell Actions")]
    [HorizontalGroup("Cell Actions/Actions")]
    [Button("Prepare Cells", ButtonSizes.Medium)]
    private void PrepareCellsClick() => PrepareCells();
    [HorizontalGroup("Cell Actions/Actions")]
    [Button("Draw Cells", ButtonSizes.Medium)]
    private void DrawCellsClick() => DrawCells();

    [TitleGroup("Waypoint Actions")]
    [HorizontalGroup("Waypoint Actions/Actions")]
    [Button("Find Waypoints", ButtonSizes.Medium)]
    private void FindWaypointsClick() => CalculatePaths();
    [HorizontalGroup("Waypoint Actions/Actions")]
    [ReadOnly]
    [Button("Reset Waypoints", ButtonSizes.Medium)]
    private void ResetWaypointsClick() => ResetWaypoints();
    #endregion


    #region Properties
    /// <summary>
    /// Hex map cells.
    /// <br /><br />
    /// Cells are stored in a dictionary using their coordinates as a key!
    /// </summary>
    public HexCellDictionary Cells => cells;
    /// <summary>
    /// List of hex map spawn cells (currently limited to 1!)
    /// </summary>
    [HideInInspector]
    public List<HexCell> SpawnCells = new List<HexCell>();
    /// <summary>
    /// List of hex map spawn cells (currently limited to 1!)
    /// </summary>
    [HideInInspector]
    public List<HexCell> DestinationCells = new List<HexCell>();
    /// <summary>
    /// List of map path waypoints
    /// </summary>
    public List<PathWaypoint> Waypoints => waypoints;

    public bool ShowCoordinates => showCoordinates;
    public bool ShowWaypoints => showWaypoints;
    public bool HasSpawn => SpawnCells.Count > 0;
    public bool HasDestination => DestinationCells.Count > 0;
    public PathWaypoint? FirstWaypoint => Waypoints.Count > 0 ? Waypoints[0] : null;
    public PathWaypoint? LastWaypoint => Waypoints.Count > 0 ? Waypoints[Waypoints.Count - 1] : null;
    #endregion


    [SerializeField, HideInInspector]
    private HexCellDictionary cells = new HexCellDictionary();
    [SerializeField, HideInInspector]
    private List<PathWaypoint> waypoints = new List<PathWaypoint>();



    #region Unity Methods
    void Start()
    {
        PrepareCells();
    }
    #endregion


    #region Accessors
    /// <summary>
    /// Set the grid cells
    /// </summary>
    /// <param name="cells">Grid cells</param>
    public void SetCells(HexCellDictionary cells)
    {
        this.cells = cells;
    }
    #endregion


    #region Custom Methods
    /// <summary>
    /// Prepare and redraw the entire map
    /// </summary>
    private void DrawCells()
    {
        PrepareCells();

        GetCellList().ForEach((cell) => cell.DrawCell());
    }

    /// <summary>
    /// Convert cell dictionary to list
    /// </summary>
    /// <returns>Cell list</returns>
    private List<HexCell> GetCellList() => Cells.Values.ToList();

    /// <summary>
    /// Convert a cell list to a cell dictionary.
    /// <br /><br />
    /// This provides much easier lookup by cell coordinate key.
    /// </summary>
    /// <param name="cellList">List of cells</param>
    /// <returns>Cell dictionary</returns>
    public HexCellDictionary CellsToDictionary(HexCell[] cellList)
    {
        HexCellDictionary cellDictionary = new HexCellDictionary();

        cellList.ForEach((cell) =>
        {
            cellDictionary.Add(cell.Coordinates.ToKey(), cell);
        });

        return cellDictionary;
    }

    /// <summary>
    /// Get a cell by its coordinates (if exists)
    /// </summary>
    /// <param name="cellDictionary">Cell list/dictionary</param>
    /// <param name="coordinates">Target coordinates</param>
    /// <returns>Targeted cell (if exists)</returns>
    public static HexCell? GetGridCell(HexCellDictionary cellDictionary, HexCoordinates coordinates)
    {
        HexCell cell;
        bool found = cellDictionary.TryGetValue((coordinates).ToKey(), out cell);
        return found ? cell : null;
    }

    /// <summary>
    /// Find hex map cells and calculate pathing
    /// </summary>
    /// <returns>Prepared map cells</returns>
    public HexCellDictionary PrepareCells()
    {
        var cells = FindCells();

        CalculatePaths();

        return cells;
    }

    /// <summary>
    /// Find grid cells and associate parents/neighbours
    /// </summary>
    public HexCellDictionary FindCells()
    {
        HexCell[] childCells = GetComponentsInChildren<HexCell>();
        var cellDictionary = CellsToDictionary(childCells);

        SpawnCells.Clear();
        DestinationCells.Clear();

        childCells.ForEach((cell) =>
        {
            cell.SetMap(this);
            cell.Waypoint.SetCell(cell);

            if (cell.PathingType == HexPathingType.SPAWN)
                SpawnCells.Add(cell);
            if (cell.PathingType == HexPathingType.DESTINATION)
                DestinationCells.Add(cell);

            // Calculate and associate neighbours
            HexCoordinates coordinates = cell.Coordinates;
            cell.ClearNeighbours();
            HexCoordinates.Directions.Keys.ToList().ForEach((direction) =>
            {
                HexCell? neighborCell = GetGridCell(cellDictionary, coordinates.Neighbor(direction));
                cell.SetNeighbour(direction, neighborCell);
            });
        });

        SetCells(cellDictionary);

        return cellDictionary;
    }

    /// <summary>
    /// Calculate map pathing (after validation)
    /// </summary>
    private List<HexCell> CalculatePaths()
    {
        List<HexCell> path = new List<HexCell>();
        List<HexCell> emptyPath = new List<HexCell>();

        bool validStartEnd = ValidatePathEnds();
        if (!validStartEnd) return emptyPath;

        // TODO: Consider resetting previous path AND waypoints?
        ResetWaypoints();

        // NOTE: Only supports a single spawn point for now!
        HexCell currentCell = SpawnCells[0];
        HexCell? nextCell = null;

        path.Add(currentCell);

        int maxIterations = 1000;
        int iterations = 0;

        do
        {
            nextCell = null;

            // TODO: Implement warning if a tile has more than 2 path neighbours!
            if (currentCell.GetNeighbourPaths().Count() > 2)
            {
                Debug.LogWarning("Paths may not have more than 2 path neighbours!");
                return emptyPath;
            }

            // Pathing should find the first (untravelled) neighbour path and then exit
            for (int i = 0; i < currentCell.Neighbours.Length; i++)
            {
                iterations++;

                var neighbour = currentCell.Neighbours[i];
                if (neighbour == null || neighbour.TileType != HexTileType.PATH) continue;

                // Prevent visiting previously travelled paths (causes infinite loops)!
                bool alreadyInPath = path.Any((p) => neighbour.Coordinates == p.Coordinates);
                if (alreadyInPath) continue;

                nextCell = neighbour;
                break;
            }

            if (nextCell == null) continue;

            path.Add(nextCell);
            currentCell = nextCell;
        } while (nextCell != null && iterations < maxIterations);

        // Only link waypoints together once pathing is validated/completed
        PathWaypoint? previousWaypoint = null;
        int waypointNumber = 1;
        path.ForEach((cell) =>
        {
            PathWaypoint currentWaypoint = cell.Waypoint;
            currentWaypoint.Init(waypointNumber, previousWaypoint);
            Waypoints.Add(currentWaypoint);

            previousWaypoint?.SetNextWaypoint(currentWaypoint);
            previousWaypoint = currentWaypoint;
            waypointNumber++;
        });

        Debug.Log($"Calculated path ({path.Count} waypoints)");

        return path;
    }

    /// <summary>
    /// Validate the map spawn/destination before calculating pathing
    /// </summary>
    private bool ValidatePathEnds()
    {
        int spawnCellCount = SpawnCells.Count;
        if (spawnCellCount != 1)
        {
            Debug.LogWarning(spawnCellCount == 0 ? "No spawn cells found for pathing!" : "Too many spawn cells found for pathing!");
            return false;
        }
        else if (SpawnCells[0].GetNeighbourPaths().Length != 1)
        {
            Debug.LogWarning("Spawn cell must be at the start of a path!");
            return false;
        }

        int destinationCellCount = DestinationCells.Count;
        if (destinationCellCount != 1)
        {
            Debug.LogWarning(destinationCellCount == 0 ? "No destination cells found for pathing!" : "Too many destination cells found for pathing!");
            return false;
        }
        else if (DestinationCells[0].GetNeighbourPaths().Length != 1)
        {
            Debug.LogWarning("Destination cell must be at the end of a path!");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Reset all hex path waypoints
    /// </summary>
    private void ResetWaypoints()
    {
        Waypoints.ForEach((point) =>
        {
            point.Reset();
        });

        Waypoints.Clear();
    }
    #endregion
}
