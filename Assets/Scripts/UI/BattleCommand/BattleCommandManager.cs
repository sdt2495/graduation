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
     
    /// <summary>
    /// 戦闘ボタンの処理
    /// </summary>
    public void OnFightButton()
    {
        battleManager.StartBattle();

        Enemy battleEnemy = spawner.GetBattleEnemy();

        battleEnemy.ResetCommand();

        commandUI.gameObject.SetActive(true);

        commandUI.UpdateCommanedText(battleEnemy, null);

        player.SetCommandInput(true);
    }
}
