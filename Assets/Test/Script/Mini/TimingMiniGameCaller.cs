
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.InputSystem;


public class TimingMiniGameCaller : MonoBehaviour
{
    // ==================================================
    // ミニゲームの種類
    // ==================================================

    private enum MiniGameType
    {
        Circular,
        VerticalGauge,
        ArrowInput
    }

    private const int MINI_GAME_TYPE_COUNT = 3;

    // ==================================================
    // 開始方法
    // ==================================================

    public enum StartMode
    {
        AutoStart,
        FirstArrowInput
    }

    // ==================================================
    // Inspector設定：ミニゲームPrefab
    // ==================================================

    [Header("円形ミニゲームPrefab")]
    [SerializeField]
    private GameObject timingMiniGamePrefab;

    [Header("4本の縦ゲージミニゲームPrefab")]
    [SerializeField]
    private VerticalGaugeManager verticalGaugeManagerPrefab;

    [Header("矢印入力ミニゲームPrefab")]
    [SerializeField]
    private GameObject arrowInputMiniGamePrefab;

    [Header("生成する親オブジェクト")]
    [SerializeField]
    private Transform miniGameParent;

    // ==================================================
    // Inspector設定：開始方法
    // ==================================================

    [Header("開始方法")]
    [Tooltip("AutoStart：ゲーム開始後に自動開始\nFirstArrowInput：矢印キー入力で開始")]
    [SerializeField]
    private StartMode startMode = StartMode.AutoStart;

    [Header("開始までの待ち時間")]
    [Tooltip("自動開始時、ミニゲームを開始するまでの待ち時間（秒）")]
    [SerializeField, Min(0f)]
    private float startDelay = 0f;

    // ==================================================
    // Inspector設定：カウントダウン
    // ==================================================

    [Header("カウントダウン設定")]
    [Tooltip("カウントダウンを表示するか")]
    [SerializeField]
    private bool showCountdown = false;

    [Tooltip("カウントダウンの開始数字")]
    [SerializeField, Min(1)]
    private int countdownStartNumber = 3;

    [Tooltip("数字を切り替える間隔（秒）")]
    [SerializeField, Min(0.1f)]
    private float countdownStepDuration = 1f;

    [Tooltip("カウントダウン表示用TextMeshPro")]
    [SerializeField]
    private TMP_Text countdownText;

    // ==================================================
    // Inspector設定：開始用矢印
    // ==================================================

    [Header("開始用矢印の設定")]
    [Tooltip("開始待ちのときに表示するTextMeshPro")]
    [SerializeField]
    private TMP_Text startArrowText;

    [Tooltip("開始待ちのときに表示する文字")]
    [SerializeField]
    private string startArrowSymbol = "▶";

    // ==================================================
    // Inspector設定：結果通知
    // ==================================================

    [Header("結果通知")]
    [SerializeField]
    private UnityEvent onClear;

    [SerializeField]
    private UnityEvent onMiss;

    // ==================================================
    // 内部状態
    // ==================================================

    private TimingMiniGame timingMiniGame;

    private VerticalGaugeManager verticalGaugeManagerInstance;

    private ArrowInputMiniGame1 arrowInputMiniGame;

    private MiniGameType currentMiniGameType;

    private bool waitingForResult;
    private bool resultNotified;

    // 開始演出中かどうか
    private bool isStarting;

    // 最初の矢印入力を受け取って開始処理に入ったか
    private bool isWaitingForArrowStart;

    // 開始演出用コルーチン
    private Coroutine startCoroutine;

    // ==================================================
    // 初期化
    // ==================================================

    private void Start()
    {
        // シーン開始時は開始用UIを非表示にする。
        // ミニゲームの開始はバトルマネージャーからの呼び出しに任せる。
        HideStartUI();
    }

    // ==================================================
    // 最初の矢印入力を監視
    // ==================================================

