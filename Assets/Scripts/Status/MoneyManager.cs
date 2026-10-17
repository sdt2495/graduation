
using UnityEngine;
using TMPro;

public class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance { get; private set; }

    [Header("ゲーム開始時の所持金")]
    public int initialMoney = 100000000;

    [Header("所持金表示（複数登録可能）")]
    public TMP_Text[] moneyTexts;

    public int Money { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Money = initialMoney;
        UpdateMoneyUI();
    }

    public bool CanAfford(int price)
    {
        return price >= 0 && Money >= price;
    }

    public bool Spend(int price)
    {
        if (price < 0 || Money < price)
            return false;

        Money -= price;
        UpdateMoneyUI();
        return true;
    }

    public void Earn(int price)
    {
        if (price <= 0)
            return;

        Money = (int)Mathf.Min(
            int.MaxValue, (long)Money + price);

        UpdateMoneyUI();
    }

    public void UpdateMoneyUI()
    {
        if (moneyTexts == null) return;

        foreach (TMP_Text text in moneyTexts)
        {
            if (text != null)
                text.text = "所持金：" + Money.ToString("N0") + " G";
        }
    }
}

