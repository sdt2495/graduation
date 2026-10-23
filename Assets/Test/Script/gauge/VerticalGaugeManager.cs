
using UnityEngine;
using System.Collections;
/// <summary>
/// 4本の縦ゲージを順番に管理するクラス。
///
/// ・操作中のゲージだけSpaceキーを受け付ける
/// ・操作中のゲージを拡大表示する
/// ・操作中以外のゲージを薄く表示する
/// ・判定後に次のゲージへ進む
/// ・4本すべて成功した場合のみクリア
/// </summary>
public class VerticalGaugeManager : MonoBehaviour
{
    // ==================================================
    // 定数
    // ==================================================

    private const int GAUGE_COUNT = 4;
    private const int FIRST_GAUGE_INDEX = 0;
    private const int NEXT_GAUGE_OFFSET = 1;

    private const float ACTIVE_SCALE = 1.15f;
    private const float INACTIVE_SCALE = 0.85f;
    private const float ACTIVE_ALPHA = 1.0f;
    private const float INACTIVE_ALPHA = 0.4f;

    // ==================================================
    // 回転演出設定
    // ==================================================

    private const float ROTATION_ANGLE = 90f;
    private const float ROTATION_DURATION = 0.35f;
    private const float ROTATION_END_ANGLE = 0f;

    [Header("回転演出")]
    [Tooltip("4本のゲージをまとめた親オブジェクト")]
    [SerializeField]
    private RectTransform gaugeRotationRoot;

    private bool isRotating;
    private Coroutine rotationCoroutine;
    // ==================================================
    // Inspector設定
    // ==================================================

    [Header("4本のゲージ")]
    [Tooltip("操作する順番にVerticalGaugeを4本設定")]
    [SerializeField]
    private VerticalGauge[] gauges =
        new VerticalGauge[GAUGE_COUNT];

    // ==================================================
    // 内部状態
    // ==================================================

    private int currentGaugeIndex = FIRST_GAUGE_INDEX;

    private bool isStarted;
    private bool isFinished;

    // 各ゲージの成功・失敗を保存する
    private bool[] gaugeResults = new bool[GAUGE_COUNT];

    // ==================================================
    // 初期化
    // ==================================================

    private void Start()
    {
        // Inspector設定の確認だけを行う。
        // 開始処理はStartMiniGame()から行う。
        if (!ValidateGauges())
        {
            enabled = false;
        }
    }

    // ==================================================
    // ミニゲーム開始
    // ==================================================

    /// <summary>
    /// 4本の縦ゲージミニゲームを開始する。
    /// </summary>
    public bool StartMiniGame()
    {
        if (!ValidateGauges())
        {
            return false;
        }

        currentGaugeIndex = FIRST_GAUGE_INDEX;
        isStarted = true;
        isFinished = false;

        // 前回の結果をすべてリセットする
        for (int i = FIRST_GAUGE_INDEX; i < GAUGE_COUNT; i++)
        {
            gaugeResults[i] = false;

            // 全ゲージの入力を無効化して停止する
            gauges[i].SetInputEnabled(false);
            gauges[i].EndMiniGame();
        }

        // 1本目だけ開始する
        gauges[currentGaugeIndex].StartMiniGame();

        UpdateGaugeAppearance();

        // 1本目だけSpaceキーを受け付ける
        gauges[currentGaugeIndex].SetInputEnabled(true);

        Debug.Log("4本の縦ゲージミニゲームを開始します。", this);

        return true;
    }

    // ==================================================
    // 更新
    // ==================================================
    private void Update()
    {
        if (!isStarted || isFinished || isRotating)
        {
            return;
        }


        // 現在のゲージの判定結果を取得
        if (!gauges[currentGaugeIndex].TryGetResult(out bool success))
        {
            return;
        }

        gaugeResults[currentGaugeIndex] = success;

        Debug.Log(
            "ゲージ " + (currentGaugeIndex + NEXT_GAUGE_OFFSET) +
            " 本目：" + (success ? "成功" : "失敗"),
            this);

        // 1本でも失敗したら即座に終了
        if (!success)
        {
            FinishMiniGame();
            return;
        }

        // 成功した場合だけ次のゲージへ進む
        AdvanceToNextGauge();
    }

    // ==================================================
    // 設定チェック
    // ==================================================

    private bool ValidateGauges()
    {
        if (gauges == null || gauges.Length != GAUGE_COUNT)
        {
            Debug.LogError(
                "VerticalGaugeManager：ゲージを4本設定してください。",
                this);

            return false;
        }

        for (int i = FIRST_GAUGE_INDEX; i < GAUGE_COUNT; i++)
        {
            if (gauges[i] == null)
            {
                Debug.LogError(
                    "VerticalGaugeManager：ゲージの " +
                    (i + NEXT_GAUGE_OFFSET) +
                    " 本目が設定されていません。",
                    this);

                return false;
            }
        }

        return true;
    }

