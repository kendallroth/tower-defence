using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Hex neighbour direction.
/// <br /><br />
/// Note that North/South directions are skipped due to hex grid layout!
/// </summary>
public enum HexDirection
{
    NE, E, SE, SW, W, NW
}


public static class HexDirectionExtensions
{
    /// <summary>
    /// Calculate the opposite side of a hex
    /// </summary>
    /// <param name="direction">Input hex edge/direction</param>
    /// <returns>Opposite hex edge/direction</returns>
    public static HexDirection Opposite(this HexDirection direction)
    {
        return (int)direction < 3 ? (direction + 3) : (direction - 3);
    }
}


[Serializable]
public struct HexCoordinates
{
    #region Properties
    [SerializeField]
    private int q;
    [SerializeField]
    private int r;
    [SerializeField]
    private int s;
    #endregion

    /// <summary>
    /// Hex coordinate Q axis value (NW/SE axis)
    /// </summary>
    public int Q => q;
    /// <summary>
    /// Hex coordinate R axis value (E/W axis)
    /// </summary>
    public int R => r;
    /// <summary>
    /// Hex coordinate S axis value (SW/NE axis)
    /// <br /><br />
    /// Can be calculated as 's = -q - r'
    /// </summary>
    public int S => s;

    /// <summary>
    /// Hex tile coordinate offsets by cardinal direction
    /// </summary>
    static public Dictionary<HexDirection, HexCoordinates> Directions = new Dictionary<HexDirection, HexCoordinates>
    {
        { HexDirection.NE, new HexCoordinates(1, -1, 0) },
        { HexDirection.E, new HexCoordinates(1, 0, -1) },
        { HexDirection.SE, new HexCoordinates(0, 1, -1) },
        { HexDirection.SW, new HexCoordinates(-1, 1, 0) },
        { HexDirection.W, new HexCoordinates(-1, 0, 1) },
        { HexDirection.NW, new HexCoordinates(0, -1, 1) },
    };

    static public Dictionary<HexDirection, int> DirectionAngles = new Dictionary<HexDirection, int>
    {
        { HexDirection.NE, 30 },
        { HexDirection.E, 90 },
        { HexDirection.SE, 150 },
        { HexDirection.SW, 210 },
        { HexDirection.W, 270 },
        { HexDirection.NW, 330 },
    };

    // TODO: Determine if tracking diagonals will be beneficial? Will need to modify shape to match above...
    //         https://www.redblobgames.com/grids/hexagons/codegen/output/lib.cs


    #region Unity Methods
    public HexCoordinates(int q, int r)
    {
        this.q = q;
        this.r = r;
        // NOTE: Coordinates must always equal 0 together!
        s = -q - r;
    }

    public HexCoordinates(int q, int r, int s)
    {
        this.q = q;
        this.r = r;
        this.s = s;
    }
    #endregion


    #region Mathematical Methods
    /// <summary>
    /// Add two hex coordinates to one another
    /// </summary>
    /// <param name="other">Other hex coordinate</param>
    /// <returns>Combined hex coordinates</returns>
    public HexCoordinates Add(HexCoordinates other)
    {
        return new HexCoordinates(q + other.q, r + other.r, s + other.s);
    }

    /// <summary>
    /// Add two hex coordinates to one another
    /// </summary>
    /// <param name="other">Other hex coordinate</param>
    /// <returns>Combined hex coordinates</returns>
    public HexCoordinates Subtract(HexCoordinates other)
    {
        return new HexCoordinates(q - other.q, r - other.r, s - other.s);
    }

    public HexCoordinates Scale(int scale)
    {
        return new HexCoordinates(q * scale, r * scale, s * scale);
    }

    public HexCoordinates RotateLeft()
    {
        return new HexCoordinates(-s, -q, -r);
    }

    public HexCoordinates RotateRight()
    {
        return new HexCoordinates(-r, -s, -q);
    }

    public int Length()
    {
        return (Math.Abs(q) + Math.Abs(r) + Math.Abs(s)) / 2;
    }

    public int Distance(HexCoordinates other)
    {
        return Subtract(other).Length();
    }
    #endregion


    #region Directions
    /// <summary>
    /// Calculate world space angle to to another hex (ie. to face towards)
    /// </summary>
    /// <param name="other">Neighbour coordinates</param>
    /// <returns>World space angle to neighbour</returns>
    public int Angle(HexCoordinates other)
    {
        HexDirection? direction = Direction(other);
        if (direction == null) return 0;

        return DirectionAngles[(HexDirection)direction];
    }

