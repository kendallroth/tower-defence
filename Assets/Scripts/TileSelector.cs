#nullable enable

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TileSelector : GameSingleton<MonoBehaviour>
{
    #region Attributes
    [SerializeField]
    private LayerMask tileLayer;
    [SerializeField]
    private Color selectionColor;
    [SerializeField]
    private Color highlightColor;
    #endregion


    #region Properties
    public HexCell? SelectedTile { get; private set; } = null;
    public HexCell? HighlightedTile { get; private set; } = null;
    #endregion

    private Color mixedColor => Color.Lerp(highlightColor, selectionColor, 0.75f);


    #region Unity Methods
    void Start()
    {

    }

    void Update()
    {
        CalculateSelection();
    }
    #endregion


    #region Custom Methods
    /// <summary>
    /// Calculate current tile selection and highlight
    /// </summary>
    private void CalculateSelection()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100f, tileLayer))
        {
            HexCell tile = hit.collider.GetComponent<HexCell>();
            if (tile == null) return;

            bool alreadyHighlighted = CompareTiles(HighlightedTile, tile);
            if (!alreadyHighlighted)
                HighlightTile(tile);

            // TODO: Move this to new input system handler!
            if (Mouse.current.leftButton.wasPressedThisFrame)
                SelectTile(tile, true);
        }
        else
        {
            ClearHighlight();
        }
    }

    /// <summary>
    /// Compare two hex tiles together for equality
    /// </summary>
    /// <param name="target">Target tile</param>
    /// <param name="other">Comparison tile</param>
    /// <param name="useCoordinates">Whether to equate by coordinates (if references are lost)</param>
    /// <returns>Whether two hex tiles are the same</returns>
    private bool CompareTiles(HexCell? target, HexCell? other, bool useCoordinates = false)
    {
        if (target == null || other == null) return false;

        return useCoordinates ? target.Coordinates == other.Coordinates : target == other;
    }

    /// <summary>
    /// Apply a highlight to a tile
    /// </summary>
    /// <param name="tile">Highlighted tile</param>
    public void HighlightTile(HexCell tile)
    {
        ClearHighlight();

        // Selected tiles should receive a mixed colour for indication
        bool selected = CompareTiles(tile, SelectedTile);
        Color color = selected ? mixedColor : highlightColor;

        tile.ToggleSelection(true, color);
        HighlightedTile = tile;
    }

    /// <summary>
    /// Clear currently highlighted tile
    /// </summary>
    public void ClearHighlight()
    {
        // Selected tiles should retain selection after removing highlight
        bool selected = CompareTiles(HighlightedTile, SelectedTile);
        if (selected)
        {
            HighlightedTile!.ToggleSelection(true, selectionColor);
        }
        else if (HighlightedTile != null)
        {
            HighlightedTile.ToggleSelection(false);
        }

        HighlightedTile = null;
    }

    /// <summary>
    /// Mark a tile as selected
    /// </summary>
    /// <param name="tile">Selected tile</param>
    /// <param name="applyHighlight">Whether to apply highlight over selection</param>
    public void SelectTile(HexCell tile, bool applyHighlight = false)
    {
        var previousTile = SelectedTile;

        // Selecting a cell twice should deselect it
        bool alreadySelected = CompareTiles(previousTile, tile);
        if (alreadySelected)
        {
            DeselectTile();

            if (applyHighlight)
                previousTile!.ToggleSelection(true, highlightColor);
        }
        else
        {
            DeselectTile();
            tile.ToggleSelection(true, !applyHighlight ? selectionColor : mixedColor);
            SelectedTile = tile;
        }
    }

    /// <summary>
    /// Clear currently selected tile
    /// </summary>
    public void DeselectTile()
    {
        if (SelectedTile == null) return;

        SelectedTile.ToggleSelection(false);
        SelectedTile = null;
    }
    #endregion
}
