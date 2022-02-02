using com.ootii.Messages;
using Sirenix.OdinInspector;
using UnityEngine;

public class EnemyExit : MonoBehaviour
{
    #region Attributes
    [Range(0, 1)]
    [SuffixLabel("%")]
    [SerializeField]
    private float _fireStartsAt = 0f;

    [Header("Effects")]
    [SerializeField]
    private ParticleSystem _fireParticles;
    #endregion


    #region Unity Methods
    private void Awake()
    {
        MessageDispatcher.AddListener(GameEvents.PLAYER__LIVES_CHANGE, OnLivesChange);
    }

    private void OnTriggerEnter(Collider other)
    {
        Enemy enemy = other.gameObject.GetComponent<Enemy>();
        if (enemy == null) return;

        enemy.ReachExit();
    }

    private void OnDestroy()
    {
        MessageDispatcher.RemoveListener(GameEvents.PLAYER__LIVES_CHANGE, OnLivesChange);
    }
    #endregion


    #region Event Handlers
    private void OnLivesChange(IMessage message)
    {
        LivesChangeData livesChange = (LivesChangeData)message.Data;

        // TODO: Add slow animation towards/away from damage
        if (livesChange.percent <= _fireStartsAt && !_fireParticles.isPlaying)
        {
            _fireParticles.Play();
        }
        else if (livesChange.percent > 0.5f && _fireParticles.isPlaying)
        {
            _fireParticles.Stop();
        }
    }
    #endregion
}
