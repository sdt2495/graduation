

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TradeSelectionUI : MonoBehaviour
{
    [Header("一覧の下にある確定ボタン")]
    public Button confirmButton;
    public TMP_Text confirmButtonText;

    [Header("背景の取引UI")]
    public CanvasGroup backgroundUI;

    [Header("購入確認パネル PurchaseConfirmationPanel")]
    public GameObject purchaseConfirmationPanel;
    public TMP_Text purchaseTitleText;
    public TMP_Text purchaseItemTitleText;
    public TMP_Text purchaseDescriptionText;
    public TMP_Text purchaseValueChangeText;
    public TMP_Text purchasePriceLabelText;
    public TMP_Text purchasePriceText;
    public Button purchaseConfirmButton;
    public Button purchaseCancelButton;
    public Button purchaseCloseButton;

    [Header("売却確認パネル SellConfirmationPanel")]
    public GameObject sellConfirmationPanel;
    public TMP_Text sellTitleText;
    public TMP_Text sellItemTitleText;
    public TMP_Text sellDescriptionText;
    public Button sellDecreaseAmountButton;
    public TMP_Text sellAmountText;
    public Button sellIncreaseAmountButton;
    public TMP_Text sellValueChangeText;
    public TMP_Text sellGoldLabelText;
    public TMP_Text sellGoldText;
    public Button sellConfirmButton;
    public Button sellCancelButton;
    public Button sellCloseButton;

    [Header("売却数量設定")]
    [Min(1)] public int initialSellAmount = 2;
    [Min(1)] public int maxSellAmount = 99;

    [Header("選択色")]
    public Color selectedColor =
        new Color(0.55f, 0.85f, 0.9f, 1f);

    private Button selectedButton;
    private Action selectedAction;

    private string selectedName = "";
    private string selectedType = "";
    private string selectedDescription = "";
    private string selectedDisplayTitle = "";

    private int selectedPrice;
    private int sellAmount;

    // 取引前後の値を計算するための情報
    private float selectedCurrentValue;
    private float selectedAmountPerTrade = 10f;
    private float selectedMaxValue = 100f;

    // 他のスクリプトから現在値を取得する
    private Func<float> selectedValueProvider;

    private readonly Dictionary<Button, Color> originalColors =
        new Dictionary<Button, Color>();

    public bool HasSelection => selectedButton != null;

    private void Awake()
    {
        sellAmount = Mathf.Clamp(
            initialSellAmount, 1, Mathf.Max(1, maxSellAmount));

        if (purchaseConfirmationPanel != null)
            purchaseConfirmationPanel.SetActive(false);

        if (sellConfirmationPanel != null)
            sellConfirmationPanel.SetActive(false);

        if (confirmButton != null)
            confirmButton.interactable = false;
    }

    private void Start()
    {
        if (confirmButton != null)
            confirmButton.onClick.AddListener(OpenConfirmation);

        if (purchaseConfirmButton != null)
            purchaseConfirmButton.onClick.AddListener(ConfirmTrade);

        if (sellConfirmButton != null)
            sellConfirmButton.onClick.AddListener(ConfirmTrade);

        if (purchaseCancelButton != null)
            purchaseCancelButton.onClick.AddListener(CloseConfirmation);

        if (purchaseCloseButton != null)
            purchaseCloseButton.onClick.AddListener(CloseConfirmation);

        if (sellCancelButton != null)
            sellCancelButton.onClick.AddListener(CloseConfirmation);

        if (sellCloseButton != null)
            sellCloseButton.onClick.AddListener(CloseConfirmation);

        if (sellDecreaseAmountButton != null)
            sellDecreaseAmountButton.onClick.AddListener(DecreaseSellAmount);

        if (sellIncreaseAmountButton != null)
            sellIncreaseAmountButton.onClick.AddListener(IncreaseSellAmount);
    }

    // 既存の Select 呼び出しと互換性を保つ
    public void Select(
        Button button,
        string itemName,
        string tradeType,
        Action action,
        string description = "",
        string valueChange = "",
        int price = 0,
        string displayTitle = "")
    {
        Select(
            button,
            itemName,
            tradeType,
            action,
            description,
            valueChange,
            price,
            displayTitle,
            50f,
            10f,
            100f
        );
    }

    // 実際のステータス値と連動する選択処理
    public void SelectLive(
        Button button,
        string itemName,
        string tradeType,
        Action action,
        Func<float> getCurrentValue,
        float amountPerTrade,
        int price,
        string description = "",
        string displayTitle = "",
        float maxValue = 100f)
    {
        if (getCurrentValue == null)
            return;

        Select(
            button,
            itemName,
            tradeType,
            action,
            description,
            "",
            price,
            displayTitle,
            getCurrentValue(),
            amountPerTrade,
            maxValue
        );

        selectedValueProvider = getCurrentValue;
    }

    // 現在値・売買量も渡す場合はこちらを使用
    public void Select(
        Button button,
        string itemName,
        string tradeType,
        Action action,
        string description,
        string valueChange,
        int price,
        string displayTitle,
        float currentValue,
        float amountPerTrade,
        float maxValue = 100f)
    {
        if (button == null || action == null)
            return;

        if (selectedButton != null)
            RestoreColor(selectedButton);

        selectedButton = button;
        selectedName = itemName;
        selectedType = tradeType;
        selectedAction = action;
        selectedDescription = description;
        selectedPrice = price;
        selectedDisplayTitle = displayTitle;

        selectedCurrentValue = currentValue;
        selectedAmountPerTrade = Mathf.Max(0f, amountPerTrade);
        selectedMaxValue = Mathf.Max(0f, maxValue);

        Graphic graphic = button.targetGraphic;

        if (graphic != null)
        {
            if (!originalColors.ContainsKey(button))
                originalColors.Add(button, graphic.color);

            graphic.color = selectedColor;
        }

        if (confirmButton != null)
            confirmButton.interactable = true;

        UpdateConfirmButtonText();
    }

    private void RestoreColor(Button button)
    {
        if (button == null || button.targetGraphic == null)
            return;

        if (originalColors.TryGetValue(button, out Color color))
            button.targetGraphic.color = color;
    }

    private void UpdateConfirmButtonText()
    {
        if (confirmButtonText == null)
            return;

        string actionName =
            selectedType == "購入" ? "購入する" : "売却する";

        confirmButtonText.text = selectedName + "を" + actionName;
    }

    public void OpenConfirmation()
    {
        if (!HasSelection || selectedAction == null)
            return;

        // 確認画面を開く直前に、最新の値を取得する
        if (selectedValueProvider != null)
        {
            selectedCurrentValue = selectedValueProvider();
        }

        bool isPurchase = selectedType == "購入";

        // 必ず片方だけ表示する
        if (purchaseConfirmationPanel != null)
            purchaseConfirmationPanel.SetActive(isPurchase);

        if (sellConfirmationPanel != null)
            sellConfirmationPanel.SetActive(!isPurchase);

        if (isPurchase)
        {
            if (purchaseTitleText != null)
                purchaseTitleText.text = "ステータスを購入";

            if (purchaseItemTitleText != null)
                purchaseItemTitleText.text =
                    string.IsNullOrEmpty(selectedDisplayTitle)
                    ? selectedName
                    : selectedDisplayTitle;

            if (purchaseDescriptionText != null)
                purchaseDescriptionText.text = selectedDescription;

            if (purchaseValueChangeText != null)
            {
                float after = Mathf.Min(
                    selectedMaxValue,
                    selectedCurrentValue + selectedAmountPerTrade);

                purchaseValueChangeText.text =
                    $"{selectedCurrentValue:0.##} → {after:0.##}";
            }

            if (purchasePriceLabelText != null)
                purchasePriceLabelText.text = "必要 Gold";

            if (purchasePriceText != null)
                purchasePriceText.text =
                    selectedPrice.ToString("N0") + " G";
        }
        else
        {
            // 売却画面を開くたびに初期数量へ戻す
            sellAmount = Mathf.Clamp(
                initialSellAmount, 1, Mathf.Max(1, maxSellAmount));

            UpdateSellPanel();
        }

        // 背景の取引UIを操作できなくする
        if (backgroundUI != null)
        {
            backgroundUI.interactable = false;
            backgroundUI.blocksRaycasts = false;
        }
    }

    private void UpdateSellPanel()
    {
        if (sellTitleText != null)
            sellTitleText.text = "SELL / " + selectedName;

        if (sellItemTitleText != null)
            sellItemTitleText.text =
                string.IsNullOrEmpty(selectedDisplayTitle)
                ? selectedName
                : selectedDisplayTitle;

        if (sellDescriptionText != null)
            sellDescriptionText.text = selectedDescription;

        if (sellAmountText != null)
            sellAmountText.text = "×" + sellAmount;

        float decrease = selectedAmountPerTrade * sellAmount;

        float after = Mathf.Max(
            0f, selectedCurrentValue - decrease);

        if (sellValueChangeText != null)
        {
            sellValueChangeText.text =
                $"{selectedCurrentValue:0.##} → {after:0.##}";
        }

        // 選択された数量に応じて売却額を計算
        long totalGold = (long)selectedPrice * sellAmount;

        if (sellGoldLabelText != null)
            sellGoldLabelText.text = "獲得 Gold";

        if (sellGoldText != null)
            sellGoldText.text = "+ " + totalGold.ToString("N0") + " G";

        if (sellDecreaseAmountButton != null)
        {
            sellDecreaseAmountButton.interactable = sellAmount > 1;
        }

        if (sellIncreaseAmountButton != null)
        {
            sellIncreaseAmountButton.interactable =
                sellAmount < Mathf.Max(1, maxSellAmount)
                && decrease < selectedCurrentValue;
        }
    }

    private void DecreaseSellAmount()
    {
        if (selectedType != "売却")
            return;

        if (sellAmount > 1)
        {
            sellAmount--;
            UpdateSellPanel();
        }
    }

    private void IncreaseSellAmount()
    {
        if (selectedType != "売却")
            return;

        if (sellAmount >= Mathf.Max(1, maxSellAmount))
            return;

        float nextDecrease =
            selectedAmountPerTrade * (sellAmount + 1);

        if (nextDecrease >= selectedCurrentValue)
            return;

        sellAmount++;
        UpdateSellPanel();
    }

    public void CloseConfirmation()
    {
        if (purchaseConfirmationPanel != null)
            purchaseConfirmationPanel.SetActive(false);

        if (sellConfirmationPanel != null)
            sellConfirmationPanel.SetActive(false);

        if (backgroundUI != null)
        {
            backgroundUI.interactable = true;
            backgroundUI.blocksRaycasts = true;
        }
    }

    public void ConfirmTrade()
    {
        if (selectedAction == null)
            return;

        // 購入処理は今までどおり1回分
        // 売却処理は選択した数量分を実行
        Action action = selectedAction;

        CloseConfirmation();

        if (selectedType == "売却")
        {
            for (int i = 0; i < sellAmount; i++)
                action.Invoke();
        }
        else
        {
            action.Invoke();
        }
    }
}




