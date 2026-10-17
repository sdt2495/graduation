
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.Events;

public class StatusTrade : MonoBehaviour
{
    [Header("レーダーチャート")]
    public StatusRadar statusRadar;

    

    [Header("ステータス数値表示")]
    public TMP_Text hpValueText;
    public TMP_Text atkValueText;
    public TMP_Text defValueText;
    public TMP_Text criValueText;
    public TMP_Text sanValueText;
    public TMP_Text tecValueText;

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

    private int money =>
    MoneyManager.Instance != null
        ? MoneyManager.Instance.Money
        : 0;

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

        RegisterButton("HP Buy", "HP", true);
        RegisterButton("HP Sell", "HP", false);

        RegisterButton("ATK Buy", "ATK", true);
        RegisterButton("ATK Sell", "ATK", false);

        RegisterButton("DEF Buy", "DEF", true);
        RegisterButton("DEF Sell", "DEF", false);

        RegisterButton("CRL Buy", "CRI", true);
        RegisterButton("CRL Sell", "CRI", false);

        RegisterButton("SAN Buy", "Other", true);
        RegisterButton("SAN Sell", "Other", false);

        RegisterButton("TEC Buy", "TEC", true);
        RegisterButton("TEC Sell", "TEC", false);

        UpdateUI();
        ShowMessage("ボタンにカーソルを乗せると結果を予測できます！");
    }


    private void RegisterButton(
        string objectName, string statusName, bool isBuy)
    {
        // 非表示のオブジェクトも含めてボタンを探す
        Button[] allButtons =
            Resources.FindObjectsOfTypeAll<Button>();

        Button button = null;

        foreach (Button candidate in allButtons)
        {
            if (candidate.gameObject.name == objectName
                && candidate.gameObject.scene.IsValid())
            {
                button = candidate;
                break;
            }
        }

        if (button == null)
        {
            Debug.LogWarning("ボタンが見つかりません: " + objectName);
            return;
        }

        // クリック時の処理
        button.onClick.AddListener(() =>
        {
            statusRadar.ClearPreview();

            if (isBuy)
                BuyStatus(statusName);
            else
                SellStatus(statusName);
        });

        // マウスカーソルの出入りを取得
        EventTrigger trigger =
            button.gameObject.GetComponent<EventTrigger>();

        if (trigger == null)
            trigger = button.gameObject.AddComponent<EventTrigger>();

        AddPointerEvent(trigger, EventTriggerType.PointerEnter, () =>
        {
            PreviewStatus(statusName, isBuy);
        });

        AddPointerEvent(trigger, EventTriggerType.PointerExit, () =>
        {
            statusRadar.ClearPreview();
        });

        Debug.Log(objectName + " の登録完了");
    }


    private void AddPointerEvent(
        EventTrigger trigger,
        EventTriggerType type,
        UnityAction action)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = type;
        entry.callback.AddListener((data) => action());

        trigger.triggers.Add(entry);
    }

    private void PreviewStatus(string name, bool isBuy)
    {
        if (statusRadar == null) return;

        float current = GetStatus(name);
        float next = current;

        if (isBuy)
        {
            if (money < buyPrice || current >= 100f || buyPrice <= 0)
            {
                ShowMessage("購入できません（所持金・上限を確認）");
                return;
            }

            next = Mathf.Min(100f, current + tradeAmount);
        }
        else
        {
            if (current <= 0f || sellPrice <= 0)
            {
                ShowMessage("これ以上売却できません！");
                return;
            }

            next = Mathf.Max(0f, current - tradeAmount);
        }

        ApplyPreview(name, next);

        string action = isBuy ? "購入" : "売却";
        int price = isBuy ? buyPrice : sellPrice;

        ShowMessage(
            name + "を" + action + "した場合\n" +
            "ステータス：" + current.ToString("F0") +
            " → " + next.ToString("F0") + "\n" +
            "所持金：" + money.ToString("N0") +
            (isBuy ? " → " + (money - price).ToString("N0") + " G"
                   : " → " + ((long)money + price).ToString("N0") + " G")
        );
    }

    private void ApplyPreview(string name, float value)
    {
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

        statusRadar.SetPreviewStatus(hp, atk, def, cri, tec, other);
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
        if (!MoneyManager.Instance.Spend(buyPrice))
        {
            ShowMessage("お金が足りません！");
            return;
        }

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
        MoneyManager.Instance.Earn(sellPrice);
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
        if (MoneyManager.Instance != null)
            MoneyManager.Instance.UpdateMoneyUI();

        if (hpValueText != null)
            hpValueText.text = "HP  " + statusRadar.HP.ToString("F0");

        if (atkValueText != null)
            atkValueText.text = "ATK  " + statusRadar.ATK.ToString("F0");

        if (defValueText != null)
            defValueText.text = "DEF  " + statusRadar.DEF.ToString("F0");

        if (criValueText != null)
            criValueText.text = "CRI  " + statusRadar.CRI.ToString("F0");

        if (sanValueText != null)
            sanValueText.text = "SAN  " + statusRadar.Other.ToString("F0");

        if (tecValueText != null)
            tecValueText.text = "TEC  " + statusRadar.TEC.ToString("F0");
    }

    private void ShowMessage(string message)
    {
        if (messageText != null)
            messageText.text = message;

        Debug.Log(message);
    }


}





