using System;
using UnityEngine;


[Serializable]
public struct HexOffsetCoordinates
{
    #region Properties
    [SerializeField]
    private int col;
    [SerializeField]
    private int row;
    #endregion

    /// <summary>
    /// Hex offset column (x axis)
    /// </summary>
    public int Col => col;
    /// <summary>
    /// Hex offset row (z axis)
    /// </summary>
    public int Row => row;


    #region Unity Methods
    public HexOffsetCoordinates(int column, int row)
    {
        this.col = column;
        this.row = row;
    }
    #endregion


    #region Custom Methods
    /// <summary>
    /// Calculate world position from offset coordinates
    /// </summary>
    /// <param name="y">Target y level</param>
    /// <returns>World position</returns>
    public Vector3 GetPosition(float y = 0f)
    {
        return new Vector3(
            // Absolute values are necessary to properly handle centered grids during an axis
            //   positive/negative change!
            (col + Math.Abs(row) * 0.5f - Math.Abs(row) / 2) * (HexConstants.InnerRadius * 2f),
            y,
            -row * HexConstants.OuterRadius * 1.5f
        );
    }

    /// <summary>
    /// Convert an offset coordinate to an axial coordinate (odd-r offset system)
    /// </summary>
    /// <returns>Axial coordinate (odd-r offset system)</returns>
    public HexCoordinates ToHexCoordinates()
    {
        int hexQ = col - (row - (row & 1)) / 2;
        int hexR = row;
        return new HexCoordinates(hexQ, hexR);
    }

    /// <summary>
    /// Represent a hex offset coordinate as a readable string
    /// </summary>
    /// <returns>Readable hex offset coordinate</returns>
    public override string ToString()
    {
        return $"({col}, {row})";
    }
    #endregion
}
