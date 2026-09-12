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

    [Header("HPBar‚ÌŒ¸­")]
    [SerializeField] private float hpBarDelay = 0.3f;
    [SerializeField] private float hpBarDecreaseSpeed = 2.0f;

    void Start()
    {
        UpdatePlayerHPBar();
    }

    void Update()
    {
        UpdatePlayerHPBar();
    }

    private void UpdatePlayerHPBar()
    {
        playerHPBar.fillAmount = (float)player.CurrentHP / player.MaxHP;
    }

    private IEnumerator DecreasePlayerHPBar()
    {
        yield return new WaitForSeconds(hpBarDelay);

        while(playerDamageBar.fillAmount > playerDamageBar.fillAmount)
        {
            playerDamageBar.fillAmount = Mathf.MoveTowards(playerDamageBar.fillAmount, playerHPBar.fillAmount, hpBarDecreaseSpeed * Time.deltaTime);
        }
        yield return null;
    }
}
