using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Enemy spawner is responsible for spawning individual enemies and displaying
///   wave warning UI, based on events driven by a WaveManager.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    #region Attributes
    [SerializeField]
    private Transform _spawnLocation;


    [Header("Progress Indicator")]
    [ColorUsage(true, true)]
    [SerializeField]
    private Color _progressColor;
    [SerializeField]
    private float _progressOffset;
    [SerializeField]
    private Image _progressImage;
    #endregion

    #region Properties
    #endregion

    private PathWaypoint _startWaypoint;
    private HexTile _tile;


    #region Unity Methods
    private void Awake()
    {
        _tile = GetComponentInParent<HexTile>();
        _startWaypoint = _tile.GetComponentInChildren<PathWaypoint>().NextWaypoint;
    }
    #endregion


    #region Custom Methods
    public Enemy SpawnEnemy(GameObject prefab)
    {
        GameObject spawned = Instantiate(prefab, _spawnLocation.position, _spawnLocation.rotation, TemporaryObjectsManager.Instance.TemporaryChildren);
        var enemy = spawned.GetComponent<Enemy>();
        enemy.Spawn(_startWaypoint);
        return enemy;
    }

    public void StartWave(int waveNumber)
    {
        // TODO
        Debug.Log($"Wave {waveNumber} is starting");
    }

    public void WarnWave(int waveNumber, float warningTime)
    {
        // TODO: Handle game ending mid warning (if needed...)!
        StartCoroutine(AnimateWarningCoroutine(warningTime));
    }

    /// <summary>
    /// Indicate warning for upcoming wave with a portal progress animation (around portal edge).
    /// </summary>
    private IEnumerator AnimateWarningCoroutine(float warningTime)
    {
        _progressImage.fillAmount = _progressOffset;
        _progressImage.color = _progressColor;

        float warningProgress = 0;
        while (warningProgress < 1)
        {
            warningProgress += Time.deltaTime / warningTime;
            _progressImage.fillAmount = warningProgress.MapFromPercent(_progressOffset, 1 - _progressOffset);
            yield return null;
        }

        // Fade portal warning progress indicator out briefly
        float fadeProgress = 1;
        Color progressColor = _progressImage.color;
        while (fadeProgress > 0)
        {
            fadeProgress -= Time.deltaTime / 0.5f;
            progressColor.a = fadeProgress;
            _progressImage.color = progressColor;
            yield return null;
        }

        _progressImage.fillAmount = _progressOffset;
        _progressImage.color = _progressColor;
    }
    #endregion
}
