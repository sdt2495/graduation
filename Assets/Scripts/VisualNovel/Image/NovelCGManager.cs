using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ノベルゲームCG管理
/// </summary>
public class NovelCGManager : MonoBehaviour
{
    // ★MEMO：イベントCGが表示されたら、立ち絵を自動で隠すようにすると、市販ノベルゲームに近い挙動になります！（まだ）


    [Header("CG Image")]
    [SerializeField] private Image cgImage;

    [Header("──────────────────────────────")]
    [Header("切替時間")]
    [SerializeField] private float transitionTime = 0.5f;


    private Coroutine transitionCoroutine;     // 実行中の演出

    private void Awake()
    {
        // 非表示
        cgImage.gameObject.SetActive(false);
    }


    #region 表示と非表示

    /// <summary>
    /// CG表示
    /// </summary>
    public void ShowCG(string cgName, TransitionType transition)
    {
        // 空欄なら何もしない
        if (string.IsNullOrEmpty(cgName))
            return;

        Sprite sprite = Resources.Load<Sprite>("CG/" + cgName);

        if (sprite == null)
        {
            Debug.LogWarning("CGが見つかりません : " + cgName);
            return;
        }

        // CG表示
        switch (transition)
        {
            case TransitionType.Instant:
                ShowInstant(sprite);
                break;

            case TransitionType.Fade:
                StartTransition(FadeIn(sprite));
                break;

            case TransitionType.Clock:
                StartTransition(ClockIn(sprite));
                break;
        }
    }


    /// <summary>
    /// CGを消す
    /// </summary>
    public void HideCG(TransitionType transition)
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
        // 新しい演出開始
        transitionCoroutine = StartCoroutine(routine);
    }

    /// <summary>
    /// 背景画像を設定
    /// </summary>
    void SetCG(Sprite sprite)
    {
        cgImage.sprite = sprite;
        cgImage.gameObject.SetActive(true);

        // 色を初期化
        cgImage.color = Color.white;
    }
    #endregion


    #region 演出処理 (表示)

    /// <summary>
    /// 一瞬で表示
    /// </summary>
    void ShowInstant(Sprite sprite)
    {
        // 画像を設定
        SetCG(sprite);
        // 一瞬で表示
        Color color = cgImage.color;
        color.a = 1f;
        cgImage.color = color;
    }


    /// <summary>
    /// フェードイン
    /// </summary>
    IEnumerator FadeIn(Sprite sprite)
    {
        // 画像を設定
        SetCG(sprite);

        // 透明状態から開始
        Color color = cgImage.color;
        color.a = 0f;
        cgImage.color = color;

        // 徐々に不透明にする
        float time = 0f;
        while (time < transitionTime)
        {
            time += Time.deltaTime;

            color.a = Mathf.Lerp(0f, 1f, time / transitionTime);
            cgImage.color = color;

            yield return null;
        }
        // 表示完了
        color.a = 1f;
        cgImage.color = color;

        transitionCoroutine = null;
    }


    /// <summary>
    /// 時計回り
    /// </summary>
    IEnumerator ClockIn(Sprite sprite)
    {
        // 画像を設定
        SetCG(sprite);

        // 画像タイプを"塗りつぶし"に変更
        cgImage.type = Image.Type.Filled;
        cgImage.fillMethod = Image.FillMethod.Radial360;
        cgImage.fillOrigin = 2;
        cgImage.fillClockwise = true;
        // 塗りつぶし０から開始
        cgImage.fillAmount = 0f;

        // 徐々に塗りつぶす
        float time = 0f;
        while (time < transitionTime)
        {
            time += Time.deltaTime;

            cgImage.fillAmount = Mathf.Lerp(0f, 1f, time / transitionTime);

            yield return null;
        }
        // 塗りつぶし完了
        cgImage.fillAmount = 1f;
        // 画像タイプを"シンプル"に変更
        cgImage.type = Image.Type.Simple;

        transitionCoroutine = null;
    }
    #endregion


    #region 演出処理 (表示)

    /// <summary>
    /// 一瞬で非表示
    /// </summary>
    void HideInstant()
    {
        cgImage.sprite = null;
        cgImage.gameObject.SetActive(false);
    }


    /// <summary>
    /// フェードアウト
    /// </summary>
    IEnumerator FadeOut()
    {
        Color color = cgImage.color;

        // 徐々に透明
        float time = 0f;
        while (time < transitionTime)
        {
            time += Time.deltaTime;

            color.a = Mathf.Lerp(1f, 0f, time / transitionTime);
            cgImage.color = color;

            yield return null;
        }

        // フェードアウト寛容
        color.a = 0f;
        cgImage.color = color;

        // 完全に消す
        cgImage.sprite = null;
        cgImage.gameObject.SetActive(false);

        transitionCoroutine = null;
    }


    /// <summary>
    /// 時計回り
    /// </summary>
    IEnumerator ClockOut()
    {
        // 画像タイプを"塗りつぶし"に変更
        cgImage.type = Image.Type.Filled;
        cgImage.fillMethod = Image.FillMethod.Radial360;
        cgImage.fillOrigin = 2;
        cgImage.fillClockwise = false;

        // 徐々に逆に塗りつぶす
        float time = 0f;
        while (time < transitionTime)
        {
            time += Time.deltaTime;

            cgImage.fillAmount = Mathf.Lerp(1f, 0f, time / transitionTime);

            yield return null;
        }

        // 逆塗りつぶし完了
        cgImage.fillAmount = 0f;
        // 完全に消す
        cgImage.sprite = null;
        cgImage.gameObject.SetActive(false);

        // 画像タイプを"シンプル"に変更
        cgImage.type = Image.Type.Simple;

        transitionCoroutine = null;
    }
    #endregion
}