
using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// EnemyのヒントUIを管理する。
///
/// ・初期状態では非表示
/// ・BattleManagerから呼び出されたときだけ開始
/// ・ヒントテキストを表示
/// ・表示アニメーションを再生
/// ・指定時間待機
/// ・UIPanelShrinkを開始
/// </summary>
public class EnemyHintUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text hintText;
    [SerializeField] private Animator animator;

    [Header("演出開始設定")]
    [SerializeField] private float cameraStartDelay = 0.3f;
    [SerializeField] private float shrinkStartDelay = 0.2f;

    [Header("パネル縮小")]
    [SerializeField] private UIPanelShrink panelShrink;

    // AnimatorのShowパラメーター
    private static readonly int ShowHash =
        Animator.StringToHash("Show");

    // 演出管理用コルーチン
    private Coroutine sequenceCoroutine;

    // 二重起動防止
    private bool isPlaying;

    private void Awake()
    {
        // ゲーム開始時はヒントUIを非表示にする
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

        // 演出シーケンス開始
        sequenceCoroutine = StartCoroutine(StartSequence());
    }

    /// <summary>
    /// ヒント表示後の演出を順番に実行する。
    /// </summary>
    private IEnumerator StartSequence()
    {
        if (cameraStartDelay > 0f)
            yield return new WaitForSeconds(cameraStartDelay);

        if (shrinkStartDelay > 0f)
            yield return new WaitForSeconds(shrinkStartDelay);

        // パネル縮小を開始
        if (panelShrink != null)
        {
            panelShrink.StartShrink();
        }
        else
        {
            Debug.LogWarning(
                "UIPanelShrinkが設定されていません。"
            );
        }

        sequenceCoroutine = null;
    }

    /// <summary>
    /// Animation Eventから呼び出す。
    /// ヒントUIを非表示にする。
    /// </summary>
    public void HidePanel()
    {
        if (sequenceCoroutine != null)
        {
            StopCoroutine(sequenceCoroutine);
            sequenceCoroutine = null;
        }

        isPlaying = false;
        gameObject.SetActive(false);
    }
}
