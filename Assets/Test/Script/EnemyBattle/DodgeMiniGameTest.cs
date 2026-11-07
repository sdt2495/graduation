using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 回避ミニゲームのテスト用クラス。
///
/// ・Tキーを押すと回避ミニゲームを開始する
/// ・Rキーを押すと回避ミニゲームを再開する
/// ・既存のCameraZoomTestには変更を加えない
/// </summary>
public class DodgeMiniGameTest : MonoBehaviour
{
    [Header("回避ミニゲーム設定")]

    [Tooltip("DodgeMiniGameを設定してください")]
    [SerializeField]
    private DodgeMiniGame dodgeMiniGame;

    [Header("テスト操作設定")]

    [Tooltip("有効にするとTキーでミニゲームを開始できます")]
    [SerializeField]
    private bool useStartKey = true;

    [Tooltip("有効にするとRキーでミニゲームを再開できます")]
    [SerializeField]
    private bool useRestartKey = true;

    private void Update()
    {
        // キー操作が無効なら処理しない
        if (!useStartKey && !useRestartKey)
        {
            return;
        }

        // キーボードが存在しない場合は処理しない
        if (Keyboard.current == null)
        {
            return;
        }

        // Tキーでミニゲームを開始
        if (useStartKey &&
            Keyboard.current.tKey.wasPressedThisFrame)
        {
            StartMiniGame();
        }

        // Rキーでミニゲームを再開
        if (useRestartKey &&
            Keyboard.current.rKey.wasPressedThisFrame)
        {
            RestartMiniGame();
        }
    }

    /// <summary>
    /// 回避ミニゲームを開始する。
    /// </summary>
    public void StartMiniGame()
    {
        if (dodgeMiniGame == null)
        {
            Debug.LogError(
                "DodgeMiniGameTest：DodgeMiniGameが設定されていません。",
                this
            );

            return;
        }

        dodgeMiniGame.StartDodgeMiniGame();

        Debug.Log("DodgeMiniGameTest：開始処理を呼び出しました。");
    }

    /// <summary>
    /// 回避ミニゲームを再開する。
    /// </summary>
    public void RestartMiniGame()
    {
        if (dodgeMiniGame == null)
        {
            Debug.LogError(
                "DodgeMiniGameTest：DodgeMiniGameが設定されていません。",
                this
            );

            return;
        }

        // 実行中なら二重に開始しない
        if (dodgeMiniGame.IsPlaying)
        {
            Debug.Log(
                "DodgeMiniGameTest：ミニゲームはすでに実行中です。",
                this
            );

            return;
        }

        dodgeMiniGame.StartDodgeMiniGame();

        Debug.Log("DodgeMiniGameTest：再開処理を呼び出しました。");
    }
}
