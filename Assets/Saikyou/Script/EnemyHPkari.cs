using UnityEngine;

public class EnemyHPkari : MonoBehaviour
{
    public int hp = 3;

    [Header("クリティカル設定")]
    [Range(0, 100)]
    public float criticalChance = 20f;
    public float criticalMultiplier = 1.2f;

    [Header("ダメージ表示")]
    public GameObject damageTextPrefab;
    public Transform damagePoint;
    public Canvas targetCanvas;

    public void TakeDamage(int damage)
    {
        ApplyDamage(damage, "Normal");
    }

    public void CounterDamage(int damage)
    {
        ApplyDamage(damage, "Counter");
    }

    public void FinishBlowDamage(int damage)
    {
        ApplyDamage(damage, "FinishBlow");
    }

    private void ApplyDamage(int damage, string attackType)
    {
        bool isCritical = Random.Range(0f, 100f) < criticalChance;

        if (isCritical)
        {
            damage = Mathf.RoundToInt(damage * criticalMultiplier);
        }

        hp -= damage;
        Debug.Log("エネミーのHP：" + hp);

        if (damageTextPrefab != null &&
            damagePoint != null &&
            targetCanvas != null)
        {
            GameObject obj = Instantiate(
                damageTextPrefab,
                targetCanvas.transform,
                false
            );

            RectTransform rect = obj.GetComponent<RectTransform>();

            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(
                null,
                damagePoint.position
            );

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                targetCanvas.GetComponent<RectTransform>(),
                screenPos,
                null,
                out Vector2 localPos
            );

            rect.anchoredPosition = localPos;

            DamageTextkari damageText = obj.GetComponent<DamageTextkari>();

            if (damageText != null)
            {
                damageText.ShowDamage(damage, isCritical, attackType);
            }
        }

        if (hp <= 0)
        {
            Destroy(gameObject);
        }
    }
}