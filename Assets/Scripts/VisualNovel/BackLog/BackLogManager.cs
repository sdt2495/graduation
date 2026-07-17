using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BackLogManager : MonoBehaviour
{
    [Header("バックログパネル")]
    [SerializeField] private GameObject backLogPanel;

    [Header("Voice")]
    [SerializeField] private NovelVoiceManager voiceManager;

    [Header("──────────────────────────────")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Transform content;
    [SerializeField] private BackLogEntry entryPrefab;

    private List<BackLogData> logs = new();      // バックログ一覧
    public bool IsOpen { get; private set; }     // バックログが開いているか


    /// <summary>
    /// ログ追加
    /// </summary>
    public void AddLog(string speaker, string message, string voice)
    {
        logs.Add(new BackLogData(speaker, message, voice));
    }

    /// <summary>
    /// ログ取得
    /// </summary>
    public List<BackLogData> GetLogs()
    {
        return logs;
    }

    #region バックログの開閉

    /// <summary>
    /// バックログを開く
    /// </summary>
    public void Open()
    {
        if (IsOpen)
            return;

        IsOpen = true;

        // バックログ表示
        backLogPanel.SetActive(true);

        // バックログのログを作成
        CreateEntries();
        // 一番下へスクロール
        StartCoroutine(ScrollToBottom());

        Debug.Log("バックログを開く");
    }

    /// <summary>
    /// バックログを閉じる
    /// </summary>
    public void Close()
    {
        if (!IsOpen)
            return;

        IsOpen = false;

        // バックログ非表示
        backLogPanel.SetActive(false);
        Debug.Log("バックログを閉じる");
    }

    #endregion


    /// <summary>
    /// ログ一覧を生成
    /// </summary>
    private void CreateEntries()
    {
        // 古いEntryを全部削除
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        // ログを表示
        foreach (BackLogData log in logs)
        {
            BackLogEntry entry = Instantiate(entryPrefab, content);
            
            // ログとVoiceManagerを引き渡し
            entry.Set(log, voiceManager);
        }
    }

    /// <summary>
    /// バックログを一番下へスクロール 
    /// </summary>
    private IEnumerator ScrollToBottom()
    {
        // レイアウト更新待ち (1f待つ)
        yield return null;

        // 一番下へスクロール
        scrollRect.verticalNormalizedPosition = 0f;
    }
}