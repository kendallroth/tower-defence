using System.Collections;
using UnityEngine;

public class EnemyAnimator : MonoBehaviour
{
    #region Attributes
    [Header("Hover")]
    [Range(0f, 0.2f)]
    [SerializeField]
    private float _hoverHeight = 0f;
    [Range(0f, 10f)]
    [SerializeField]
    private float _hoverSpeed = 1f;

    [Header("Spawn / Exit")]
    [Range(0, 1f)]
    [SerializeField]
    private float _animationInDelay = 0f;
    [Range(0, 2f)]
    [SerializeField]
    private float _animationOutDistance = 1f;
    [Range(0, 1f)]
    [SerializeField]
    private float _animationOutSize = 0.5f;
    [Range(0, 1f)]
    [SerializeField]
    private float _animationTime = 0.5f;
    [SerializeField]
    private LayerMask _exitLayer;
    #endregion


    #region Properties
    #endregion

    private Enemy _enemy;
    private Transform _enemyModel;
    private bool _animatingOut = false;
    private float _hoverTimeOffset = 0f;


    #region Unity Methods
    private void Awake()
    {
        _enemy = GetComponent<Enemy>();
        _enemyModel = transform.GetChild(0);
        _hoverTimeOffset = Random.value * (Mathf.PI / 2);
    }

    private void Update()
    {
        AnimateHover();

        // Exit animation begins a certain distance away from the exit
        if (!_animatingOut && Physics.Raycast(transform.position, transform.forward, _animationOutDistance, _exitLayer, QueryTriggerInteraction.Collide))
        {
            AnimateOut();
        }
    }
    #endregion


    #region Custom Methods
    public void AnimateIn()
    {
        StartCoroutine(AnimateSizeCoroutine(_animationInDelay, 0f, 1f));
    }

    public void AnimateOut()
    {
        if (_animatingOut) return;
        _animatingOut = true;

        StartCoroutine(AnimateSizeCoroutine(0, 1f, _animationOutSize));
    }

    private void AnimateHover()
    {
        float newY = Mathf.Sin((Time.time + _hoverTimeOffset) * _hoverSpeed) * _hoverHeight;
        _enemyModel.localPosition = new Vector3(0, newY, 0);
    }

    /// <summary>
    /// Animate size when spawning (or nearing tower)
    /// </summary>
    private IEnumerator AnimateSizeCoroutine(float delay, float startSize, float targetSize)
    {
        Vector3 startScale = new Vector3(startSize, startSize, startSize);
        Vector3 targetScale = new Vector3(targetSize, targetSize, targetSize);

        transform.localScale = startScale;

        yield return new WaitForSeconds(delay);

        float scaleTime = 0;
        while (scaleTime < _animationTime)
        {
            if (!_enemy.alive) yield break;

            scaleTime += Time.deltaTime;
            transform.localScale = Vector3.Lerp(startScale, targetScale, scaleTime / _animationTime);
            yield return null;
        }
    }
    #endregion
}
