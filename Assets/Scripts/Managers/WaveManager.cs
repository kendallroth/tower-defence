#nullable enable

using com.ootii.Messages;
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
    [DisplayAsString]
    [PropertyOrder(-3)]
    [ShowInInspector]
    private string _enemiesAliveDisplay => $"{_enemiesAlive} enemies alive";

    [Header("Configuration")]
    [SerializeField]
    private bool _pauseBetweenWaves = true;
    [SuffixLabel("seconds")]
    [Range(0f, 5f)]
    [SerializeField]
    private float _waveDelay = 1f;
    [Range(0f, 10f)]
    [SuffixLabel("seconds")]
    [SerializeField]
    private float _waveWarningTime = 2f;

    [PropertySpace(SpaceBefore = 8)]
    [SerializeField]
    private Wave[] _waves = new Wave[1];
    #endregion

    #region Properties
    public EnemySpawner spawner => _spawner!;
    public Wave currentWave => _waveIdx < _waves.Length ? _waves[_waveIdx] : _waves[_waves.Length - 1];
    public bool spawning => _spawningWave;
    #endregion

    private EnemySpawner? _spawner;
    private Coroutine? _spawnCoroutine;
    private int _waveNumber = 0;
    private int _waveIdx => Mathf.Max(_waveNumber - 1, 0);
    private bool _spawningWave = false;
    private bool _finishedSpawningWave => _enemiesSpawned >= currentWave.enemyCount;
    private bool _gameOver => GameManager.Instance.gameOver;
    private float _waveCountdown = 0f;
    private int _enemiesSpawned = 0;
    private int _enemiesAlive = 0;


    #region Unity Methods
    private void Awake()
    {
        _spawner = FindObjectOfType<EnemySpawner>();
        _waveCountdown = _waveWarningTime;

        MessageDispatcher.AddListener(GameEvents.ENEMY__DESTROYED, OnEnemyGone);
        MessageDispatcher.AddListener(GameEvents.ENEMY__REACHED_EXIT, OnEnemyGone);
        MessageDispatcher.AddListener(GameEvents.GAME__OVER, OnGameOver);
    }

    void Start()
    {
        // Waves are spawned recursively upon completion of previous wave
        _spawnCoroutine = StartCoroutine(SpawnWaveCoroutine(_waveDelay));

        _spawner?.TogglePortal(true);
    }

    private void OnDestroy()
    {
        MessageDispatcher.RemoveListener(GameEvents.ENEMY__DESTROYED, OnEnemyGone);
        MessageDispatcher.RemoveListener(GameEvents.ENEMY__REACHED_EXIT, OnEnemyGone);
        MessageDispatcher.RemoveListener(GameEvents.GAME__OVER, OnGameOver);
    }
    #endregion


    #region Custom Methods
    private IEnumerator SpawnWaveCoroutine(float delay = 0)
    {
        if (_gameOver || _spawner == null) yield break;

        yield return new WaitForSeconds(delay);

        _waveNumber++;
        _enemiesSpawned = 0;

        // Wave warning / delay /////////////////////////////////////////////////

        _spawner.WarnWave(_waveNumber, _waveWarningTime);

        _waveCountdown = _waveWarningTime;
        while (_waveCountdown > 0)
        {
            _waveCountdown -= Time.deltaTime;
            yield return null;
        }

        _spawner.StartWave(_waveNumber);

        // Wave spawning ////////////////////////////////////////////////////////

        _spawningWave = true;

        for (int sectionIdx = 0; sectionIdx < currentWave.waveSections.Length; sectionIdx++)
        {
            WaveSection section = currentWave.waveSections[sectionIdx];

            yield return new WaitForSeconds(section.sectionDelay);

            // TODO: Support random section enemy order spawning...
            for (int enemyIdx = 0; enemyIdx < section.enemies.Length; enemyIdx++)
            {
                WaveSectionEnemy enemyGroup = section.enemies[enemyIdx];

                for (int counter = 0; counter < enemyGroup.count; counter++)
                {
                    _enemiesSpawned++;
                    _enemiesAlive++;

                    _spawner.SpawnEnemy(enemyGroup.enemyPrefab);

                    yield return new WaitForSeconds(enemyGroup.spawnDelay);
                }
            }
        }

        _spawningWave = false;

        // Wave cleanup /////////////////////////////////////////////////////////

        if (_waveNumber >= _waves.Length)
            yield break;

        // Next wave preparation ////////////////////////////////////////////////

        // TODO: Consider moving into separate function to be triggered when all enemies are dead in a wave???

        while (_enemiesAlive > 0)
            yield return null;

        if (!_pauseBetweenWaves)
        {
            StopCoroutine(_spawnCoroutine);
            _spawnCoroutine = StartCoroutine(SpawnWaveCoroutine(_waveDelay));
        }
    }
    #endregion


    #region Event Handlers
    private void OnGameOver(IMessage message)
    {
        StopCoroutine(_spawnCoroutine);
    }

    private void OnEnemyGone(IMessage message)
    {
        _enemiesAlive--;
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
