
using TMPro;
using UnityEngine;

/// <summary>
/// EnemyのヒントUIを管理する。
///
/// ・初期状態では非表示
/// ・BattleManagerから呼び出されたときだけ開始
/// ・ヒントテキストを表示
/// ・表示アニメーションを再生
/// ・Animation Eventから非表示にする
/// </summary>
public class EnemyHintUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text hintText;
    [SerializeField] private Animator animator;

    // AnimatorのShowパラメーター
    private static readonly int ShowHash =
        Animator.StringToHash("Show");

    // 二重起動防止
    private bool isPlaying;

    private void Awake()
    {
        // ゲーム開始時はヒントUIを非表示
        gameObject.SetActive(false);
    }

    /// <summary>
    /// BattleManagerから呼び出してヒントを表示する。
    /// </summary>
    public void ShowHint(string message)
    {
        if (isPlaying)
            return;

        if (hintText == null || animator == null)
        {
            Debug.LogWarning(
                "EnemyHintUIのTextまたはAnimatorが設定されていません。"
            );

            return;
        }

        isPlaying = true;

        // UIを表示
        hintText.text = message;
        gameObject.SetActive(true);

        // Showアニメーションを再生
        animator.ResetTrigger(ShowHash);
        animator.SetTrigger(ShowHash);
    }

    /// <summary>
    /// Animation Eventから呼び出す。
    /// ヒントUIを非表示にする。
    /// </summary>
    public void HidePanel()
    {
        isPlaying = false;
        gameObject.SetActive(false);
    }
}
