using UnityEngine;

public class PlayerAttackkari : MonoBehaviour
{
    public EnemyHPkari enemy;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (enemy != null)
            {
                enemy.TakeDamage(1);
            }
        }
    }
}