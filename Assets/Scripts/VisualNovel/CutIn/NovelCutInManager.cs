using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// カットイン画像の表示・スライド・フェードを管理するクラス
/// 登場時と退場時の移動方向やフェードの有無、演出時間を指定できる
/// </summary>
public class NovelCutInManager : MonoBehaviour
{
    #region スライド方向

    /// <summary>
    /// NONE：移動なし
    /// LEFT：画面左へ
    /// RIGHT：画面右へ
    /// </summary>
    public enum SlideDirection
    {
        NONE,
        LEFT,
        RIGHT,
    }
    #endregion

    [Header("カットイン画像")]
    [SerializeField] private Image cutInImage;

    [Header("画像の読み込み先")]
    [SerializeField] private string resourceFolder = "CutIn";    // Resourcesフォルダ内にあるカットイン画像の保存先


    private RectTransform rectTransform;    // カットイン画像の位置やサイズを操作するためのRectTransform
    private Vector2 targetPosition;    // カットインが通常表示される位置(登場時の移動先と退場後の位置の復元に使用する)

    private Coroutine animationCoroutine;    // 現在実行中のスライド・フェード演出を管理するCoroutine

    #region 初期化

    /// <summary>
    /// オブジェクトの初期化処理 (Imageの取得、通常表示位置の保存、初期状態の設定)
    /// </summary>
    private void Awake()
    {
        if (cutInImage == null)
        {
            Debug.LogError("カットイン画像のImageが設定されていません。", this);
            enabled = false;
            return;
        }

        // RectTransformを取得
        rectTransform = cutInImage.rectTransform;
        // 初期位置
        targetPosition = rectTransform.anchoredPosition;
        // カットイン非表示
        cutInImage.gameObject.SetActive(false);
        // 透明度を初期化
        SetAlpha(1f);
    }
    #endregion


    #region カットイン表示

