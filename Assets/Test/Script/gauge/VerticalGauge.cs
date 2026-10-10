using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

/// <summary>
/// 縦ゲージを上下に動かし、ランダムな位置に表示された横線に
/// 合わせてSpaceキーを押すミニゲームを管理するクラス。
///
/// ・ゲージは上下に往復する
/// ・目標マークはランダムな高さに表示される
/// ・Spaceキーでゲージを停止する
/// ・ゲージの先端が目標マークと視覚的に一致したら成功
/// ・リセットすると目標マークも再抽選される
/// ・難易度ごとにゲージ速度を設定できる
/// </summary>
public class VerticalGauge : MonoBehaviour
{
    // ==================================================
    // 定数
    // ==================================================

    /// <summary>難易度リストの最初の番号</summary>
    private const int FIRST_DIFFICULTY_INDEX = 0;

    /// <summary>難易度を表示する際の番号補正</summary>
    private const int DIFFICULTY_DISPLAY_OFFSET = 1;

    /// <summary>速度の最小値</summary>
    private const float MIN_CHANGE_SPEED = 0f;

    /// <summary>ゲージの最小値</summary>
    private const float MIN_GAUGE_VALUE = 0f;

    /// <summary>正規化した値の最大値</summary>
    private const float MAX_NORMALIZED_VALUE = 1f;

    /// <summary>位置・アンカーの横方向の中央</summary>
    private const float CENTER_X = 0.5f;

    /// <summary>位置・アンカーの縦方向の下端</summary>
    private const float BOTTOM_Y = 0f;

    /// <summary>目標マークの横方向のアンカー開始位置</summary>
    private const float MARK_ANCHOR_MIN_X = 0f;

    /// <summary>目標マークの横方向のアンカー終了位置</summary>
    private const float MARK_ANCHOR_MAX_X = 1f;

    /// <summary>目標マークの横方向の位置</summary>
    private const float MARK_POSITION_X = 0f;

    /// <summary>目標マークの横幅調整値</summary>
    private const float MARK_WIDTH = 0f;

    /// <summary>距離ログの小数点以下の桁数</summary>
    private const string DISTANCE_FORMAT = "F2";

    // ==================================================
    // ゲージ画像
    // ==================================================

    [Header("ゲージ画像")]
    [Tooltip("上下に動くゲージ画像。Image TypeはFilledに設定")]
    [SerializeField] private Image gaugeImage;

    // ==================================================
    // ゲージの範囲
    // ==================================================

    [Header("ゲージの範囲")]
    [Tooltip("ゲージと目標マークを配置する共通の親領域")]
    [SerializeField] private RectTransform gaugeArea;

    // ==================================================
    // 目標マーク
    // ==================================================

    [Header("目標マーク")]
    [Tooltip("ランダムな高さに表示する横線のImage")]
    [SerializeField] private Image targetMark;

    // ==================================================
    // ゲージ設定
    // ==================================================

    [Header("ゲージ設定")]
    [Tooltip("ゲージの最大値")]
    [SerializeField] private float maxValue = 100f;

    [Tooltip("現在のゲージ値")]
    [SerializeField] private float currentValue = MIN_GAUGE_VALUE;

    // ==================================================
    // 難易度設定
    // ==================================================

    [Header("難易度設定")]
    [Tooltip("難易度ごとのゲージ速度。必要な数だけ追加できます")]
    [SerializeField] private List<float> difficultySpeeds = new List<float>();

    [Tooltip("開始時の難易度番号。0が最初です")]
    [SerializeField, Min(FIRST_DIFFICULTY_INDEX)]
    private int initialDifficultyIndex = FIRST_DIFFICULTY_INDEX;

    /// <summary>現在適用されているゲージ速度</summary>
    private float changeSpeed = MIN_CHANGE_SPEED;

    /// <summary>現在の難易度番号</summary>
    private int currentDifficultyIndex = FIRST_DIFFICULTY_INDEX;

    // ==================================================
    // 判定設定
    // ==================================================

    [Header("判定設定")]
    [Tooltip("描画上の一致とみなす最大距離（ピクセル）")]
    [SerializeField] private float matchTolerancePixels = 0.5f;

    // ==================================================
    // 内部状態
    // ==================================================

    // ゲージが上昇しているかどうか
    private bool isIncreasing = true;

    // ゲージが停止しているかどうか
    private bool isStopped = false;

    // 目標マークの位置をゲージ値で保持する
    private float targetValue = MIN_GAUGE_VALUE;

    // ==================================================
    // 外部公開情報
    // ==================================================

    // 判定結果を取得できるようにする
    public bool HasJudged { get; private set; }
    public bool IsSuccess { get; private set; }

    // 外部スクリプトから目標値を取得できる
    public float TargetValue => targetValue;

    // 外部スクリプトから停止状態を取得できる
    public bool IsStopped => isStopped;

    // 現在の難易度番号を取得できる
    public int CurrentDifficultyIndex => currentDifficultyIndex;

