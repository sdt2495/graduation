using System;
using UnityEngine;
using UnityEngine.UI;

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
    
    [Header("フレーム設定")]
    [SerializeField] private RectTransform selectFrame;
    [SerializeField] private float slideSpeed = 1000f;

    [Header("選択ボタン")]
    [SerializeField] private Button[] commandButton;

    private int selectCommandIndex = 0;
    private Vector3 targetFramePosition;

    private void Start()
    {
        // 最初に選択するボタンを設定
        selectCommandIndex = 0;

        // フレームの移動先を最初のボタンの位置にする
        RectTransform buttonRect = commandButton[selectCommandIndex].GetComponent<RectTransform>();
        targetFramePosition = new Vector3(buttonRect.anchoredPosition.x, buttonRect.anchoredPosition.y, selectFrame.localPosition.z);
    }

    private void Update()
    {
        if(battleCommandUI == null || !battleCommandUI.activeSelf) return;
        if(commandButton == null || commandButton.Length == 0 || selectFrame == null ) return;

        if(Input.GetKeyDown(KeyCode.LeftArrow))
        {
            MoveSelection(-1);
        }
        else if(Input.GetKeyDown(KeyCode.RightArrow))
        {
            MoveSelection(1);
        }

        if(Input.GetKeyDown(KeyCode.Space))
        {
            commandButton[selectCommandIndex].onClick.Invoke();
        }

        // 選択フレームを目標位置まで滑らかに移動させる
        selectFrame.anchoredPosition = Vector3.MoveTowards(selectFrame.anchoredPosition, new Vector2(targetFramePosition.x, targetFramePosition.y), slideSpeed * Time.unscaledDeltaTime);
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
    
        // 端を超えたときだけ即座に移動
        if(wrapped)
        {
            selectFrame.anchoredPosition = new Vector2(targetFramePosition.x, targetFramePosition.y);
        }
    }

    /// <summary>
    /// 戦闘コマンド選択画面を表示する
    /// </summary>
    public void ShowBattleCommandSelection()
    {
        Enemy battleEnemy = spawner.GetBattleEnemy();

        battleCommandUI.SetActive(true);
        battleEnemy.ResetCommand();
        commandUI.SetInitialNextCommands();
        commandUI.UpdateCommanedText(battleEnemy);

        selectCommandIndex = 0;

        RectTransform buttonRect = commandButton[selectCommandIndex].GetComponent<RectTransform>();

        targetFramePosition = new Vector3(buttonRect.anchoredPosition.x, buttonRect.anchoredPosition.y, selectFrame.localPosition.z);
    }
     
    /// <summary>
    /// 戦闘ボタンの処理
    /// </summary>
    public void OnFightButton()
    {
        // コマンド選択UIを非表示にする
        battleCommandUI.SetActive(false);

        // 戦闘開始
        battleManager.StartBattle();

        Enemy battleEnemy = spawner.GetBattleEnemy();
        battleEnemy.ResetCommand();

        commandUI.SetInitialNextCommands();

        commandUI.UpdateCommanedText(battleEnemy);

        player.SetCommandInput(true);
    }
}
