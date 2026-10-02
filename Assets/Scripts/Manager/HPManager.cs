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

    [Header("HPBarの減少")]
    [SerializeField] private float hpBarDelay = 0.3f;
    [SerializeField] private float hpBarDecreaseSpeed = 2.0f;

    private float previsousHP;

    void Start()
    {
        previsousHP = player.CurrentHP;
        UpdatePlayerHPBar();
        playerDamageBar.fillAmount = playerHPBar.fillAmount;
    }

    void Update()
    {
        UpdatePlayerHPBar();
    }

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
        if (player.CurrentHP < previsousHP)
        {
            StartCoroutine(DecreasePlayerHPBar());
        }
        previsousHP = player.CurrentHP;

    }

    private IEnumerator DecreasePlayerHPBar()
    {
        yield return new WaitForSeconds(hpBarDelay);

        while(playerDamageBar.fillAmount > playerHPBar.fillAmount)
        {
            playerDamageBar.fillAmount = Mathf.MoveTowards(playerDamageBar.fillAmount, playerHPBar.fillAmount, hpBarDecreaseSpeed * Time.deltaTime);

            yield return null;
        }
    }
}
