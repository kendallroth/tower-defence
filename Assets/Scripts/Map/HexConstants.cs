using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HexConstants
{
    #region Properties
    /// <summary>
    /// Distance from hex center to any corner (hex points)
    /// </summary>
    public const float OuterRadius = 1.15f;

    /// <summary>
    /// Distance from hex center to any edge center (hex edges)
    /// </summary>
    public const float InnerRadius = OuterRadius * 0.866025404f;

    public const float HeightMultiplier = 0.5f;
    #endregion
}
