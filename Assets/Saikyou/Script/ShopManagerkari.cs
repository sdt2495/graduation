
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

    private string[] effectNames =
    {
        "AttackUP",
        "SpeedUP",
        "MaxHP UP",
        "TAKESHI",
        "MASARUTV"
    };

    private int[] prices = { 100, 100, 150, 150, 500000 };

    private int[] currentEffects = new int[3];

    private bool[] purchased = new bool[3];

    private int selectedIndex = -1;

    void Start()
    {
        shopPanel.SetActive(false);
        purchaseButton.interactable = false;
        UpdateCoinText();

        hasRerolled = false;
        rerollButton.interactable = true;
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

        // 効果の番号を用意
        int[] availableEffects = { 0, 1, 2, 3, 4 };

        // Fisher-Yatesシャッフル
        for (int i = 0; i < availableEffects.Length; i++)
        {
            int random = Random.Range(i, availableEffects.Length);

            int temp = availableEffects[i];
            availableEffects[i] = availableEffects[random];
            availableEffects[random] = temp;
        }

        // シャッフルした先頭3つを使用
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

        hasPurchased = true;

        // RE-ROLLを使用不可にする
        rerollButton.interactable = false;

        purchased[selectedIndex] = true;

        effectTexts[selectedIndex].text = "購入済み";

        // すべての商品を選択不可にする
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
}
