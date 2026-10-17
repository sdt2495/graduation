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
        Debug.Log($"SceneCount = {SceneManager.sceneCount}");

        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            Debug.Log($"Loaded Scene : {scene.name}");
        }

        AsyncOperation op = SceneManager.UnloadSceneAsync("ConfigScene");

        Debug.Log($"Unload Result = {op}");
    }
}