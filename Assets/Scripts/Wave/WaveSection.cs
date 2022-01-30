using System;
using System.Linq;
using UnityEngine;


public enum WaveSectionOrder
{
    RANDOM,
    SEQUENTIAL
}


[Serializable]
public class WaveSection
{
    #region Attributes
    [Range(0, 5f)]
    [SerializeField]
    private float _sectionDelay = 0f;
    [SerializeField]
    WaveSectionOrder _order = WaveSectionOrder.SEQUENTIAL;
    [SerializeField]
    WaveSectionEnemy[] _enemies = new WaveSectionEnemy[1];
    #endregion


    #region Properties
    public float sectionDelay => _sectionDelay;
    public WaveSectionOrder order => _order;
    public WaveSectionEnemy[] enemies => _enemies;
    public int enemyCount => _enemies.Aggregate(0, (accum, enemy) => accum + enemy.count);
    #endregion
}
