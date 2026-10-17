
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

    // 通常ダメージ
    public void TakeDamage(int damage)
    {
        ApplyDamage(damage, "Normal");
    }

    // カウンターダメージ
    public void CounterDamage(int damage)
    {
        ApplyDamage(damage, "Counter");
    }

    // フィニッシュブローダメージ
    public void FinishBlowDamage(int damage)
    {
        ApplyDamage(damage, "FinishBlow");
    }

    // ダメージ処理
    private void ApplyDamage(int damage, string attackType)
    {
        // クリティカル判定
        bool isCritical = Random.Range(0f, 100f) < criticalChance;

        if (isCritical)
        {
            damage = Mathf.RoundToInt(damage * criticalMultiplier);
        }

        // HPを減らす
        hp -= damage;

        Debug.Log("エネミーのHP：" + hp);

        // ダメージ表示に必要なオブジェクトがあるか確認
        if (damageTextPrefab != null &&
            damagePoint != null &&
            targetCanvas != null)
        {
            // ダメージ表示オブジェクトを生成
            GameObject obj = Instantiate(
                damageTextPrefab,
                targetCanvas.transform,
                false
            );

            // ダメージ表示位置を取得
            RectTransform rect = obj.GetComponent<RectTransform>();

            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(
                null,
                damagePoint.position
            );

            // スクリーン座標をCanvas内の座標に変換
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                targetCanvas.GetComponent<RectTransform>(),
                screenPos,
                null,
                out Vector2 localPos
            );

            rect.anchoredPosition = localPos;

            // ダメージ表示スクリプトを取得
            DamageTextkari damageText =
                obj.GetComponent<DamageTextkari>();

            if (damageText != null)
            {
                damageText.ShowDamage(
                    damage,
                    isCritical,
                    attackType
                );
            }
        }

        // HPが0以下なら敵を破壊
        if (hp <= 0)
        {
            Destroy(gameObject);
        }
    }
}
