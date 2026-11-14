
using UnityEngine;

/// <summary>
/// Playerの周囲に4方向の画像を表示する。
///
/// ・PlayerをTagから自動取得
/// ・PlayerはCanvasの外に配置可能
/// ・Playerのワールド座標をCanvas座標に変換
/// ・上下左右の画像をPlayerの外側に配置
/// ・4方向の画像を同時に外側へ移動
/// ・移動終了後は表示したまま残す
/// </summary>
public class DirectionImageController : MonoBehaviour
{
    [Header("Player設定")]
    [SerializeField] private string playerTag = "Player";

    [Header("Canvas設定")]
    [SerializeField] private Canvas targetCanvas;

    [Header("4方向の画像")]
    [SerializeField] private RectTransform upImage;
    [SerializeField] private RectTransform downImage;
    [SerializeField] private RectTransform leftImage;
    [SerializeField] private RectTransform rightImage;

    [Header("Playerからの初期距離")]
    [SerializeField] private float initialOffset = 100f;

    [Header("画像の広がり設定")]
    [SerializeField] private float spreadDistance = 50f;
    [SerializeField] private float moveDuration = 0.4f;

    // Player
    private Transform player;

    // CanvasのRectTransform
    private RectTransform canvasRect;

    // 移動開始位置
    private Vector2 upStartPosition;
    private Vector2 downStartPosition;
    private Vector2 leftStartPosition;
    private Vector2 rightStartPosition;

    // 移動終了位置
    private Vector2 upEndPosition;
    private Vector2 downEndPosition;
    private Vector2 leftEndPosition;
    private Vector2 rightEndPosition;

    // 移動時間
    private float timer;

    // 移動中かどうか
    private bool isMoving;

    // 開始済みかどうか
    private bool hasStarted;

    private void Awake()
    {
        // Canvas取得
        if (targetCanvas == null)
        {
            targetCanvas = GetComponentInParent<Canvas>();

            if (targetCanvas == null)
            {
                targetCanvas = FindFirstObjectByType<Canvas>();
            }
        }

        if (targetCanvas != null)
        {
            canvasRect = targetCanvas.GetComponent<RectTransform>();
        }

        // Player取得
        FindPlayer();

        // 初期状態では画像を非表示
        SetImagesActive(false);
    }

    private void Update()
    {
        if (!isMoving)
            return;

        // 経過時間
        timer += Time.deltaTime;

        // 移動進行度
        float t = moveDuration <= 0f
            ? 1f
            : Mathf.Clamp01(timer / moveDuration);

        // 滑らかに移動
        float smoothT = Mathf.SmoothStep(0f, 1f, t);

        // 上
        if (upImage != null)
        {
            upImage.anchoredPosition = Vector2.Lerp(
                upStartPosition,
                upEndPosition,
                smoothT
            );
        }

        // 下
        if (downImage != null)
        {
            downImage.anchoredPosition = Vector2.Lerp(
                downStartPosition,
                downEndPosition,
                smoothT
            );
        }

        // 左
        if (leftImage != null)
        {
            leftImage.anchoredPosition = Vector2.Lerp(
                leftStartPosition,
                leftEndPosition,
                smoothT
            );
        }

        // 右
        if (rightImage != null)
        {
            rightImage.anchoredPosition = Vector2.Lerp(
                rightStartPosition,
                rightEndPosition,
                smoothT
            );
        }

        // 移動終了
        if (t >= 1f)
        {
            isMoving = false;

            Debug.Log("4方向の画像の移動が完了しました。");
        }
    }

    /// <summary>
    /// PlayerをTagから取得する。
    /// </summary>
    private void FindPlayer()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag(playerTag);

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogWarning(
                "TagがPlayerのオブジェクトが見つかりません。"
            );
        }
    }

    /// <summary>
    /// Playerのワールド座標をCanvas座標に変換する。
    /// </summary>
    private bool GetPlayerCanvasPosition(out Vector2 canvasPosition)
    {
        canvasPosition = Vector2.zero;

        if (player == null)
        {
            FindPlayer();
        }

        if (player == null || canvasRect == null)
            return false;

        // CanvasのRender Modeに応じてカメラを取得
        Camera uiCamera = null;

        if (targetCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            uiCamera = targetCanvas.worldCamera;

            if (uiCamera == null)
            {
                uiCamera = Camera.main;
            }
        }

        // Playerのワールド座標をスクリーン座標に変換
        Vector3 screenPosition = RectTransformUtility.WorldToScreenPoint(
            Camera.main,
            player.position
        );

        // スクリーン座標をCanvasのローカル座標に変換
        return RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPosition,
            uiCamera,
            out canvasPosition
        );
    }

    /// <summary>
    /// 4方向の画像演出を開始する。
    /// UIPanelShrinkから呼び出す。
    /// </summary>
    public void StartDirectionImages()
    {
        // 二重起動防止
        if (hasStarted)
            return;

        // PlayerのCanvas座標を取得
        if (!GetPlayerCanvasPosition(out Vector2 centerPosition))
        {
            Debug.LogWarning(
                "PlayerまたはCanvasの取得に失敗しました。"
            );

            return;
        }

        // Playerの外側に4方向の画像を配置
        SetInitialPositions(centerPosition);

        // 画像を表示
        SetImagesActive(true);

        // 移動開始位置を保存
        SaveStartPositions();

        // 移動先を設定
        SetEndPositions();

        // 移動開始
        timer = 0f;
        isMoving = true;
        hasStarted = true;

        Debug.Log("4方向の画像演出を開始しました。");
    }

    /// <summary>
    /// Playerの外側に4方向の画像を配置する。
    /// </summary>
    private void SetInitialPositions(Vector2 centerPosition)
    {
        // 上
        if (upImage != null)
        {
            upImage.anchoredPosition =
                centerPosition + Vector2.up * initialOffset;
        }

        // 下
        if (downImage != null)
        {
            downImage.anchoredPosition =
                centerPosition + Vector2.down * initialOffset;
        }

        // 左
        if (leftImage != null)
        {
            leftImage.anchoredPosition =
                centerPosition + Vector2.left * initialOffset;
        }

        // 右
        if (rightImage != null)
        {
            rightImage.anchoredPosition =
                centerPosition + Vector2.right * initialOffset;
        }
    }

    /// <summary>
    /// 移動開始位置を保存する。
    /// </summary>
    private void SaveStartPositions()
    {
        if (upImage != null)
            upStartPosition = upImage.anchoredPosition;

        if (downImage != null)
            downStartPosition = downImage.anchoredPosition;

        if (leftImage != null)
            leftStartPosition = leftImage.anchoredPosition;

        if (rightImage != null)
            rightStartPosition = rightImage.anchoredPosition;
    }

    /// <summary>
    /// 4方向の移動先を設定する。
    /// </summary>
    private void SetEndPositions()
    {
        upEndPosition =
            upStartPosition + Vector2.up * spreadDistance;

        downEndPosition =
            downStartPosition + Vector2.down * spreadDistance;

        leftEndPosition =
            leftStartPosition + Vector2.left * spreadDistance;

        rightEndPosition =
            rightStartPosition + Vector2.right * spreadDistance;
    }

    /// <summary>
    /// 画像の表示状態を変更する。
    /// </summary>
    private void SetImagesActive(bool active)
    {
        if (upImage != null)
            upImage.gameObject.SetActive(active);

        if (downImage != null)
            downImage.gameObject.SetActive(active);

        if (leftImage != null)
            leftImage.gameObject.SetActive(active);

        if (rightImage != null)
            rightImage.gameObject.SetActive(active);
    }
}
