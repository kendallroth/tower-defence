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

    private void OnTriggerEnter(Collider other)
    {
        Enemy enemy = other.gameObject.GetComponent<Enemy>();
        if (enemy == null) return;

        // TODO: Take tower damage (if still alive)

        enemy.Kill(null);
    }
    #endregion


    #region Custom Methods
    #endregion
}
