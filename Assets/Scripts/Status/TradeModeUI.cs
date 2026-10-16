
using UnityEngine;

public class TradeModeUI : MonoBehaviour
{
    [Header("Buy/Sellボタン（常に表示）")]
    public GameObject modeButtons;

    [Header("購入ボタンの親")]
    public GameObject buyButtons;

    [Header("売却ボタンの親")]
    public GameObject sellButtons;

    private void Start()
    {
        ShowModeSelect();
    }

    // 最初の画面：BuyとSellだけ表示
    public void ShowModeSelect()
    {
        if (modeButtons != null)
            modeButtons.SetActive(true);

        if (buyButtons != null)
            buyButtons.SetActive(false);

        if (sellButtons != null)
            sellButtons.SetActive(false);
    }

    // Buyを押したとき：Buyと購入ボタンを表示
    public void ShowBuy()
    {
        if (modeButtons != null)
            modeButtons.SetActive(true);

        if (buyButtons != null)
            buyButtons.SetActive(true);

        if (sellButtons != null)
            sellButtons.SetActive(false);
    }

    // Sellを押したとき：Sellと売却ボタンを表示
    public void ShowSell()
    {
        if (modeButtons != null)
            modeButtons.SetActive(true);

        if (buyButtons != null)
            buyButtons.SetActive(false);

        if (sellButtons != null)
            sellButtons.SetActive(true);
    }
}


