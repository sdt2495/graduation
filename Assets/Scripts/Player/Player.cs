using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] private Enemy enemy;
    [SerializeField] private SpriteRenderer renderer;

    [Header("HP")]
    [SerializeField] private int maxHP = 10;
    private int curretHP;

    [Header("コンボ")]
    [SerializeField] private ComboUI commboUI;
    [SerializeField] private ComboBar comboBar;
    private int commbo = 0;

    [Header("コマンドUI")]
    [SerializeField] private CommandUI commandUI;

    [Header("バトルマネージャー")]
    [SerializeField] private BattleManager battleManager;

    private bool isCommandAnimation = false;
    private bool isCommandInput = false;
    private bool isCommandFailed = false;

    [Header("入力設定")]
    [SerializeField] private InputActionReference moveAction;

    // 外部参照用
    public int CurrentHP { get { return curretHP; } }
    public int MaxHP { get { return maxHP; } }

    private void Start()
    {
        curretHP = maxHP;

        commboUI.UpdateCombo(commbo);
        comboBar.UpdateComboBar(commbo);
    }

    private void Update()
    {
        if (enemy == null)
        {
            return;
        }

        if (isCommandAnimation)
        {
            return;
        }

        if(IsDead())
        {
            isCommandInput = false;
            return;
        }

        if(!isCommandInput)
        {
            return;
        }

        if(moveAction.action.WasPressedThisFrame())
        {
            Vector2 input = moveAction.action.ReadValue<Vector2>();

            if(Mathf.Abs(input.x) > Mathf.Abs(input.y))
            {
                if(input.x < -0.5f)
                {
                    CheckCommaned(CommandType.Left);
                }
                else if(input.x > 0.5f)
                {
                    CheckCommaned(CommandType.Right);
                }
            }
            else
            {
                if(input.y > 0.5f)
                {
                    CheckCommaned(CommandType.Up);
                }
                else if(input.y < -0.5f)
                {
                    CheckCommaned(CommandType.Down);
                }
            }
        }
    }

    public void SetEnemy(Enemy newEnemy)
    {
        enemy = newEnemy;
    }

    /// <summary>
    /// 戦闘コマンドが選択されているか
    /// </summary>
    /// <param name="active"></param>
    public void SetCommandInput(bool active)
    {
        isCommandInput = active;
    }

    private void CheckCommaned(CommandType command)
    {
        battleManager.StartTimer();

        CheckResult result = enemy.Check(command);

        switch (result)
        {
            case CheckResult.Success:

                // Enemyにダメージ
                battleManager.CompleteCommand();

                // 成功した矢印を水色にする
                commandUI.SetSuccessComand(enemy.GetCurrentIndex() - 1);

                commbo++;
                commboUI.UpdateCombo(commbo);
                comboBar.UpdateComboBar(commbo);

                commandUI.UpdateActiveComand(enemy.GetCurrentIndex());
                break;

            case CheckResult.Complete:

                // Enemyにダメージ
                battleManager.CompleteCommand();

                // 最後の矢印も成功状態にする
                commandUI.SetSuccessComand(enemy.GetCurrentIndex() - 1);

                StartCoroutine(CompleteCommand());
                break;

            case CheckResult.Miss:
                isCommandFailed = true;
                StartCoroutine(MissCommand());
                break;
        }
    }

    /// <summary>
    /// 最後のコマンド入力に成功したときのアニメーション
    /// </summary>
    /// <returns></returns>
    private IEnumerator CompleteCommand()
    {
        isCommandAnimation = true;

        // 一度もミスをしていない場合のみ大きく揺らす
        if (!isCommandFailed)
        {
            CameraShake.instance.Shake(0.14f, 0.2f);
        }

        yield return StartCoroutine(commandUI.PlayCompleteAnimation(enemy.GetCurrentIndex() - 1));

        commbo++;
        commboUI.UpdateCombo(commbo);
        comboBar.UpdateComboBar(commbo);

        commandUI.ApplyNext(enemy);
        commandUI.UpdateNextCommand();
        commandUI.UpdateCommanedText(enemy);

        isCommandFailed = false;
        isCommandAnimation = false;
    }

    /// <summary>
    /// 入力をミスした時のアニメーションと処理
    /// </summary>
    /// <returns></returns>
    private IEnumerator MissCommand()
    {
        isCommandAnimation = true;

        int missIndex = enemy.GetMissIndex();
        int nextIndex = enemy.GetCurrentIndex();

        // ミスをしたらダメージを受ける
        yield return StartCoroutine(commandUI.PlayMissAnimation(missIndex, nextIndex));
        Damage(1);

        if(enemy.IsCommandFinished())
        { 
            enemy.SetRandomCommands();
            isCommandFailed = false;

            commandUI.UpdateCommanedText(enemy);
        }

        isCommandAnimation = false;
    }

    /// <summary>
    /// プレイヤーがダメージを受ける関数
    /// </summary>
    public void Damage(int amont)
    {
        curretHP -= amont;

        // HPがマイナスにならないようにする
        curretHP = Mathf.Max(0, curretHP);

        commbo = 0;
        commboUI.UpdateCombo(commbo);
        comboBar.UpdateComboBar(commbo);

        StartCoroutine(FlashRed());

        // HPが0になったらコマンド入力を停止
        if(IsDead())
        {
            SetCommandInput(false);
        }
    }

    /// <summary>
    /// プレイヤーが死亡しているか
    /// </summary>
    /// <returns></returns>
    public bool IsDead()
    {
        return curretHP <= 0;
    }

    /// <summary>
    /// 一瞬だけ赤く点滅させる関数
    /// </summary>
    /// <returns></returns>
    private IEnumerator FlashRed()
    {
        renderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        renderer.color = Color.white;
    }

    /// <summary>
    /// プレイヤーの状態(アニメーション中か)を返す関数
    /// </summary>
    /// <returns></returns>
    public bool IsCommandAnimation()
    {
        return isCommandAnimation;
    }
}
