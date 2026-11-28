using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopManagerkari : MonoBehaviour
{
    public GameObject shopPanel;

    public TMP_Text coinText;

    [Header("商品表示")]
    public TMP_Text[] itemNameTexts;
    public TMP_Text[] effectNameTexts;
    public TMP_Text[] priceTexts;

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

    private string[] itemNames =
    {
         "均衡調律",
         "大丈夫だ。問題ない",
         "あったまってきた",
         "背水の陣",
         "狂乱",
         "黄金律",
         "指数関数",
         "完璧主義",
         "攻撃こそ最大の防御"
    };

    private string[] effectNames =
    {
       "すべてのステータスを6増加させる",
       "１連の入力に成功すると、テクニックが3ずつ増加していく、失敗すると半減する、また戦闘が終わると増加数値は０となる",
       "現在HPが30以下の時に発動、攻撃力+30、クリティカルダメ―ジ+50％増加する",
       "獲得時、SANが30低下する、SANの減少量に対して、攻撃力、クリティカルダメージが伸びる(SAN-1(SANは100の想定)につき、攻撃力+1、クリティカルダメージ+2%)",
       "所持goldに比例して、攻撃力が上がる(所持gold5に対して攻撃力1)",
       "コンボが一定数に到達すると発動し固定のダメージを与える、発動タイミングは10、15、18、20~以降は毎回発動する",
       "一定数コンボを繋がないと与えるダメージが70%減少する、コンボが10を超えたらそれ以降の倍率が『×3.0』になる(通常×1.0　 ×1.05　× 1.1 　×1.2の順)",
       "コンボ数が5の倍数に達するたびに、0.35秒入力受付時間が延長される"
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

            int effect = currentEffects[i];

            itemNameTexts[i].text = itemNames[effect];

            effectNameTexts[i].text = effectNames[effect];

            priceTexts[i].text =
                "価格: " + prices[effect] + "コイン";

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

        itemNameTexts[selectedIndex].text = "購入済み";

        effectNameTexts[selectedIndex].text = "";

        priceTexts[selectedIndex].text = "";

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
