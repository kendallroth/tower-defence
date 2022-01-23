#nullable enable

using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class MapEditorWindow : OdinEditorWindow
{
    #region Properties
    [Title("Selected Hex")]
    [PropertyOrder(-3)]
    [LabelText("Coordinates")]
    [DisplayAsString]
    [ShowInInspector]
    private string currentHexCoordinates => currentHex?.Coordinates.ToString() ?? "-";

    [PropertyOrder(-2)]
    [SerializeField]
    private Color selectionColor;

    [PropertySpace(SpaceBefore = 8)]
    [PropertyOrder(-1)]
    [Button("@\"Clear Selection (\" + selections.Count + \")\"", ButtonSizes.Medium)]
    [DisableIf("@!hasSelection")]
    private void ClearClick() => ClearSelection();

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

    private HexCell? currentHex => selections.Count > 0 ? selections.Last() : null;
    private bool hasSelection => selections.Count > 0;
    private List<HexCell> selections = new List<HexCell>();
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
        // Always reset selected tool (will hide again if selecting hex)
        if (lastTool != Tool.None)
            Tools.current = lastTool;

        if (!Selection.activeGameObject)
        {
            ClearSelection();
            return;
        }

        // Clicking on an already selected cell will target objects inside the parent cell,
        //   but we only care about the hex cell (hence checking the parents).
        HexCell selectedHex = Selection.activeGameObject.GetComponent<HexCell>();
        if (selectedHex == null)
        {
            selectedHex = Selection.activeGameObject.GetComponentInParent<HexCell>();
            if (selectedHex == null)
            {
                ClearSelection();
                return;
            }
        }

        // Set update form properties
        SetFormValues(selectedHex);

        // Only store last tool if valid and hide current tool to prevent moving/rotating
        if (Tools.current != Tool.None)
            lastTool = Tools.current;
        Tools.current = Tool.None;

        // Deselect hexes when they are re-selected
        HexCell? alreadySelectedHex = selections.Find((hex) => hex.Coordinates == selectedHex.Coordinates);
        if (alreadySelectedHex != null)
        {
            SetFormValues(null);
            alreadySelectedHex.ToggleSelection(false);
            selections.Remove(alreadySelectedHex);
            return;
        }

        // Allow selecting multiple tiles with shift
        if (!Keyboard.current.leftShiftKey.isPressed)
            ClearSelection();

        selectedHex.ToggleSelection(true, selectionColor);

        // TODO: Figure out how to clear previous selection???
        //Selection.objects = new GameObject[0];
        //Selection.objects = new GameObject[]{selectedHex.gameObject};

        selections.Add(selectedHex);
    }
    #endregion


    #region Custom Methods
    /// <summary>
    /// Clear selected cells
    /// </summary>
    private void ClearSelection()
    {
        if (selections.Count == 0) return;

        selections.ForEach((hex) =>
        {
            hex.ToggleSelection(false);
        });
        selections.Clear();
    }

    /// <summary>
    /// Set the selected cell form values
    /// </summary>
    /// <param name="selectedHex">Selected hex (or last selected)</param>
    private void SetFormValues(HexCell? selectedHex)
    {
        height = selectedHex != null ? selectedHex.Height : 0;
        tileType = selectedHex != null ? selectedHex.TileType : HexTileType.EMPTY;
        pathingType = selectedHex != null ? selectedHex.PathingType : HexPathingType.NORMAL;
    }

    private void UpdateHeight()
    {
        selections.ForEach((hex) =>
        {
            hex.SetHeight(height);
            hex.DrawCell();
        });
    }

    private void UpdateTileType()
    {
        // TODO: Consider pathing options when changing to/from a path!!!

        selections.ForEach((hex) =>
        {
            hex.SetTypes(tileType, hex.PathingType);
            hex.DrawCell();
        });
    }

    private void UpdatePathingType()
    {
        selections.ForEach((hex) =>
        {
            hex.SetTypes(hex.TileType, pathingType);
            hex.DrawCell();
        });
    }
    #endregion
}