    /// <summary>
    /// カットイン表示
    /// </summary>
    public void ShowCutIn(string imageName, SlideDirection slideDirection, bool useFade, float duration)
    {
        // Resourcesフォルダから指定された画像を読み込む
        Sprite sprite = Resources.Load<Sprite>($"{resourceFolder}/{imageName}");

        // 画像が見つからない場合は警告を表示して処理を終了する
        if (sprite == null)
        {
            Debug.LogWarning($"カットイン画像が見つかりません: {resourceFolder}/{imageName}");
            return;
        }

        // 前の登場・退場演出が残っている場合は停止
        StopAnimation();

        // 読み込んだ画像をImageに設定する
        cutInImage.sprite = sprite;
        // CanvasのRectTransformを取得する
        RectTransform canvasRect = cutInImage.canvas.rootCanvas.GetComponent<RectTransform>();

        // 画面の縦幅を取得する
        float canvasHeight = canvasRect.rect.height;
        // 元画像の縦横比を取得する
        float aspectRatio = sprite.rect.width / sprite.rect.height;

        // 画像の高さを画面の縦幅に合わせ、横幅は縦横比から計算する
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, canvasHeight);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, canvasHeight * aspectRatio);

        // 最終表示位置を計算する
        targetPosition = GetEdgePosition(slideDirection);

        // 登場開始位置を計算
        Vector2 startPosition = GetOutsidePosition(targetPosition, slideDirection);

        // カットインを表示状態にする
        cutInImage.gameObject.SetActive(true);

        // 演出開始位置に画像を配置する
        rectTransform.anchoredPosition = startPosition;

        // フェードインを使用する場合は透明度0から開始 (使用しない場合は透明度1で開始する)
        SetAlpha(useFade ? 0f : 1f);

        // スライドとフェードを同時に実行 (フェードを使用しない場合、開始時と終了時の透明度は1)
        animationCoroutine = StartCoroutine(PlayAnimation(startPosition, targetPosition, useFade ? 0f : 1f, 1f, duration));
    }
    #endregion


    #region カットイン非表示

    /// <summary>
    /// カットイン非表示
    /// </summary>
    public void HideCutIn(SlideDirection slideDirection, bool useFade, float duration)
    {
        // すでに非表示の場合は何もしない
        if (!cutInImage.gameObject.activeSelf)
            return;

        // 実行中の演出があれば停止する
        StopAnimation();

        // 現在の画像の位置を退場開始位置として取得する
        Vector2 startPosition = rectTransform.anchoredPosition;

        // 退場方向に応じて、画面外へ移動する
        Vector2 endPosition = rectTransform.anchoredPosition;

        switch (slideDirection)
        {
            case SlideDirection.LEFT:
                endPosition.x -= rectTransform.rect.width;
                break;

            case SlideDirection.RIGHT:
                endPosition.x += rectTransform.rect.width;
                break;
        }

        // スライドと必要に応じたフェードアウトを実行する
        animationCoroutine = StartCoroutine(HideAnimation(startPosition, endPosition, useFade, duration));
    }
    #endregion


    #region 画像の端を画面の端に合わせた表示位置を取得
    /// <summary>
    /// 画像の端を画面の端に合わせた表示位置を取得
    /// </summary>
    private Vector2 GetEdgePosition(SlideDirection direction)
    {
        RectTransform canvasRect = cutInImage.canvas.rootCanvas.GetComponent<RectTransform>();

        Rect rect = rectTransform.rect;

        float left = canvasRect.rect.xMin;
        float right = canvasRect.rect.xMax;

        switch (direction)
        {
            case SlideDirection.LEFT:
                // 画像の左端を画面の左端に合わせる
                return new Vector2(left - rect.xMin, 0f);

            case SlideDirection.RIGHT:
                // 画像の右端を画面の右端に合わせる
                return new Vector2(right - rect.xMax, 0f);

            case SlideDirection.NONE:
            default:
                return rectTransform.anchoredPosition;
        }
    }
    #endregion


    #region 画像の幅・高さ分だけ方向に移動した位置を取得

    /// <summary>
    /// 登場方向に応じたオフセットを取得
    /// </summary>
    private Vector2 GetDirectionOffset(SlideDirection direction)
    {
        float width = rectTransform.rect.width;

        switch (direction)
        {
            case SlideDirection.LEFT:
                return new Vector2(width, 0f);

            case SlideDirection.RIGHT:
                return new Vector2(-width, 0f);

            default:
                return Vector2.zero;
        }
    }
    #endregion


    #region 登場時は画面外、退場時は指定方向の画面外となる位置を取得

    /// <summary>
    /// 登場時の開始位置を取得
    /// </summary>
    private Vector2 GetOutsidePosition(Vector2 basePosition, SlideDirection direction)
    {
        switch (direction)
        {
            case SlideDirection.LEFT:
                // 左側から登場するため、画像1枚分だけ左へずらす
                return basePosition + new Vector2(-rectTransform.rect.width, 0f);

            case SlideDirection.RIGHT:
                // 右側から登場するため、画像1枚分だけ右へずらす
                return basePosition + new Vector2(rectTransform.rect.width, 0f);

            default:
                return basePosition;
        }
    }
    #endregion



    #region  登場時のアニメーションを実行

    /// <summary>
    /// 登場時のアニメーションを実行
    /// </summary>
    private IEnumerator PlayAnimation(Vector2 startPosition, Vector2 endPosition, float startAlpha, float endAlpha, float duration)
    {
        // 指定された時間をかけて位置と透明度を変化
        yield return Animate(
            startPosition,
            endPosition,
            startAlpha,
            endAlpha,
            duration);

        // 補間処理の誤差を防ぐため、最終位置を正確に設定する
        rectTransform.anchoredPosition = endPosition;

        // 最終的な透明度を確定する
        SetAlpha(endAlpha);

        // Coroutineの実行が終了したことを記録する
        animationCoroutine = null;
    }
    #endregion


    #region 退場時のアニメーションを実行

    /// <summary>
    /// 退場時のアニメーションを実行
    /// </summary>
    private IEnumerator HideAnimation(Vector2 startPosition, Vector2 endPosition, bool useFade, float duration)
    {
        // 現在の透明度を取得する
        float startAlpha = cutInImage.color.a;

        // フェードアウトを使用する場合は透明度0を目標 (使用しない場合は現在の透明度を維持)
        float endAlpha = useFade ? 0f : startAlpha;

        // 指定された時間をかけて退場アニメーションを実行する
        yield return Animate(
            startPosition,
            endPosition,
            startAlpha,
            endAlpha,
            duration);

        // 最終位置と透明度を確定する
        rectTransform.anchoredPosition = endPosition;
        SetAlpha(endAlpha);

        // アニメーションが完了したらカットインを非表示にする
        cutInImage.gameObject.SetActive(false);

        // 次回の表示に備えて位置を通常表示位置に戻す
        rectTransform.anchoredPosition = targetPosition;

        // 次回の表示に備えて透明度を1に戻す
        SetAlpha(1f);

        // Coroutineの実行が終了したことを記録する
        animationCoroutine = null;
    }
    #endregion


    #region スライドとフェードを同時に実行する共通処理

    /// <summary>
    /// スライドとフェードを同時に実行する共通処理
    /// 開始位置から終了位置まで移動しながら、透明度を変化させる
    /// </summary>
    private IEnumerator Animate(Vector2 startPosition, Vector2 endPosition, float startAlpha, float endAlpha, float duration)
    {
        // 演出時間が0以下の場合はアニメーションを行わない
        if (duration <= 0f)
        {
            rectTransform.anchoredPosition = endPosition;
            SetAlpha(endAlpha);
            yield break;
        }

        // アニメーション開始からの経過時間
        float elapsed = 0f;

        // 指定された時間が経過するまで繰り返す
        while (elapsed < duration)
        {
            // 前フレームからの経過時間を加算する
            elapsed += Time.deltaTime;

            // 経過時間を0～1の割合に変換する
            float t = Mathf.Clamp01(elapsed / duration);

            // 開始位置から終了位置まで、割合tに応じて移動させる
            rectTransform.anchoredPosition = Vector2.Lerp(startPosition, endPosition, t);

            // 開始時の透明度から終了時の透明度まで変化させる
            SetAlpha(Mathf.Lerp(startAlpha, endAlpha, t));

            // 次のフレームまで待機する
            yield return null;
        }
    }
    #endregion


    #region カットイン画像の透明度を変更

    /// <summary>
    /// カットイン画像の透明度を変更
    /// </summary>
    private void SetAlpha(float alpha)
    {
        // 現在の色を取得する
        Color color = cutInImage.color;

        // 透明度のみを変更する
        color.a = alpha;

        // 変更した色をImageに適用する
        cutInImage.color = color;
    }
    #endregion


    #region 実行中のスライド・フェード演出を停止
    
    /// <summary>
    /// 実行中のスライド・フェード演出を停止
    /// </summary>
    private void StopAnimation()
    {
        // Coroutineが実行中の場合のみ停止する
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);

            // 停止後は参照を破棄し、実行中の演出がない状態にする
            animationCoroutine = null;
        }
    }
    #endregion
}