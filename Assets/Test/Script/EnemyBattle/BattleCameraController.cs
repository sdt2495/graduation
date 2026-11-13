
using System.Collections;
using UnityEngine;

/// <summary>
/// OPTICAL SCAN演出に合わせてカメラとPlayerを制御する。
///
/// ・背景の左側を基準にカメラを移動する
/// ・ズームと同時にカメラを移動する
/// ・Playerもカメラと同じ方向へ移動する
/// ・Playerの移動距離はカメラの移動距離の0.5倍
/// ・カメラとPlayerは同じ補間率で移動する
/// ・スキャン中はカメラとPlayerの位置を維持する
/// ・終了時に両方を元の位置へ戻す
/// </summary>
public class BattleCameraController : MonoBehaviour
{
    [Header("対象カメラ")]
    [SerializeField] private Camera targetCamera;

    [Header("移動対象")]
    [Tooltip("移動させるPlayer")]
    [SerializeField] private Transform player;

    [Header("ズーム設定")]
    [Tooltip("ズーム倍率")]
    [SerializeField] private float zoomMultiplier = 1.5f;

    [Tooltip("カメラINにかける時間")]
    [SerializeField] private float zoomDuration = 1f;

    [Tooltip("カメラOUTにかける時間")]
    [SerializeField] private float returnDuration = 1f;

    [Header("ズーム中心の設定")]
    [Tooltip("ズームの基準にする背景の位置")]
    [SerializeField] private Transform zoomFocus;

    [Tooltip("ズーム中心の上下位置を調整する距離")]
    [SerializeField] private float verticalOffset = 0f;

    [Header("カメラ移動設定")]
    [Tooltip("ズーム時にカメラを右方向へ移動させる距離")]
    [SerializeField] private float horizontalMoveDistance = 2f;

    [Header("Player移動設定")]
    [Tooltip("カメラの移動距離に対するPlayerの移動倍率")]
    [SerializeField] private float playerMoveRate = 0.5f;

    [Header("補間設定")]
    [Tooltip("カメラとPlayerの移動・ズームに使用する補間カーブ")]
    [SerializeField]
    private AnimationCurve zoomCurve =
        AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    // 元のカメラ情報
    private float originalFOV;
    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;

    // 元のPlayer位置
    private Vector3 originalPlayerPosition;

    // カメラの移動先
    private Vector3 targetCameraPosition;

    // Playerの移動先
    private Vector3 targetPlayerPosition;

    // カメラ制御用コルーチン
    private Coroutine cameraCoroutine;

    // ズーム倍率の最小値
    private const float MIN_ZOOM_MULTIPLIER = 0.01f;

    private void Awake()
    {
        // 対象カメラを取得
        if (targetCamera == null)
        {
            targetCamera = GetComponent<Camera>();
        }

        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (targetCamera == null)
        {
            Debug.LogError(
                "BattleCameraController：カメラが見つかりません。",
                this
            );

            enabled = false;
            return;
        }

        // Playerを自動取得
        if (player == null)
        {
            GameObject playerObject =
                GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }

        if (player == null)
        {
            Debug.LogError(
                "BattleCameraController：Playerが見つかりません。",
                this
            );

            enabled = false;
            return;
        }

        // Perspectiveカメラを確認
        if (targetCamera.orthographic)
        {
            Debug.LogError(
                "BattleCameraController：Perspectiveカメラを設定してください。",
                this
            );

            enabled = false;
            return;
        }

        // 元のカメラ情報を保存
        originalFOV = targetCamera.fieldOfView;

        originalCameraPosition =
            targetCamera.transform.position;

        originalCameraRotation =
            targetCamera.transform.rotation;

        // 元のPlayer位置を保存
        originalPlayerPosition = player.position;
    }

