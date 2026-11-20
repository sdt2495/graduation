using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TradeSelectionUI : MonoBehaviour
{
    [Header("ページ切り替え")]
    public PageSwitcher pageSwitcher;

    [Header("一覧の下にある確定ボタン")]
    public Button confirmButton;
    public TMP_Text confirmButtonText;

    [Header("背景の取引UI")]
    public CanvasGroup backgroundUI;

    [Header("購入確認パネル")]
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

    [Header("売却確認パネル")]
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

    private float selectedCurrentValue;
    private float selectedAmountPerTrade = 10f;
    private float selectedMaxValue = 100f;

    private Func<float> selectedValueProvider;

    // レーダーチャートの予測値を更新する処理
    private Action<float> selectedPreviewUpdater;

    private readonly Dictionary<Button, Color> originalColors =
        new Dictionary<Button, Color>();

    public bool HasSelection => selectedButton != null;

    public static bool HasAnySelection { get; private set; }

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

    // 既存のSelect呼び出しとの互換性を維持
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

    // 最新のステータス値と予測更新処理を登録
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
        float maxValue = 100f,
        Action<float> previewUpdater = null)
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
        selectedPreviewUpdater = previewUpdater;

        // 選択時点でも現在の数量に合わせて予測値を更新
        UpdatePreview();
    }

    // 実際の選択処理
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

        selectedValueProvider = null;
        selectedPreviewUpdater = null;

        HasAnySelection = true;

        if (pageSwitcher != null)
            pageSwitcher.SwitchToPageContaining(button);

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

        if (selectedValueProvider != null)
            selectedCurrentValue = selectedValueProvider();

        bool isPurchase = selectedType == "購入";

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

            // 購入時の予測値
            if (selectedPreviewUpdater != null)
            {
                float after = Mathf.Min(
                    selectedMaxValue,
                    selectedCurrentValue + selectedAmountPerTrade);

                selectedPreviewUpdater(after);
            }
        }
        else
        {
            sellAmount = Mathf.Clamp(
                initialSellAmount, 1, Mathf.Max(1, maxSellAmount));

            UpdateSellPanel();
        }

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

        long totalGold = (long)selectedPrice * sellAmount;

        if (sellGoldLabelText != null)
            sellGoldLabelText.text = "獲得 Gold";

        if (sellGoldText != null)
            sellGoldText.text =
                "+ " + totalGold.ToString("N0") + " G";

        if (sellDecreaseAmountButton != null)
            sellDecreaseAmountButton.interactable = sellAmount > 1;

        if (sellIncreaseAmountButton != null)
        {
            float nextDecrease =
                selectedAmountPerTrade * (sellAmount + 1);

            sellIncreaseAmountButton.interactable =
                sellAmount < Mathf.Max(1, maxSellAmount)
                && nextDecrease < selectedCurrentValue;
        }

        // ★ 数量に応じた売却後の値をレーダーチャートへ反映
        UpdatePreview();
    }

    private void UpdatePreview()
    {
        if (selectedPreviewUpdater == null)
            return;

        float after;

        if (selectedType == "売却")
        {
            after = Mathf.Max(
                0f,
                selectedCurrentValue -
                selectedAmountPerTrade * sellAmount);
        }
        else if (selectedType == "購入")
        {
            after = Mathf.Min(
                selectedMaxValue,
                selectedCurrentValue + selectedAmountPerTrade);
        }
        else
        {
            return;
        }

        selectedPreviewUpdater(after);
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

        // キャンセル時は予測表示を解除
        if (selectedPreviewUpdater != null)
        {
            selectedPreviewUpdater(float.NaN);
        }
    }

    public void ConfirmTrade()
    {
        if (selectedAction == null)
            return;

        Action action = selectedAction;
        bool isSell = selectedType == "売却";
        int amount = sellAmount;

        CloseConfirmation();

        if (isSell)
        {
            for (int i = 0; i < amount; i++)
                action.Invoke();
        }
        else
        {
            action.Invoke();
        }

        ClearSelection();
    }

    public void ClearSelection()
    {
        if (selectedButton != null)
            RestoreColor(selectedButton);

        // 予測表示を解除
        if (selectedPreviewUpdater != null)
            selectedPreviewUpdater(float.NaN);

        selectedButton = null;
        selectedAction = null;
        selectedValueProvider = null;
        selectedPreviewUpdater = null;

        selectedName = "";
        selectedType = "";
        selectedDescription = "";
        selectedDisplayTitle = "";

        selectedPrice = 0;

        HasAnySelection = false;

        if (confirmButton != null)
            confirmButton.interactable = false;

        if (confirmButtonText != null)
            confirmButtonText.text = "選択してください";
    }
}