    private void Update()
    {
        // 矢印入力で開始するモード以外は監視しない
        if (startMode != StartMode.FirstArrowInput)
        {
            return;
        }

        // 開始演出中・プレイ中・開始処理中は受け付けない
        if (isStarting || isWaitingForArrowStart || waitingForResult)
        {
            return;
        }

        // キーボードが存在しない場合は何もしない
        if (Keyboard.current == null)
        {
            return;
        }

        // いずれかの矢印キーが押されたか確認
        bool arrowPressed =
            Keyboard.current.upArrowKey.wasPressedThisFrame ||
            Keyboard.current.downArrowKey.wasPressedThisFrame ||
            Keyboard.current.leftArrowKey.wasPressedThisFrame ||
            Keyboard.current.rightArrowKey.wasPressedThisFrame;

        if (!arrowPressed)
        {
            return;
        }

        // 二重受付を防ぐ
        isWaitingForArrowStart = true;

        // 開始用矢印を非表示にする
        if (startArrowText != null)
        {
            startArrowText.gameObject.SetActive(false);
        }

        // 次のフレームで開始し、開始キーの誤入力を防ぐ
        startCoroutine = StartCoroutine(StartAfterArrowInput());
    }

    // ==================================================
    // 開始演出の開始
    // ==================================================

    /// <summary>
    /// 自動開始用の開始演出を開始する。
    /// </summary>
    private void BeginStartSequence()
    {
        if (isStarting || waitingForResult)
        {
            return;
        }

        isStarting = true;
        startCoroutine = StartCoroutine(StartMiniGameRoutine());
    }

    /// <summary>
    /// 開始用矢印を表示する。
    /// </summary>
    private void ShowStartArrow()
    {
        if (startArrowText == null)
        {
            Debug.LogWarning(
                "TimingMiniGameCaller：開始用矢印Textが未設定です。",
                this);

            return;
        }

        startArrowText.text = startArrowSymbol;
        startArrowText.gameObject.SetActive(true);
    }

    // ==================================================
    // 自動開始コルーチン
    // ==================================================

    /// <summary>
    /// 待ち時間とカウントダウンの後にミニゲームを開始する。
    /// </summary>
    private IEnumerator StartMiniGameRoutine()
    {
        // 指定時間だけ待つ
        if (startDelay > 0f)
        {
            yield return new WaitForSecondsRealtime(startDelay);
        }

        // カウントダウンを表示する設定の場合
        if (showCountdown)
        {
            if (countdownText == null)
            {
                Debug.LogWarning(
                    "TimingMiniGameCaller：カウントダウン用Textが未設定です。",
                    this);
            }
            else
            {
                countdownText.gameObject.SetActive(true);

                // 3、2、1など、設定された数字からカウントする
                int startNumber = Mathf.Max(1, countdownStartNumber);

                for (int count = startNumber; count > 0; count--)
                {
                    countdownText.text = count.ToString();

                    yield return new WaitForSecondsRealtime(
                        Mathf.Max(0.1f, countdownStepDuration));
                }

                countdownText.gameObject.SetActive(false);
            }
        }

        // 念のため、開始演出用UIを非表示にする
        HideStartUI();

        // ランダムなミニゲームを開始する
        StartTimingMiniGame();

        isStarting = false;
        startCoroutine = null;
    }

    // ==================================================
    // 矢印入力後の開始コルーチン
    // ==================================================

    /// <summary>
    /// 開始に使用した矢印キーがミニゲーム側に渡らないよう、
    /// 次のフレームで開始する。
    /// </summary>
    private IEnumerator StartAfterArrowInput()
    {
        yield return null;

        // 開始演出用UIを非表示にする
        HideStartUI();

        // ランダムなミニゲームを開始する
        StartTimingMiniGame();

        isWaitingForArrowStart = false;
        startCoroutine = null;
    }

    // ==================================================
    // 開始演出用UIの非表示
    // ==================================================

