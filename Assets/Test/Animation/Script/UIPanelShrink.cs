
using UnityEngine;

/// <summary>
/// パネルの縮小演出を管理する。
///
/// ・初期状態では非表示
/// ・StartShrink()が呼び出されるまで非表示
/// ・呼び出されたらパネルを表示
/// ・縮小開始と同時にカメラをズーム
/// ・縮小終了後、指定時間だけ待機
/// ・スキャン用の線を上から下へ移動
/// ・線の移動とSliderの進行度を同期
/// ・線が下端に到達したらスキャン完了
/// </summary>
public class UIPanelShrink : MonoBehaviour
{
    [Header("対象")]
    [SerializeField] private RectTransform target;

    [Header("サイズ")]
    [SerializeField] private float startWidth = 1200f;
    [SerializeField] private float endWidth = 700f;
    [SerializeField] private float height = 700f;

    [Header("収縮時間")]
    [SerializeField] private float duration = 0.5f;

    [Header("縮小基準")]
    [Range(0f, 1f)]
    [SerializeField] private float pivotX = 0.5f;

    [Header("カメラ制御")]
    [SerializeField]
    private BattleCameraController battleCameraController;

    [Header("スキャン制御")]
    [SerializeField]
    private SliderPercentUI sliderPercentUI;

    [Header("四隅のImage")]
    [SerializeField] private RectTransform[] cornerImages;

    [Header("Imageの移動")]
    [SerializeField] private float startX = 1400f;
    [SerializeField] private float endX = 750f;
    [SerializeField] private float fixedY = 750f;

    [Header("スキャン用の線")]
    [SerializeField]
    private RectTransform scanLine;

    [Header("スキャン線の移動設定")]
    [SerializeField] private float scanStartY = 350f;
    [SerializeField] private float scanEndY = -350f;
    [SerializeField] private float scanDuration = 1.5f;

    [Header("スキャン開始までの待機時間")]
    [SerializeField] private float scanStartDelay = 0.5f;

    // 縮小経過時間
    private float timer;

    // スキャン線の経過時間
    private float scanTimer;

    // スキャン開始までの待機経過時間
    private float scanDelayTimer;

    // 縮小中かどうか
    private bool shrinking;

    // スキャン開始待ちかどうか
    private bool waitingForScan;

    // スキャン線が移動中かどうか
    private bool scanningLine;

    // 縮小が完了したかどうか
    private bool shrinkCompleted;

    private void Awake()
    {
        if (target == null)
        {
            target = GetComponent<RectTransform>();
        }

        // 初期状態ではスキャン線を非表示
        if (scanLine != null)
        {
            scanLine.gameObject.SetActive(false);
        }

        // パネルを非表示
        gameObject.SetActive(false);
    }

    private void Update()
    {
        // =====================================
        // パネル縮小処理
        // =====================================

        if (shrinking && target != null)
        {
            timer += Time.deltaTime;

            float t = duration <= 0f
                ? 1f
                : Mathf.Clamp01(timer / duration);

            float width = Mathf.Lerp(
                startWidth,
                endWidth,
                t
            );

            target.sizeDelta = new Vector2(
                width,
                height
            );

            // 縮小終了
            if (t >= 1f)
            {
                target.sizeDelta = new Vector2(
                    endWidth,
                    height
                );

                shrinking = false;
                shrinkCompleted = true;

                // スキャン開始まで待機
                scanDelayTimer = 0f;
                waitingForScan = true;

                Debug.Log("パネル縮小終了：スキャン開始待機");
            }
        }

        // =====================================
        // スキャン開始までの待機処理
        // =====================================

        if (waitingForScan)
        {
            scanDelayTimer += Time.deltaTime;

            // 指定時間が経過したらスキャン開始
            if (scanDelayTimer >= scanStartDelay)
            {
                waitingForScan = false;

                StartScanLine();
            }
        }

        // =====================================
        // スキャン線とSliderの連動処理
        // =====================================

        if (scanningLine && scanLine != null)
        {
            scanTimer += Time.deltaTime;

            // 進行度（0～1）
            float t = scanDuration <= 0f
                ? 1f
                : Mathf.Clamp01(scanTimer / scanDuration);

            // スキャン線のY座標
            float currentY = Mathf.Lerp(
                scanStartY,
                scanEndY,
                t
            );

            // 線を移動
            scanLine.anchoredPosition = new Vector2(
                scanLine.anchoredPosition.x,
                currentY
            );

            // Sliderの進行度を同期
            if (sliderPercentUI != null)
            {
                sliderPercentUI.UpdateScanProgress(t);
            }

            // =================================
            // スキャン終了
            // =================================

            if (t >= 1f)
            {
                scanLine.anchoredPosition = new Vector2(
                    scanLine.anchoredPosition.x,
                    scanEndY
                );

                scanningLine = false;

                // 線を非表示
                scanLine.gameObject.SetActive(false);

                // Sliderを100%にして完了処理
                if (sliderPercentUI != null)
                {
                    sliderPercentUI.CompleteScan();
                }

                Debug.Log("スキャン線到達：スキャン完了");
            }
        }
    }

    /// <summary>
    /// パネル縮小を開始する。
    /// ExclamationMarkから呼び出す。
    /// </summary>
    public void StartShrink()
    {
        // 二重起動防止
        if (shrinking || shrinkCompleted)
            return;

        // パネルを表示
        gameObject.SetActive(true);

        if (target == null)
        {
            Debug.LogWarning(
                "縮小対象のRectTransformが設定されていません。"
            );

            return;
        }

        // 初期化
        timer = 0f;
        scanTimer = 0f;
        scanDelayTimer = 0f;

        shrinking = true;
        waitingForScan = false;
        scanningLine = false;

        // 縮小基準を設定
        target.pivot = new Vector2(
            pivotX,
            target.pivot.y
        );

        // 初期サイズ
        target.sizeDelta = new Vector2(
            startWidth,
            height
        );

        // カメラズーム
        if (battleCameraController != null)
        {
            battleCameraController.StartZoom();
        }
        else
        {
            Debug.LogWarning(
                "BattleCameraControllerが設定されていません。"
            );
        }
    }

    /// <summary>
    /// スキャン線の移動を開始する。
    /// </summary>
    private void StartScanLine()
    {
        if (scanLine == null)
        {
            Debug.LogWarning(
                "スキャン用のImageが設定されていません。"
            );

            // 線がない場合は従来のスキャン開始
            if (sliderPercentUI != null)
            {
                sliderPercentUI.StartScan();
                sliderPercentUI.UpdateScanProgress(1f);
                sliderPercentUI.CompleteScan();
            }

            return;
        }

        // 経過時間を初期化
        scanTimer = 0f;

        // 線を上端に配置
        scanLine.anchoredPosition = new Vector2(
            scanLine.anchoredPosition.x,
            scanStartY
        );

        // 線を表示
        scanLine.gameObject.SetActive(true);

        // Sliderも同時に開始
        if (sliderPercentUI != null)
        {
            sliderPercentUI.StartScan();
        }
        else
        {
            Debug.LogWarning(
                "SliderPercentUIが設定されていません。"
            );
        }

        // 移動開始
        scanningLine = true;

        Debug.Log("スキャン線・Slider連動開始");
    }
}
