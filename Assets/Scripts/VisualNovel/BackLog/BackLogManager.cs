using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BackLogManager : MonoBehaviour
{
    [Header("NovelManager")]
    [SerializeField] private NovelManager novelManager;

    [Header("バックログパネル")]
    [SerializeField] private GameObject backLogPanel;

    [Header("Voice")]
    [SerializeField] private NovelVoiceManager voiceManager;

    [Header("1回の入力でスクロールする量")]
    [SerializeField] private float scrollAmount = 0.15f;

    [Header("──────────────────────────────")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Transform content;
    [SerializeField] private BackLogEntry entryPrefab;

    private List<BackLogData> logs = new();      // バックログ一覧
    public bool IsOpen { get; private set; }     // バックログが開いているか

    private const float BOTTOM_THRESHOLD = 0.001f;     // 一番下にいると判定するための許容値


    /// <summary>
    /// バックログ操作
    /// </summary>
    private void Update()
    {
        // バックログが開いていないなら何もしない
        if (!IsOpen)
            return;

        // 上方向キー
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            ScrollUp();
        }
        // 下方向キー
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            // 一番下ならバックログを閉じる
            if (IsAtBottom())
            {
                novelManager.CloseBackLog();
                return;
            }
            ScrollDown();
        }


        // マウスホイール
        float wheel = Input.mouseScrollDelta.y;
        // ホイール上
        if (wheel > 0)
        {
            ScrollUp();
        }
        // ホイール下
        else if (wheel < 0)
        {
            // 一番下ならバックログを閉じる
            if (IsAtBottom())
            {
                novelManager.CloseBackLog();
                return;
            }
            ScrollDown();
        }
    }

    #region スクロール処理

    /// <summary>
    /// バックログを上へスクロール
    /// </summary>
    private void ScrollUp()
    {
        scrollRect.verticalNormalizedPosition += scrollAmount;
        // 1を超えないようにする
        scrollRect.verticalNormalizedPosition = Mathf.Clamp01(scrollRect.verticalNormalizedPosition);
    }
    /// <summary>
    /// バックログを下へスクロール
    /// </summary>
    private void ScrollDown()
    {
        scrollRect.verticalNormalizedPosition -= scrollAmount;
        // 0を下回らないようにする
        scrollRect.verticalNormalizedPosition = Mathf.Clamp01(scrollRect.verticalNormalizedPosition);
    }

    /// <summary>
    /// バックログが一番下にいるか
    /// </summary>
    private bool IsAtBottom()
    {
        // 誤差を考慮して判定
        return scrollRect.verticalNormalizedPosition <= BOTTOM_THRESHOLD;
    }
    #endregion




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