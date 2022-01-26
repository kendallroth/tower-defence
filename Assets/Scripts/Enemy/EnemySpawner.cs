using System.Collections;
using UnityEngine;

/// <summary>
/// Enemy spawner is responsible for spawning individual enemies and displaying
///   wave warning UI, based on events driven by a WaveManager.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    #region Attributes
    [SerializeField]
    private Transform _spawnLocation;
    #endregion

    #region Properties
    #endregion

    private PathWaypoint _startWaypoint;
    private float _waveProgress = 0f;


    #region Unity Methods
    private void Start()
    {
        var tile = GetComponentInParent<HexTile>();
        _startWaypoint = tile.GetComponentInChildren<PathWaypoint>().NextWaypoint;
    }
    #endregion


    #region Custom Methods
    public Enemy SpawnEnemy(GameObject prefab, float progress = 0)
    {
        GameObject spawned = Instantiate(prefab, _spawnLocation.position, _spawnLocation.rotation, TemporaryObjectsManager.Instance.TemporaryChildren);
        var enemy = spawned.GetComponent<Enemy>();
        enemy.Spawn(_startWaypoint);
        _waveProgress = progress;
        return enemy;
    }

    public void StartWave(int waveNumber)
    {
        // TODO
        Debug.Log($"Wave {waveNumber} is starting");
    }

    public void WarnWave(int waveNumber, float warningTime)
    {
        // TODO
        Debug.Log($"Wave {waveNumber} begins in {warningTime}s");
    }
    #endregion
}
