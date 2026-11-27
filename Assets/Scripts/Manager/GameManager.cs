using UnityEngine;

public enum GamePhase
{
    Dialogue,
    BattlePreparation,
    Battle,
    Upgrade,
    StageClear,
    GameOver
}

public class GameManager : MonoBehaviour
{
    [SerializeField] private BattleEnemySpawner enemySpawner;

    [Header("マネージャー設定")]
    [SerializeField] private StageManager stageManager;

    [SerializeField] private BattaleCommandManager commandManager;
    [SerializeField] private BattleManager battleManager;

    private GamePhase currentPhase;

    public void Start()
    {
        // PlayerStatsの存在確認と初期化
        PlayerStats playerStats = PlayerStats.Instance;

        if (playerStats == null)
        {
            GameObject manager = new GameObject("PlayerStatsManager");
            playerStats = manager.AddComponent<PlayerStats>();
        }

        playerStats.InitializeIfNeeded(50f);

        // StageManagerの参照を取得
        if (stageManager == null)
        {
            stageManager = StageManager.Instance;
        }

        if (stageManager == null)
        {
            Debug.LogError("StageManagerが見つかりません。");
            return;
        }

        ChangePhase(GamePhase.Dialogue);
    }

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

        // StageManagerから敵データを取得
        EnemyData enemyData = stageManager.GetCurrentEnemyData();

        if (enemyData == null)
            return;

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
        if (currentPhase != GamePhase.BattlePreparation)
            return;

        ChangePhase(GamePhase.Battle);
    }
}