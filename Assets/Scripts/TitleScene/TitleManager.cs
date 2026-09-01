using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// "TitleScene" マネージャー
/// </summary>
public class TitleManager : MonoBehaviour
{

    #region ボタン (仮)

    /// <summary>
    /// ゲーム開始 (GameScene)
    /// </summary>
    public void OnClickStartGameButton()
    {
        // シーン移動
        SceneManager.LoadScene("GameScene");
    }



    /// <summary>
    /// 設定画面 (ConfigScene)
    /// </summary>
    public void OnClickConfigButton()
    {
        ConfigSceneController.OpenFromGame();
    }


    /// <summary>
    /// ゲーム終了
    /// </summary>
    public void OnClickQuitGameButton()
    {
        // ゲーム終了
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    #endregion



    /// <summary>
    /// Scene変更 (String指定)
    /// </summary>
    public void OnClickLoadStringSneceButton(string sceneName)
    {
        // シーン移動
        SceneManager.LoadScene(sceneName);
    }
}