    // ==================================================
    // 次のゲージへ進む
    // ==================================================

    // ==================================================
    // 次のゲージへ進む
    // ==================================================

    private void AdvanceToNextGauge()
    {
        // 現在のゲージの入力を無効にする
        gauges[currentGaugeIndex].SetInputEnabled(false);
        gauges[currentGaugeIndex].EndMiniGame();

        // 次のゲージ番号へ進む
        currentGaugeIndex += NEXT_GAUGE_OFFSET;

        // 4本すべて判定したら終了する
        if (currentGaugeIndex >= GAUGE_COUNT)
        {
            FinishMiniGame();
            return;
        }

        // 回転演出を開始する
        if (gaugeRotationRoot != null)
        {
            if (rotationCoroutine != null)
            {
                StopCoroutine(rotationCoroutine);
            }

            rotationCoroutine =
                StartCoroutine(RotateToNextGauge());
        }
        else
        {
            Debug.LogError(
                "VerticalGaugeManager：GaugeRotationRootが設定されていません。",
                this);

            // 親が設定されていない場合は従来どおり進める
            StartCurrentGauge();
        }
    }

    // ==================================================
    // 回転演出
    // ==================================================

    private IEnumerator RotateToNextGauge()
    {
        isRotating = true;

        // 回転開始時の角度を取得
        float startAngle = gaugeRotationRoot.localEulerAngles.z;

        // 次のゲージへ90度回転
        float targetAngle = startAngle - ROTATION_ANGLE;

        float elapsedTime = 0f;

        while (elapsedTime < ROTATION_DURATION)
        {
            elapsedTime += Time.deltaTime;

            float progress = Mathf.Clamp01(
                elapsedTime / ROTATION_DURATION);

            // 滑らかに回転させる
            float easedProgress =
                Mathf.SmoothStep(0f, 1f, progress);

            float angle = Mathf.Lerp(
                startAngle,
                targetAngle,
                easedProgress);

            gaugeRotationRoot.localRotation =
                Quaternion.Euler(0f, 0f, angle);

            yield return null;
        }

        // 最終角度を正確に設定
        gaugeRotationRoot.localRotation =
            Quaternion.Euler(0f, 0f, targetAngle);

        isRotating = false;
        rotationCoroutine = null;

        // 回転終了後に次のゲージを開始
        StartCurrentGauge();
    }

    // ==================================================
    // 現在のゲージを開始
    // ==================================================

    private void StartCurrentGauge()
    {
        gauges[currentGaugeIndex].StartMiniGame();

        UpdateGaugeAppearance();

        gauges[currentGaugeIndex].SetInputEnabled(true);
    }

    // ==================================================
    // 表示更新
    // ==================================================

    private void UpdateGaugeAppearance()
    {
        for (int i = FIRST_GAUGE_INDEX; i < GAUGE_COUNT; i++)
        {
            VerticalGauge gauge = gauges[i];

            RectTransform gaugeRect =
                gauge.GetComponent<RectTransform>();

            if (gaugeRect == null)
            {
                Debug.LogError(
                    "VerticalGaugeManager：ゲージのRectTransformがありません。",
                    gauge);

                continue;
            }

            // CanvasGroupがなければ追加する
            CanvasGroup canvasGroup =
                gauge.GetComponent<CanvasGroup>();

            if (canvasGroup == null)
            {
                canvasGroup =
                    gauge.gameObject.AddComponent<CanvasGroup>();
            }

            bool isActiveGauge = i == currentGaugeIndex;

            gaugeRect.localScale = Vector3.one *
                (isActiveGauge ? ACTIVE_SCALE : INACTIVE_SCALE);

            canvasGroup.alpha =
                isActiveGauge ? ACTIVE_ALPHA : INACTIVE_ALPHA;
        }
    }

    // ==================================================
    // 最終結果
    // ==================================================
    private void FinishMiniGame()
    {
        isFinished = true;
        isStarted = false;

        // すべてのゲージの入力を停止
        for (int i = FIRST_GAUGE_INDEX; i < GAUGE_COUNT; i++)
        {
            gauges[i].SetInputEnabled(false);
            gauges[i].EndMiniGame();
        }

        Debug.Log(
            "4本のゲージ終了。最終結果：" +
            (IsClear() ? "成功" : "失敗"),
            this);
    }
    /// <summary>
    /// 4本すべての判定が終わったか取得する。
    /// </summary>
    public bool IsFinished()
    {
        return isFinished;
    }

    /// <summary>
    /// 4本すべて成功した場合だけtrueを返す。
    /// </summary>
    public bool IsClear()
    {
        if (!isFinished)
        {
            return false;
        }

        for (int i = FIRST_GAUGE_INDEX; i < GAUGE_COUNT; i++)
        {
            if (!gaugeResults[i])
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 現在のゲージ番号を取得する。番号は0から始まる。
    /// </summary>
    public int GetCurrentGaugeIndex()
    {
        return currentGaugeIndex;
    }
}
