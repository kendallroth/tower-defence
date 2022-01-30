#nullable enable

using com.ootii.Messages;
using Drawing;
using System;
using UnityEngine;


[RequireComponent(typeof(EnemyAnimator))]
public class Enemy : MonoBehaviour
{
    #region Attributes
    [Header("Charateristics")]
    [Range(1f, 10f)]
    [SerializeField]
    private float _speed = 2f;
    [Range(1, 100)]
    [SerializeField]
    private int _reward = 1;
    [Range(1, 50)]
    [SerializeField]
    private int _damage = 1;
    [Range(1, 100)]
    [SerializeField]
    private float _startingHealth = 10f;
    [Range(0, 1f)]
    [SerializeField]
    private float _heightOffset = 0f;
    #endregion

    #region Properties
    public bool alive { get; private set; } = true;
    public int damage => _damage;
    public float health { get; private set; }
    public int reward => _reward;
    public float speed => _speed;
    #endregion

    private PathWaypoint? _waypoint;
    private DebugManager? _debugManager;
    private EnemyAnimator? _animator;


    #region Unity Methods
    void Awake()
    {
        _animator = GetComponent<EnemyAnimator>();
        _debugManager = DebugManager.Instance;
    }

    void Update()
    {
        MoveTowardsWaypoint();

        if (_debugManager?.showEnemyDirection ?? false)
            Draw.ingame.ArrowheadArc(transform.position + transform.forward / 3, transform.forward, 0.25f, Color.blue);
    }
    #endregion


    #region Custom Methods
    public void Spawn(PathWaypoint target)
    {
        alive = true;
        health = _startingHealth;
        _waypoint = target;

        _animator!.AnimateIn();
    }

    private void MoveTowardsWaypoint()
    {
        // NOTE: No waypoint likely means the enemy has completed the path
        //         and will be destroyed shortly (by exit collision), so
        //         the enemy must keep moving forward for the collision.
        Vector3 target = _waypoint != null ? _waypoint.transform.position + Vector3.up * _heightOffset : transform.position + transform.forward;

        Vector3 position = transform.position;
        Vector3 direction = target - position;
        transform.Translate(
            direction.normalized * _speed * Time.deltaTime,
            Space.World
        );
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(position.DirectionTo(target)),
            Time.deltaTime * _speed * 2
        );

        // Transition between waypoints as necessary (they are linked together)
        if (Vector3.Distance(transform.position, target) < 0.01f)
            FindNextWaypoint();
    }

    private void FindNextWaypoint()
    {
        // NOTE: Reaching the last waypoint without colliding with the exit is
        //         possible depending on exit model collider. In this case,
        //         the enemy should automatically die within a second of no collision.
        if (_waypoint?.NextWaypoint == null)
        {
            this.Wait(1, () => ReachExit());
        }

        _waypoint = _waypoint?.NextWaypoint ?? null;
    }

    public void TakeDamage(float damage, Tower tower)
    {
        if (!alive) return;

        health -= damage;

        // TODO: Add damage effect/sound
        // TODO: Invoke global event???

        if (health <= 0)
            Kill(tower);
    }

    /// <summary>
    /// Respond to enemy reaching exit (not death)
    /// </summary>
    public void ReachExit()
    {
        if (!alive) return;
        alive = false;

        MessageDispatcher.SendMessageData(GameEvents.ENEMY__REACHED_EXIT, this, -1);

        Destroy(gameObject);
    }

    public void Kill(Tower killer)
    {
        if (!alive) return;

        alive = false;
        health = 0;

        // TODO: Add death effect/sound

        EnemyDestroyedData message = new EnemyDestroyedData(this, killer);
        MessageDispatcher.SendMessageData(GameEvents.ENEMY__DESTROYED, message, -1);

        Destroy(gameObject);
    }
    #endregion
}
