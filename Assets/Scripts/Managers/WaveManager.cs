using Sirenix.OdinInspector;
using Sirenix.Utilities.Editor;
using System.Collections;
using UnityEngine;

/// <summary>
/// Wave manager is responsible for managing waves and sending events
///   to an EnemySpawner for individual spawning.
/// </summary>
public class WaveManager : MonoBehaviour
{
    #region Attributes
    [DisplayAsString]
    [PropertyOrder(-5)]
    [ShowInInspector]
    private string _totalWaveProgress => $"{_waveNumber} / {_waves.Length}";
    [DisplayAsString]
    [PropertyOrder(-4)]
    [ShowInInspector]
    private string _waveEnemyProgress => $"{_enemiesSpawned} / {currentWave?.enemyCount ?? 0} spawned";
    [Range(0f, 10f)]
    [SerializeField]
    private float _timeBetweenWaves = 2f;
    [PropertySpace(SpaceBefore = 8)]
    [SerializeField]
    private EnemyWave[] _waves;
    #endregion

    #region Properties
    public EnemySpawner spawner => _spawner;
    public EnemyWave currentWave => _waveIdx < _waves.Length ? _waves[_waveIdx] : _waves[_waves.Length - 1];
    [ShowInInspector]
    public int waveNumber => _waveNumber;
    public bool spawning => _spawningWave;
    #endregion

    private EnemySpawner _spawner;
    private Coroutine _spawnCoroutine;
    private int _waveNumber = 0;
    private int _waveIdx => Mathf.Max(_waveNumber - 1, 0);
    private bool _spawningWave = false;
    private bool _finishedSpawning = false;
    private float _waveCountdown = 0f;
    private int _enemiesSpawned = 0;
    //private int _enemiesAlive = 0;


    #region Unity Methods
    void Start()
    {
        _spawner = FindObjectOfType<EnemySpawner>();
        _waveCountdown = _timeBetweenWaves;

        // Waves are spawned recursively upon completion of previous wave
        _spawnCoroutine = StartCoroutine(SpawnWaveCoroutine());
    }
    #endregion


    #region Custom Methods
    private IEnumerator SpawnWaveCoroutine()
    {
        _waveNumber++;
        _enemiesSpawned = 0;

        Debug.Log($"Beginning to spawn wave {_waveNumber}");

        // Wave warning / delay /////////////////////////////////////////////////

        _spawner.WarnWave(_waveNumber, _timeBetweenWaves);

        _waveCountdown = _timeBetweenWaves;
        while (_waveCountdown > 0)
        {
            _waveCountdown -= Time.deltaTime;
            yield return null;
        }

        _spawner.StartWave(_waveNumber);

        // Wave spawning ////////////////////////////////////////////////////////

        _spawningWave = true;

        while (_enemiesSpawned < currentWave.enemyCount)
        {
            // TODO: Spawner should only run while the game is not over

            _enemiesSpawned++;
            GUIHelper.RequestRepaint();

            _spawner.SpawnEnemy(currentWave.enemyPrefab, _enemiesSpawned / currentWave.enemyCount);
            GUIHelper.RequestRepaint();
            yield return new WaitForSeconds(currentWave.spawnRate);
        }

        _spawningWave = false;

        Debug.Log($"Finished spawning wave {_waveNumber}");

        // Wave cleanup /////////////////////////////////////////////////////////

        // TODO: Move into separate function to be triggered when all enemies are dead in a wave

        if (waveNumber >= _waves.Length)
        {
            Debug.Log("WaveManager has finished spawning");
            _finishedSpawning = true;
            yield break;
        }

        _spawnCoroutine = StartCoroutine(SpawnWaveCoroutine());
    }
    #endregion


    #region Debug Methods
    [OnInspectorGUI]
    private void RedrawInspector()
    {
        // NOTE: This could be expensive if the inspector has a lot going on!
        GUIHelper.RequestRepaint();
    }
    #endregion
}
