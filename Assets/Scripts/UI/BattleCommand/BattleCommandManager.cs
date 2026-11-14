using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class BattaleCommandManager : MonoBehaviour
{
    // ===================================
    // バトルコマンドの管理
    // ===================================
    
    [SerializeField] private CommandUI commandUI;
    [SerializeField] private BattleEnemySpawner spawner;
    [SerializeField] private Player player;
    [SerializeField] private BattleManager battleManager;
    [SerializeField] private GameObject battleCommandUI;
    [SerializeField] private GameObject timerUI;
    
    [Header("フレーム設定")]
    [SerializeField] private RectTransform selectFrame;
    [SerializeField] private float slideDuration = 0.15f;
    private Vector2 startFramePosition;
    private float slideTimer;
    private bool isSliding;

    [Header("選択ボタン")]
    [SerializeField] private Button[] commandButton;

    [Header("選択時の拡大設定")]
    [SerializeField] private float selectScale = 1.1f;
    private Vector3[] originalButtonScale;
    private Vector3 originalFrameScale;

    private int selectCommandIndex = 0;
    private Vector3 targetFramePosition;

    [Header("入力設定")]
    [SerializeField] private InputActionReference navigateAction;
    [SerializeField] private InputActionReference submitAction;

    [SerializeField] private GameManager gameManager;

    private void Start()
    {
        // 最初に選択するボタンを設定
        selectCommandIndex = 0;

        // ボタンの元の大きさを保存
        originalButtonScale = new Vector3[commandButton.Length];

        for(int i = 0; i < commandButton.Length; i++)
        {
            originalButtonScale[i] = commandButton[i].transform.localScale;
        }

        // フレームの元の大きさを保存
        originalFrameScale = selectFrame.localScale;

        // フレームの移動先を最初のボタンの位置にする
        RectTransform buttonRect = commandButton[selectCommandIndex].GetComponent<RectTransform>();
        targetFramePosition = new Vector3(buttonRect.anchoredPosition.x, buttonRect.anchoredPosition.y, selectFrame.localPosition.z);

        selectFrame.anchoredPosition = buttonRect.anchoredPosition;

        UpdateSelectionScale();
    }

    private void Update()
    {
        if(battleCommandUI == null || !battleCommandUI.activeSelf) return;
        if(commandButton == null || commandButton.Length == 0 || selectFrame == null ) return;

        if (navigateAction.action.WasPressedThisFrame())
        {
            Vector2 navigate = navigateAction.action.ReadValue<Vector2>();

            if (navigate.x < -0.5f)
            {
                MoveSelection(-1);
            }
            else if (navigate.x > 0.5f)
            {
                MoveSelection(1);
            }
        }

        if (submitAction.action.WasPressedThisFrame())
        {
            commandButton[selectCommandIndex].onClick.Invoke();
        }

        // 選択フレームを目標位置までイージングで移動させる
        if (isSliding)
        {
            slideTimer += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(slideTimer / slideDuration);

            // 最初は速く、終盤はゆっくり
            float easedT = 1f - Mathf.Pow(1f - t, 3f);

            selectFrame.anchoredPosition = Vector2.Lerp(startFramePosition, new Vector2(targetFramePosition.x, targetFramePosition.y), easedT);

            if (t >= 1f)
            {
                selectFrame.anchoredPosition = new Vector2(targetFramePosition.x, targetFramePosition.y);

                isSliding = false;
            }
        }
    }

    private void MoveSelection(int direction)
    {
        selectCommandIndex += direction;

        // 端を超えたかどうか
        bool wrapped = false;

        // 左右の端を超えたら反対側へ移動
        if(selectCommandIndex < 0)
        {
            selectCommandIndex = commandButton.Length - 1;
            wrapped = true;
        }
        else if(selectCommandIndex >= commandButton.Length)
        {
            selectCommandIndex = 0;
            wrapped = true;
        }

        RectTransform buttonRect = commandButton[selectCommandIndex].GetComponent<RectTransform>();

        targetFramePosition = new Vector3(buttonRect.anchoredPosition.x, buttonRect.anchoredPosition.y, buttonRect.localPosition.z);

        // 通常移動はカーブをつけてスライド
        startFramePosition = selectFrame.anchoredPosition;
        slideTimer = 0f;
        isSliding = true;

        UpdateSelectionScale();
    }

    /// <summary>
    /// 戦闘コマンド選択画面を表示する
    /// </summary>
    public void ShowBattleCommandSelection()
    {
        // 戦闘開始ボタンの画面を表示
        battleCommandUI.SetActive(true);

        // タイマーは非表示
        timerUI.SetActive(false);

        Enemy battleEnemy = spawner.GetBattleEnemy();

        // Enemyのコマンド位置を初期化
        battleEnemy.ResetCommand();

        // NEXTコマンドを生成
        commandUI.SetInitialNextCommands();

        // 現在のコマンドとNEXTコマンドをUIに反映
        commandUI.UpdateCommanedText(battleEnemy);

        // 矢印UIを表示
        commandUI.SetCommandUIVisible(true);

        // ボタン選択を初期化
        selectCommandIndex = 0;

        RectTransform buttonRect =
            commandButton[selectCommandIndex].GetComponent<RectTransform>();

        targetFramePosition = new Vector3(
            buttonRect.anchoredPosition.x,
            buttonRect.anchoredPosition.y,
            selectFrame.localPosition.z
        );
    }

    /// <summary>
    /// 戦闘ボタンの処理
    /// </summary>
    public void OnFightButton()
    {
        // 戦闘開始ボタンの画面を非表示
        battleCommandUI.SetActive(false);

        // タイマーを表示
        timerUI.SetActive(true);

        // プレイヤーの入力を有効化
        player.SetCommandInput(true);

        // 戦闘開始
        gameManager.StartBattlePhase();
    }

    private void UpdateSelectionScale()
    {
        for(int i = 0; i < commandButton.Length; i++)
        {
            // 選択中のボタンだけ大きくする
            float scale = (i == selectCommandIndex) ? selectScale : 1f;

            commandButton[i].transform.localScale = originalButtonScale[i] * scale;
        }

        // 選択フレームも同じ倍率で拡大
        selectFrame.localScale = originalFrameScale * selectScale;
    }

    /// <summary>
    /// 戦闘終了後の処理
    /// </summary>
    public void OnBattleEnded()
    {
        // コマンドUIを非表示
        commandUI.SetCommandUIVisible(false);

        // タイマーUIを非表示
        timerUI.SetActive(false);

        // 次の戦闘開始ボタンを表示
        battleCommandUI.SetActive(true);
    }
}
