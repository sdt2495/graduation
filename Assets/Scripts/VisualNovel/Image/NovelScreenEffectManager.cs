using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class NovelScreenEffectManager : MonoBehaviour
{
    [Header("Screen Effect Image")]
    [SerializeField] private Image effectImage;

    [Header("──────────────────────────────")]
    [Header("切替時間")]
    [SerializeField] private float transitionTime = 0.5f;

    private void Awake()
    {
        // 非表示
        effectImage.gameObject.SetActive(false);
    }


    #region 表示・非表示

    /// <summary>
    /// 画面エフェクト表示
    /// </summary>
    public IEnumerator Show(Color color, TransitionType transition)
    {
        switch (transition)
        {
            // 一瞬で表示
            case TransitionType.Instant:
                yield return ShowInstant(color);
                break;


            // フェードイン
            case TransitionType.Fade:
                yield return FadeIn(color);
                break;


            // 時計ワイプ
            case TransitionType.Clock:
                yield return ClockIn(color);
                break;
        }
    }

    /// <summary>
    /// 画面エフェクト非表示
    /// </summary>
    public IEnumerator Hide(TransitionType transition)
    {
        switch (transition)
        {
            // 一瞬で非表示
            case TransitionType.Instant:
                yield return HideInstant();
                break;


            // フェードアウト
            case TransitionType.Fade:
                yield return FadeOut();
                break;


            // 時計ワイプ解除
            case TransitionType.Clock:
                yield return ClockOut();
                break;
        }
    }
    #endregion


    #region 共通処理

    /// <summary>
    /// 画面色を設定
    /// </summary>
    void SetScreenColor(Color color)
    {
        // 表示
        effectImage.gameObject.SetActive(true);
        // 色を設定
        effectImage.color = color;
    }
    #endregion


    #region 演出処理 (表示)

    /// <summary>
    /// 一瞬で表示
    /// </summary>
    IEnumerator ShowInstant(Color color)
    {
        // 色を設定
        SetScreenColor(color);
        // 完全に不透明
        color.a = 1f;
        effectImage.color = color;

        yield return null;
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
    }


    /// <summary>
    /// 時計ワイプ
    /// </summary>
    IEnumerator ClockIn(Color color)
    {
        // 色を設定
        SetScreenColor(color);

        // 画像タイプを「塗りつぶし」に変更
        effectImage.type = Image.Type.Filled;
        effectImage.fillMethod = Image.FillMethod.Radial360;
        effectImage.fillOrigin = 2;
        effectImage.fillClockwise = true;
        // 塗りつぶし0から開始
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
        // 画像タイプを「シンプル」に戻す
        effectImage.type = Image.Type.Simple;
    }
    #endregion


    #region 演出処理 (非表示)

    /// <summary>
    /// 一瞬で非表示
    /// </summary>
    IEnumerator HideInstant()
    {
        // 完全に透明
        Color color = effectImage.color;
        color.a = 0f;
        effectImage.color = color;
        // 非表示
        effectImage.gameObject.SetActive(false);

        yield return null;
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

        // 非表示
        effectImage.gameObject.SetActive(false);
    }


    /// <summary>
    /// 時計ワイプ解除
    /// </summary>
    IEnumerator ClockOut()
    {
        // 画像タイプを「塗りつぶし」に変更
        effectImage.type = Image.Type.Filled;
        effectImage.fillMethod = Image.FillMethod.Radial360;
        effectImage.fillOrigin = 2;
        effectImage.fillClockwise = false;

        // 徐々に逆塗りつぶし
        float time = 0f;
        while (time < transitionTime)
        {
            time += Time.deltaTime;
            effectImage.fillAmount = Mathf.Lerp(1f, 0f, time / transitionTime);

            yield return null;
        }
        // 完全に逆塗りつぶし
        effectImage.fillAmount = 0f;
        // 非表示
        effectImage.gameObject.SetActive(false);

        // 画像タイプを「シンプル」に戻す
        effectImage.type = Image.Type.Simple;
    }

    #endregion
}