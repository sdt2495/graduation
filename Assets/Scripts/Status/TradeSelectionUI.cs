
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TradeSelectionUI : MonoBehaviour
{
    [Header("一覧の下にあるボタン")]
    public Button confirmButton;
    public TMP_Text confirmButtonText;

    [Header("確認ウィンドウ")]
    public GameObject confirmationPanel;
    public TMP_Text confirmationTitleText;
    public TMP_Text itemTitleText;
    public TMP_Text descriptionText;
    public TMP_Text valueChangeText;
    public TMP_Text priceText;

    [Header("確認ウィンドウのボタン")]
    public Button modalConfirmButton;
    public Button cancelButton;
    public Button closeButton;

    [Header("選択色")]
    public Color selectedColor =
        new Color(0.55f, 0.85f, 0.9f, 1f);

    private Button selectedButton;
    private Action selectedAction;
    private string selectedName;
    private string selectedType;
    private string selectedDescription;
    private string selectedValueChange;
    private string selectedDisplayTitle;
    private int selectedPrice;

    private readonly Dictionary<Button, Color> originalColors =
        new Dictionary<Button, Color>();

    public bool HasSelection => selectedButton != null;

    private void Start()
    {
        if (confirmationPanel != null)
            confirmationPanel.SetActive(false);

        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(OpenConfirmation);
            confirmButton.interactable = false;
        }

        if (modalConfirmButton != null)
            modalConfirmButton.onClick.AddListener(ConfirmTrade);

        if (cancelButton != null)
            cancelButton.onClick.AddListener(CloseConfirmation);

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseConfirmation);
    }

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
        if (button == null || action == null) return;

        // 前の商品の色を戻す
        if (selectedButton != null)
            RestoreColor(selectedButton);

        selectedButton = button;
        selectedName = itemName;
        selectedType = tradeType;
        selectedAction = action;
        selectedDescription = description;
        selectedValueChange = valueChange;
        selectedPrice = price;
        selectedDisplayTitle = displayTitle;

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
        if (confirmButtonText == null) return;

        string actionName =
            selectedType == "購入" ? "購入する" : "売却する";

        confirmButtonText.text =
            selectedName + "を" + actionName;
    }

    public void OpenConfirmation()
    {
        if (!HasSelection || selectedAction == null)
            return;

        if (confirmationTitleText != null)
        {
            confirmationTitleText.text =
                selectedType == "購入"
                ? "ステータスを購入"
                : "ステータスを売却";
        }

        if (itemTitleText != null)
        {
            itemTitleText.text =
                string.IsNullOrEmpty(selectedDisplayTitle)
                ? selectedName
                : selectedDisplayTitle;
        }

        if (descriptionText != null)
            descriptionText.text = selectedDescription;

        if (valueChangeText != null)
            valueChangeText.text = selectedValueChange;

        if (priceText != null)
        {
            string label =
                selectedType == "購入"
                ? "必要 Gold"
                : "獲得 Gold";

            priceText.text =
                label + "　　　　　　　　 " +
                selectedPrice.ToString("N0") + " G";
        }

        if (confirmationPanel != null)
            confirmationPanel.SetActive(true);
    }

    public void CloseConfirmation()
    {
        if (confirmationPanel != null)
            confirmationPanel.SetActive(false);
    }

    public void ConfirmTrade()
    {
        if (selectedAction == null) return;

        CloseConfirmation();

        // 実際の購入・売却処理はここで初めて実行
        selectedAction.Invoke();
    }
}


