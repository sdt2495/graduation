
using UnityEngine;
using System;

/// <summary>
/// ゲーム全体の進行を管理するクラス
/// 会話の開始や、会話終了後の処理を管理する
/// </summary>
public class GameFlowManager : MonoBehaviour
{
    // どのスクリプトからでもアクセスできるインスタンス
    public static GameFlowManager Instance { get; private set; }

    [Header("NovelManager")]
    [SerializeField] private NovelManager novelManager;

    private bool isPlayingConversation = false;    // 会話中かどうか


    #region 読み取り専用変数 (Get)

    public bool IsPlayingConversation => isPlayingConversation;
    #endregion


    #region 初期化

    private void Awake()
    {
        // 既に別のインスタンスが存在する場合
        if (Instance != null && Instance != this)
        {
            // 重複したGameFlowManagerを破棄
            Destroy(gameObject);
            return;
        }

        // このオブジェクトをインスタンスとして登録
        Instance = this;

        // NovelManagerが設定されていない場合
        if (novelManager == null)
        {
            Debug.LogError("NovelManagerが設定されていません。");
        }
    }

    private void OnDestroy()
    {
        // 自分自身が登録中のインスタンスなら解除
        if (Instance == this)
        {
            Instance = null;
        }
    }
    #endregion


    #region 会話管理

    /// <summary>
    /// 会話を開始する
    /// 会話終了後に指定された処理を実行する
    /// </summary>
    public void PlayConversation(string csvName, Action onFinished = null)
    {
        // NovelManagerが設定されていない場合
        if (novelManager == null)
        {
            Debug.LogError("NovelManagerが見つかりません。");
            return;
        }

        // 既に会話中の場合は開始しない
        if (isPlayingConversation)
        {
            Debug.LogWarning("既に会話が再生されています。");
            return;
        }

        // 会話中にする
        isPlayingConversation = true;

        // NovelManagerで会話を開始
        novelManager.StartConversation(csvName, () =>
        {
            // 会話終了
            isPlayingConversation = false;

            // 指定された終了後の処理を実行
            onFinished?.Invoke();
        });
    }
    #endregion
}
