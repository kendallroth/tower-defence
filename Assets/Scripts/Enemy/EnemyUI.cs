using UnityEngine;
using UnityEngine.UI;

public class EnemyUI : MonoBehaviour
{
    #region Attributes
    [SerializeField]
    private Slider _healthBar;
    #endregion

    #region Properties
    #endregion

    private Enemy _enemy;


    #region Unity Methods
    void Awake()
    {
        _enemy = GetComponent<Enemy>();
    }

    void Update()
    {
        
    }
    #endregion


    #region Custom Methods
    #endregion

    #region Input Actions
    #endregion
}
