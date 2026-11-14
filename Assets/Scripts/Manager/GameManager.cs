using System;
using UnityEngine;

public enum GamePhase
{
    Dialogue,               // 会話パート
    BattlePreparation,      // コマンド選択待機
    Battle,                 // 戦闘
    Upgrade,                // 強化パート
    StageClear,             // ステージクリア
    GameOver                // ゲームオーバー
}

public class GameManager : MonoBehaviour
{
    [SerializeField] private BattleEnemySpawner enemySpawner;

    [Header("ステージ設定")]
    [SerializeField] private EnemyData enemyData;

    [SerializeField] private BattaleCommandManager commandManager;
    [SerializeField] private BattleManager battleManager;

    private GamePhase currentPhase;

    public void Start()
    {
        ChangePhase(GamePhase.Dialogue);
    }

    /// <summary>
    /// デバック用に追加
    /// Enterキーで次のPHASEへ移行
    /// </summary>
    private void Update()
    {
        // デバッグ用：F1キーで準備画面へ移行
        if (currentPhase == GamePhase.Dialogue &&
            Input.GetKeyDown(KeyCode.F1))
        {
            ChangePhase(GamePhase.BattlePreparation);
        }
    }

    private void ChangePhase(GamePhase nextphase)
    {
        currentPhase = nextphase;

        switch (currentPhase)
        {
            case GamePhase.Dialogue:
                StartDialogue();
                break;

            case GamePhase.BattlePreparation:
                StartBattlePreparation();
                break;

            case GamePhase.Battle:
                StartBattle();
                break;

            case GamePhase.Upgrade:
                StartUpgrade();
                break;

            case GamePhase.StageClear:
                StartStageClear();
                break;

            case GamePhase.GameOver:
                StartGameOver();
                break;
        }
    }

    private void StartDialogue()
    {
        Debug.Log("会話パート開始");
    }

    private void StartBattlePreparation()
    {
        Debug.Log("戦闘準備開始");

        // 敵を生成
        enemySpawner.SpawnEnemy(enemyData);

        // コマンド選択画面を表示
        commandManager.ShowBattleCommandSelection();
    }

    private void StartBattle()
    {
        Debug.Log("戦闘開始");

        battleManager.StartBattle();
    }

    private void StartUpgrade()
    {
        Debug.Log("強化パート開始");
    }

    private void StartStageClear()
    {
        Debug.Log("ステージクリア");
    }

    private void StartGameOver()
    {
        Debug.Log("ゲームオーバー");
    }

    public void StartBattlePhase()
    {
        // 戦闘準備中の場合だけ戦闘を開始する
        if (currentPhase != GamePhase.BattlePreparation)
            return;

        ChangePhase(GamePhase.Battle);
    }
}
