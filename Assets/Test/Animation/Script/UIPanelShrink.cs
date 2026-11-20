
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

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
/// ・消える画像はスキャン線に合わせて上から表示
/// ・敵の攻撃終了時、カメラ復帰と同時にCanvas全体を消す
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
    [Tooltip("収縮完了からの待機")]
    [SerializeField] private float scanStartDelay = 0.5f;

    [Header("方向画像制御")]
    [SerializeField]
    private DirectionImageController directionImageController;

    [Header("消える画像")]
    [SerializeField] private Image scanImage;

    [SerializeField] private float hideDuration = 0.5f;

    [Tooltip("スキャン完了から何秒後に自動で消すか。-1で自動では消さない")]
    [SerializeField] private float hideDelayAfterScan = -1f;

    [Header("スキャン線の減速")]
    [Range(0f, 0.99f)]
    [SerializeField] private float slowStart = 0.75f;

    [Range(1f, 4f)]
    [SerializeField] private float slowStrength = 1.5f;


    // =====================================
    // Canvas全体の消滅演出
    // =====================================

    [Header("Canvas全体の消滅演出")]

    [Tooltip("CanvasのルートにあるRectTransform")]
    [SerializeField] private RectTransform canvasTransform;

    [Header("① 横幅を縮めて縦幅を伸ばす")]
    [SerializeField] private float firstEffectDuration = 0.2f;
    [SerializeField] private float firstWidthScale = 0.65f;
    [SerializeField] private float firstHeightScale = 1.25f;

    [Header("② 横に伸ばして縦幅を縮める")]
    [SerializeField] private float secondEffectDuration = 0.25f;
    [SerializeField] private float secondWidthScale = 1.3f;
    [SerializeField] private float secondHeightScale = 0.12f;

    [Header("③ 一本の横線にする")]
    [SerializeField] private float thirdEffectDuration = 0.15f;
    [SerializeField] private float finalWidthScale = 1.2f;
    [SerializeField] private float finalHeightScale = 0.005f;

    // Canvasの元のスケール
    private Vector3 canvasOriginalScale;

    // Canvas消滅中か
    private bool canvasHiding;

    // Canvas消滅コルーチン
    private Coroutine canvasHideCoroutine;


    // =====================================
    // 既存の状態管理
    // =====================================

    private float timer;
    private float scanTimer;
    private float scanDelayTimer;
    private float hideTimer;

    private bool shrinking;
    private bool waitingForScan;
    private bool scanningLine;
    private bool shrinkCompleted;
    private bool scanFinished;

    private float scanProgress;

    private bool waitingForHide;
    private bool scanImageHidden;


    private void Awake()
    {
        if (target == null)
        {
            target = GetComponent<RectTransform>();
        }

        // Canvasの初期スケールを保存
        if (canvasTransform != null)
        {
            canvasOriginalScale = canvasTransform.localScale;
        }

        if (scanLine != null)
        {
            scanLine.gameObject.SetActive(false);
        }

        if (scanImage != null)
        {
            scanImage.type = Image.Type.Filled;
            scanImage.fillMethod = Image.FillMethod.Vertical;
            scanImage.fillOrigin = (int)Image.OriginVertical.Top;
            scanImage.fillAmount = 0f;
        }

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

            float width = Mathf.Lerp(startWidth, endWidth, t);

            target.sizeDelta = new Vector2(width, height);

            if (t >= 1f)
            {
                target.sizeDelta = new Vector2(endWidth, height);

                shrinking = false;
                shrinkCompleted = true;

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

            float rawT = scanDuration <= 0f
                ? 1f
                : Mathf.Clamp01(scanTimer / scanDuration);

            float t = ApplyScanEase(rawT);

            scanProgress = t;

            float currentY = Mathf.Lerp(scanStartY, scanEndY, t);

            scanLine.anchoredPosition = new Vector2(
                scanLine.anchoredPosition.x,
                currentY
            );

            if (sliderPercentUI != null)
            {
                sliderPercentUI.UpdateScanProgress(t);
            }

            // スキャン終了
            if (rawT >= 1f)
            {
                scanLine.anchoredPosition = new Vector2(
                    scanLine.anchoredPosition.x,
                    scanEndY
                );

                scanningLine = false;
                scanFinished = true;

                scanLine.gameObject.SetActive(false);

                if (sliderPercentUI != null)
                {
                    sliderPercentUI.CompleteScan();
                }

                if (directionImageController != null)
                {
                    directionImageController.StartDirectionImages();
                }
                else
                {
                    Debug.LogWarning(
                        "DirectionImageControllerが設定されていません。"
                    );
                }

                BeginAutoHide();

                Debug.Log("スキャン線到達：スキャン完了");
            }
        }


        // =====================================
        // 消える画像の自動非表示
        // =====================================

        if (waitingForHide)
        {
            hideTimer += Time.deltaTime;

            if (hideTimer >= hideDelayAfterScan)
            {
                waitingForHide = false;
                HideScanPanel();
            }
        }

        UpdateScanFill();
    }


    /// <summary>
    /// スキャンの進行度に合わせて画像を上から表示する。
    /// </summary>
    private void UpdateScanFill()
    {
        if (scanImage == null || scanImageHidden) return;

        float fill = 0f;

        if (shrinkCompleted && scanFinished)
        {
            fill = 1f;
        }
        else if (shrinkCompleted && scanningLine)
        {
            fill = scanProgress;
        }

        scanImage.fillAmount = fill;
    }


    /// <summary>
    /// パネル縮小を開始する。
    /// </summary>
    public void StartShrink()
    {
        if (shrinking || shrinkCompleted)
            return;

        // Canvasを再表示
        if (canvasTransform != null)
        {
            canvasTransform.gameObject.SetActive(true);
            canvasTransform.localScale = canvasOriginalScale;
        }

        canvasHiding = false;

        gameObject.SetActive(true);

        if (target == null)
        {
            Debug.LogWarning(
                "縮小対象のRectTransformが設定されていません。"
            );

            return;
        }

        timer = 0f;
        scanTimer = 0f;
        scanDelayTimer = 0f;
        hideTimer = 0f;
        scanProgress = 0f;
        scanFinished = false;
        scanImageHidden = false;

        shrinking = true;
        waitingForScan = false;
        scanningLine = false;
        waitingForHide = false;

        target.pivot = new Vector2(pivotX, target.pivot.y);

        target.sizeDelta = new Vector2(startWidth, height);

        if (scanImage != null)
        {
            scanImage.fillAmount = 0f;

            Color c = scanImage.color;
            c.a = 1f;
            scanImage.color = c;
        }

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
    /// 消える画像だけをフェードして消す。
    /// </summary>
    public void HideScanPanel()
    {
        if (scanImage == null || scanImageHidden) return;

        scanImageHidden = true;
        StartCoroutine(HideScanImageRoutine());
    }


    private IEnumerator HideScanImageRoutine()
    {
        float t = 0f;
        Color c = scanImage.color;

        while (t < hideDuration)
        {
            t += Time.deltaTime;

            c.a = 1f - Mathf.Clamp01(t / hideDuration);
            scanImage.color = c;

            yield return null;
        }

        scanImage.fillAmount = 0f;
    }


    private void BeginAutoHide()
    {
        if (hideDelayAfterScan < 0f) return;

        hideTimer = 0f;
        waitingForHide = true;
    }


    private void StartScanLine()
    {
        if (scanLine == null)
        {
            Debug.LogWarning(
                "スキャン用のImageが設定されていません。"
            );

            if (sliderPercentUI != null)
            {
                sliderPercentUI.StartScan();
                sliderPercentUI.UpdateScanProgress(1f);
                sliderPercentUI.CompleteScan();
            }

            scanProgress = 1f;
            scanFinished = true;

            BeginAutoHide();

            return;
        }

        scanTimer = 0f;
        scanProgress = 0f;
        scanFinished = false;

        scanLine.anchoredPosition = new Vector2(
            scanLine.anchoredPosition.x,
            scanStartY
        );

        scanLine.gameObject.SetActive(true);

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

        scanningLine = true;

        Debug.Log("スキャン線・Slider連動開始");
    }


    private float ApplyScanEase(float x)
    {
        if (x <= slowStart) return x;

        float tail = 1f - slowStart;
        float u = (x - slowStart) / tail;

        float d = 1f - 1f / slowStrength;

        float eased = u - d * u * u * 0.5f;

        eased /= (1f - d * 0.5f);

        return slowStart + tail * eased;
    }


    // =====================================
    // Canvas消滅演出
    // =====================================

    /// <summary>
    /// カメラの復帰開始に合わせてCanvasを消す。
    /// </summary>
    public void StartCanvasHide()
    {
        if (canvasHiding)
            return;

        if (canvasTransform == null)
        {
            Debug.LogWarning(
                "CanvasTransformが設定されていません。"
            );
            return;
        }

        canvasHideCoroutine = StartCoroutine(HideCanvasRoutine());
    }


    /// <summary>
    /// Canvas全体を4段階で変形させて消す。
    /// </summary>
    private IEnumerator HideCanvasRoutine()
    {
        canvasHiding = true;

        Vector3 original = canvasOriginalScale;

        // ① 横幅を縮めて縦幅を伸ばす
        Vector3 firstScale = new Vector3(
            original.x * firstWidthScale,
            original.y * firstHeightScale,
            original.z
        );

        yield return AnimateCanvasScale(
            original,
            firstScale,
            firstEffectDuration
        );


        // ② 横に伸ばして縦幅を縮める
        Vector3 secondScale = new Vector3(
            original.x * secondWidthScale,
            original.y * secondHeightScale,
            original.z
        );

        yield return AnimateCanvasScale(
            firstScale,
            secondScale,
            secondEffectDuration
        );


        // ③ 一本の横線にする
        Vector3 finalScale = new Vector3(
            original.x * finalWidthScale,
            original.y * finalHeightScale,
            original.z
        );

        yield return AnimateCanvasScale(
            secondScale,
            finalScale,
            thirdEffectDuration
        );


        // ④ Canvasを非表示
        canvasTransform.localScale = finalScale;
        canvasTransform.gameObject.SetActive(false);

        canvasHiding = false;
        canvasHideCoroutine = null;

        Debug.Log("Canvas消滅演出完了");
    }


    /// <summary>
    /// Canvasのスケールを滑らかに変更する。
    /// </summary>
    private IEnumerator AnimateCanvasScale(
        Vector3 startScale,
        Vector3 endScale,
        float effectDuration)
    {
        float elapsed = 0f;

        if (effectDuration <= 0f)
        {
            canvasTransform.localScale = endScale;
            yield break;
        }

        while (elapsed < effectDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / effectDuration);

            t = Mathf.SmoothStep(0f, 1f, t);

            canvasTransform.localScale = Vector3.Lerp(
                startScale,
                endScale,
                t
            );

            yield return null;
        }

        canvasTransform.localScale = endScale;
    }


    // =====================================
    // 敵の攻撃終了
    // =====================================

    /// <summary>
    /// 敵の攻撃演出を終了する。
    /// カメラ復帰とCanvas消滅を同時に開始する。
    /// </summary>
    public void EndEnemyAttack()
    {
        shrinking = false;
        shrinkCompleted = false;
        waitingForScan = false;
        scanningLine = false;
        scanFinished = false;
        waitingForHide = false;
        scanImageHidden = false;

        timer = 0f;
        scanTimer = 0f;
        scanDelayTimer = 0f;
        hideTimer = 0f;
        scanProgress = 0f;

        if (scanLine != null)
        {
            scanLine.gameObject.SetActive(false);
        }

        if (scanImage != null)
        {
            scanImage.fillAmount = 0f;

            Color c = scanImage.color;
            c.a = 1f;
            scanImage.color = c;
        }

        if (target != null)
        {
            target.sizeDelta = new Vector2(startWidth, height);
        }

        // カメラ復帰開始
        // Canvas消滅も同じタイミングで開始する
        if (battleCameraController != null)
        {
            battleCameraController.EndZoom();
        }
        else
        {
            Debug.LogWarning(
                "BattleCameraControllerが設定されていません。"
            );

            StartCanvasHide();
        }

        // ここではgameObjectを非表示にしない。
        // 消滅コルーチンが最後まで動く必要があるため。
    }
}
