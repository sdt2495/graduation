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

        if (Input.GetKeyDown(KeyCode.F))
        {
            if (enemy != null)
            {
                enemy.CounterDamage(1);
            }
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            if (enemy != null)
            {
                enemy.FinishBlowDamage(1);
            }
        }
    }
}