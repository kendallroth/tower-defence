#nullable enable

using Drawing;
using System;
using System.Collections;
using UnityEngine;

public enum EnemyDeath
{
    SELF,
    TOWER,
}

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
    public float damage => _damage;
    public float health { get; private set; }
    public float reward => _reward;
    public float speed => _speed;

    public Action<Enemy, Tower?>? OnDeath;
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
            this.Wait(1, () => Kill(null));
        }

        _waypoint = _waypoint?.NextWaypoint ?? null;
    }

    public void TakeDamage(float damage, Tower tower)
    {
        health -= damage;

        // TODO: Add damage effect/sound
        // TODO: Invoke global event

        if (health <= 0)
            Kill(tower);
    }

    public void Kill(Tower? killer = null)
    {
        if (!alive) return;

        alive = false;
        health = 0;

        // TODO: Add death effect/sound (if killed)
        // TODO: Reward player (if killed)
        // TODO: Invoke global event

        OnDeath?.Invoke(this, killer);

        Destroy(gameObject);
    }
    #endregion
}
