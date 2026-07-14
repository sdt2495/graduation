using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// "TitleScene" マネージャー
/// </summary>
public class TitleManager : MonoBehaviour
{
    [Header("TitleSEManager")]
    [SerializeField] private TitleSEManager titleSEManager; // タイトルでのSEを再生するスクリプト

    #region ボタン (仮)

    /// <summary>
    /// Scene変更 (ボタン)
    /// </summary>
    public void OnClickChangeSneceButton(string sceneName)
    {
        // シーン移動
        StartCoroutine(ChangeScene(sceneName));
    }
    private IEnumerator ChangeScene(string sceneName)
    {
        float seLength = 0f;

        if (titleSEManager != null)
        {
            // SE再生
            seLength = titleSEManager.PlayDecisionSE();
        }
        else
        {
            Debug.LogWarning("TitleSEManagerが設定されていません");
        }

        // SE終了待ち
        yield return new WaitForSeconds(seLength);

        // シーン遷移
        SceneManager.LoadScene(sceneName);
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
}
