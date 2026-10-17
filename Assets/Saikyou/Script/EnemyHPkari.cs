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

        // クリティカル判定（敵側で実行）
        bool isCritical = Random.Range(0f, 100f) < criticalChance;

        if (isCritical)
        {
            damage = Mathf.RoundToInt(damage * criticalMultiplier);
        }

        hp -= damage;

        Debug.Log("エネミーのHP：" + hp);

        // ダメージ表示
        if (damageTextPrefab != null && damagePoint != null && targetCanvas != null)
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
                damageText.ShowDamage(damage, isCritical);
            }
        }

        if (hp <= 0)
        {
            Destroy(gameObject);
        }
    }
}