using UnityEngine;

public class EnemyExit : MonoBehaviour
{
    #region Attributes
    #endregion

    #region Properties
    #endregion


    #region Unity Methods
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        Enemy enemy = other.gameObject.GetComponent<Enemy>();
        if (enemy == null) return;

        // TODO: Take tower damage

        enemy.Kill(null);
    }

    #endregion


    #region Custom Methods
    #endregion

    #region Input Actions
    #endregion
}
