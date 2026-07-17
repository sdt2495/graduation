using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// ノベルゲーム全体の進行を管理するクラス
/// </summary>
public class NovelManager : MonoBehaviour
{
    #region CSV列番号 (あとでcsvReaderに移動?)
    private const int COL_ID = 0;                // ID
    private const int COL_SPEAKER = 1;           // 発言者
    private const int COL_MESSAGE = 2;           // セリフ

    private const int COL_CHARACTER = 3;         // 立ち絵:表示画像
    private const int COL_POSITION = 4;          // 立ち絵:表示位置
    private const int COL_FLIP = 5;              // 立ち絵:反転

    private const int COL_VOICE = 6;             // VOICE再生
    private const int COL_SE = 7;                // SE再生
    private const int COL_BGM = 8;               // BGM再生
    private const int COL_AMBIENT = 9;           // 環境音再生

    private const int COL_BG = 10;               // BG画像
    private const int COL_BG_EFFECT = 11;        // BG画像_演出
    private const int COL_CG = 12;               // CG画像
    private const int COL_CG_EFFECT = 13;        // CG画像_演出
    private const int COL_SCREEN = 14;           // 画面エフェクト色
    private const int COL_SCREEN_EFFECT = 15;    // 画面エフェクト演出
    #endregion

    [Header("csvReader")]
    [SerializeField] private CSVReader csvReader;                            // CSVを読み込むスクリプト
    // ★ テキスト ★
    [Header("TextTyper")]
    [SerializeField] private TextTyper textTyper;                            // 一文字ずつ表示するスクリプト

    [Header("NovelSystemSEManager")]
    [SerializeField] private NovelSystemSEManager systemSEManager;           // システムSEを再生するスクリプト

    [Header("BackLogManager")]
    [SerializeField] private BackLogManager backLogManager;                  // ログを遡るスクリプト
    // ★オーディオ ★
    [Header("NovelVoiceManager")]
    [SerializeField] private NovelVoiceManager voiceManager;                 // 音声を再生するスクリプト

    [Header("NovelSEManager")]
    [SerializeField] private NovelSEManager seManager;                       // SEを再生するスクリプト
    
    [Header("NovelBGMManager")]
    [SerializeField] private NovelBGMManager bgmManager;                     // BGMを再生するスクリプト

    [Header("NovelAmbientManager")]
    [SerializeField] private NovelAmbientManager ambientManager;             // 環境音を再生するスクリプト
    // ★イメージ ★
    [Header("NovelBackgroundManager")]
    [SerializeField] private NovelBackgroundManager backgroundManager;       // BGを表示するスクリプト

    [Header("NovelCGManager")]
    [SerializeField] private NovelCGManager cgManager;                       // CGを表示するスクリプト

    [Header("NovelScreenEffectManager")]
    [SerializeField] private NovelScreenEffectManager screenEffectManager;   // スクリーンエフェクトを表示するスクリプト


    [Header("──────────────────────────────")]
    [Header("名前表示")]
    [SerializeField] private TextMeshProUGUI nameText;   // 発言者の名前

    [Header("メッセージUI (全体)")]
    [SerializeField] private GameObject messageUI;   // メッセージUIまとめて

    [Header("ウィンドウの枠")]
    [SerializeField] private Image messageWindowImage;
    [SerializeField, Range(0f, 1f)] private float messageWindowAlpha = 0.7f;

    [Header("次へマーク")]
    [SerializeField] private GameObject nextMark;

    [Header("ボタン")]
    [SerializeField] private UnityEngine.UI.Button autoButton;
    [SerializeField] private UnityEngine.UI.Button skipButton;
    [SerializeField] private UnityEngine.UI.Button backLogButton;
    [SerializeField] private UnityEngine.UI.Button messageButton;

    [Header("ボタン色")]
    [SerializeField] private Color normalButtonColor = Color.white;
    [SerializeField] private Color activeButtonColor = Color.green;

    [Header("──────────────────────────────")]
    [Header("オートモード待機時間")]
    [SerializeField] private float autoWaitTime = 1.5f;  // オートモード待機時間

    [Header("音声の終了を待つか？")]
    [SerializeField] private bool waitVoiceEnd = true;
    
    [Header("──────────────────────────────")]
    [Header("スキップモード待機時間")]
    [SerializeField] private float skipWaitTime = 0.1f;  // スキップモード待機時間


