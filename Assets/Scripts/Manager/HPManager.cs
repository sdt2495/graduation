using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HPManager : MonoBehaviour
{
    [Header("Player HP")]
    [SerializeField] private Player player;
    [SerializeField] private Image playerHPBar;
    [SerializeField] private Image playerDamageBar;

    [Header("Enemy HP")]
    [SerializeField] private Enemy enemy;
    [SerializeField] private Image enemyHPBar;
    [SerializeField] private Image enemyDamageBar;

    [Header("HPBarの減少")]
    [SerializeField] private float hpBarDelay = 0.3f;
    [SerializeField] private float hpBarDecreaseSpeed = 2.0f;

    private float previsousPlayerHP;
    private float previsousEnemyHP;

    void Start()
    {
        previsousPlayerHP = player.CurrentHP;
        UpdatePlayerHPBar();
        playerDamageBar.fillAmount = playerHPBar.fillAmount;
    }

    void Update()
    {
        UpdatePlayerHPBar();

        if (enemy != null)
        {
            UpdateEnemyPBar();
        }
    }

    public void SetEnemy(Enemy newEnemy)
    {
        enemy = newEnemy;

        Debug.Log("Enemy CurrentHP: " + enemy.CurrentHP);
        Debug.Log("Enemy MaxHP: " + enemy.MaxHP);

        previsousEnemyHP = enemy.CurrentHP;

        UpdateEnemyPBar();
        enemyDamageBar.fillAmount = enemyHPBar.fillAmount;

        Debug.Log("Enemy HPBar: " + enemyHPBar.fillAmount);
        Debug.Log("Enemy DamageBar: " + enemyDamageBar.fillAmount);
    }

    /// <summary>
    /// プレイヤーのHPを管理
    /// </summary>
    private void UpdatePlayerHPBar()
    {
        float hpRate = (float)player.CurrentHP / player.MaxHP;
        playerHPBar.fillAmount = hpRate;

        if (hpRate >= 0.5f)
        {
            // 緑→黄色
            float t = (1.0f - hpRate) / 0.5f;
            playerHPBar.color = Color.Lerp(Color.green, Color.yellow , t);
        }
        else
        {
            // 黄色→赤
            float t = hpRate / 0.5f;
            playerHPBar.color = Color.Lerp(Color.red, Color.yellow , t);
        }

        // HPが減ったらダメージバーを遅れて減らす
        if (player.CurrentHP < previsousPlayerHP)
        {
            StartCoroutine(DecreasePlayerHPBar(playerHPBar, playerDamageBar));
        }
        previsousPlayerHP = player.CurrentHP;

    }

    private void UpdateEnemyPBar()
    {
        float hpRate = (float)enemy.CurrentHP / enemy.MaxHP;
        enemyHPBar.fillAmount = hpRate;

        if (hpRate >= 0.5f)
        {
            // 緑→黄色
            float t = (1.0f - hpRate) / 0.5f;
            enemyHPBar.color = Color.Lerp(Color.green, Color.yellow, t);
        }
        else
        {
            // 黄色→赤
            float t = hpRate / 0.5f;
            enemyHPBar.color = Color.Lerp(Color.red, Color.yellow, t);
        }

        // HPが減ったらダメージバーを遅れて減らす
        if (enemy.CurrentHP < previsousEnemyHP)
        {
            StartCoroutine(DecreasePlayerHPBar(enemyHPBar, enemyDamageBar));
        }
        previsousEnemyHP = enemy.CurrentHP;

    }

    /// <summary>
    /// ダメージバーを遅れて減らす
    /// </summary>
    /// <returns></returns>
    private IEnumerator DecreasePlayerHPBar(Image hpBar, Image damageBar)
    {
        yield return new WaitForSeconds(hpBarDelay);

        while(damageBar.fillAmount > hpBar.fillAmount)
        {
           damageBar.fillAmount = Mathf.MoveTowards(damageBar.fillAmount, hpBar.fillAmount, hpBarDecreaseSpeed * Time.deltaTime);

            yield return null;
        }
    }
}
