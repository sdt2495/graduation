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
    public IEnumerator ShowCG(string cgName, TransitionType transition)
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

        // 現在CGが表示されているか
        bool isShowingCG = currentImage.gameObject.activeSelf;



        // CGが表示されていない
        if (!isShowingCG)
        {
            SetImage(nextImage, sprite);

            switch (transition)
            {
                case TransitionType.Instant:
                    SetAlpha(nextImage, 1f);
                    break;

                case TransitionType.Fade:
                    yield return FadeIn(nextImage);
                    break;

                case TransitionType.Clock:
                    yield return ClockIn(nextImage);
                    break;
            }
            SwapImages();
            yield break;
        }


        // CGがすでに表示されている
        SetImage(nextImage, sprite);

        switch (transition)
        {
            case TransitionType.Instant:
                SetAlpha(nextImage, 1f);
                currentImage.gameObject.SetActive(false);
                break;

            case TransitionType.Fade:
                // Fadeを指定した場合、CG表示中なら自動的にクロスフェード
                yield return CrossFade();
                break;

            case TransitionType.Clock:
                // 時計ワイプの場合は現在CGを消してから表示
                yield return ClockIn(nextImage);
                currentImage.gameObject.SetActive(false);
                break;
        }
        SwapImages();
    }


    /// <summary>
    /// CGを消す
    /// </summary>
    public IEnumerator HideCG(TransitionType transition)
    {
        // CGが表示されていなければ何もしない
        if (!currentImage.gameObject.activeSelf)
            yield break;

        switch (transition)
        {
            case TransitionType.Instant:
                HideInstant();
                break;

            case TransitionType.Fade:
                yield return FadeOut();
                break;

            case TransitionType.Clock:
                yield return ClockOut();
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

        SetAlpha(image, 1f);

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
    IEnumerator FadeIn(Image image)
    {
        // 画像を透明にして設定
        SetAlpha(image, 0f);
        // 徐々に不透明
        float time = 0f;
        while (time < transitionTime)
        {
            time += Time.deltaTime;
            float rate = Mathf.Clamp01(time / transitionTime);
            SetAlpha(image, rate);

            yield return null;
        }
        // 完全に不透明
        SetAlpha(image, 1f);
    }

    /// <summary>
    /// クロスフェード
    /// 現在のCGを表示したまま、新しいCGを重ねて表示する
    /// </summary>
    IEnumerator CrossFade()
    {
        // 変更後画像を透明にする
        SetAlpha(nextImage, 0f);

        // 徐々に不透明
        float time = 0f;
        while (time < transitionTime)
        {
            time += Time.deltaTime;
            float rate = Mathf.Clamp01(time / transitionTime);
            // 現在のCG
            SetAlpha(currentImage, 1f - rate);
            // 次のCG
            SetAlpha(nextImage, rate);

            yield return null;
        }
        // 完了
        SetAlpha(currentImage, 0f);
        SetAlpha(nextImage, 1f);

        currentImage.gameObject.SetActive(false);
    }


    /// <summary>
    /// 時計回りに表示
    /// </summary>
    IEnumerator ClockIn(Image image)
    {
        // 画像タイプを"塗りつぶし"に変更
        image.type = Image.Type.Filled;
        image.fillMethod = Image.FillMethod.Radial360;
        image.fillOrigin = 2;
        image.fillClockwise = true;
        image.fillAmount = 0f;

        // 徐々に塗りつぶし
        float time = 0f;
        while (time < transitionTime)
        {
            time += Time.deltaTime;
            float rate = Mathf.Clamp01(time / transitionTime);
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
    IEnumerator FadeOut()
    {
        // 徐々に透明
        float time = 0f;
        while (time < transitionTime)
        {
            time += Time.deltaTime;
            float rate = Mathf.Clamp01(time / transitionTime);
            SetAlpha(currentImage, 1f - rate);

            yield return null;
        }
        // CG画像を非表示
        ClearImage(currentImage);
    }

    /// <summary>
    /// 時計回りに消す
    /// </summary>
    IEnumerator ClockOut()
    {
        // 画像タイプを"塗りつぶし"に変更
        currentImage.type = Image.Type.Filled;
        currentImage.fillMethod = Image.FillMethod.Radial360;
        currentImage.fillOrigin = 2;
        currentImage.fillClockwise = false;
        currentImage.fillAmount = 1f;

        // 徐々に逆に塗りつぶされる
        float time = 0f;
        while (time < transitionTime)
        {
            time += Time.deltaTime;
            float rate = Mathf.Clamp01(time / transitionTime);
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