using System.Collections;
using TMPro;
using UnityEngine;
using System;

/// <summary>
/// セリフを一文字ずつ表示するクラス
/// </summary>
public class TextTyper : MonoBehaviour
{
    [Header("表示するテキスト")]
    [SerializeField] private TextMeshProUGUI messageText;

    [Header("文字送り速度（秒）")]
    [SerializeField] private float textSpeed = 0.1f;

    [Header("──────────────────────────────")]
    [Header("「、」(読点) 一時停止機能")]
    [SerializeField] private bool pauseAtComma = true;
    [Tooltip("「、」(読点) 一時停止時間")][SerializeField] private float commaPauseTime = 0.5f;

    [Header("「。」(句点) 一時停止機能")]
    [SerializeField] private bool pauseAtPeriod = true;
    [Tooltip("「。」(句点) 一時停止時間")][SerializeField] private float periodPauseTime = 0.5f;


    private bool isSkipMode = false; // Skipモード中か


    private string currentMessage;           // 現在表示している全文を保存
    private Coroutine typingCoroutine;       // 文字送りのCoroutineを保存

    public event Action OnTypingFinished;    // 文字送り終了時に呼ばれるイベント


    #region Get:読み取り関数
    public bool IsTyping { get; private set; } // 現在文字送り中かどうか (true = 表示中│false = 表示完了)
    #endregion


    #region Skipモード

    /// <summary>
    /// Skipモードの状態を設定 (NovelManagerからSkipモードの状態を受け取る)
    /// </summary>
    public void SetSkipMode(bool skipMode)
    {
        isSkipMode = skipMode;
    }

    #endregion


    #region 1文字ずつ表示
    /// <summary>
    /// 新しい文章を表示開始する
    /// </summary>
    public void StartTyping(string message)
    {
        // 表示する文章を保存
        currentMessage = message;

        // もし前回の文字送りがまだ動いていたら止める
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        // 一文字ずつ表示開始
        typingCoroutine = StartCoroutine(TypeText());
    }

    /// <summary>
    /// 一文字ずつ表示するCoroutine
    /// </summary>
    IEnumerator TypeText()
    {
        // 文字送り開始
        IsTyping = true;

        // 最初は空文字にする
        messageText.text = "";


        // 現在の文章を一文字ずつ処理
        for (int i = 0; i < currentMessage.Length; i++)
        {
            // Rich Textのタグを検出
            if (currentMessage[i] == '<')
            {
                // 「>」までをタグとして取得
                int tagEnd = currentMessage.IndexOf('>', i);

                // タグが見つかった場合
                if (tagEnd != -1)
                {
                    // タグ全体を一気に追加
                    messageText.text += currentMessage.Substring(i, tagEnd - i + 1);

                    // タグの最後まで進める
                    i = tagEnd;

                    // タグ自体では待機しない
                    continue;
                }
            }


            // 一文字追加
            messageText.text += currentMessage[i];

            // Skipモード中は待機しない
            if (isSkipMode)
            {
                continue;
            }

            // 「、」の場合は専用の待機時間
            if (pauseAtComma && currentMessage[i] == '、')
            {
                yield return new WaitForSeconds(commaPauseTime);
            }
            // 「。」の場合は専用の待機時間
            else if (pauseAtPeriod && currentMessage[i] == '。')
            {
                yield return new WaitForSeconds(periodPauseTime);
            }
            else
            {
                // 通常の文字送り
                yield return new WaitForSeconds(textSpeed);
            }
        }

        // 文字送り終了
        IsTyping = false;
        // 「文字送り終了」を通知 (イベント)
        OnTypingFinished?.Invoke();
    }
    #endregion


    #region 文字送りスキップ
    /// <summary>
    /// 文字送りスキップ (クリック時など)
    /// </summary>
    public void Skip(bool callEvent = true)
    {
        // 文字送り中でなければ何もしない
        if (!IsTyping) return;

        // Coroutineを停止
        StopCoroutine(typingCoroutine);
        // 全文表示
        messageText.text = currentMessage;

        // 文字送り終了
        IsTyping = false;

        if (callEvent)
        {
            //「文字送り終了」を通知 (イベント)
            OnTypingFinished?.Invoke();
        }
    }
    #endregion


    #region 全文表示
    /// <summary>
    /// 文章を一瞬で表示する
    /// </summary>
    public void ShowInstant(string message)
    {
        // Coroutine停止 (あってもなくてもいい)
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        currentMessage = message;

        messageText.text = message;

        IsTyping = false;

        OnTypingFinished?.Invoke();
    }
    #endregion
}