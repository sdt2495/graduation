using UnityEngine;

public class EnemyHPkari : MonoBehaviour
{
    public int hp = 3;

    [Header("ダメージ表示")]
    public GameObject damageTextPrefab;
    public Transform damagePoint;

    public void TakeDamage(int damage)
    {
        hp -= damage;

        Debug.Log("エネミーのHP：" + hp);

        // ダメージ表示
        if (damageTextPrefab != null && damagePoint != null)
        {
            GameObject obj = Instantiate(
                damageTextPrefab,
                damagePoint.position,
                Quaternion.identity
            );

            DamageTextkari damageText =
                obj.GetComponent<DamageTextkari>();

            if (damageText != null)
            {
                damageText.ShowDamage(damage);
            }
        }

        if (hp <= 0)
        {
            Destroy(gameObject);
        }
    }
}