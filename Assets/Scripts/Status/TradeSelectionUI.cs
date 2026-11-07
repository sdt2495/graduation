
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TradeSelectionUI : MonoBehaviour
{
    [Header("下部の確定ボタン")]
    public Button confirmButton;
    public TMP_Text confirmButtonText;

    [Header("選択色")]
    public Color normalColor = Color.white;
    public Color selectedColor = new Color(
        0.55f, 0.85f, 0.9f, 1f
    );

    private Button selectedButton;
    private Action selectedAction;
    private string selectedName;
    private string selectedType;

    private readonly System.Collections.Generic.Dictionary<Button, Color>
        originalColors =
        new System.Collections.Generic.Dictionary<Button, Color>();

    private void Start()
    {
        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(ConfirmTrade);
            confirmButton.interactable = false;
        }
    }

    public void Select(
        Button button,
        string itemName,
        string tradeType,
        Action action)
    {
        if (button == null || action == null) return;

        // 前の選択を解除
        if (selectedButton != null)
        {
            RestoreColor(selectedButton);
        }

        selectedButton = button;
        selectedName = itemName;
        selectedType = tradeType;
        selectedAction = action;

        // 元の色を保存
        Graphic graphic = button.targetGraphic;

        if (graphic != null)
        {
            if (!originalColors.ContainsKey(button))
                originalColors[button] = graphic.color;

            graphic.color = selectedColor;
        }

        if (confirmButton != null)
            confirmButton.interactable = true;

        UpdateConfirmText();

        Debug.Log("選択中：" + selectedName);
    }

    private void RestoreColor(Button button)
    {
        if (button == null || button.targetGraphic == null)
            return;

        if (originalColors.TryGetValue(button, out Color color))
            button.targetGraphic.color = color;
        else
            button.targetGraphic.color = normalColor;
    }

    private void UpdateConfirmText()
    {
        if (confirmButtonText == null) return;

        string actionName =
            selectedType == "購入" ? "購入する" : "売却する";

        confirmButtonText.text =
            selectedName + "を" + actionName;
    }

    public void ConfirmTrade()
    {
        if (selectedAction == null) return;

        // 確定ボタンを連打できないようにする
        if (confirmButton != null)
            confirmButton.interactable = false;

        try
        {
            selectedAction.Invoke();
        }
        finally
        {
            // 取引後も商品を選択状態に保つ
            if (confirmButton != null)
                confirmButton.interactable = true;
        }
    }
}

