using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ConfigSceneの開閉を管理するクラス
/// </summary>
public class ConfigSceneController : MonoBehaviour
{
    public static ConfigSceneController Instance { get; private set; }

    private static string returnSceneName;    // Configを開く前のシーン

    // インスタンス化 (アクセスし放題)
    private void Awake()
    {
        Instance = this;
    }


    /// <summary>
    /// MainGameからConfigSceneを開く
    /// </summary>
    public static void OpenFromGame()
    {
        // 現在のシーンを記録
        returnSceneName = SceneManager.GetActiveScene().name;
        // ConfigSceneを追加読み込み
        SceneManager.LoadScene("ConfigScene", LoadSceneMode.Additive);
    }


    /// <summary>
    /// ConfigSceneを閉じる
    /// </summary>
    public void CloseConfig()
    {
        SceneManager.UnloadSceneAsync("ConfigScene");
    }
}