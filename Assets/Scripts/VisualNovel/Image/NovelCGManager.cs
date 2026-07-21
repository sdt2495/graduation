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
    [SerializeField] private Image cgImageA;
    [SerializeField] private Image cgImageB;

    [Header("──────────────────────────────")]
    [Header("切替時間")]
    [SerializeField] private float transitionTime = 0.5f;

    private Image currentImage; // 現在表示してるImage
    private Image nextImage;    // 次に表示するImage


    private void Awake()
    {
        // Aを現在の画像として使用
        currentImage = cgImageA;
        nextImage = cgImageB;
        // 初期状態
        cgImageA.gameObject.SetActive(false);
        cgImageB.gameObject.SetActive(false);
    }


    #region 表示と非表示

    /// <summary>
    /// CG表示
    /// </summary>
    public IEnumerator ShowCG(string cgName, TransitionType transition, float? customTransitionTime = null)
    {
        // 空欄なら何もしない
        if (string.IsNullOrEmpty(cgName))
            yield break;

        // CG読み込み
        Sprite sprite = Resources.Load<Sprite>("CG/" + cgName);
        if (sprite == null)
        {
            Debug.LogWarning("CGが見つかりません : " + cgName);
            yield break;
        }

        // 演出時間 (CSVに時間指定があればそれを使用,空欄ならInspectorのデフォルト値を使用)
        // ※customTransitionTime が null なら transitionTime。null でなければ customTransitionTime。
        float time = customTransitionTime ?? transitionTime;

        // 現在CGが表示されているか
        bool isShowingCG = currentImage.gameObject.activeSelf;

        // CGが表示されていない
        if (!isShowingCG)
        {
            SetImage(nextImage, sprite);

            switch (transition)
            {
                case TransitionType.Instant:
                    // 即座に表示
                    SetAlpha(nextImage, 1f);

                    break;

                case TransitionType.Fade:
                    // 透明からフェードイン
                    yield return FadeIn(nextImage, time);

                    break;

                case TransitionType.Clock:
                    // 時計ワイプ (最初から完全に表示)
                    SetAlpha(nextImage, 1f);
                    yield return ClockIn(nextImage, time);
                    break;
            }
            //// 現在画像と次画像を入れ替える
            SwapImages();
            yield break;
        }

        // CGがすでに表示されている
        SetImage(nextImage, sprite);

        // 演出の出し方
        switch (transition)
        {
            case TransitionType.Instant:
                // 新しいCGを即座に表示
                SetAlpha(nextImage, 1f);
                // 古いCGを非表示
                currentImage.gameObject.SetActive(false);
                break;

            case TransitionType.Fade:
                // 新しいCGを古いCGの上に重ねる
                yield return FadeInOver(nextImage, time);
                break;

            case TransitionType.Clock:
                // 新しいCGを完全表示
                SetAlpha(nextImage, 1f);
                // 時計ワイプ
                yield return ClockIn(nextImage, time);
                // 古いCGを非表示
                currentImage.gameObject.SetActive(false);

                break;
        }
        // 現在画像と次画像を入れ替える
        SwapImages();
    }


    /// <summary>
    /// CGを消す
    /// </summary>
    public IEnumerator HideCG(TransitionType transition, float? customTransitionTime = null)
    {
        // CGが表示されていなければ何もしない
        if (!currentImage.gameObject.activeSelf)
            yield break;

        // 演出時間 (CSVに時間指定があればそれを使用,空欄ならInspectorのデフォルト値を使用)
        // ※customTransitionTime が null なら transitionTime。null でなければ customTransitionTime。
        float time = customTransitionTime ?? transitionTime;

        switch (transition)
        {
            case TransitionType.Instant:
                HideInstant();
                break;

            case TransitionType.Fade:
                yield return FadeOut(time);
                break;

            case TransitionType.Clock:
                yield return ClockOut(time);
                break;
        }
    }
    #endregion


    #region 共通処理

    /// <summary>
    /// ImageにCGを設定
    /// </summary>
    void SetImage(Image image, Sprite sprite)
    {
        image.sprite = sprite;
        image.gameObject.SetActive(true);

        // Filled状態を解除
        image.type = Image.Type.Simple;
    }

    /// <summary>
    /// Imageの透明度を設定
    /// </summary>
    void SetAlpha(Image image, float alpha)
    {
        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }

    /// <summary>
    /// 現在画像と次画像を入れ替える
    /// </summary>
    void SwapImages()
    {
        Image temp = currentImage;

        currentImage = nextImage;
        nextImage = temp;
    }

    /// <summary>
    /// CGを完全に消す
    /// </summary>
    void ClearImage(Image image)
    {
        image.sprite = null;
        image.gameObject.SetActive(false);
        SetAlpha(image, 1f);
    }
    #endregion


    #region 演出処理 (表示)

    /// <summary>
    /// フェードイン
    /// </summary>
    IEnumerator FadeIn(Image image, float duration)
    {
        // 画像を透明にして設定
        SetAlpha(image, 0f);
        // 徐々に不透明
        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            float rate = Mathf.Clamp01(time / duration);
            SetAlpha(image, rate);

            yield return null;
        }
        // 完全に不透明
        SetAlpha(image, 1f);
    }


    /// <summary>
    /// 上からフェードイン
    /// 現在のCGを表示したまま、新しいCGを上から重ねて表示する
    /// </summary>
    /// <summary>
    /// 上からフェードイン
    /// 現在のCGを表示したまま、新しいCGを上から重ねて表示する
    /// </summary>
    IEnumerator FadeInOver(Image image, float duration)
    {
        // 新しいCGを表示
        image.gameObject.SetActive(true);

        // 新しいCGを現在のCGより前面にする
        image.transform.SetAsLastSibling();

        // 新しいCGを完全に透明にする
        SetAlpha(image, 0f);
        // 現在のCGは完全に表示したまま
        SetAlpha(currentImage, 1f);

        // 0秒以下なら即時表示
        if (duration <= 0f)
        {
            SetAlpha(image, 1f);
            currentImage.gameObject.SetActive(false);
            yield break;
        }

        // フェード開始
        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            float rate = Mathf.Clamp01(time / duration);
            // 古いCGは最後まで表示
            SetAlpha(currentImage, 1f);
            // 新しいCGだけ徐々に表示
            SetAlpha(image, rate);

            yield return null;
        }

        // 最終状態
        SetAlpha(image, 1f);
        // 古いCGを非表示
        currentImage.gameObject.SetActive(false);
    }


    /// <summary>
    /// 時計回りに表示
    /// </summary>
    IEnumerator ClockIn(Image image, float duration)
    {
        // CG自体は完全に表示
        SetAlpha(image, 1f);

        // 画像タイプを"塗りつぶし"に変更
        image.type = Image.Type.Filled;
        image.fillMethod = Image.FillMethod.Radial360;
        image.fillOrigin = 2;
        image.fillClockwise = true;
        image.fillAmount = 0f;

        // 徐々に塗りつぶし
        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            float rate = Mathf.Clamp01(time / duration);
            image.fillAmount = rate;

            yield return null;
        }
        // 完全に塗りつぶし
        image.fillAmount = 1f;
        // 画像タイプを"シンプル"に変更
        image.type = Image.Type.Simple;
    }
    #endregion


    #region 演出処理 (非表示)

    /// <summary>
    /// 一瞬で非表示
    /// </summary>
    void HideInstant()
    {
        ClearImage(currentImage);
    }

    /// <summary>
    /// フェードアウト
    /// </summary>
    IEnumerator FadeOut(float duration)
    {
        // 徐々に透明
        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            float rate = Mathf.Clamp01(time / duration);
            SetAlpha(currentImage, 1f - rate);

            yield return null;
        }
        // CG画像を非表示
        ClearImage(currentImage);
    }

    /// <summary>
    /// 時計回りに消す
    /// </summary>
    IEnumerator ClockOut(float duration)
    {
        // 画像タイプを"塗りつぶし"に変更
        currentImage.type = Image.Type.Filled;
        currentImage.fillMethod = Image.FillMethod.Radial360;
        currentImage.fillOrigin = 2;
        currentImage.fillClockwise = false;
        currentImage.fillAmount = 1f;

        // 徐々に逆に塗りつぶされる
        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            float rate = Mathf.Clamp01(time / duration);
            currentImage.fillAmount = 1f - rate;

            yield return null;
        }
        // 完全に逆に塗りつぶされる
        currentImage.fillAmount = 0f;

        ClearImage(currentImage);
        // 画像タイプを"シンプル"に変更
        currentImage.type = Image.Type.Simple;
    }
    #endregion
}