using UnityEngine;
using TMPro;

/// <summary>
/// ノベルゲーム全体の進行を管理するクラス
/// </summary>

// CSVの中身
// line[0] = ID
// line[1] = 名前
// line[2] = セリフ
public class NovelManager : MonoBehaviour
{
    [Header("CSVReader")]
    [SerializeField] private CSVReader csvReader;        // CSVを読み込むスクリプト

    [Header("TextTyper")]
    [SerializeField] private TextTyper textTyper;        // 一文字ずつ表示するスクリプト

    [Header("seManager")]
    [SerializeField] private NovelSEManager seManager;

    [Header("──────────────────────────────")]
    [Header("名前表示")]
    [SerializeField] private TextMeshProUGUI nameText;   // 発言者の名前

    [Header("Messageウィンドウ")]
    [SerializeField] private GameObject messageWindow;   // メッセージウィンドウ

    [Header("──────────────────────────────")]
    [Header("オートモード")]
    [SerializeField] private bool autoMode = false;      // オートモードのON/OFF
    [SerializeField] private float autoWaitTime = 1.5f;  // オートモード待機時間

    [Header("──────────────────────────────")]
    [Header("スキップモード")]
    [SerializeField] private bool skipMode = false;


    private float autoTimer = 0f;        // オートモードタイマー
    private bool autoWaiting = false;    // オートタイマーが動いているか

    private int currentLine = 0;         // 現在読んでいる行番号

    private bool messageVisible = true;  // ウィンドウの表示

    #region イベント
    void Start()
    {
        // イベント登録 (TextTyperの終了イベントを受け取る)
        textTyper.OnTypingFinished += OnTypingFinished;
    }
    void OnDestroy()
    {
        // イベント登録解除
        textTyper.OnTypingFinished -= OnTypingFinished;
    }

    /// <summary>
    /// 文字送り終了時
    /// </summary>
    void OnTypingFinished()
    {
        BeginNextLineWait();
    }
    #endregion


    void Update()
    {
        // 入力処理
        UpdateInput();
        // オートモードのタイマー更新処理
        UpdateAutoMode();
    }

    #region Updateを分割

    /// <summary>
    /// プレイヤーからの入力を処理する
    /// </summary>
    void UpdateInput()
    {
        // [!] 操作キーは仮

        // [左クリック]
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            // ウィンドウ非表示中なら表示だけする
            if (!messageVisible)
            {
                SetMessageWindow(true);
                return;
            }

            // メッセージを進める
            AdvanceMessage();
        }

        // [Aキー]
        if (Input.GetKeyDown(KeyCode.A))
        {
            SetAutoMode(!autoMode);
        }

