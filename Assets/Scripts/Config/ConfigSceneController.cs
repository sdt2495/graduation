
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 設定画面の開閉と状態を管理するクラス
/// </summary>
public class ConfigSceneController : MonoBehaviour
{
    public static ConfigSceneController Instance { get; private set; }

    // 設定画面が開いているかどうか
    public static bool IsConfigOpen { get; private set; }

    private static string returnSceneName;

    private void Awake()
    {
        // インスタンスを登録
        Instance = this;

        // 設定画面が開いた状態にする
        IsConfigOpen = true;
    }

    /// <summary>
    /// ゲーム中から設定画面を開く
    /// </summary>
    public static void OpenFromGame()
    {
        // 現在のシーン名を保存
        returnSceneName = SceneManager.GetActiveScene().name;

        // 設定画面を追加読み込み
        SceneManager.LoadScene("ConfigScene", LoadSceneMode.Additive);
    }

    /// <summary>
    /// 設定画面を閉じる
    /// </summary>
    public void CloseConfig()
    {
        Debug.Log("設定画面を閉じます");

        // タイトル画面の状態をリセット
        TitleManager titleManager = FindFirstObjectByType<TitleManager>();

        if (titleManager != null)
        {
            titleManager.OnReturnFromConfig();
        }

        // 設定画面だけを閉じる
        SceneManager.UnloadSceneAsync("ConfigScene");
    }

    /// <summary>
    /// 設定画面が破棄されたとき
    /// </summary>
    private void OnDestroy()
    {
        // 設定画面が閉じられた状態にする
        IsConfigOpen = false;

        // インスタンスを解除
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
