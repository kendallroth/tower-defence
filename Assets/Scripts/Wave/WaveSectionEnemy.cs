using Sirenix.OdinInspector;
using System;
using UnityEngine;


[Serializable]
public class WaveSectionEnemy
{
    #region Attributes
    [Required]
    [SerializeField]
    private GameObject _enemyPrefab;
    [Range(1, 100)]
    [SerializeField]
    private int _count = 5;
    [Range(0.1f, 2f)]
    [SuffixLabel("seconds")]
    [SerializeField]
    private float _spawnDelay = 0.5f;
    #endregion


    #region Properties
    public GameObject enemyPrefab => _enemyPrefab;
    public int count => _count;
    public float spawnDelay => _spawnDelay;
    #endregion
}
