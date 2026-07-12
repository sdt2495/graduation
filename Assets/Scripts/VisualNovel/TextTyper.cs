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


    private string currentMessage;           // 現在表示している全文を保存
    private Coroutine typingCoroutine;       // 文字送りのCoroutineを保存

    public event Action OnTypingFinished;    // 文字送り終了時に呼ばれるイベント


    #region Get:読み取り関数
    public bool IsTyping { get; private set; } // 現在文字送り中かどうか (true = 表示中│false = 表示完了)
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

        // 文章を一文字ずつ取り出す
        foreach (char c in currentMessage)
        {
            // 一文字追加
            messageText.text += c;

            // 指定秒数待つ
            yield return new WaitForSeconds(textSpeed);
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