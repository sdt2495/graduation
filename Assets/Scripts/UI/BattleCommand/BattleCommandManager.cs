using UnityEngine;

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
