
using UnityEngine;

public static class ArrowDamageCalculator
{
    // 基礎ダメージ係数
    public const float BaseAttackCoefficient = 0.5f;

    /// <summary>
    /// 基礎ダメージ B = ATK * 0.5 * 100 / (100 + 2 * DEF)
    /// 小数点以下は内部で保持する
    /// </summary>
    public static float CalculateBaseDamage(float atk, float enemyDef)
    {
        atk = Mathf.Max(0f, atk);
        enemyDef = Mathf.Max(0f, enemyDef);

        float denominator = 100f + 2f * enemyDef;

        if (denominator <= 0f)
        {
            return 0f;
        }

        return atk * BaseAttackCoefficient * 100f / denominator;
    }

    /// <summary>
    /// 入力列の矢印数による倍率
    /// 1本：1.00
    /// 2本：1.00
    /// 3本：1.05
    /// 4本：1.10
    /// 5本：1.15
    /// </summary>
    public static float GetLengthMultiplier(int arrowCount)
    {
        arrowCount = Mathf.Clamp(arrowCount, 1, 5);

        return 1f + 0.05f * Mathf.Max(0, arrowCount - 2);
    }

    /// <summary>
    /// 成功した矢印のダメージ
    /// </summary>
    public static int CalculateDamage(
        float atk,
        float enemyDef,
        float multiplier)
    {
        float baseDamage = CalculateBaseDamage(atk, enemyDef);
        float finalDamage = baseDamage * multiplier;

        return Mathf.Max(1, Mathf.FloorToInt(finalDamage));
    }

    /// <summary>
    /// TECに応じた入力列の矢印数
    /// 各範囲内は均等確率の仮設定
    /// </summary>
    public static int GetRandomArrowCount(float tec)
    {
        tec = Mathf.Clamp(tec, 0f, 100f);

        if (tec == 0f)
        {
            return 1;
        }

        if (tec < 25f)
        {
            return Random.Range(1, 3); // 1～2
        }

        if (tec < 50f)
        {
            return Random.Range(2, 4); // 2～3
        }

        if (tec < 75f)
        {
            return Random.Range(3, 5); // 3～4
        }

        if (tec < 100f)
        {
            return Random.Range(4, 6); // 4～5
        }

        return 5;
    }
}

