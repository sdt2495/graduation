
using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// EnemyのヒントUIを管理する。
///
/// ・ヒントテキストを表示
/// ・表示アニメーションを再生
/// ・指定時間待機
/// ・UIPanelShrinkを開始
/// ・カメラズームはUIPanelShrinkに任せる
/// </summary>
public class EnemyHintUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text hintText;
    [SerializeField] private Animator animator;

    [Header("演出開始設定")]
    [Tooltip("ヒント表示からパネル縮小開始までの待機時間")]
    [SerializeField] private float cameraStartDelay = 0.3f;

    [Tooltip("追加で待機してからパネル縮小を開始する時間")]
    [SerializeField] private float shrinkStartDelay = 0.2f;

    [Header("パネル縮小")]
    [SerializeField] private UIPanelShrink panelShrink;

    // AnimatorのShowパラメーター
    private static readonly int ShowHash =
        Animator.StringToHash("Show");

    // 演出管理用コルーチン
    private Coroutine sequenceCoroutine;

    /// <summary>
    /// ヒントを表示する。
    /// </summary>
    public void ShowHint(string message)
    {
        if (hintText == null || animator == null)
        {
            Debug.LogWarning(
                "EnemyHintUIのTextまたはAnimatorが設定されていません。"
            );

            return;
        }

        // 既存の演出を停止
        if (sequenceCoroutine != null)
        {
            StopCoroutine(sequenceCoroutine);
            sequenceCoroutine = null;
        }

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
        // ヒント表示後の待機
        if (cameraStartDelay > 0f)
        {
            yield return new WaitForSeconds(cameraStartDelay);
        }

        // 追加待機
        if (shrinkStartDelay > 0f)
        {
            yield return new WaitForSeconds(shrinkStartDelay);
        }

        // パネル縮小を開始
        // カメラズームもUIPanelShrink側で同時に開始される
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
        // 実行中の演出を停止
        if (sequenceCoroutine != null)
        {
            StopCoroutine(sequenceCoroutine);
            sequenceCoroutine = null;
        }

        gameObject.SetActive(false);
    }
}