    private float autoTimer = 0f;        // オートモードタイマー
    private bool autoWaiting = false;    // オートタイマーが動いているか

    private int currentLine = 0;         // 現在読んでいる行番号

    private bool messageVisible = true;  // ウィンドウの表示

    private bool waitingVoice = false;   // 音声待ち

    private int transitionCount = 0;     // 現在実行中の演出数

    #region プロパティ
    public bool IsAutoMode => playMode == PlayMode.Auto;
    public bool IsSkipMode => playMode == PlayMode.Skip;
    #endregion


    #region 状態遷移

    /// <summary>
    /// 現在の画面状態 (状態遷移)
    /// </summary>
    private enum NovelState
    {
        Normal,    // 通常
        BackLog    // バックログ表示中       
    }
    private NovelState state = NovelState.Normal;    // 現在の状態

    /// <summary>
    /// メッセージ送りモード
    /// </summary>
    private enum PlayMode
    {
        Normal,
        Auto,
        Skip
    }
    private PlayMode playMode = PlayMode.Normal;     // 現在のモード
    #endregion


    #region イベント登録 +α
    void Start()
    {
        // メッセージウィンドウ透明度設定
        SetMessageWindowAlpha();

        // イベント登録 (TextTyperの終了イベントを受け取る)
        textTyper.OnTypingFinished += OnTypingFinished;

        // ボタン表示更新
        UpdateButtonView();

        //「次へ」アイコン
        UpdateNextMark();
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
        //「次へ」アイコン
        UpdateNextMark();

        // 待機開始
        BeginNextLineWait();
    }
    #endregion


    void Update()
    {
        // 状態遷移
        switch (state)
        {
            case NovelState.Normal:

                UpdateInput();
                UpdateAutoMode();

                break;

            case NovelState.BackLog:

                UpdateBackLogInput(); // バックログ中の操作
                break;
        }

    }


    #region Updateを分割

