

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TradeSelectionUI : MonoBehaviour
{
    [Header("ˆê——‚Ì‰º‚É‚ ‚éŠm’èƒ{ƒ^ƒ“")]
    public Button confirmButton;
    public TMP_Text confirmButtonText;

    [Header("”wŒi‚ÌæˆøUI")]
    public CanvasGroup backgroundUI;

    [Header("w“üŠm”Fƒpƒlƒ‹ PurchaseConfirmationPanel")]
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

    [Header("”„‹pŠm”Fƒpƒlƒ‹ SellConfirmationPanel")]
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

    [Header("”„‹p”—Êİ’è")]
    [Min(1)] public int initialSellAmount = 2;
    [Min(1)] public int maxSellAmount = 99;

    [Header("‘I‘ğF")]
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

    // æˆø‘OŒã‚Ì’l‚ğŒvZ‚·‚é‚½‚ß‚Ìî•ñ
    private float selectedCurrentValue;
    private float selectedAmountPerTrade = 10f;
    private float selectedMaxValue = 100f;

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

    // Šù‘¶‚Ì Select ŒÄ‚Ño‚µ‚ÆŒİŠ·«‚ğ•Û‚Â
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

    // Œ»İ’lE”„”ƒ—Ê‚à“n‚·ê‡‚Í‚±‚¿‚ç‚ğg—p
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
            selectedType == "w“ü" ? "w“ü‚·‚é" : "”„‹p‚·‚é";

        confirmButtonText.text = selectedName + "‚ğ" + actionName;
    }

    public void OpenConfirmation()
    {
        if (!HasSelection || selectedAction == null)
            return;

        bool isPurchase = selectedType == "w“ü";

        // •K‚¸•Ğ•û‚¾‚¯•\¦‚·‚é
        if (purchaseConfirmationPanel != null)
            purchaseConfirmationPanel.SetActive(isPurchase);

        if (sellConfirmationPanel != null)
            sellConfirmationPanel.SetActive(!isPurchase);

        if (isPurchase)
        {
            if (purchaseTitleText != null)
                purchaseTitleText.text = "ƒXƒe[ƒ^ƒX‚ğw“ü";

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
                    $"{selectedCurrentValue:0.##} ¨ {after:0.##}";
            }

            if (purchasePriceLabelText != null)
                purchasePriceLabelText.text = "•K—v Gold";

            if (purchasePriceText != null)
                purchasePriceText.text =
                    selectedPrice.ToString("N0") + " G";
        }
        else
        {
            // ”„‹p‰æ–Ê‚ğŠJ‚­‚½‚Ñ‚É‰Šú”—Ê‚Ö–ß‚·
            sellAmount = Mathf.Clamp(
                initialSellAmount, 1, Mathf.Max(1, maxSellAmount));

            UpdateSellPanel();
        }

        // ”wŒi‚ÌæˆøUI‚ğ‘€ì‚Å‚«‚È‚­‚·‚é
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
            sellAmountText.text = "~" + sellAmount;

        float decrease = selectedAmountPerTrade * sellAmount;

        float after = Mathf.Max(
            0f, selectedCurrentValue - decrease);

        if (sellValueChangeText != null)
        {
            sellValueChangeText.text =
                $"{selectedCurrentValue:0.##} ¨ {after:0.##}";
        }

        // ‘I‘ğ‚³‚ê‚½”—Ê‚É‰‚¶‚Ä”„‹pŠz‚ğŒvZ
        long totalGold = (long)selectedPrice * sellAmount;

        if (sellGoldLabelText != null)
            sellGoldLabelText.text = "Šl“¾ Gold";

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
        if (selectedType != "”„‹p")
            return;

        if (sellAmount > 1)
        {
            sellAmount--;
            UpdateSellPanel();
        }
    }

    private void IncreaseSellAmount()
    {
        if (selectedType != "”„‹p")
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

        // w“üˆ—‚Í¡‚Ü‚Å‚Ç‚¨‚è1‰ñ•ª
        // ”„‹pˆ—‚Í‘I‘ğ‚µ‚½”—Ê•ª‚ğÀs
        Action action = selectedAction;

        CloseConfirmation();

        if (selectedType == "”„‹p")
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




