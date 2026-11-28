
using UnityEngine;

public class ArrowInputSequence : MonoBehaviour
{
    public int ArrowCount { get; private set; }
    public int CurrentArrowNumber { get; private set; }
    public bool HasMissed { get; private set; }
    public bool IsActive { get; private set; }

    private float lengthMultiplier = 1f;

    /// <summary>
    /// TECを参照して新しい入力列を開始する。
    /// </summary>
    public void BeginSequence()
    {
        if (PlayerStats.Instance == null)
        {
            Debug.LogError("PlayerStatsが見つかりません。");
            IsActive = false;
            return;
        }

        BeginSequence(PlayerStats.Instance.TEC);
    }

    /// <summary>
    /// 指定TECで入力列を開始する。
    /// テスト時はこちらも使用できる。
    /// </summary>
    public void BeginSequence(float tec)
    {
        ArrowCount =
            ArrowDamageCalculator.GetRandomArrowCount(tec);

        CurrentArrowNumber = 0;
        HasMissed = false;
        IsActive = true;

        lengthMultiplier =
            ArrowDamageCalculator.GetLengthMultiplier(ArrowCount);

        Debug.Log(
            $"入力列開始：TEC={tec:F0}、矢印数={ArrowCount}、" +
            $"入力列倍率=×{lengthMultiplier:F2}"
        );
    }

    /// <summary>
    /// 矢印1個の入力結果を処理する。
    /// 戻り値が、その矢印で与えるダメージ。
    /// </summary>
    public int ResolveInput(bool success, float atk, float enemyDef)
    {
        if (!IsActive)
        {
            Debug.LogWarning("入力列が開始されていません。");
            return 0;
        }

        if (CurrentArrowNumber >= ArrowCount)
        {
            Debug.LogWarning("この入力列はすでに終了しています。");
            return 0;
        }

        CurrentArrowNumber++;

        // MISSした矢印自体は0ダメージ。
        if (!success)
        {
            HasMissed = true;

            Debug.Log(
                $"{CurrentArrowNumber}個目：MISS、ダメージ0"
            );

            FinishIfComplete();
            return 0;
        }

        // MISSする前だけ入力列倍率を適用する。
        float appliedMultiplier =
            HasMissed ? 1f : lengthMultiplier;

        int damage = ArrowDamageCalculator.CalculateDamage(
            atk,
            enemyDef,
            appliedMultiplier
        );

        Debug.Log(
            $"{CurrentArrowNumber}個目：成功、" +
            $"倍率×{appliedMultiplier:F2}、ダメージ{damage}"
        );

        FinishIfComplete();
        return damage;
    }

    private void FinishIfComplete()
    {
        if (CurrentArrowNumber >= ArrowCount)
        {
            IsActive = false;
            Debug.Log("入力列終了。次の入力列を開始できます。");
        }
    }
}

