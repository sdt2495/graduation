using System.Collections;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

/// <summary>
/// ノベルゲーム背景管理
/// </summary>
public class NovelBackgroundManager : MonoBehaviour
{
    [Header("背景Image")]
    [SerializeField] private Image backgroundImage;

    [Header("──────────────────────────────")]
    [Header("切替時間")]
    [SerializeField] private float transitionTime = 0.5f;

    private Coroutine transitionCoroutine; // 現在実行中の演出


    /// <summary>
    /// 背景変更
    /// </summary>
    public void ChangeBackground(string bgName, TransitionType transition)
    {
        // 空欄なら変更しない
        if (string.IsNullOrEmpty(bgName))
            return;

        // 背景なし
        if (bgName == "NONE")
        {
            ClearBackground();
            return;
        }

        // 読み込み
        Sprite sprite = Resources.Load<Sprite>("Background/" + bgName);

        if (sprite == null)
        {
            Debug.LogWarning($"背景が見つかりません : {bgName}");
            return;
        }

        // 変更演出
        switch (transition)
        {
            case TransitionType.Instant:
                ChangeInstant(sprite);
                break;

            case TransitionType.Fade:
                StartTransition(Fade(sprite));
                break;

            case TransitionType.Clock:
                StartTransition(Clock(sprite));
                break;
        }
    }

    #region 共通処理

    /// <summary>
    /// 背景画像を設定
    /// </summary>
    void SetBackground(Sprite sprite)
    {
        backgroundImage.enabled = true;
        backgroundImage.sprite = sprite;

        // 色を初期化
        backgroundImage.color = Color.white;
    }

    /// <summary>
    /// 演出開始
    /// </summary>
    void StartTransition(IEnumerator routine)
    {
        // 演出中なら停止
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }
        // 新しい演出開始
        transitionCoroutine = StartCoroutine(routine);
    }

    /// <summary>
    /// 背景を消す
    /// </summary>
    void ClearBackground()
    {
        backgroundImage.sprite = null;
        backgroundImage.enabled = false;
    }
    #endregion


    #region 演出処理


    /// <summary>
    /// 一瞬で表示
    /// </summary>
    void ChangeInstant(Sprite sprite)
    {
        // 画像を設定
        SetBackground(sprite);

        // 一瞬で表示
        Color color = backgroundImage.color;
        color.a = 1f;
        backgroundImage.color = color;
    }

    /// <summary>
    /// フェードイン
    /// </summary>
    IEnumerator Fade(Sprite sprite)
    {
        // 画像を設定
        SetBackground(sprite);

        // 透明状態から開始
        Color color = backgroundImage.color;
        color.a = 0f;
        backgroundImage.color = color;

        // 徐々に不透明にする
        float time = 0f;
        while (time < transitionTime)
        {
            time += Time.deltaTime;

            color.a = Mathf.Lerp(0f, 1f, time / transitionTime);
            backgroundImage.color = color;

            yield return null;
        }
        // 表示完了
        color.a = 1f;
        backgroundImage.color = color;

        transitionCoroutine = null;
    }

    /// <summary>
    /// 時計回り
    /// </summary>
    IEnumerator Clock(Sprite sprite)
    {
        // 画像を設定
        SetBackground(sprite);

        // 画像タイプを"塗りつぶし"に変更
        backgroundImage.type = Image.Type.Filled;
        backgroundImage.fillMethod = Image.FillMethod.Radial360;
        backgroundImage.fillOrigin = 2;
        backgroundImage.fillClockwise = true;
        // 塗りつぶし０から開始
        backgroundImage.fillAmount = 0f;

        // 徐々に塗りつぶす
        float time = 0f;
        while (time < transitionTime)
        {
            time += Time.deltaTime;

            backgroundImage.fillAmount = Mathf.Lerp(0f, 1f, time / transitionTime);

            yield return null;
        }
        // 塗りつぶし完了
        backgroundImage.fillAmount = 1f;
        // 画像タイプを"シンプル"に変更
        backgroundImage.type = Image.Type.Simple;

        transitionCoroutine = null;
    }
    #endregion
}