        // [Ctrlキー]
        if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl))
        {
            SetSkipMode(!skipMode);
        }

        // [スペース]
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CancelAutoSkip();
            HandleMessageInput();
        }
    }

    /// <summary>
    /// オートモードのタイマーを更新する
    /// </summary>
    void UpdateAutoMode()
    {
        // ウィンドウ非表示中はオート進行しない
        if (!messageVisible)
            return;
        // 「オートでもスキップでもない」または「待機中でない」ならreturn
        if ((!autoMode && !skipMode) || !autoWaiting)
            return;

        // タイマーを進める
        autoTimer += Time.deltaTime;

        // skipModeがtrueなら0f、falseならautoWaitTime (条件 ? 条件がtrueの場合 : 条件がfalseの場合)
        float waitTime = skipMode ? 0f : autoWaitTime;

        // 待機時間を超えたら
        if (autoTimer >= waitTime)
        {
            // 待機終了
            autoWaiting = false;
            autoTimer = 0f;

            // メッセージを進める
            AdvanceMessage();
        }
    }

    #endregion



    #region 次のメッセージへ進む

    /// <summary>
    /// メッセージを進める
    /// </summary>
    void AdvanceMessage()
    {
        // 文字送り中なら全文表示
        if (textTyper.IsTyping)
        {
            textTyper.Skip();
            return;
        }

        // CSVの最後まで読んだら終了
        if (currentLine >= csvReader.GetCount())
        {
            Debug.Log("CSVの最後まで読みました。");

            SetAutoMode(false);
            SetSkipMode(false);
            return;
        }

        // 次の文章へ進むSE
        if (seManager != null)
        {
            seManager.PlayNextLineSE();
        }
        // 現在の行を表示
        DisplayCurrentLine();
    }


    /// <summary>
    /// 現在の行を表示する
    /// </summary>
    void DisplayCurrentLine()
    {
        // 現在の行を取得
        string[] line = csvReader.GetLine(currentLine);


        // 発言者を表示
        ShowSpeaker(line[1]);
        // セリフを表示
        ShowMessage(line[2]);
        // デバッグを表示
        Debug.Log(line[0] + " : " + line[1] + " : " + line[2]);


        // 次の行へ
        currentLine++;
    }
    #endregion

    #region テキストを表示する

    /// <summary>
    /// 発言者を表示する
    /// </summary>
    void ShowSpeaker(string speaker)
    {
        // 名前が空欄なら地の文
        if (string.IsNullOrEmpty(speaker))
        {
            nameText.gameObject.SetActive(false);    // 名前欄を非表示
        }
        else
        {
            nameText.gameObject.SetActive(true);     // 名前欄を表示
            nameText.text = speaker;                 // 名前を表示
        }
    }

    /// <summary>
    /// セリフを表示する
    /// </summary>
    void ShowMessage(string message)
    {
        // スキップモード
        if (skipMode)
        {
            // セリフを一気に表示
            textTyper.ShowInstant(message);  // csvの3列目を読んでる
        }
        else
        {
            // セリフを一文字ずつ表示
            textTyper.StartTyping(message);  // csvの3列目を読んでる
        }
    }
    #endregion

    #region オートモード

    /// <summary>
    /// オートモードを設定する
    /// </summary>
    void SetAutoMode(bool enable)
    {
        // AutoをONにする場合
        if (enable)
        {
            // Skip中なら解除
            if (skipMode)
            {
                SetSkipMode(false);
            }
        }
        autoMode = enable;

        // Auto切替SE
        if (seManager != null)
        {
            seManager.PlayAutoSE(enable);
        }

        // オートタイマーリセット
        autoTimer = 0f;
        // オート待機解除
        autoWaiting = false;

        // オートON時かつ、すでに文字送り終了
        if (autoMode && !textTyper.IsTyping)
        {
            BeginNextLineWait();
        }

        Debug.Log("AutoMode : " + autoMode);
    }

    /// <summary>
    /// 待機を開始
    /// </summary>
    void BeginNextLineWait()
    {
        // オートモード/スキップモードOFFなら何もしない
        if (!autoMode && !skipMode)
            return;

        // タイマー開始
        autoWaiting = true;
        autoTimer = 0f;
    }
    #endregion

    #region スキップモード

    /// <summary>
    /// スキップモードを設定する
    /// </summary>
    void SetSkipMode(bool enable)
    {
        // すでに同じ状態なら何もしない
        if (skipMode == enable)
            return;

        // Skip開始
        if (enable)
        {
            // Auto中なら解除
            if (autoMode)
            {
                SetAutoMode(false);
            }
        }
        // スキップモード切り替え
        skipMode = enable;
        // Skip切替SE
        if (seManager != null)
        {
            seManager.PlaySkipSE(enable);
        }

        // タイマーリセット
        autoWaiting = false;
        autoTimer = 0f;

        if (skipMode)
        {
            // 文字送り中なら全文表示
            if (textTyper.IsTyping)
            {
                textTyper.Skip();
            }
            // 待機中なら即次へ
            else
            {
                BeginNextLineWait();
            }
        }
        // デバッグ
        Debug.Log("SkipMode : " + skipMode);
    }
    #endregion



    /// <summary>
    /// Spaceキー処理
    /// </summary>
    void HandleMessageInput()
    {
        // ウィンドウ非表示中
        if (!messageVisible)
        {
            // ウィンドウ表示
            SetMessageWindow(true);
        }
        // 文字送り中
        else if (textTyper.IsTyping)
        {
            // 全文表示
            textTyper.Skip(false);
        }
        // ウィンドウ表示中 かつ 全文表示済み
        else
        {
            // ウィンドウ非表示
            SetMessageWindow(false);
        }
    }

    #region ウィンドウ表示切替
    /// <summary>
    /// Messageウィンドウの表示切替
    /// </summary>
    void SetMessageWindow(bool enable)
    {
        messageVisible = enable;
        // ウィンドウ非表示
        if (messageWindow != null)
        {
            messageWindow.SetActive(enable);
        }

        // 表示した時はオート待機をリセット
        if (enable)
        {
            autoTimer = 0f;
        }
    }
    #endregion


    /// <summary>
    /// Auto / Skip解除
    /// </summary>
    void CancelAutoSkip()
    {
        // Auto解除
        if (autoMode)
        {
            SetAutoMode(false);
        }

        // Skip解除
        if (skipMode)
        {
            SetSkipMode(false);
        }
    }
}