    private void HideStartUI()
    {
        if (startArrowText != null)
        {
            startArrowText.gameObject.SetActive(false);
        }

        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
        }
    }

    // ==================================================
    // ミニゲーム開始
    // ==================================================

    /// <summary>
    /// 3種類のミニゲームからランダムに1つを開始する。
    /// 敵の攻撃時など、外部からも呼び出せる。
    /// </summary>
    public void StartTimingMiniGame()
    {
        Debug.Log("[TimingMiniGameCaller] ミニゲーム呼び出し", this);

        // 二重起動を防止
        if (waitingForResult)
        {
            Debug.LogWarning("ミニゲームはすでに実行中です。", this);
            return;
        }

        // 開始演出中の外部呼び出しを防止
        if (isStarting && startCoroutine == null)
        {
            Debug.LogWarning("ミニゲームの開始演出中です。", this);
            return;
        }

        // 生成先を確認
        if (miniGameParent == null)
        {
            Debug.LogError(
                "Mini Game Parentが設定されていません。",
                this);
            return;
        }

        // 前回の情報をリセット
        timingMiniGame = null;
        verticalGaugeManagerInstance = null;
        arrowInputMiniGame = null;

        resultNotified = false;

        // 3種類からランダムに選択
        currentMiniGameType =
            (MiniGameType)Random.Range(0, MINI_GAME_TYPE_COUNT);

        switch (currentMiniGameType)
        {
            case MiniGameType.Circular:
                StartCircularGame();
                break;

            case MiniGameType.VerticalGauge:
                StartVerticalGaugeGame();
                break;

            case MiniGameType.ArrowInput:
                StartArrowInputGame();
                break;
        }
    }

    // ==================================================
    // 円形ミニゲーム開始
    // ==================================================

    private void StartCircularGame()
    {
        if (timingMiniGamePrefab == null)
        {
            Debug.LogError(
                "円形ミニゲームPrefabが設定されていません。",
                this);
            return;
        }

        GameObject miniGameObject = Instantiate(
            timingMiniGamePrefab,
            miniGameParent,
            false);

        miniGameObject.SetActive(true);

        timingMiniGame =
            miniGameObject.GetComponent<TimingMiniGame>();

        if (timingMiniGame == null)
        {
            Debug.LogError(
                "円形ミニゲームPrefabにTimingMiniGameがありません。",
                miniGameObject);

            Destroy(miniGameObject);
            return;
        }

        // 円形ミニゲームを開始
        timingMiniGame.StartMiniGame();

        waitingForResult = true;

        Debug.Log(
            "[TimingMiniGameCaller] 円形ミニゲームを開始",
            this);
    }

    // ==================================================
    // 4本の縦ゲージミニゲーム開始
    // ==================================================

    private void StartVerticalGaugeGame()
    {
        if (verticalGaugeManagerPrefab == null)
        {
            Debug.LogError(
                "4本の縦ゲージミニゲームPrefabが設定されていません。",
                this);
            return;
        }

        GameObject miniGameObject = Instantiate(
            verticalGaugeManagerPrefab.gameObject,
            miniGameParent,
            false);

        miniGameObject.SetActive(true);

        verticalGaugeManagerInstance =
            miniGameObject.GetComponent<VerticalGaugeManager>();

        if (verticalGaugeManagerInstance == null)
        {
            Debug.LogError(
                "縦ゲージPrefabにVerticalGaugeManagerがありません。",
                miniGameObject);

            Destroy(miniGameObject);
            return;
        }

        // 4本の縦ゲージを開始
        if (!verticalGaugeManagerInstance.StartMiniGame())
        {
            Debug.LogError(
                "縦ゲージミニゲームを開始できませんでした。",
                miniGameObject);

            Destroy(miniGameObject);
            verticalGaugeManagerInstance = null;
            return;
        }

        waitingForResult = true;

        Debug.Log(
            "[TimingMiniGameCaller] 4本の縦ゲージを開始",
            this);
    }

    // ==================================================
    // 矢印入力ミニゲーム開始
    // ==================================================

    private void StartArrowInputGame()
    {
        if (arrowInputMiniGamePrefab == null)
        {
            Debug.LogError(
                "矢印入力ミニゲームPrefabが設定されていません。",
                this);
            return;
        }

        GameObject miniGameObject = Instantiate(
            arrowInputMiniGamePrefab,
            miniGameParent,
            false);

        miniGameObject.SetActive(true);

        arrowInputMiniGame =
            miniGameObject.GetComponent<ArrowInputMiniGame1>();

        if (arrowInputMiniGame == null)
        {
            Debug.LogError(
                "矢印入力PrefabにArrowInputMiniGame1がありません。",
                miniGameObject);

            Destroy(miniGameObject);
            return;
        }

        // 矢印入力ミニゲームを開始
        arrowInputMiniGame.StartArrowMiniGame();

        waitingForResult = true;

        Debug.Log(
            "[TimingMiniGameCaller] 矢印入力ミニゲームを開始",
            this);
    }

    // ==================================================
    // 結果確認
    // ==================================================

    private void LateUpdate()
    {
        // 結果待ち中でなければ何もしない
        if (!waitingForResult || resultNotified)
        {
            return;
        }

        bool isFinished = false;
        bool isClear = false;
        GameObject finishedGameObject = null;

        // ----------------------------------------------
        // 円形ミニゲーム
        // ----------------------------------------------

        if (currentMiniGameType == MiniGameType.Circular)
        {
            if (timingMiniGame == null)
            {
                HandleMissingMiniGame("円形ミニゲーム");
                return;
            }

            isFinished = timingMiniGame.IsFinished();

            if (isFinished)
            {
                isClear = timingMiniGame.IsClear();
                finishedGameObject = timingMiniGame.gameObject;
            }
        }

        // ----------------------------------------------
        // 4本の縦ゲージミニゲーム
        // ----------------------------------------------

        else if (currentMiniGameType == MiniGameType.VerticalGauge)
        {
            if (verticalGaugeManagerInstance == null)
            {
                HandleMissingMiniGame("縦ゲージミニゲーム");
                return;
            }

            isFinished = verticalGaugeManagerInstance.IsFinished();

            if (isFinished)
            {
                isClear = verticalGaugeManagerInstance.IsClear();

                finishedGameObject =
                    verticalGaugeManagerInstance.gameObject;
            }
        }

        // ----------------------------------------------
        // 矢印入力ミニゲーム
        // ----------------------------------------------

        else if (currentMiniGameType == MiniGameType.ArrowInput)
        {
            if (arrowInputMiniGame == null)
            {
                HandleMissingMiniGame("矢印入力ミニゲーム");
                return;
            }

            isFinished = arrowInputMiniGame.IsFinished;

            if (isFinished)
            {
                isClear = arrowInputMiniGame.IsClear;

                finishedGameObject =
                    arrowInputMiniGame.gameObject;
            }
        }

        // まだ終了していなければ待つ
        if (!isFinished)
        {
            return;
        }

        // 結果の二重通知を防止
        resultNotified = true;
        waitingForResult = false;

        // ----------------------------------------------
        // 成功・失敗を通知
        // ----------------------------------------------

        if (isClear)
        {
            Debug.Log(
                "[TimingMiniGameCaller] ミニゲーム成功！",
                this);

            onClear?.Invoke();
        }
        else
        {
            Debug.LogWarning(
                "[TimingMiniGameCaller] ミニゲーム失敗！",
                this);

            onMiss?.Invoke();
        }

        // 終了したミニゲームを非アクティブにする
        if (finishedGameObject != null)
        {
            finishedGameObject.SetActive(false);
        }

        // 次回の起動に備えて参照を解除
        timingMiniGame = null;
        verticalGaugeManagerInstance = null;
        arrowInputMiniGame = null;

        Debug.Log(
            "[TimingMiniGameCaller] ミニゲームを非アクティブにしました。",
            this);
    }

    // ==================================================
    // ミニゲームが見つからない場合
    // ==================================================

    private void HandleMissingMiniGame(string miniGameName)
    {
        Debug.LogError(
            $"[TimingMiniGameCaller] {miniGameName}が見つかりません。",
            this);

        waitingForResult = false;
    }
}
