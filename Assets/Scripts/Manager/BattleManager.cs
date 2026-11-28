using System.Collections;
using TMPro;
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
    [SerializeField] private GameObject timerUI;
    [Header("Timerの色設定")]
    [SerializeField] private Color noramlColor = Color.white;
    [SerializeField] private Color warningColor = Color.yellow;
    [SerializeField] private Color dangerColor = Color.red;

    [Header("TimerBar")]
    [SerializeField] private UnityEngine.UI.Image timerBarFill;

    [SerializeField] private BattaleCommandManager battleCommandManager;

    [Header("敵の攻撃演出")]
    [SerializeField] private ExclamationMark exclamationMark;

    [Header("EnemyヒントUI")]
    [SerializeField] private EnemyHintUI enemyHintUI;

    [Header("敵の攻撃(4方向の危険表示)")]
    [SerializeField] private DirectionImageController directionImageController;

    [SerializeField] private CommandUI commandUI;
    [SerializeField] private GameManager gameManager;

    // 制限時間
    [SerializeField] private float battleTimeLimit = 10.0f;
    private float reaminingTime;

    // Enemy攻撃終了待ち変数
    private bool isEnemyAttackFinished = false;

    private void Update()
    {
        if(!isBattle || isEndingBattle || !isTimerStarted) return;

        // プレイヤーが死亡していたらタイマーを停止
        if(player.IsDead())
        {
            PlayerDeath();
            return;
        }

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
        Debug.Log("BattleManager.StartBattle 呼び出し");
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
        // 攻撃終了フラグをリセット
        isEnemyAttackFinished = false;

        // Enemyの頭上アニメーションを開始
        if (exclamationMark != null)
        {
            exclamationMark.StartExclamation();
        }

        // EnemyのヒントUIを表示
        if (enemyHintUI != null)
        {
            enemyHintUI.ShowHint("敵の攻撃！");
        }

        // 4方向の攻撃演出を開始し、終了通知(OnEnemyAttackFinished)が来るまで待つ
        if (directionImageController != null)
        {
            directionImageController.StartEnemyAttack();

            while (!isEnemyAttackFinished)
            {
                yield return null;
            }
        }
        else
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

        // プレイヤーが倒れていたら戦闘終了
        if(player.IsDead())
        {
            EndBattle();
            yield break;
        }

        // 敵が倒れていなければ敵の攻撃処理
        Enemy battleEnemy = spawner.GetBattleEnemy();

        // 矢印入力UI・NEXT表示を非表示
        commandUI.SetCommandUIVisible(false);

        // タイマーUIを非表示
        timerUI.SetActive(false);

        if (battleEnemy != null && !battleEnemy.IsDead())
        {
            // Enemyの攻撃処理
            yield return StartCoroutine(EnemyAttack());
        }

        // 敵の攻撃後にプレイヤーが倒れた場合も考慮
        if(player.IsDead())
        {
            EndBattle();
            yield break;
        }

        // 一連の処理が終わったら戦闘終了
        EndBattle();

        // 入力途中で終了していた場合
        if (!battleEnemy.IsCommandFinished())
        {
            // NEXTを現在のコマンドに適用
            commandUI.ApplyNext(battleEnemy);

            // 新しいNEXTを生成
            commandUI.UpdateNextCommand();

            // UIを更新
            commandUI.UpdateCommanedText(battleEnemy);
        }

        // 次回の入力位置を初期化
        battleEnemy.ResetCommand();

        // コマンド選択画面へ戻る
        battleCommandManager.OnBattleEnded();
    }

    public void EndBattle()
    {
        isBattle = false;
    }


    /// <summary>
    /// コマンド入力成功時に敵へダメージを与える
    /// </summary>
    public void CompleteCommand()
    {
        Enemy battleEnemy = spawner.GetBattleEnemy();

        if (battleEnemy == null || battleEnemy.IsDead())
        {
            return;
        }

        if (PlayerStats.Instance == null ||
            !PlayerStats.Instance.IsInitialized)
        {
            Debug.LogError("PlayerStatsが存在しない、または初期化されていません。");
            return;
        }

        // プレイヤーATK
        float atk = PlayerStats.Instance.ATK;

        // 敵DEF
        float enemyDef = battleEnemy.Defense;

        // 現在の入力列の倍率
        float multiplier = battleEnemy.GetCurrentDamageMultiplier();

        // ダメージ計算
        int damage = ArrowDamageCalculator.CalculateDamage(
            atk,
            enemyDef,
            multiplier
        );

        Debug.Log(
            $"攻撃成功：ATK={atk:F1}, DEF={enemyDef:F1}, " +
            $"入力列倍率=×{multiplier:F2}, ダメージ={damage}"
        );

        battleEnemy.Damage(damage);

        if (battleEnemy.IsDead())
        {
            battleEnemy.Die();
            EnemyDeath();
        }
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
        Color currentColor;

        if(reaminingTime > 2f)
        {
            // 残り時間が2秒より多い場合は通常色
            currentColor = noramlColor;
        }
        else if(reaminingTime > 1f)
        {
            // 残り２秒から１秒にかけて黄色から赤色へ変化
            float t = 2f - reaminingTime;
            currentColor = Color.Lerp(warningColor, dangerColor, t);
        }
        else
        {
            // 残り１秒以下は赤色
            currentColor = dangerColor;
        }

        // テキストとバーの色を同期
        timerText.color = currentColor;
        timerBarFill.color = currentColor;

        // 残り時間に応じてバーを短くする
        timerBarFill.fillAmount = Mathf.Clamp01(reaminingTime / battleTimeLimit);
    }

    public void StartTimer()
    {
        if (!isBattle || isTimerStarted || isEndingBattle) return;

        isTimerStarted = true;
    }

    /// <summary>
    /// プレイヤー死亡時の処理
    /// </summary>
    private void PlayerDeath()
    {
        // 二重実行を防ぐ
        if(isEndingBattle) return ;

        isEndingBattle = true;

        // 入力とタイマーを停止
        player.SetCommandInput(false);
        isTimerStarted = false;

        // 戦闘終了
        EndBattle();

        // ゲームオーバー処理を呼ぶ
    }

    /// <summary>
    /// Enemy死亡処理
    /// </summary>
    private void EnemyDeath()
    {
        // 二重実行を防ぐ
        if (isEndingBattle) return;

        isEndingBattle = true;

        // 入力とタイマーを停止
        player.SetCommandInput(false);
        isTimerStarted = false;

        // 戦闘終了
        EndBattle();

        // 強化シーンへ移動
        gameManager.ChangeToUpgradePhase();
    }
}
