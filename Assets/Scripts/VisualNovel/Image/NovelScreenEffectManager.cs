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
    public IEnumerator Show(Color color, TransitionType transition, float? customTransitionTime = null)
    {
        // 演出時間 (CSVに時間指定があればそれを使用,空欄ならInspectorのデフォルト値を使用)
        // ※customTransitionTime が null なら transitionTime。null でなければ customTransitionTime。
        float duration = customTransitionTime ?? transitionTime;

        switch (transition)
        {
            // 一瞬で表示
            case TransitionType.Instant:
                yield return ShowInstant(color);
                break;

            // フェードイン
            case TransitionType.Fade:
                yield return FadeIn(color, duration);
                break;

            // 時計ワイプ
            case TransitionType.Clock:
                yield return ClockIn(color, duration);
                break;


            // 左→右
            case TransitionType.LeftToRight:
                yield return LeftToRightIn(color, duration);
                break;

            // 右→左
            case TransitionType.RightToLeft:
                yield return RightToLeftIn(color, duration);
                break;

            // 上→下
            case TransitionType.TopToBottom:
                yield return TopToBottomIn(color, duration);
                break;

            // 下→上
            case TransitionType.BottomToTop:
                yield return BottomToTopIn(color, duration);
                break;
        }
    }

    /// <summary>
    /// 画面エフェクト非表示
    /// </summary>
    public IEnumerator Hide(TransitionType transition, float? customTransitionTime = null)
    {
        // 演出時間 (CSVに時間指定があればそれを使用,空欄ならInspectorのデフォルト値を使用)
        // ※customTransitionTime が null なら transitionTime。null でなければ customTransitionTime。
        float duration = customTransitionTime ?? transitionTime;

        // 演出
        switch (transition)
        {
            // 一瞬で非表示
            case TransitionType.Instant:
                yield return HideInstant();
                break;

            // フェードアウト
            case TransitionType.Fade:
                yield return FadeOut(duration);
                break;

            // 時計ワイプ解除
            case TransitionType.Clock:
                yield return ClockOut(duration);
                break;


            // 左→右解除
            case TransitionType.LeftToRight:
                yield return LeftToRightOut(duration);
                break;

            // 右→左解除
            case TransitionType.RightToLeft:
                yield return RightToLeftOut(duration);
                break;

            // 上→下解除
            case TransitionType.TopToBottom:
                yield return TopToBottomOut(duration);
                break;

            // 下→上解除
            case TransitionType.BottomToTop:
                yield return BottomToTopOut(duration);
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
    IEnumerator FadeIn(Color color, float duration)
    {
        // 色を設定
        SetScreenColor(color);
        // 透明状態から開始
        color.a = 0f;
        effectImage.color = color;

        // 徐々に不透明
        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            color.a = Mathf.Lerp(0f, 1f, time / duration);
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
    IEnumerator ClockIn(Color color, float duration)
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
        while (time < duration)
        {
            time += Time.deltaTime;
            effectImage.fillAmount = Mathf.Lerp(0f, 1f, time / duration);

            yield return null;
        }
        // 完全に塗りつぶし
        effectImage.fillAmount = 1f;
        // 画像タイプを「シンプル」に戻す
        effectImage.type = Image.Type.Simple;
    }


    /// <summary>
    /// 左から右へ表示
    /// </summary>
    IEnumerator LeftToRightIn(Color color, float duration)
    {
        SetScreenColor(color);

        effectImage.type = Image.Type.Filled;
        effectImage.fillMethod = Image.FillMethod.Horizontal;
        effectImage.fillOrigin = 0;
        effectImage.fillAmount = 0f;

        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            effectImage.fillAmount = Mathf.Lerp(0f, 1f, time / duration);

            yield return null;
        }

        effectImage.fillAmount = 1f;
        effectImage.type = Image.Type.Simple;
    }


    /// <summary>
    /// 右から左へ表示
    /// </summary>
    IEnumerator RightToLeftIn(Color color, float duration)
    {
        SetScreenColor(color);

        effectImage.type = Image.Type.Filled;
        effectImage.fillMethod = Image.FillMethod.Horizontal;
        effectImage.fillOrigin = 1;
        effectImage.fillAmount = 0f;

        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            effectImage.fillAmount = Mathf.Lerp(0f, 1f, time / duration);

            yield return null;
        }

        effectImage.fillAmount = 1f;
        effectImage.type = Image.Type.Simple;
    }


    /// <summary>
    /// 上から下へ表示
    /// </summary>
    IEnumerator TopToBottomIn(Color color, float duration)
    {
        SetScreenColor(color);

        effectImage.type = Image.Type.Filled;
        effectImage.fillMethod = Image.FillMethod.Vertical;
        effectImage.fillOrigin = 1;
        effectImage.fillAmount = 0f;

        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            effectImage.fillAmount = Mathf.Lerp(0f, 1f, time / duration);

            yield return null;
        }

        effectImage.fillAmount = 1f;
        effectImage.type = Image.Type.Simple;
    }


    /// <summary>
    /// 下から上へ表示
    /// </summary>
    IEnumerator BottomToTopIn(Color color, float duration)
    {
        SetScreenColor(color);

        effectImage.type = Image.Type.Filled;
        effectImage.fillMethod = Image.FillMethod.Vertical;
        effectImage.fillOrigin = 0;
        effectImage.fillAmount = 0f;

        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            effectImage.fillAmount = Mathf.Lerp(0f, 1f, time / duration);

            yield return null;
        }

        effectImage.fillAmount = 1f;
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
    IEnumerator FadeOut(float duration)
    {
        Color color = effectImage.color;

        // 徐々に透明
        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            color.a = Mathf.Lerp(1f, 0f, time / duration);
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
    IEnumerator ClockOut(float duration)
    {
        // 画像タイプを「塗りつぶし」に変更
        effectImage.type = Image.Type.Filled;
        effectImage.fillMethod = Image.FillMethod.Radial360;
        effectImage.fillOrigin = 2;
        effectImage.fillClockwise = false;

        // 徐々に逆塗りつぶし
        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            effectImage.fillAmount = Mathf.Lerp(1f, 0f, time / duration);

            yield return null;
        }
        // 完全に逆塗りつぶし
        effectImage.fillAmount = 0f;
        // 非表示
        effectImage.gameObject.SetActive(false);

        // 画像タイプを「シンプル」に戻す
        effectImage.type = Image.Type.Simple;
    }

    /// <summary>
    /// 左から右へ非表示
    /// </summary>
    IEnumerator LeftToRightOut(float duration)
    {
        effectImage.type = Image.Type.Filled;
        effectImage.fillMethod = Image.FillMethod.Horizontal;
        effectImage.fillOrigin = 0;
        effectImage.fillAmount = 1f;

        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            effectImage.fillAmount = Mathf.Lerp(1f, 0f, time / duration);

            yield return null;
        }

        effectImage.fillAmount = 0f;
        effectImage.gameObject.SetActive(false);

        effectImage.type = Image.Type.Simple;
    }

    /// <summary>
    /// 右から左へ非表示
    /// </summary>
    IEnumerator RightToLeftOut(float duration)
    {
        effectImage.type = Image.Type.Filled;
        effectImage.fillMethod = Image.FillMethod.Horizontal;
        effectImage.fillOrigin = 1;
        effectImage.fillAmount = 1f;

        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            effectImage.fillAmount = Mathf.Lerp(1f, 0f, time / duration);

            yield return null;
        }

        effectImage.fillAmount = 0f;
        effectImage.gameObject.SetActive(false);

        effectImage.type = Image.Type.Simple;
    }

    /// <summary>
    /// 上から下へ非表示
    /// </summary>
    IEnumerator TopToBottomOut(float duration)
    {
        effectImage.type = Image.Type.Filled;
        effectImage.fillMethod = Image.FillMethod.Vertical;
        effectImage.fillOrigin = 1;
        effectImage.fillAmount = 1f;

        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            effectImage.fillAmount = Mathf.Lerp(1f, 0f, time / duration);

            yield return null;
        }

        effectImage.fillAmount = 0f;
        effectImage.gameObject.SetActive(false);

        effectImage.type = Image.Type.Simple;
    }

    /// <summary>
    /// 下から上へ非表示
    /// </summary>
    IEnumerator BottomToTopOut(float duration)
    {
        effectImage.type = Image.Type.Filled;
        effectImage.fillMethod = Image.FillMethod.Vertical;
        effectImage.fillOrigin = 0;
        effectImage.fillAmount = 1f;

        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            effectImage.fillAmount = Mathf.Lerp(1f, 0f, time / duration);

            yield return null;
        }

        effectImage.fillAmount = 0f;
        effectImage.gameObject.SetActive(false);

        effectImage.type = Image.Type.Simple;
    }
    #endregion
}