#nullable enable

using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class HexTileDictionary : UnitySerializedDictionary<string, HexTile> { }

public class HexMap : GameSingleton<MonoBehaviour>
{
    #region Attributes
    [Title("Tiles")]
    [DisplayAsString]
    [ShowInInspector]
    public int TileCount => tiles.Count;
    [DisplayAsString]
    [ShowInInspector]
    public int SpawnLocations => SpawnTiles.Count;
    [DisplayAsString]
    [ShowInInspector]
    public int DestinationLocations => DestinationTiles.Count;

    [Title("Waypoints")]
    [DisplayAsString]
    [ShowInInspector]
    public int WaypointCount => waypoints.Count;

    [Title("Debug")]
    [SerializeField]
    private bool showCoordinates = false;
    [SerializeField]
    private bool showWaypoints = false;

    [TitleGroup("Tile Actions")]
    [HorizontalGroup("Tile Actions/Actions")]
    [Button("Prepare Tiles", ButtonSizes.Medium)]
    private void PrepareTilesClick() => PrepareTiles();
    [HorizontalGroup("Tile Actions/Actions")]
    [Button("Draw Tiles", ButtonSizes.Medium)]
    private void DrawTilesClick() => DrawTiles();

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
    /// Hex map tiles.
    /// <br /><br />
    /// Tiles are stored in a dictionary using their coordinates as a key!
    /// </summary>
    public HexTileDictionary Tiles => tiles;
    /// <summary>
    /// List of hex map spawn tiles (currently limited to 1!)
    /// </summary>
    [HideInInspector]
    public List<HexTile> SpawnTiles = new List<HexTile>();
    /// <summary>
    /// List of hex map spawn tiles (currently limited to 1!)
    /// </summary>
    [HideInInspector]
    public List<HexTile> DestinationTiles = new List<HexTile>();
    /// <summary>
    /// List of map path waypoints
    /// </summary>
    public List<PathWaypoint> Waypoints => waypoints;

    public bool ShowCoordinates => showCoordinates;
    public bool ShowWaypoints => showWaypoints;
    public bool HasSpawn => SpawnTiles.Count > 0;
    public bool HasDestination => DestinationTiles.Count > 0;
    public PathWaypoint? FirstWaypoint => Waypoints.Count > 0 ? Waypoints[0] : null;
    public PathWaypoint? LastWaypoint => Waypoints.Count > 0 ? Waypoints[Waypoints.Count - 1] : null;
    #endregion


    [SerializeField, HideInInspector]
    private HexTileDictionary tiles = new HexTileDictionary();
    [SerializeField, HideInInspector]
    private List<PathWaypoint> waypoints = new List<PathWaypoint>();



    #region Unity Methods
    void Start()
    {
        PrepareTiles();
    }
    #endregion


    #region Accessors
    /// <summary>
    /// Set the grid tiles
    /// </summary>
    /// <param name="tiles">Grid tiles</param>
    public void SetTiles(HexTileDictionary tiles)
    {
        this.tiles = tiles;
    }
    #endregion


    #region Custom Methods
    /// <summary>
    /// Prepare and redraw the entire map
    /// </summary>
    private void DrawTiles()
    {
        PrepareTiles();

        GetTileList().ForEach((tile) => tile.DrawTile());
    }

    /// <summary>
    /// Convert tile dictionary to list
    /// </summary>
    /// <returns>Tile list</returns>
    private List<HexTile> GetTileList() => Tiles.Values.ToList();

    /// <summary>
    /// Convert a tile list to a tile dictionary.
    /// <br /><br />
    /// This provides much easier lookup by tile coordinate key.
    /// </summary>
    /// <param name="tileList">List of tiles</param>
    /// <returns>Tile dictionary</returns>
    public HexTileDictionary TilesToDictionary(HexTile[] tileList)
    {
        HexTileDictionary tileDictionary = new HexTileDictionary();

        tileList.ForEach((tile) =>
        {
            tileDictionary.Add(tile.Coordinates.ToKey(), tile);
        });

        return tileDictionary;
    }

    /// <summary>
    /// Get a tile by its coordinates (if exists)
    /// </summary>
    /// <param name="tileDictionary">Tile list/dictionary</param>
    /// <param name="coordinates">Target coordinates</param>
    /// <returns>Targeted tile (if exists)</returns>
    public static HexTile? GetGridTile(HexTileDictionary tileDictionary, HexCoordinates coordinates)
    {
        HexTile tile;
        bool found = tileDictionary.TryGetValue((coordinates).ToKey(), out tile);
        return found ? tile : null;
    }

