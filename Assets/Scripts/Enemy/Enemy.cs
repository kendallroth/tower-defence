using Drawing;
using UnityEngine;

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
    #endregion

    #region Properties
    public float Damage => _damage;
    public float health { get; private set; }
    public float Reward => _reward;
    public float Speed => _speed;
    #endregion

    private PathWaypoint _waypoint;
    private DebugManager _debugManager;


    #region Unity Methods
    void Awake()
    {
        _debugManager = DebugManager.Instance;
    }

    void Update()
    {
        MoveTowardsWaypoint();

        if (_debugManager.showEnemyDirection)
            Draw.ingame.ArrowheadArc(transform.position + transform.forward / 3, transform.forward, 0.25f, Color.blue);
    }

    #endregion


    #region Custom Methods
    public void Spawn(PathWaypoint target)
    {
        health = _startingHealth;
        _waypoint = target;
    }

    private void MoveTowardsWaypoint()
    {
        Vector3 target = _waypoint.transform.position;
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

        // Transition between waypoints as necessary
        if (Vector3.Distance(transform.position, target) < 0.01f)
            FindNextWaypoint();
    }

    private void FindNextWaypoint()
    {
        // NOTE: Should never reach last waypoint (exit should destroy them first)!
        if (_waypoint.NextWaypoint == null)
        {
            Destroy(gameObject);
            return;
        }

        _waypoint = _waypoint.NextWaypoint;
    }

    public void TakeDamage(float damage)
    {
        health -= damage;

        // TODO: Add damage effect/sound

        if (health <= Mathf.Epsilon)
        {
            Kill();
        }
    }

    private void Kill()
    {
        // TODO: Add death effect/sound
        // TODO: Reward player

        Destroy(gameObject);
    }

    #endregion
}
