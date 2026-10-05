using System;
using System.Collections;
using JetBrains.Annotations;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private Enemy enemy;
    [SerializeField] private SpriteRenderer renderer;

    [Header("HP")]
    [SerializeField] private HPUI hpUI;
    [SerializeField] private int maxHP = 10;
    private int curretHP;

    [Header("コンボ")]
    [SerializeField] private ComboUI commboUI;
    private int commbo = 0;

    [Header("コマンドUI")]
    [SerializeField] private CommandUI commandUI;

    [Header("バトルマネージャー")]
    [SerializeField] private BattleManager battleManager;

    private bool isCommandAnimation = false;
    private bool isCommandInput = false;

    // 外部参照用
    public int CurrentHP { get { return curretHP; } }
    public int MaxHP { get { return maxHP; } }

    private void Start()
    {
        curretHP = maxHP;
        hpUI.UpdateHP(curretHP);
        commboUI.UpdateCombo(commbo);
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

        if(!isCommandInput)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            CheckCommaned(CommandType.Left);
        }
        else if(Input.GetKeyDown(KeyCode.RightArrow))
        {
            CheckCommaned(CommandType.Right);
        }
        else if(Input.GetKeyDown(KeyCode.UpArrow))
        {
            CheckCommaned(CommandType.Up);
        }
        else if( Input.GetKeyDown(KeyCode.DownArrow))
        {
            CheckCommaned(CommandType.Down);
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
        CheckResult result = enemy.Check(command);

        switch (result)
        {
            case CheckResult.Success:
                // 小さい画面揺れ
                CameraShake.instance.Shake(0.08f, 0.05f);
                commbo++;
                commboUI.UpdateCombo(commbo);
                commandUI.UpdateActiveComand(enemy.GetCurrentIndex());
                break;

            case CheckResult.Complete:
                StartCoroutine(CompleteCommand());
                break;

            case CheckResult.Miss:
                StartCoroutine(MissCommand());
                break;
        }
    }

    private IEnumerator CompleteCommand()
    {
        isCommandAnimation = true;

        // 大きい画面揺れ
        CameraShake.instance.Shake(0.18f, 0.2f);

        yield return StartCoroutine(commandUI.PlayCompleteAnimation(enemy.GetCurrentIndex() - 1));

        battleManager.CompleteCommand();
        commbo++;
        commboUI.UpdateCombo(commbo);

        commandUI.ApplyNext(enemy);
        commandUI.UpdateNextCommand();
        commandUI.UpdateCommanedText(enemy);

        isCommandAnimation = false;
    }

    private IEnumerator MissCommand()
    {
        isCommandAnimation = true;

        int missIndex = enemy.GetMissIndex();
        int nextIndex = enemy.GetCurrentIndex();

        // ミスをしたらダメージを受ける
        yield return StartCoroutine(commandUI.PlayMissAnimation(missIndex, nextIndex));
        Damage();

        if(enemy.IsCommandFinished())
        { 
            enemy.SetRandomCommands();

            commandUI.UpdateCommanedText(enemy);
        }

        isCommandAnimation = false;
    }

    /// <summary>
    /// プレイヤーがダメージを受ける関数
    /// </summary>
    private void Damage()
    {
        curretHP--;

        hpUI.UpdateHP(curretHP);

        commbo = 0;
        commboUI.UpdateCombo(commbo);

        StartCoroutine(FlashRed());
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
}
