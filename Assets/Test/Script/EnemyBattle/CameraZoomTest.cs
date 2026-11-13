
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// スペースキーでカメラのズームと復帰を切り替え、
/// 動作確認を行うテスト用クラス。
///
/// ・スペースキーで左方向へズーム
/// ・もう一度押すと元の状態に戻る
/// </summary>
public class CameraZoomTest : MonoBehaviour
{
    [Header("カメラ制御")]

    [Tooltip("BattleCameraControllerを設定します。")]
    [SerializeField]
    private BattleCameraController battleCameraController;

    [Header("テスト設定")]

    [Tooltip("有効にするとスペースキーで操作できます。")]
    [SerializeField]
    private bool useSpaceKey = true;

    // 現在ズーム中かどうか
    private bool isZoomed;

    private void Update()
    {
        // キー操作が無効なら何もしない
        if (!useSpaceKey)
        {
            return;
        }

        // キーボードが存在しない場合は何もしない
        if (Keyboard.current == null)
        {
            return;
        }

        // スペースキーが押された瞬間に切り替える
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            ToggleZoom();
        }
    }

    /// <summary>
    /// ズームと元の状態への復帰を切り替える。
    /// </summary>
    public void ToggleZoom()
    {
        if (battleCameraController == null)
        {
            Debug.LogError(
                "CameraZoomTest：BattleCameraControllerが設定されていません。",
                this
            );

            return;
        }

        if (isZoomed)
        {
            battleCameraController.EndZoom();
            isZoomed = false;
        }
        else
        {
            battleCameraController.StartZoom();
            isZoomed = true;
        }
    }
}
