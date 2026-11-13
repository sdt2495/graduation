
using UnityEngine;
using UnityEngine.UI;

public class PageSwitcher : MonoBehaviour
{
    [Header("ページ")]
    public GameObject page1;
    public GameObject page2;

    [Header("メインステータスのボタン")]
    public Button[] mainStatusButtons;

    [Header("サブステータスのボタン")]
    public Button[] subStatusButtons;

    [Header("サブステータス管理")]
    public SubStatusTrade subStatusTrade;

    private void Start()
    {
        // 最初はメインページ
        ShowPage1();

        RegisterButtons(mainStatusButtons, true);
        RegisterButtons(subStatusButtons, false);

        // 起動時にも数値を更新
        RefreshSubStatusLabels();
    }

    private void RegisterButtons(Button[] buttons, bool isMain)
    {
        if (buttons == null) return;

        foreach (Button button in buttons)
        {
            if (button == null) continue;

            StatusButtonHover hover =
                button.GetComponent<StatusButtonHover>();

            if (hover == null)
            {
                hover = button.gameObject.AddComponent<StatusButtonHover>();
            }

            hover.Initialize(this, isMain);
        }
    }

    public void ShowPage1()
    {
        if (page1 != null) page1.SetActive(true);
        if (page2 != null) page2.SetActive(false);
    }

    public void ShowPage2()
    {
        if (page1 != null) page1.SetActive(false);
        if (page2 != null) page2.SetActive(true);

        // サブページを開いたら数値を更新
        RefreshSubStatusLabels();
    }

    public void SwitchPage(bool isMain)
    {
        if (isMain)
        {
            ShowPage1();
        }
        else
        {
            ShowPage2();
        }
    }

    private void RefreshSubStatusLabels()
    {
        if (subStatusTrade == null)
        {
            subStatusTrade = FindFirstObjectByType<SubStatusTrade>();
        }

        if (subStatusTrade != null)
        {
            subStatusTrade.RefreshSubStatusButtonLabels();
        }
    }
}







