
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SubStatusTrade : MonoBehaviour
{
    [System.Serializable]
    public class SubStatus
    {
        public string statusName;

        [Range(0, 100)]
        public float value = 50f;

        public TMP_Text valueText;
        public Button buyButton;
        public Button sellButton;
    }

    [Header("サブステータス6個")]
    public SubStatus[] statuses = new SubStatus[6];

    [Header("売買設定")]
    public float tradeAmount = 10f;
    public int buyPrice = 100;
    public int sellPrice = 50;

    [Header("所持金・メッセージ")]
    public TMP_Text messageText;

    private int money =>
    MoneyManager.Instance != null
        ? MoneyManager.Instance.Money
        : 0;

    private void Start()
    {
        string[] names =
        {
            "CRD", "EVA", "LUK",
            "CMB", "REV", "VAM"
        };

        for (int i = 0; i < statuses.Length; i++)
        {
            if (statuses[i] == null)
                statuses[i] = new SubStatus();

            if (string.IsNullOrEmpty(statuses[i].statusName))
                statuses[i].statusName = names[i];

            int index = i;

            if (statuses[i].buyButton != null)
            {
                statuses[i].buyButton.onClick.AddListener(
                    () => Buy(index));
            }

            if (statuses[i].sellButton != null)
            {
                statuses[i].sellButton.onClick.AddListener(
                    () => Sell(index));
            }
        }

        UpdateUI();
    }

    private void Buy(int index)
    {
        SubStatus s = statuses[index];

        if (s.value >= 100f)
        {
            ShowMessage(s.statusName + " は最大値です！");
            return;
        }

        if (buyPrice <= 0 || tradeAmount <= 0f)
        {
            ShowMessage("購入価格と売買量を確認してください。");
            return;
        }

        if (money < buyPrice)
        {
            ShowMessage("お金が足りません！");
            return;
        }

        if (MoneyManager.Instance == null ||
    !MoneyManager.Instance.Spend(buyPrice))
        {
            ShowMessage("お金が足りません！");
            return;
        }

        s.value = Mathf.Min(100f, s.value + tradeAmount);

        UpdateUI();
        ShowMessage(s.statusName + " を購入しました！");
    }

    private void Sell(int index)
    {
        SubStatus s = statuses[index];

        if (s.value <= 0f)
        {
            ShowMessage(s.statusName + " はこれ以上売却できません！");
            return;
        }

        if (sellPrice <= 0 || tradeAmount <= 0f)
        {
            ShowMessage("売却価格と売買量を確認してください。");
            return;
        }

        if (MoneyManager.Instance == null)
        {
            ShowMessage("MoneyManager が見つかりません！");
            return;
        }

        s.value = Mathf.Max(0f, s.value - tradeAmount);
        MoneyManager.Instance.Earn(sellPrice);

        UpdateUI();
        ShowMessage(s.statusName + " を売却しました！");
    }

    private void UpdateUI()
    {
        foreach (SubStatus s in statuses)
        {
            if (s == null) continue;

            if (s.valueText != null)
            {
                s.valueText.text =
                    s.statusName + "  " + s.value.ToString("F0");
            }
        }

        if (MoneyManager.Instance != null)
            MoneyManager.Instance.UpdateMoneyUI();
    }

    private void ShowMessage(string message)
    {
        if (messageText != null)
            messageText.text = message;

        Debug.Log(message);
    }
}
