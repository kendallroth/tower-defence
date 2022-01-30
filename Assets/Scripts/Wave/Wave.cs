using Sirenix.OdinInspector;
using System;
using System.Linq;
using UnityEngine;


[Serializable]
public class Wave
{
    #region Attributes
    [ReadOnly]
    [DisplayAsString]
    [LabelText("Wave Number")]
    [SerializeField]
    private int _number = 0;
    [SerializeField]
    private WaveSection[] _waveSections = new WaveSection[1];
    #endregion


    #region Properties
    public int number => _number;
    public WaveSection[] waveSections => _waveSections;
    public int enemyCount => _waveSections.Aggregate(0, (accum, section) => accum + section.enemyCount);
    #endregion
}

