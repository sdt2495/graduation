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

    private GamePhase currentPhase;

    public void Start()
    {
        ChangePhase(GamePhase.Dialogue);
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
    }

    private void StartBattle()
    {
        Debug.Log("戦闘開始");
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
}
