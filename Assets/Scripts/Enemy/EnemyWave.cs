using Sirenix.OdinInspector;
using System;
using UnityEngine;

[Serializable]
public class EnemyWave
{
    #region Attributes
    [Required]
    [SerializeField]
    private GameObject _enemyPrefab;
    [Range(1, 100)]
    [SerializeField]
    private float _enemyCount = 10;
    [Range(0.1f, 2f)]
    [SerializeField]
    private float _spawnRate = 0.5f;
    #endregion

    #region Properties
    public GameObject enemyPrefab => _enemyPrefab;
    public float enemyCount => _enemyCount;
    public float spawnRate => _spawnRate;
    #endregion
}

