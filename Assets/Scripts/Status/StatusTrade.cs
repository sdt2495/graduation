
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatusTrade : MonoBehaviour
{
    [Header("レーダーチャート")]
    public StatusRadar statusRadar;

    [Header("所持金")]
    public int money = 100000000;
    public TMP_Text moneyText;

    [Header("売買価格")]
    public int buyPrice = 100;
    public int sellPrice = 50;

    [Header("1回の売買量")]
    public float tradeAmount = 10f;

    [Header("メッセージ")]
    public TMP_Text messageText;

    [Header("テスト用初期ステータス")]
    public bool initializeTestStats = true;
    [Range(0f, 100f)]
    public float initialStatValue = 50f;

    private void Start()
    {
        if (statusRadar == null)
        {
            Debug.LogError("StatusTrade: StatusRadarを設定してください！");
            return;
        }

        if (initializeTestStats)
        {
            float value = Mathf.Clamp(initialStatValue, 0f, 100f);
            statusRadar.SetStatus(value, value, value, value, value, value);
        }

        // Hierarchyのボタン名でクリック処理を登録
        RegisterButton("HP Buy", () => BuyStatus("HP"));
        RegisterButton("HP Sell", () => SellStatus("HP"));

        RegisterButton("ATK Buy", () => BuyStatus("ATK"));
        RegisterButton("ATK Sell", () => SellStatus("ATK"));

        RegisterButton("DEF Buy", () => BuyStatus("DEF"));
        RegisterButton("DEF Sell", () => SellStatus("DEF"));

        RegisterButton("CRL Buy", () => BuyStatus("CRI"));
        RegisterButton("CRL Sell", () => SellStatus("CRI"));

        RegisterButton("SAN Buy", () => BuyStatus("Other"));
        RegisterButton("SAN Sell", () => SellStatus("Other"));

        RegisterButton("TEC Buy", () => BuyStatus("TEC"));
        RegisterButton("TEC Sell", () => SellStatus("TEC"));

        UpdateUI();
        ShowMessage("売買テストを開始できます！");
    }

    private void RegisterButton(string objectName, UnityEngine.Events.UnityAction action)
    {
        GameObject buttonObject = GameObject.Find(objectName);

        if (buttonObject == null)
        {
            Debug.LogWarning("ボタンが見つかりません: " + objectName);
            return;
        }

        Button button = buttonObject.GetComponent<Button>();

        if (button == null)
        {
            Debug.LogWarning(objectName + " にButtonコンポーネントがありません！");
            return;
        }

        button.onClick.AddListener(action);
        Debug.Log(objectName + " の登録完了");
    }

    private void BuyStatus(string name)
    {
        if (statusRadar == null) return;

        if (buyPrice <= 0 || tradeAmount <= 0f)
        {
            ShowMessage("購入価格と売買量を確認してください！");
            return;
        }

        float current = GetStatus(name);

        if (current >= 100f)
        {
            ShowMessage(name + "は最大値です！");
            return;
        }

        if (money < buyPrice)
        {
            ShowMessage("お金が足りません！");
            return;
        }

        float amount = Mathf.Min(tradeAmount, 100f - current);

        money -= buyPrice;
        SetStatus(name, current + amount);

        UpdateUI();
        ShowMessage(name + "を購入！ -" + buyPrice + " G");
    }

    private void SellStatus(string name)
    {
        if (statusRadar == null) return;

        if (sellPrice <= 0 || tradeAmount <= 0f)
        {
            ShowMessage("売却価格と売買量を確認してください！");
            return;
        }

        float current = GetStatus(name);

        if (current <= 0f)
        {
            ShowMessage(name + "はこれ以上売却できません！");
            return;
        }

        float amount = Mathf.Min(tradeAmount, current);

        money += sellPrice;
        SetStatus(name, current - amount);

        UpdateUI();
        ShowMessage(name + "を売却！ +" + sellPrice + " G");
    }

    private float GetStatus(string name)
    {
        switch (name)
        {
            case "HP": return statusRadar.HP;
            case "ATK": return statusRadar.ATK;
            case "DEF": return statusRadar.DEF;
            case "CRI": return statusRadar.CRI;
            case "TEC": return statusRadar.TEC;
            case "Other": return statusRadar.Other;
            default: return 0f;
        }
    }

    private void SetStatus(string name, float value)
    {
        value = Mathf.Clamp(value, 0f, 100f);

        float hp = statusRadar.HP;
        float atk = statusRadar.ATK;
        float def = statusRadar.DEF;
        float cri = statusRadar.CRI;
        float tec = statusRadar.TEC;
        float other = statusRadar.Other;

        switch (name)
        {
            case "HP": hp = value; break;
            case "ATK": atk = value; break;
            case "DEF": def = value; break;
            case "CRI": cri = value; break;
            case "TEC": tec = value; break;
            case "Other": other = value; break;
        }

        statusRadar.SetStatus(hp, atk, def, cri, tec, other);
    }

    private void UpdateUI()
    {
        if (moneyText != null)
            moneyText.text = "所持金：" + money + " G";
    }

    private void ShowMessage(string message)
    {
        if (messageText != null)
            messageText.text = message;

        Debug.Log(message);
    }
}