    /// <summary>
    /// プレイヤーからの入力を処理する
    /// </summary>
    void UpdateInput()
    {
        // UIクリック中はゲーム側の入力を受け付けない
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        // [!] 操作キーは仮 //

        // [左クリック]
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.DownArrow) || Input.mouseScrollDelta.y < 0)
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

        // [Aキー] オートモード
        if (Input.GetKeyDown(KeyCode.A))
        {
            OnClickAutoButton();
        }

        // [Ctrlキー] スキップモード
        if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl))
        {
            OnClickSkipButton();
        }

        // [スペース] ウィンドウ非表示
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(1))
        {
            OnClickMessageWindowButton();
        }

        // [↑]　バックログ
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.mouseScrollDelta.y > 0)
        {
            OnClickBackLogButton();
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
        if ((!IsAutoMode && !IsSkipMode) || !autoWaiting)
            return;

        // Auto時のみボイス待ち
        if (IsAutoMode && waitVoiceEnd)
        {
            if (voiceManager != null && voiceManager.IsPlayingVoice)
            {
                waitingVoice = true;
                return;
            }

            // ボイス終了した瞬間
            if (waitingVoice)
            {
                waitingVoice = false;
                autoTimer = 0f;
            }
        }

        // タイマーを進める
        autoTimer += Time.deltaTime;

        // スキップモード中なら skipWaitTime、falseならautoWaitTime (条件 ? 条件がtrueの場合 : 条件がfalseの場合)
        float waitTime = playMode == PlayMode.Skip ? skipWaitTime : autoWaitTime;

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
        //「次へ」アイコン
        UpdateNextMark();

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
        systemSEManager?.PlayNextLineSE();
        
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
        ShowSpeaker(line[COL_SPEAKER]);
        // セリフを表示
        ShowMessage(line[COL_MESSAGE]);

        // ボイス再生
        voiceManager?.PlayVoice(line[COL_VOICE]);
        // SE再生
        seManager?.PlaySE(line[COL_SE]);
        // BGM再生
        bgmManager?.PlayBGM(line[COL_BGM]);
        // 環境音再生
        ambientManager?.PlayAmbient(line[COL_AMBIENT]);

        // BG表示の方法を取得 (誤字ならInstance表示)
        if (!System.Enum.TryParse(line[COL_BG_EFFECT], true, out TransitionType bgTransition))
        {
            bgTransition = TransitionType.Instant;
        }
        // BG表示
        backgroundManager?.ChangeBackground(line[COL_BG], bgTransition);

        // CG表示の方法を取得 (誤字ならInstance表示)
        if (!System.Enum.TryParse(line[COL_CG_EFFECT], true, out TransitionType cgTransition))
        {
            cgTransition = TransitionType.Instant;
        }
        // CG表示
        if (line[COL_CG] == "NONE")
        {
            // 非表示
            cgManager?.HideCG(cgTransition);
        }
        else
        {
            // 表示
            cgManager?.ShowCG(line[COL_CG], cgTransition);
        }

        // 画面エフェクト表示の方法を取得 (誤字ならInstance表示)
        if (!System.Enum.TryParse(line[COL_SCREEN_EFFECT], true, out TransitionType screenTransition))
        {
            screenTransition = TransitionType.Instant;
        }
        // 画面エフェクト表示
        switch (line[COL_SCREEN])
        {
            case "BLACK":
                screenEffectManager?.Show(Color.black, screenTransition);
                break;

            case "WHITE":
                screenEffectManager?.Show(Color.white, screenTransition);
                break;

            case "NONE":
                screenEffectManager?.Hide(screenTransition);
                break;
        }


        // デバッグを表示
        Debug.Log(line[COL_ID] + " : " + line[COL_SPEAKER] + " : " + line[COL_MESSAGE]);


        // 次の行へ
        currentLine++;

        // こういうこと？
        backLogManager.AddLog(line[COL_SPEAKER], line[COL_MESSAGE]);
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
        //「次へ」アイコン
        UpdateNextMark();

        // スキップモード
        if (IsSkipMode)
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


    #region オートモード と スキップモード

    /// <summary>
    /// オートモードを設定する
    /// </summary>
    void SetAutoMode(bool enable)
    {
        // すでに同じ状態なら何もしない
        if (IsAutoMode == enable)
            return;

        // AutoをONにする場合はNormal状態でしかONにできない
        if (enable && playMode != PlayMode.Normal)
            return;

        // モード変更
        ChangePlayMode(enable ? PlayMode.Auto : PlayMode.Normal);

        // ボタン表示更新
        UpdateButtonView();

        //「次へ」アイコン
        UpdateNextMark();

        // Auto切替SE
        systemSEManager?.PlayAutoSE(enable);

        // オートON時かつ、すでに文字送り終了
        if (enable && !textTyper.IsTyping)
        {
            BeginNextLineWait();
        }
    }

    /// <summary>
    /// スキップモードを設定する
    /// </summary>
    void SetSkipMode(bool enable)
    {
        // すでに同じ状態なら何もしない
        if (IsSkipMode == enable)
            return;

        // SkipをONにする場合はNormal状態でしかONにできない
        if (enable && playMode != PlayMode.Normal)
            return;

        // モード変更
        ChangePlayMode(enable ? PlayMode.Skip : PlayMode.Normal);

        // ボタン表示更新
        UpdateButtonView();

        //「次へ」アイコン
        UpdateNextMark();

        // Skip切替SE
        systemSEManager?.PlaySkipSE(enable);

        if (enable)
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
    }

    /// <summary>
    /// プレイモード変更
    /// </summary>
    void ChangePlayMode(PlayMode mode)
    {
        playMode = mode;

        autoWaiting = false;
        autoTimer = 0f;

        Debug.Log(playMode);
    }


    /// <summary>
    /// 待機を開始
    /// </summary>
    void BeginNextLineWait()
    {
        // オートモード・スキップモードOFFなら何もしない
        if (playMode == PlayMode.Normal)
            return;

        // タイマー開始
        autoWaiting = true;
        autoTimer = 0f;
    }

    /// <summary>
    /// Auto / Skip解除
    /// </summary>
    void CancelAutoSkip()
    {
        switch (playMode)
        {
            case PlayMode.Auto:
                SetAutoMode(false);
                break;

            case PlayMode.Skip:
                SetSkipMode(false);
                break;
        }
    }
    #endregion


    #region メッセージUIの表示切替

    /// <summary>
    /// メッセージUI表示切替入力処理
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


    /// <summary>
    /// Messageウィンドウの表示切替
    /// </summary>
    void SetMessageWindow(bool enable)
    {
        messageVisible = enable;

        // ウィンドウ非表示
        if (messageUI != null)
        {
            messageUI.SetActive(enable);
        }

        //「次へ」アイコン (一応)
        UpdateNextMark();

        // SE再生
        if (systemSEManager != null)
        {
            if (enable)
            {
                systemSEManager.PlayMessageShowSE();
            }
            else
            {
                systemSEManager.PlayMessageHideSE();
            }
        }

        // 表示した時はオート待機をリセット
        if (enable)
        {
            autoTimer = 0f;
        }
    }


    /// <summary>
    /// 演出開始
    /// </summary>
    public void BeginTransition()
    {
        // 現在実行中の演出数+１
        transitionCount++;

        if (messageUI != null)
        {
            // メッセージUI非表示
            messageUI.SetActive(false);
        }
    }
    /// <summary>
    /// 演出終了
    /// </summary>
    public void EndTransition()
    {
        // 現在実行中の演出数-１
        transitionCount--;
        // マイナス防止
        if (transitionCount < 0)
        {
            transitionCount = 0;
        }

        // 全演出終了
        if (transitionCount == 0)
        {
            if (messageUI != null)
            {
                // メッセージUI表示
                messageUI.SetActive(true);
            }
        }
    }


    /// <summary>
    /// メッセージウィンドウ(枠のみ)の透明度設定
    /// </summary>
    void SetMessageWindowAlpha()
    {
        if (messageWindowImage == null)
            return;

        Color color = messageWindowImage.color;
        color.a = messageWindowAlpha;
        messageWindowImage.color = color;
    }
    #endregion


    #region「次へ」アイコン(NextMark)の表示切替

    /// <summary>
    ///「次へ」アイコンの表示更新
    /// </summary>
    void UpdateNextMark()
    {
        if (nextMark == null)
            return;

        // 表示する条件 (全てtrueなら表示)
        bool visible =
            !textTyper.IsTyping &&   // 文字送り終了
            !IsAutoMode &&           // Autoじゃない
            !IsSkipMode &&           // Skipじゃない
            messageVisible;          // メッセージウィンドウ表示中

        //「次へ」アイコンの表示
        nextMark.SetActive(visible);
    }
    #endregion


    #region バックログでの入力操作

    /// <summary>
    /// バックログを開く
    /// </summary>
    void OpenBackLog()
    {
        // 状態変更
        state = NovelState.BackLog;

        // SE再生
        systemSEManager?.PlayBackLogOpenSE();

        // バックログを開く
        backLogManager.Open();
    }

    /// <summary>
    /// バックログを閉じる
    /// </summary>
    void CloseBackLog()
    {
        // 状態変更
        state = NovelState.Normal;
        
        // SE再生
        systemSEManager?.PlayBackLogCloseSE();

        // バックログを閉じる
        backLogManager.Close();
    }

    /// <summary>
    /// バックログでの入力操作
    /// </summary>
    void UpdateBackLogInput()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(1))
        {
            // バックログを閉じる
            CloseBackLog();
        }
    }
    #endregion


    #region UIボタン

    /// <summary>
    /// ボタンの見た目を更新
    /// </summary>
    void UpdateButtonView()
    {
        // ボタンの色
        autoButton.image.color = IsAutoMode ? activeButtonColor : normalButtonColor;
        skipButton.image.color = IsSkipMode ? activeButtonColor : normalButtonColor;

        // 押せるかどうか
        autoButton.interactable = !IsSkipMode;
        skipButton.interactable = !IsAutoMode;
    }

    // ────────────────────

    /// <summary>
    /// Autoボタン
    /// </summary>
    public void OnClickAutoButton()
    {
        SetAutoMode(!IsAutoMode);
    }

    /// <summary>
    /// Skipボタン
    /// </summary>
    public void OnClickSkipButton()
    {
        SetSkipMode(!IsSkipMode);
    }

    /// <summary>
    /// メッセージウィンドウ表示切替ボタン
    /// </summary>
    public void OnClickMessageWindowButton()
    {
        CancelAutoSkip();
        HandleMessageInput();
    }

    /// <summary>
    /// BackLogボタン
    /// </summary>
    public void OnClickBackLogButton()
    {
        // Auto・Skip中は開けない
        if (playMode != PlayMode.Normal)
            return;

        OpenBackLog();
    }

    /// <summary>
    /// BackLogを閉じるボタン
    /// </summary>
    public void OnClickCloseBackLogButton()
    {
        CloseBackLog();
    }

    #endregion
}