    /// <summary>
    /// Calculate world space angle to to another hex (ie. to face towards)
    /// </summary>
    /// <param name="direction">Neighbour direction</param>
    /// <returns>World space angle of direction</returns>
    public int Angle(HexDirection direction)
    {
        return DirectionAngles[direction];
    }

    /// <summary>
    /// Get the hex coordinate offset for a direction
    /// </summary>
    /// <param name="direction">Hex direction</param>
    /// <returns>Coordinate offset for direction</returns>
    public HexCoordinates Direction(HexDirection direction)
    {
        return Directions[direction];
    }

    /// <summary>
    /// Calculate direction to a neighbour
    /// </summary>
    /// <param name="other">Neighbour coordinates</param>
    /// <returns>Direction to neighbour</returns>
    public HexDirection? Direction(HexCoordinates other)
    {
        HexCoordinates offset = other.Subtract(this);
        // NOTE: Non-neighbours should return null (may not map to direction)!
        if (offset.Length() != 1)
        {
            Debug.LogWarning($"Tiles being compared for direction are not adjacent ({ToString()} - {other})!");
            return null;
        }

        foreach (HexDirection key in Directions.Keys)
        {
            if (Directions[key] == offset) return key;
        }

        return null;
    }

    /// <summary>
    /// Calculate hex coordinates of a directional neighbour
    /// </summary>
    /// <param name="direction">Neighbour direction</param>
    /// <returns>Directional neighbour coordinates</returns>
    public HexCoordinates Neighbor(HexDirection direction)
    {
        return Add(Direction(direction));
    }

    /// <summary>
    /// Determine a hex tile's grid position
    /// </summary>
    /// <param name="y">Position y-level</param>
    /// <returns>Hex tile grid position</returns>
    public Vector3 GetPosition(float y = 0f)
    {
        return ToOffset().GetPosition(y);
    }
    #endregion


    #region Conversion
    public static HexCoordinates FromOffset(int col, int row)
    {
        var offset = new HexOffsetCoordinates(col, row);
        return offset.ToHexCoordinates();
    }

    /// <summary>
    /// Convert an axial coordinate to offset (odd-r offset system)
    /// </summary>
    /// <returns>Offset coordinates (odd-r offset system)</returns>
    public HexOffsetCoordinates ToOffset()
    {
        int x = q + (r - (r & 1)) / 2;
        int z = r;
        return new HexOffsetCoordinates(x, z);
    }

    /// <summary>
    /// Represent a hex coordinate as a readable string
    /// </summary>
    /// <returns>Readable hex coordinate</returns>
    public override string ToString()
    {
        return $"({q}, {r}, {s})";
    }

    /// <summary>
    /// Represent a hex coordinate as a serializable key
    /// </summary>
    /// <returns>Serializable hex coordinate</returns>
    public string ToKey()
    {
        return $"{q},{r},{s}";
    }
    #endregion


    #region Equality
    /// <summary>
    /// Determine whether two coordinates are unequal
    /// </summary>
    /// <param name="first">First coordinate</param>
    /// <param name="second">Second coordinate</param>
    /// <returns>Whether two coordinates are unequal</returns>
    public static bool operator !=(HexCoordinates first, HexCoordinates second)
    {
        return first.Q != second.Q || first.R != second.R;
    }

    /// <summary>
    /// Determine whether two coordinates are equal
    /// </summary>
    /// <param name="first">First coordinate</param>
    /// <param name="second">Second coordinate</param>
    /// <returns>Whether two coordinates are equal</returns>
    public static bool operator ==(HexCoordinates first, HexCoordinates second)
    {
        return first.Q == second.Q && first.R == second.R;
    }

    /// <summary>
    /// Determine another coordinate is equal
    /// </summary>
    /// <param name="other">Other coordinate</param>
    /// <returns>Whether two coordinates are equal</returns>
    public override bool Equals(object other)
    {
        try
        {
            HexCoordinates otherCoordinate = (HexCoordinates)other;

            return otherCoordinate == this;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Get the hash code from the object
    /// </summary>
    /// <returns>Object hash code</returns>
    public override int GetHashCode()
    {
        var hashCode = 1861411795;
        hashCode = hashCode * -1521134295 + Q.GetHashCode();
        hashCode = hashCode * -1521134295 + R.GetHashCode();
        return hashCode;
    }
    #endregion
}
