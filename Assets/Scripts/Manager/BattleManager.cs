using System.Collections;
using TMPro;
using UnityEditor;
using UnityEngine;
using static TimingMiniGame;

public class BattleManager : MonoBehaviour
{
    [SerializeField] private Enemy enemy;
    [SerializeField] private Player player;
    [SerializeField] private BattleEnemySpawner spawner;

    [Header("タイミングミニゲーム")]
    [SerializeField] private TimingMiniGameCaller timingMiniGameCaller;
    
    private bool isBattle = false;
    private bool isEndingBattle = false;
    private bool isTimerStarted = false;

    [Header("TimerUI")]
    [SerializeField] private TMP_Text timerText;

    [SerializeField] private BattaleCommandManager battleCommandManager;

    // 制限時間
    [SerializeField] private float battleTimeLimit = 10.0f;
    private float reaminingTime;

    // Enemy攻撃終了待ち変数
    private bool isEnemyAttackFinished = false;

    private void Update()
    {
        if(!isBattle || isEndingBattle || !isTimerStarted) return;

        reaminingTime -= Time.deltaTime;

        if( reaminingTime < 0 )
        {
            reaminingTime = 0;
        }

        UpdateTimerText();

        if (reaminingTime <= 0f)
        {
            reaminingTime = 0f;
            isEndingBattle = true;

            StartCoroutine(EndBattleSequence());
        }
    }

    public bool IsBattle()
    {
        return isBattle;
    }

    public void StartBattle()
    {
        isBattle = true;
        isEndingBattle = false;
        isTimerStarted = false;

        reaminingTime = battleTimeLimit;

        UpdateTimerText();
    }

    /// <summary>
    /// Enemyの攻撃処理をここで呼ぶ
    /// </summary>
    /// <returns></returns>
    private IEnumerator EnemyAttack()
    {
        isEnemyAttackFinished = false;
        timingMiniGameCaller.StartTimingMiniGame();
        
        // ミニゲームが終わるまで待つ
        while(!isEnemyAttackFinished)
        {
            yield return null;
        }
    }

    /// <summary>
    /// プレイヤー攻撃終了後、Enemy攻撃へ
    /// 全てが終了後バトルを終わらせる
    /// </summary>
    /// <returns></returns>
    private IEnumerator EndBattleSequence()
    {
        // プレイヤーのコマンド入力停止
        player.SetCommandInput(false);

        // プレイヤー側の攻撃処理が終わるまで待つ
        while(player.IsCommandAnimation())
        {
            yield return null;
        }

        // Enemyの攻撃処理
        yield return StartCoroutine(EnemyAttack());

        // 一連の処理が終わったら戦闘終了
        EndBattle();

        // コマンド選択画面へ戻る
        battleCommandManager.ShowBattleCommandSelection();
    }

    public void EndBattle()
    {
        isBattle = false;
    }
    
    /// <summary>
    /// コマンド入力に成功時敵にダメージを与える
    /// </summary>
    public void CompleteCommand()
    {
        Enemy enemy = spawner.GetBattleEnemy();

        enemy.Damage(1);
    }

    /// <summary>
    /// Enemyの攻撃終了を通知する
    /// </summary>
    public void OnEnemyAttackFinished()
    {
        isEnemyAttackFinished = true;
    }

    /// <summary>
    /// 残り時間をTimerUIに表示する
    /// </summary>
    private void UpdateTimerText()
    {
        timerText.text = reaminingTime.ToString("F2");
    }

    public void StartTimer()
    {
        if (!isBattle || isTimerStarted || isEndingBattle) return;

        isTimerStarted = true;
    }
}