    /// <summary>
    /// Find hex map tiles and calculate pathing
    /// </summary>
    /// <returns>Prepared map tiles</returns>
    public HexTileDictionary PrepareTiles()
    {
        var tiles = FindTiles();

        CalculatePaths();

        return tiles;
    }

    /// <summary>
    /// Find grid tiles and associate parents/neighbours
    /// </summary>
    public HexTileDictionary FindTiles()
    {
        HexTile[] childTiles = GetComponentsInChildren<HexTile>();
        var tileDictionary = TilesToDictionary(childTiles);

        SpawnTiles.Clear();
        DestinationTiles.Clear();

        childTiles.ForEach((tile) =>
        {
            tile.SetMap(this);
            tile.Waypoint.SetTile(tile);

            if (tile.PathingType == HexPathingType.SPAWN)
                SpawnTiles.Add(tile);
            if (tile.PathingType == HexPathingType.DESTINATION)
                DestinationTiles.Add(tile);

            // Calculate and associate neighbours
            HexCoordinates coordinates = tile.Coordinates;
            tile.ClearNeighbours();
            HexCoordinates.Directions.Keys.ToList().ForEach((direction) =>
            {
                HexTile? neighbourTile = GetGridTile(tileDictionary, coordinates.Neighbor(direction));
                tile.SetNeighbour(direction, neighbourTile);
            });
        });

        SetTiles(tileDictionary);

        return tileDictionary;
    }

    /// <summary>
    /// Calculate map pathing (after validation)
    /// </summary>
    private List<HexTile> CalculatePaths()
    {
        List<HexTile> path = new List<HexTile>();
        List<HexTile> emptyPath = new List<HexTile>();

        bool validStartEnd = ValidatePathEnds();
        if (!validStartEnd) return emptyPath;

        // TODO: Consider resetting previous path AND waypoints?
        ResetWaypoints();

        // NOTE: Only supports a single spawn point for now!
        HexTile currentTile = SpawnTiles[0];
        HexTile? nextTile = null;

        path.Add(currentTile);

        int maxIterations = 1000;
        int iterations = 0;

        do
        {
            nextTile = null;

            // TODO: Implement warning if a tile has more than 2 path neighbours!
            if (currentTile.GetNeighbourPaths().Count() > 2)
            {
                Debug.LogWarning("Paths may not have more than 2 path neighbours!");
                return emptyPath;
            }

            // Pathing should find the first (untravelled) neighbour path and then exit
            for (int i = 0; i < currentTile.Neighbours.Length; i++)
            {
                iterations++;

                var neighbour = currentTile.Neighbours[i];
                if (neighbour == null || neighbour.TileType != HexTileType.PATH) continue;

                // Prevent visiting previously travelled paths (causes infinite loops)!
                bool alreadyInPath = path.Any((p) => neighbour.Coordinates == p.Coordinates);
                if (alreadyInPath) continue;

                nextTile = neighbour;
                break;
            }

            if (nextTile == null) continue;

            path.Add(nextTile);
            currentTile = nextTile;
        } while (nextTile != null && iterations < maxIterations);

        // Only link waypoints together once pathing is validated/completed
        PathWaypoint? previousWaypoint = null;
        int waypointNumber = 1;
        path.ForEach((tile) =>
        {
            PathWaypoint currentWaypoint = tile.Waypoint;
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
        int spawnTileCount = SpawnTiles.Count;
        if (spawnTileCount != 1)
        {
            Debug.LogWarning(spawnTileCount == 0 ? "No spawn tiles found for pathing!" : "Too many spawn tiles found for pathing!");
            return false;
        }
        else if (SpawnTiles[0].GetNeighbourPaths().Length != 1)
        {
            Debug.LogWarning("Spawn tile must be at the start of a path!");
            return false;
        }

        int destinationTileCount = DestinationTiles.Count;
        if (destinationTileCount != 1)
        {
            Debug.LogWarning(destinationTileCount == 0 ? "No destination tiles found for pathing!" : "Too many destination tiles found for pathing!");
            return false;
        }
        else if (DestinationTiles[0].GetNeighbourPaths().Length != 1)
        {
            Debug.LogWarning("Destination tile must be at the end of a path!");
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
            point?.Reset();
        });

        Waypoints.Clear();
    }
    #endregion
}
