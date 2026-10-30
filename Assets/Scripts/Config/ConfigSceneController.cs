using UnityEngine;
using UnityEngine.SceneManagement;

public class ConfigSceneController : MonoBehaviour
{
    public static ConfigSceneController Instance { get; private set; }

    private static string returnSceneName;

    private void Awake()
    {
        Instance = this;
    }

    // 設定画面を開く
    public static void OpenFromGame()
    {
        returnSceneName = SceneManager.GetActiveScene().name;

        SceneManager.LoadScene(
            "ConfigScene",
            LoadSceneMode.Additive
        );
    }

    // 設定画面を閉じる
    public void CloseConfig()
    {
        Debug.Log("設定画面を閉じます");

        // タイトル画面の状態をリセット
        TitleManager titleManager =
            FindFirstObjectByType<TitleManager>();

        if (titleManager != null)
        {
            titleManager.OnReturnFromConfig();
        }

        // 設定画面だけを閉じる
        SceneManager.UnloadSceneAsync("ConfigScene");
    }
}