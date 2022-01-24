using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;


[Serializable]
public class HexBlock
{
    [Required]
    public string Name;
    [Required]
    public GameObject Prefab;
    [Required]
    public HexTileType Type;
}


[RequireComponent(typeof(HexMap))]
public class HexMapGenerator : GameSingleton<HexMapGenerator>
{
    #region Attributes
    [SerializeField]
    private int width = 15;
    [SerializeField]
    private int height = 7;

    [SerializeField]
    [ReadOnly]
    private bool CenterGrid = true;

    [Title("Generator Blocks")]
    [InfoBox("All tiles share the same parent prefab, but with different 'Block' children.", InfoMessageType = InfoMessageType.Info)]
    [Required]
    [AssetsOnly]
    [SerializeField]
    private HexTile tilePrefab;

    [SerializeField]
    private HexTileType defaultTileType = HexTileType.GRASS;

    [PropertySpace]
    [SerializeField]
    private List<HexBlock> blocks;

    [SerializeField]
    private GameObject spawnBlock;
    [SerializeField]
    private GameObject destinationBlock;

    [TitleGroup("Generator")]
    [HorizontalGroup("Generator/Generate", 0.75f)]
    [Button("Generate Grid", ButtonSizes.Medium), GUIColor(0.4f, 0.8f, 1)]
    private void GenerateClick()
    {
        bool hasGrid = GetComponentsInChildren<HexTile>().Length > 0;
        if (hasGrid)
        {
            EditorUtility.DisplayDialog("Already generated", "Map has already been generated and must be reset first!", "OK");
            return;
        }

        GenerateGrid();
    }


    [Button("Reset", ButtonSizes.Medium), GUIColor(1f, 0.8f, 0.4f)]
    [HorizontalGroup("Generator/Generate", 0.25f)]
    private void ResetClick()
    {
        bool hasGrid = GetComponentsInChildren<HexTile>().Length > 0;
        if (hasGrid)
        {
            bool confirmation = EditorUtility.DisplayDialog("Regenerate map?", "Are you sure you want to regenerate the map?", "Confirm", "Cancel");
            if (!confirmation) return;
        }

        ResetMap();
    }
    #endregion


    #region Properties
    public List<HexBlock> Blocks => blocks;
    public GameObject SpawnBlock => spawnBlock;
    public GameObject DestinationBlock => destinationBlock;
    #endregion


    #region Unity Methods
    #endregion


    #region Custom Methods
    private void GenerateGrid()
    {
        // Prevent regenerating grid over existing grid
        bool hasGrid = GetComponentsInChildren<HexTile>().Length > 0;
        if (hasGrid)
        {
            Debug.LogWarning("Grid has already been generated!");
            return;
        }
        ResetMap();

        int halfHeight = (int)Mathf.Floor(height / 2);
        int halfWidth = (int)Mathf.Floor(width / 2);
        bool evenHeight = height % 2 == 0;
        bool evenWidth = width % 2 == 0;

        // Grids that start from the center require some specific math to appropriately handle
        //   the axis changes from positive/negative. Additionally, centered and odd-length axis
        //   need length correction due to the flooring needed to properly calculate half lengths.
        int heightStart = CenterGrid ? -halfHeight : 0;
        int heightEnd = CenterGrid ? evenHeight ? halfHeight : halfHeight + 1 : height;
        int widthStart = CenterGrid ? -halfWidth : 0;
        int widthEnd = CenterGrid ? evenWidth ? halfWidth : halfWidth + 1 : width;

        HexTile[] tiles = new HexTile[height * width];

        for (int z = heightStart, i = 0; z < heightEnd; z++)
        {
            for (int x = widthStart; x < widthEnd; x++)
            {
                CreateTile(x, z, i++);
            }
        }

        GetComponent<HexMap>().PrepareTiles();
    }

    /// <summary>
    /// Create a new Hex tile
    /// </summary>
    /// <param name="x">Tile x offset coordinate</param>
    /// <param name="z">Tile z offset coordinate</param>
    /// <param name="i">Tile index</param>
    /// <returns>New Hex tile</returns>
    private HexTile CreateTile(int x, int z, int i)
    {
        HexCoordinates coordinates = HexCoordinates.FromOffset(x, z);

        HexTile tile = Instantiate(tilePrefab, transform);
        tile.Init(coordinates, defaultTileType);
        return tile;
    }

    /// <summary>
    /// Get a block from the block map
    /// </summary>
    /// <param name="tileType">Block search tile type</param>
    /// <returns>Matching block</returns>
    public HexBlock GetBlock(HexTileType tileType)
    {
        bool isValidBlock = Blocks.Any(b => b.Type == tileType);

        if (!isValidBlock) throw new Exception("No matching block type found for " + tileType.ToString());

        return Blocks.Find(b => b.Type == tileType);
    }

    /// <summary>
    /// Reset/clear the hex map
    /// </summary>
    private void ResetMap()
    {
        HexTile[] tiles = GetComponentsInChildren<HexTile>();
        tiles.ForEach((tile) =>
        {
            if (tile == null) return;
            tile.gameObject.DestroySelf();
        });
    }
    #endregion
}