    /// <summary>
    /// カメラとPlayerを移動させながらズームする。
    /// </summary>
    public void StartZoom()
    {
        StopCameraCoroutine();

        if (zoomFocus == null)
        {
            Debug.LogError(
                "ズーム中心のTransformが設定されていません。",
                this
            );

            return;
        }

        // カメラの向きを取得
        Vector3 cameraForward =
            originalCameraRotation * Vector3.forward;

        Vector3 cameraRight =
            originalCameraRotation * Vector3.right;

        Vector3 cameraUp =
            originalCameraRotation * Vector3.up;

        // ズームする背景の位置
        Vector3 focusPosition =
            zoomFocus.position + cameraUp * verticalOffset;

        // ズーム後のFOV
        float targetFOV =
            originalFOV /
            Mathf.Max(zoomMultiplier, MIN_ZOOM_MULTIPLIER);

        // 注目位置までの奥行きを計算
        float distance = Vector3.Dot(
            focusPosition - originalCameraPosition,
            cameraForward
        );

        // 注目位置が画面中央に来るカメラ位置
        targetCameraPosition =
            focusPosition - cameraForward * distance;

        // カメラを右方向へ移動
        Vector3 cameraMove =
            cameraRight * horizontalMoveDistance;

        targetCameraPosition += cameraMove;

        // Playerの現在位置を取得
        Vector3 startPlayerPosition = player.position;

        // カメラの移動量を取得
        Vector3 actualCameraMove =
            targetCameraPosition - originalCameraPosition;

        // Playerはカメラの移動距離の指定倍率だけ移動
        targetPlayerPosition =
            startPlayerPosition +
            actualCameraMove * playerMoveRate;

        // ズームと移動を同時に開始
        cameraCoroutine = StartCoroutine(
            ZoomCamera(
                targetFOV,
                targetCameraPosition,
                targetPlayerPosition,
                zoomDuration
            )
        );
    }

    /// <summary>
    /// カメラとPlayerを元の位置へ戻す。
    /// </summary>
    public void EndZoom()
    {
        StopCameraCoroutine();

        cameraCoroutine = StartCoroutine(
            ZoomCamera(
                originalFOV,
                originalCameraPosition,
                originalPlayerPosition,
                returnDuration
            )
        );
    }

    /// <summary>
    /// 実行中のコルーチンを停止する。
    /// </summary>
    private void StopCameraCoroutine()
    {
        if (cameraCoroutine != null)
        {
            StopCoroutine(cameraCoroutine);
            cameraCoroutine = null;
        }
    }

    /// <summary>
    /// カメラとPlayerを同じ補間率で移動させる。
    /// </summary>
    private IEnumerator ZoomCamera(
        float targetFOV,
        Vector3 destinationCameraPosition,
        Vector3 destinationPlayerPosition,
        float duration)
    {
        // 開始時のカメラ情報
        float startFOV = targetCamera.fieldOfView;

        Vector3 startCameraPosition =
            targetCamera.transform.position;

        // 開始時のPlayer位置
        Vector3 startPlayerPosition = player.position;

        float elapsedTime = 0f;

        // 時間が0以下なら即座に変更
        if (duration <= 0f)
        {
            targetCamera.fieldOfView = targetFOV;

            targetCamera.transform.position =
                destinationCameraPosition;

            player.position = destinationPlayerPosition;

            cameraCoroutine = null;
            yield break;
        }

        // 指定時間まで補間
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            // 進行度
            float progress =
                Mathf.Clamp01(elapsedTime / duration);

            // 補間カーブ
            float curveValue =
                zoomCurve.Evaluate(progress);

            // FOVを変更
            targetCamera.fieldOfView = Mathf.Lerp(
                startFOV,
                targetFOV,
                curveValue
            );

            // カメラを移動
            targetCamera.transform.position =
                Vector3.Lerp(
                    startCameraPosition,
                    destinationCameraPosition,
                    curveValue
                );

            // Playerをカメラと同じ補間率で移動
            player.position =
                Vector3.Lerp(
                    startPlayerPosition,
                    destinationPlayerPosition,
                    curveValue
                );

            yield return null;
        }

        // 最終位置を確実に設定
        targetCamera.fieldOfView = targetFOV;

        targetCamera.transform.position =
            destinationCameraPosition;

        player.position =
            destinationPlayerPosition;

        cameraCoroutine = null;
    }
}
