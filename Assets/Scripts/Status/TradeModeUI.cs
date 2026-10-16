
using UnityEngine;

public class TradeModeUI : MonoBehaviour
{
    [Header("最初に表示するBuy/Sellボタン")]
    public GameObject modeButtons;

    [Header("購入ボタンの親")]
    public GameObject buyButtons;

    [Header("売却ボタンの親")]
    public GameObject sellButtons;

    private void Start()
    {
        ShowModeSelect();
    }

    // 最初の画面に戻る
    public void ShowModeSelect()
    {
        modeButtons.SetActive(true);
        buyButtons.SetActive(false);
        sellButtons.SetActive(false);
    }

    // Buyを押したとき
    public void ShowBuy()
    {
        modeButtons.SetActive(false);
        buyButtons.SetActive(true);
        sellButtons.SetActive(false);
    }

    // Sellを押したとき
    public void ShowSell()
    {
        modeButtons.SetActive(false);
        buyButtons.SetActive(false);
        sellButtons.SetActive(true);
    }
}

