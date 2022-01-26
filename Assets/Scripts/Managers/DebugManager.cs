using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

public enum GizmoDebug
{ 
    ALWAYS,
    SELECTED,
    NEVER
}

public class DebugManager : GameSingleton<DebugManager>
{
    #region Variables
    /// <summary>
    /// Whether overall debug mode is enabled
    /// </summary>
    [ReadOnly]
    public bool debugMode = true;

    [FoldoutGroup("Enemy", true)]
    [OdinSerialize]
    public bool showEnemyDirection = true;
    #endregion
}

