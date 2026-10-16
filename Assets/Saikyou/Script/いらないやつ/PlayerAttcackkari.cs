using UnityEngine;

public class PlayerAttack : MonoBehaviour
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