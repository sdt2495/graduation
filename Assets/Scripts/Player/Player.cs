using System.Collections;
using JetBrains.Annotations;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private Enemy enemy;
    [SerializeField] private EnemySpawner spawner;
    [SerializeField] private SpriteRenderer renderer;

    [Header("HP")]
    [SerializeField] private HPUI hpUI;
    [SerializeField] private int maxHP = 3;
    private int curretHP;

    [Header("コンボ")]
    [SerializeField] private ComboUI commboUI;
    private int commbo = 0;

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
        
        if(Input.GetKeyDown(KeyCode.LeftArrow))
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
                break;

            case CheckResult.Complete:
                // 大きい画面揺れ
                CameraShake.instance.Shake(0.18f, 0.2f);
                spawner.StartSpawn();
                commbo++;
                commboUI.UpdateCombo(commbo);
                break;

            case CheckResult.Miss:
                // ミスをしたらダメージを受ける
                Damage();
                break;
        }
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
