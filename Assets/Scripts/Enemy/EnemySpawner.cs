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
    private HexTile _tile;
    private float _waveProgress = 0f;


    #region Unity Methods
    private void Start()
    {
        _tile = GetComponentInParent<HexTile>();
        _startWaypoint = _tile.GetComponentInChildren<PathWaypoint>().NextWaypoint;
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
        StartCoroutine(AnimateProgressCoroutine(warningTime));

        // TODO
        Debug.Log($"Wave {waveNumber} begins in {warningTime}s");
    }

    private IEnumerator AnimateProgressCoroutine(float warningTime)
    {
        float warningTimer = warningTime;
        while (warningTimer > 0f)
        {
            warningTimer -= Time.deltaTime;
            _tile.ShowProgress((warningTime - warningTimer) / warningTime, Color.red);
            yield return null;
        }

        // TODO: Consider animating/fading colour out?

        _tile.ToggleSelection(false);
    }
    #endregion
}
