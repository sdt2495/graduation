using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class NovelScreenEffectManager : MonoBehaviour
{
    [Header("Screen Effect Image")]
    [SerializeField] private Image effectImage;

    [Header("NovelManager")]
    [SerializeField] private NovelManager novelManager;

    [Header("──────────────────────────────")]
    [Header("切替時間")]
    [SerializeField] private float transitionTime = 0.5f;


    private Coroutine transitionCoroutine;

    private void Awake()
    {
        effectImage.gameObject.SetActive(false);
    }


    #region 表示・非表示
    /// <summary>
    /// 暗転表示 (演出方法と色)
    /// </summary>
    public void Show(Color color, TransitionType transition)
    {
        // 演出方法分岐
        switch (transition)
        {
            case TransitionType.Instant:
                ShowInstant(color);
                break;

            case TransitionType.Fade:
                StartTransition(FadeIn(color));
                break;

            case TransitionType.Clock:
                StartTransition(ClockIn(color));
                break;
        }
    }

    /// <summary>
    /// 暗転解除 (演出方法のみ)
    /// </summary>
    public void Hide(TransitionType transition)
    {
        switch (transition)
        {
            case TransitionType.Instant:
                HideInstant();
                break;

            case TransitionType.Fade:
                StartTransition(FadeOut());
                break;

            case TransitionType.Clock:
                StartTransition(ClockOut());
                break;
        }
    }
    #endregion


    #region 共通処理

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
        // 演出開始 (ウィンドウUI非表示)
        novelManager?.BeginTransition();
        // 新しい演出開始
        transitionCoroutine = StartCoroutine(routine);
    }

    /// <summary>
    /// 画面色を設定
    /// </summary>
    void SetScreenColor(Color color)
    {
        effectImage.gameObject.SetActive(true);
        effectImage.color = color;
    }
    #endregion


    #region 演出処理 (表示)

    /// <summary>
    /// 一瞬で表示
    /// </summary>
    void ShowInstant(Color color)
    {
        // 演出開始 (ウィンドウUI非表示)
        novelManager?.BeginTransition();
        // 色を設定
        SetScreenColor(color);
        // 一瞬で表示
        color.a = 1f;
        effectImage.color = color;

        // 演出終了 (ウィンドウUI表示)
        novelManager?.EndTransition();
    }


    /// <summary>
    /// フェードイン
    /// </summary>
    IEnumerator FadeIn(Color color)
    {
        // 色を設定
        SetScreenColor(color);
        // 透明状態から開始
        color.a = 0f;
        effectImage.color = color;
        // 徐々に不透明
        float time = 0f;
        while (time < transitionTime)
        {
            time += Time.deltaTime;

            color.a = Mathf.Lerp(0f, 1f, time / transitionTime);
            effectImage.color = color;

            yield return null;
        }
        // 完全に不透明
        color.a = 1f;
        effectImage.color = color;

        transitionCoroutine = null;
        // 演出終了 (ウィンドウUI表示)
        novelManager?.EndTransition();
    }


    /// <summary>
    /// 時計ワイプ
    /// </summary>
    IEnumerator ClockIn(Color color)
    {
        // 色を設定
        SetScreenColor(color);
        // 画像タイプを"塗りつぶし"に変更
        effectImage.type = Image.Type.Filled;
        effectImage.fillMethod = Image.FillMethod.Radial360;
        effectImage.fillOrigin = 2;
        effectImage.fillClockwise = true;
        // 塗りつぶし０から開始
        effectImage.fillAmount = 0f;

        // 徐々に塗りつぶす
        float time = 0f;
        while (time < transitionTime)
        {
            time += Time.deltaTime;

            effectImage.fillAmount = Mathf.Lerp(0f, 1f, time / transitionTime);

            yield return null;
        }

        // 完全に塗りつぶし
        effectImage.fillAmount = 1f;
        // 画像タイプを"シンプル"に変更
        effectImage.type = Image.Type.Simple;

        transitionCoroutine = null;
        // 演出終了 (ウィンドウUI表示)
        novelManager?.EndTransition();
    }
    #endregion


    #region 演出処理 (非表示)

    /// <summary>
    /// 一瞬で非表示
    /// </summary>
    void HideInstant()
    {
        // 演出開始 (ウィンドウUI非表示)
        novelManager?.BeginTransition();
        // 一瞬で非表示
        Color color = effectImage.color;
        color.a = 0f;
        effectImage.color = color;
        // 完全に透明
        effectImage.gameObject.SetActive(false);
        // 演出終了 (ウィンドウUI表示)
        novelManager?.EndTransition();
    }


    /// <summary>
    /// フェードアウト
    /// </summary>
    IEnumerator FadeOut()
    {
        Color color = effectImage.color;

        // 徐々に透明
        float time = 0f;
        while (time < transitionTime)
        {
            time += Time.deltaTime;

            color.a = Mathf.Lerp(1f, 0f, time / transitionTime);
            effectImage.color = color;

            yield return null;
        }

        // 完全に透明
        color.a = 0f;
        effectImage.color = color;
        effectImage.gameObject.SetActive(false);

        transitionCoroutine = null;
        // 演出終了 (ウィンドウUI表示)
        novelManager?.EndTransition();
    }


    /// <summary>
    /// 時計ワイプ解除
    /// </summary>
    IEnumerator ClockOut()
    {
        // 画像タイプを"塗りつぶし"に変更
        effectImage.type = Image.Type.Filled;
        effectImage.fillMethod = Image.FillMethod.Radial360;
        effectImage.fillOrigin = 2;
        effectImage.fillClockwise = false;

        // 徐々に逆塗りつぶす
        float time = 0f;
        while (time < transitionTime)
        {
            time += Time.deltaTime;

            effectImage.fillAmount = Mathf.Lerp(1f, 0f, time / transitionTime);

            yield return null;
        }
        // 完全に逆塗りつぶし
        effectImage.fillAmount = 0f;
        effectImage.gameObject.SetActive(false);
        // 画像タイプを"シンプル"に変更
        effectImage.type = Image.Type.Simple;

        transitionCoroutine = null;
        // 演出終了 (ウィンドウUI表示)
        novelManager?.EndTransition();
    }
    #endregion
}