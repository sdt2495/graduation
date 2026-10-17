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

        // 選択フレームを目標位置までイージングで移動させる
        if(isSliding)
        {
            slideTimer += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(slideTimer / slideDuration);

            // 最初は速く、終盤はゆっくり
            float easedT = 1f - Mathf.Pow(1f - t, 3f);

            selectFrame.anchoredPosition = Vector2.Lerp(startFramePosition, new Vector2(targetFramePosition.x, targetFramePosition.y), easedT);

            if(t >= 1f)
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

        // 端を超えたときだけ即座に移動
        //if(wrapped)
        //{
        //    selectFrame.anchoredPosition = new Vector2(targetFramePosition.x, targetFramePosition.y);
        //    isSliding = false;
        //}
        //else
        //{
        //    // 通常移動はカーブをつけてスライド
        //    startFramePosition = selectFrame.anchoredPosition;
        //    slideTimer = 0f;
        //    isSliding = true;
        //}

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
}
