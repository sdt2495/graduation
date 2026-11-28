
using UnityEngine;

public static class ArrowDamageCalculator
{
    // 基本攻撃係数（仮）
    public const float BaseAttackCoefficient = 0.5f;

    /// <summary>
    /// 端数を保持したデフォルト基礎ダメージBを計算する。
    /// TEC・入力列倍率・クリティカルなどは含めない。
    /// </summary>
    public static float CalculateBaseDamage(float atk, float enemyDef)
    {
        atk = Mathf.Max(0f, atk);
        enemyDef = Mathf.Max(0f, enemyDef);

        float denominator = 100f + 2f * enemyDef;

        if (denominator <= 0f)
            return 0f;

        return atk * BaseAttackCoefficient
            * 100f / denominator;
    }

    /// <summary>
    /// 入力列の矢印数から倍率を取得する。
    /// </summary>
    public static float GetLengthMultiplier(int arrowCount)
    {
        arrowCount = Mathf.Clamp(arrowCount, 1, 5);

        return 1f + 0.05f * Mathf.Max(0, arrowCount - 2);
    }

    /// <summary>
    /// 成功した矢印1個分のダメージを計算する。
    /// </summary>
    public static int CalculateDamage(
        float atk,
        float enemyDef,
        float lengthMultiplier)
    {
        float baseDamage = CalculateBaseDamage(atk, enemyDef);

        float damage = baseDamage * lengthMultiplier;

        // 仮仕様：成功時は最低1ダメージ
        return Mathf.Max(1, Mathf.FloorToInt(damage));
    }

    /// <summary>
    /// TECに応じて入力列の矢印数をランダムに決める。
    /// 出現確率は未確定のため、範囲内で均等抽選する仮実装。
    /// </summary>
    public static int GetRandomArrowCount(float tec)
    {
        tec = Mathf.Clamp(tec, 0f, 100f);

        if (tec <= 0f)
            return 1;

        if (tec < 25f)
            return Random.Range(1, 3); // 1～2個

        if (tec < 50f)
            return Random.Range(2, 4); // 2～3個

        if (tec < 75f)
            return Random.Range(3, 5); // 3～4個

        if (tec < 100f)
            return Random.Range(4, 6); // 4～5個

        return 5;
    }
}