    // ==================================================
    // 初期化
    // ==================================================

    private void Start()
    {
        // Inspectorの設定を確認
        if (!ValidateSettings())
        {
            enabled = false;
            return;
        }

        // 開始時の難易度を適用
        if (!SetDifficulty(initialDifficultyIndex))
        {
            enabled = false;
            return;
        }

        // 最初の目標マークをランダムな位置に表示
        GenerateTargetMark();

        // ゲージ画像を初期化
        UpdateGauge();
    }

    private void Update()
    {
        // 停止中は何もしない
        if (isStopped)
        {
            return;
        }

        // Spaceキーが押されたら判定して停止
        if (Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            StopGauge();
            return;
        }

        // ゲージを上下に動かす
        MoveGauge();
    }

    // ==================================================
    // 設定チェック
    // ==================================================

    /// <summary>
    /// Inspectorの設定が正しいか確認する
    /// </summary>
    private bool ValidateSettings()
    {
        if (gaugeImage == null)
        {
            Debug.LogError(
                "VerticalGauge：Gauge Imageが設定されていません。",
                this);
            return false;
        }

        if (gaugeArea == null)
        {
            Debug.LogError(
                "VerticalGauge：Gauge Areaが設定されていません。",
                this);
            return false;
        }

        if (targetMark == null)
        {
            Debug.LogError(
                "VerticalGauge：Target Markが設定されていません。",
                this);
            return false;
        }

        if (maxValue <= MIN_GAUGE_VALUE)
        {
            Debug.LogError(
                "VerticalGauge：Max Valueは0より大きい値にしてください。",
                this);
            return false;
        }

        if (gaugeArea.rect.height <= MIN_GAUGE_VALUE)
        {
            Debug.LogError(
                "VerticalGauge：Gauge Areaの高さが0です。",
                this);
            return false;
        }

        // 難易度リストが設定されているか確認
        if (difficultySpeeds == null ||
            difficultySpeeds.Count <= FIRST_DIFFICULTY_INDEX)
        {
            Debug.LogError(
                "VerticalGauge：難易度の速度を1つ以上設定してください。",
                this);
            return false;
        }

        // すべての難易度の速度を確認
        for (int i = FIRST_DIFFICULTY_INDEX;
             i < difficultySpeeds.Count;
             i++)
        {
            if (difficultySpeeds[i] < MIN_CHANGE_SPEED)
            {
                Debug.LogError(
                    "VerticalGauge：難易度番号 " + i +
                    " の速度は0以上にしてください。",
                    this);
                return false;
            }
        }

        // 開始時の難易度番号を確認
        if (initialDifficultyIndex < FIRST_DIFFICULTY_INDEX ||
            initialDifficultyIndex >= difficultySpeeds.Count)
        {
            Debug.LogError(
                "VerticalGauge：開始時の難易度番号が範囲外です。",
                this);
            return false;
        }

        return true;
    }

    // ==================================================
    // 難易度変更
    // ==================================================

    /// <summary>
    /// 指定した難易度の速度を適用する
    /// difficultyIndexは0から始まる
    /// </summary>
    public bool SetDifficulty(int difficultyIndex)
    {
        // 難易度設定が空の場合
        if (difficultySpeeds == null ||
            difficultySpeeds.Count <= FIRST_DIFFICULTY_INDEX)
        {
            Debug.LogError(
                "VerticalGauge：難易度の速度が設定されていません。",
                this);
            return false;
        }

        // 指定された番号が範囲外の場合
        if (difficultyIndex < FIRST_DIFFICULTY_INDEX ||
            difficultyIndex >= difficultySpeeds.Count)
        {
            Debug.LogError(
                "VerticalGauge：指定された難易度番号が範囲外です。",
                this);
            return false;
        }

        // 速度が負の値の場合
        if (difficultySpeeds[difficultyIndex] < MIN_CHANGE_SPEED)
        {
            Debug.LogError(
                "VerticalGauge：ゲージ速度は0以上にしてください。",
                this);
            return false;
        }

        // 難易度と速度を更新
        currentDifficultyIndex = difficultyIndex;
        changeSpeed = difficultySpeeds[currentDifficultyIndex];

        Debug.Log(
            "難易度：" +
            (currentDifficultyIndex + DIFFICULTY_DISPLAY_OFFSET) +
            " / 速度：" + changeSpeed,
            this);

        return true;
    }

    /// <summary>
    /// 次の難易度へ進む
    /// </summary>
    public void IncreaseDifficulty()
    {
        // 次の難易度番号を計算
        int nextDifficultyIndex =
            currentDifficultyIndex + DIFFICULTY_DISPLAY_OFFSET;

        // 最後の難易度に到達している場合
        if (difficultySpeeds == null ||
            nextDifficultyIndex >= difficultySpeeds.Count)
        {
            Debug.Log("これ以上の難易度はありません。", this);
            return;
        }

        SetDifficulty(nextDifficultyIndex);
    }

