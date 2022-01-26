#nullable enable

using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class MapEditorWindow : OdinEditorWindow
{
    #region Properties
    [Title("Selected Tile")]
    [PropertyOrder(-3)]
    [LabelText("Coordinates")]
    [DisplayAsString]
    [ShowInInspector]
    private string currentTileCoordinates => currentTile?.Coordinates.ToString() ?? "-";

    [PropertyOrder(-2)]
    [SerializeField]
    private Color selectionColor;

    [PropertySpace(SpaceBefore = 8)]
    [PropertyOrder(-1)]
    [Button("@\"Clear Selection (\" + selections.Count + \")\"", ButtonSizes.Medium)]
    [DisableIf("@!hasSelection")]
    private void ClearClick() => ClearSelectedTiles();

    [Title("Update")]
    [DisableIf("@!hasSelection")]
    [InlineButton("UpdateTileType", "Update")]
    [SerializeField]
    private HexTileType tileType = HexTileType.EMPTY;

    [DisableIf("@!hasSelection")]
    [InlineButton("UpdatePathingType", "Update")]
    [SerializeField]
    private HexPathingType pathingType = HexPathingType.NORMAL;

    [DisableIf("@!hasSelection")]
    [InlineButton("UpdateHeight", "Update")]
    [SerializeField]
    private int height;
    #endregion

    private HexTile? currentTile => selections.Count > 0 ? selections.Last() : null;
    private bool hasSelection => selections.Count > 0;
    private List<HexTile> selections = new List<HexTile>();
    private Tool lastTool = Tool.None;


    #region Unity Methods
    [MenuItem("Tools/Map Editor")]
    private static void OpenWindow()
    {
        var window = GetWindow<MapEditorWindow>();
        window.Show();
        window.titleContent = new GUIContent("Map Editor");
    }

    private void OnSelectionChange()
    {
        // Always reset selected tool (will hide again if selecting tile)
        if (lastTool != Tool.None)
            Tools.current = lastTool;

        if (!Selection.activeGameObject)
        {
            ClearSelectedTiles();
            return;
        }

        // Clicking on an already selected tile will target objects inside the parent tile,
        //   but we only care about the hex tile (hence checking the parents).
        HexTile selectedTile = Selection.activeGameObject.GetComponent<HexTile>();
        selectedTile ??= Selection.activeGameObject.GetComponentInParent <HexTile>();

        if (selectedTile != null)
            HandleTileSelect(selectedTile);
        else
            ClearSelectedTiles();
    }
    #endregion


    #region Custom Methods
    private void HandleTileSelect(HexTile tile)
    {
        // Set update form properties
        SetFormValues(tile);

        // Allow selecting multiple tiles with shift
        bool selectingMultiple = Keyboard.current.leftShiftKey.isPressed;

        // Only store last tool if valid and hide current tool to prevent moving/rotating
        if (Tools.current != Tool.None)
            lastTool = Tools.current;
        Tools.current = Tool.None;

        // Selecting a tile again typically will unselect it, although this changes when
        //   multiple tiles are being selected.
        HexTile? alreadySelectedTile = selections.Find((t) => t.Coordinates == tile.Coordinates);
        if (alreadySelectedTile != null)
        {
            if (selectingMultiple) return;

            alreadySelectedTile.ToggleSelection(false);
            selections.Remove(alreadySelectedTile);
            SetFormValues(null);
            DeselectCurrentObject();
            return;
        }

        if (!selectingMultiple)
            ClearSelectedTiles();

        tile.ToggleSelection(true, selectionColor);
        selections.Add(tile);
    }

    /// <summary>
    /// Deselecting currently selected objects must be done after the selection handler
    ///   has completed, otherwise any children will be set as selected object!
    /// </summary>
    private async void DeselectCurrentObject()
    {
        await Task.Yield();
        Selection.activeGameObject = null;
    }

    private void ClearSelectedTiles()
    {
        if (selections.Count == 0) return;

        selections.ForEach((tile) =>
        {
            tile.ToggleSelection(false);
        });
        selections.Clear();
    }

    /// <summary>
    /// Set the selected tile form values
    /// </summary>
    /// <param name="selectedTile">Selected tile (or last selected)</param>
    private void SetFormValues(HexTile? selectedTile)
    {
        height = selectedTile != null ? selectedTile.Height : 0;
        tileType = selectedTile != null ? selectedTile.TileType : HexTileType.EMPTY;
        pathingType = selectedTile != null ? selectedTile.PathingType : HexPathingType.NORMAL;
    }

    private void UpdateHeight()
    {
        selections.ForEach((tile) =>
        {
            tile.SetHeight(height);
            tile.DrawTile();
        });
    }

    private void UpdateTileType()
    {
        // TODO: Consider pathing options when changing to/from a path!!!

        selections.ForEach((tile) =>
        {
            tile.SetTypes(tileType, tile.PathingType);
            tile.DrawTile();
        });
    }

    private void UpdatePathingType()
    {
        selections.ForEach((tile) =>
        {
            tile.SetTypes(tile.TileType, pathingType);
            tile.DrawTile();
        });
    }
    #endregion
}
