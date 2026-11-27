using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopManagerkari : MonoBehaviour
{
    public GameObject shopPanel;

    public TMP_Text coinText;
    public TMP_Text[] effectTexts;

    public Button[] selectButtons;
    public Button purchaseButton;

    public TMP_Text selectedText;

    private int coins = 250;

    public Button rerollButton;

    private bool hasRerolled = false;
    private bool hasPurchased = false;

    [Header("選択ボタン演出")]
    public float moveAmount = 15f;
    public float moveSpeed = 8f;
    public Color selectedColor = Color.yellow;

    private RectTransform[] buttonRects;
    private Vector2[] originalPositions;
    private Outline[] buttonOutlines;

    private string[] effectNames =
    {
        "ATK+6",
        "HP+7",
        "DEF+90",
        "CRI+7",
        "ATK+999999"
    };

    private int[] prices = { 100, 100, 150, 150, 500000 };

    private int[] currentEffects = new int[3];
    private bool[] purchased = new bool[3];

    private int selectedIndex = -1;

    [Header("ステータス連携")]
    public StatusRadar statusRadar;

    [Header("ステータス数値表示")]
    public TMP_Text hpValueText;
    public TMP_Text atkValueText;
    public TMP_Text defValueText;
    public TMP_Text criValueText;
    public TMP_Text sanValueText;
    public TMP_Text tecValueText;

    void Start()
    {
        shopPanel.SetActive(false);
        purchaseButton.interactable = false;
        UpdateCoinText();

        hasRerolled = false;
        rerollButton.interactable = true;

        buttonRects = new RectTransform[selectButtons.Length];
        originalPositions = new Vector2[selectButtons.Length];
        buttonOutlines = new Outline[selectButtons.Length];

        for (int i = 0; i < selectButtons.Length; i++)
        {
            buttonRects[i] = selectButtons[i].GetComponent<RectTransform>();

            originalPositions[i] = buttonRects[i].anchoredPosition;

            // 枠線を追加
            buttonOutlines[i] = selectButtons[i].gameObject.GetComponent<Outline>();

            if (buttonOutlines[i] == null)
            {
                buttonOutlines[i] = selectButtons[i].gameObject.AddComponent<Outline>();
            }

            buttonOutlines[i].effectColor = selectedColor;
            buttonOutlines[i].effectDistance = new Vector2(5, 5);

            buttonOutlines[i].enabled = false;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            bool open = !shopPanel.activeSelf;
            shopPanel.SetActive(open);

            if (open)
            {
                GenerateShop();
            }
        }
    }

    void GenerateShop()
    {
        hasRerolled = false;
        rerollButton.interactable = true;

        selectedIndex = -1;
        hasPurchased = false;
        selectedText.text = "商品を選択してください";

        purchaseButton.interactable = false;

        int[] availableEffects = { 0, 1, 2, 3, 4 };

        for (int i = 0; i < availableEffects.Length; i++)
        {
            int random = Random.Range(i, availableEffects.Length);

            int temp = availableEffects[i];
            availableEffects[i] = availableEffects[random];
            availableEffects[random] = temp;
        }

        for (int i = 0; i < 3; i++)
        {
            purchased[i] = false;
            currentEffects[i] = availableEffects[i];

            effectTexts[i].text =
                effectNames[currentEffects[i]] +
                "\n価格: " + prices[currentEffects[i]] + "コイン";

            int index = i;

            selectButtons[i].interactable = true;
            selectButtons[i].onClick.RemoveAllListeners();

            selectButtons[i].onClick.AddListener(
                () => SelectItem(index)
            );
        }

        // 選択演出をリセット
        StopAllCoroutines();

        for (int i = 0; i < selectButtons.Length; i++)
        {
            buttonOutlines[i].enabled = false;
            buttonRects[i].anchoredPosition = originalPositions[i];
        }
    }

    void SelectItem(int index)
    {
        if (purchased[index])
            return;

        selectedIndex = index;

        int effect = currentEffects[index];

        selectedText.text =
            "選択中: " + effectNames[effect] +
            " / " + prices[effect] + "コイン";

        purchaseButton.interactable = true;

        // 全ボタンの選択枠を解除
        for (int i = 0; i < selectButtons.Length; i++)
        {
            buttonOutlines[i].enabled = false;
        }

        // 選択したボタンだけ枠を表示
        buttonOutlines[index].enabled = true;

        // 選択したボタンを上下に動かす
        StopAllCoroutines();
        StartCoroutine(MoveButton(index));
    }

    public void Purchase()
    {
        if (hasPurchased)
            return;

        if (selectedIndex == -1)
            return;

        int effect = currentEffects[selectedIndex];
        int price = prices[effect];

        if (coins < price)
        {
            selectedText.text = "コインが足りません！";
            return;
        }

        coins -= price;

        if (statusRadar != null)
        {
            float hp = statusRadar.HP;
            float atk = statusRadar.ATK;
            float def = statusRadar.DEF;
            float cri = statusRadar.CRI;
            float tec = statusRadar.TEC;
            float other = statusRadar.Other;

            switch (effect)
            {
                case 0:
                    atk += 6;
                    break;

                case 1:
                    hp += 7;
                    break;

                case 2:
                    def += 90;
                    break;

                case 3:
                    cri += 7;
                    break;

                case 4:
                    atk += 999999;
                    break;
            }

            statusRadar.SetStatus(
                Mathf.Clamp(hp, 0, 100),
                Mathf.Clamp(atk, 0, 100),
                Mathf.Clamp(def, 0, 100),
                Mathf.Clamp(cri, 0, 100),
                Mathf.Clamp(tec, 0, 100),
                Mathf.Clamp(other, 0, 100)
            );

            // 数値テキストを更新
            UpdateStatusText();

            Debug.Log("ショップ購入によるステータス更新完了！");
        }
        else
        {
            Debug.LogError("ShopManagerkari: StatusRadarが設定されていません！");
        }

        hasPurchased = true;

        rerollButton.interactable = false;

        purchased[selectedIndex] = true;
        effectTexts[selectedIndex].text = "購入済み";

        buttonOutlines[selectedIndex].enabled = false;

        for (int i = 0; i < 3; i++)
        {
            selectButtons[i].interactable = false;
        }

        selectedText.text = "購入完了！";

        selectedIndex = -1;
        purchaseButton.interactable = false;

        UpdateCoinText();

        Debug.Log("購入成功！");
    }

    // ステータス数値表示を更新
    private void UpdateStatusText()
    {
        if (statusRadar == null) return;

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

    void UpdateCoinText()
    {
        coinText.text = "Coin: " + coins;
    }

    public void Reroll()
    {
        if (hasRerolled)
            return;

        hasRerolled = true;

        GenerateShop();

        rerollButton.interactable = false;

        Debug.Log("RE-ROLLしました！");
    }

    IEnumerator MoveButton(int index)
    {
        RectTransform rect = buttonRects[index];

        Vector2 original = originalPositions[index];
        Vector2 target = original + new Vector2(0, moveAmount);

        float time = 0f;

        // 上に移動
        while (time < 1f)
        {
            time += Time.deltaTime * moveSpeed;

            rect.anchoredPosition = Vector2.Lerp(
                original,
                target,
                time
            );

            yield return null;
        }

        rect.anchoredPosition = target;

        // 少し待つ
        yield return new WaitForSeconds(0.1f);

        // 元の位置に戻る
        time = 0f;

        while (time < 1f)
        {
            time += Time.deltaTime * moveSpeed;

            rect.anchoredPosition = Vector2.Lerp(
                target,
                original,
                time
            );

            yield return null;
        }

        rect.anchoredPosition = original;
    }
}
