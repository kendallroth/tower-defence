#nullable enable

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TileSelector : GameSingleton<MonoBehaviour>
{
    #region Attributes
    [SerializeField]
    private LayerMask _tileLayer;

    [Header("Functionality")]
    [SerializeField]
    private bool _clickEnabled = true;
    [SerializeField]
    private bool _hoverEnabled = true;

    [Header("Colors")]
    [SerializeField]
    private Color _selectionColor;
    [SerializeField]
    private Color _highlightColor;
    #endregion


    #region Properties
    public HexTile? selectedTile { get; private set; } = null;
    public HexTile? highlightedTile { get; private set; } = null;
    #endregion

    private Color _mixedColor => Color.Lerp(_highlightColor, _selectionColor, 0.75f);
#nullable disable annotations
    private PlayerInput _playerInput;
#nullable enable annotations


    #region Unity Methods
    private void Awake()
    {
        _playerInput = new PlayerInput();
    }

    private void Update()
    {
        CalculateSelection();
    }

    private void OnEnable()
    {
        _playerInput.Camera.Enable();
    }

    private void OnDisable()
    {
        _playerInput.Camera.Disable();
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
        if (Physics.Raycast(ray, out hit, 100f, _tileLayer))
        {
            HexTile tile = hit.collider.GetComponent<HexTile>();
            if (tile == null) return;

            bool alreadyHighlighted = CompareTiles(highlightedTile, tile);
            if (!alreadyHighlighted && _hoverEnabled)
                HighlightTile(tile);

            if (_playerInput.Interaction.Select.triggered)
            {
                Debug.Log("Triggered");
                if (_clickEnabled)
                    SelectTile(tile, true);
                else
                    DeselectTile();
            }
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
    private bool CompareTiles(HexTile? target, HexTile? other, bool useCoordinates = false)
    {
        if (target == null || other == null) return false;

        return useCoordinates ? target.Coordinates == other.Coordinates : target == other;
    }

    /// <summary>
    /// Apply a highlight to a tile
    /// </summary>
    /// <param name="tile">Highlighted tile</param>
    public void HighlightTile(HexTile tile)
    {
        ClearHighlight();

        // Selected tiles should receive a mixed colour for indication
        bool selected = CompareTiles(tile, selectedTile);
        Color color = selected ? _mixedColor : _highlightColor;

        tile.ToggleSelection(true, color);
        highlightedTile = tile;
    }

    /// <summary>
    /// Clear currently highlighted tile
    /// </summary>
    public void ClearHighlight()
    {
        // Selected tiles should retain selection after removing highlight
        bool selected = CompareTiles(highlightedTile, selectedTile);
        if (selected)
        {
            highlightedTile!.ToggleSelection(true, _selectionColor);
        }
        else if (highlightedTile != null)
        {
            highlightedTile.ToggleSelection(false);
        }

        highlightedTile = null;
    }

    /// <summary>
    /// Mark a tile as selected
    /// </summary>
    /// <param name="tile">Selected tile</param>
    /// <param name="applyHighlight">Whether to apply highlight over selection</param>
    public void SelectTile(HexTile tile, bool applyHighlight = false)
    {
        var previousTile = selectedTile;

        // Selecting a cell twice should deselect it
        bool alreadySelected = CompareTiles(previousTile, tile);
        if (alreadySelected)
        {
            DeselectTile();

            if (applyHighlight)
                previousTile!.ToggleSelection(true, _highlightColor);
        }
        else
        {
            DeselectTile();
            tile.ToggleSelection(true, !applyHighlight ? _selectionColor : _mixedColor);
            selectedTile = tile;
        }
    }

    /// <summary>
    /// Clear currently selected tile
    /// </summary>
    public void DeselectTile()
    {
        if (selectedTile == null) return;

        selectedTile.ToggleSelection(false);
        selectedTile = null;
    }
    #endregion


    #region Input Actions
    private void HandleSelect(InputAction.CallbackContext ctx)
    {
        //ResetPosition();
    }
    #endregion
}