    // ==================================================
    // ゲージ移動
    // ==================================================

    /// <summary>
    /// ゲージを上下に往復させる
    /// </summary>
    private void MoveGauge()
    {
        // 上昇処理
        if (isIncreasing)
        {
            currentValue += changeSpeed * Time.deltaTime;

            // 最大値に到達したら下降に切り替える
            if (currentValue >= maxValue)
            {
                currentValue = maxValue;
                isIncreasing = false;
            }
        }
        // 下降処理
        else
        {
            currentValue -= changeSpeed * Time.deltaTime;

            // 0に到達したら上昇に切り替える
            if (currentValue <= MIN_GAUGE_VALUE)
            {
                currentValue = MIN_GAUGE_VALUE;
                isIncreasing = true;
            }
        }

        // ゲージ画像を更新
        UpdateGauge();
    }

    // ==================================================
    // ゲージ表示更新
    // ==================================================

    /// <summary>
    /// ゲージ画像の表示量を更新する
    /// </summary>
    private void UpdateGauge()
    {
        if (gaugeImage == null || maxValue <= MIN_GAUGE_VALUE)
        {
            return;
        }

        // 現在値を0～1の範囲に変換する
        float normalizedValue = Mathf.Clamp01(currentValue / maxValue);

        // Filled画像の表示量を更新
        gaugeImage.fillAmount = normalizedValue;
    }

    // ==================================================
    // 目標マーク生成
    // ==================================================

    /// <summary>
    /// 目標マークをランダムな高さに配置する
    /// </summary>
    private void GenerateTargetMark()
    {
        if (gaugeArea == null || targetMark == null)
        {
            return;
        }

        // ゲージの0～最大値の範囲から目標値を抽選
        targetValue = Random.Range(MIN_GAUGE_VALUE, maxValue);

        // 目標値を高さの割合に変換
        float normalizedValue = targetValue / maxValue;

        // ゲージ領域の下端から目標位置までの距離
        float targetY = normalizedValue * gaugeArea.rect.height;

        // マークの位置をゲージ領域の下端基準で設定
        RectTransform markRect = targetMark.rectTransform;

        markRect.anchorMin = new Vector2(
            MARK_ANCHOR_MIN_X,
            BOTTOM_Y);

        markRect.anchorMax = new Vector2(
            MARK_ANCHOR_MAX_X,
            BOTTOM_Y);

        markRect.pivot = new Vector2(
            CENTER_X,
            CENTER_X);

        markRect.anchoredPosition = new Vector2(
            MARK_POSITION_X,
            targetY);

        // 横線をゲージ領域いっぱいに表示する
        markRect.sizeDelta = new Vector2(
            MARK_WIDTH,
            markRect.sizeDelta.y);

        targetMark.gameObject.SetActive(true);

        Debug.Log("目標位置：" + targetValue);
    }

    // ==================================================
    // 停止・判定
    // ==================================================

    /// <summary>
    /// ゲージを停止し、目標位置と比較して成功・失敗を判定する
    /// </summary>
    private void StopGauge()
    {
        isStopped = true;
        HasJudged = true;

        // ゲージ値を画面上の高さに変換する
        float gaugeY =
            (currentValue / maxValue) * gaugeArea.rect.height;

        // 目標値を画面上の高さに変換する
        float targetY =
            (targetValue / maxValue) * gaugeArea.rect.height;

        // 画面上の距離が許容範囲内なら成功
        float distance = Mathf.Abs(gaugeY - targetY);

        IsSuccess = distance <= matchTolerancePixels;

        if (IsSuccess)
        {
            Debug.Log("成功！ 目標位置に一致しました。");
        }
        else
        {
            Debug.Log(
                "失敗！ 目標位置との差：" +
                distance.ToString(DISTANCE_FORMAT) + " px");
        }

        Debug.Log(
            "現在値：" + currentValue +
            " / 目標値：" + targetValue);
    }

    // ==================================================
    // 再開・リセット
    // ==================================================

    /// <summary>
    /// 停止したゲージを再開する
    /// ※目標マークの位置は変更しない
    /// </summary>
    public void RestartGauge()
    {
        isStopped = false;
        HasJudged = false;
        IsSuccess = false;
    }

    /// <summary>
    /// ゲージを初期状態に戻して再開する
    /// ※目標マークも新しいランダム位置にする
    /// </summary>
    public void ResetGauge()
    {
        currentValue = MIN_GAUGE_VALUE;
        isIncreasing = true;
        isStopped = false;

        HasJudged = false;
        IsSuccess = false;

        GenerateTargetMark();
        UpdateGauge();
    }

    // ==================================================
    // 値の取得
    // ==================================================

    /// <summary>
    /// 現在のゲージ値を取得する
    /// </summary>
    public float GetGaugeValue()
    {
        return currentValue;
    }

    /// <summary>
    /// 目標位置を取得する
    /// </summary>
    public float GetTargetValue()
    {
        return targetValue;
    }
}